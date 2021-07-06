using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tailviewer.Normalizer.Core.Sources
{
	/// <summary>
	///     Treats an archive as an <see cref="IFilesystem" />.
	/// </summary>
	public sealed class ArchiveFilesystem
		: IFilesystem
		, IDisposable
	{
		private readonly ZipArchive _archive;
		private readonly Dictionary<string, IDirectoryInfo> _directories;
		private readonly Dictionary<string, ArchiveFileInfo> _files;
		private readonly IDirectoryInfo _root;

		public ArchiveFilesystem(ZipArchive archive)
		{
			_archive = archive ?? throw new ArgumentNullException(nameof(archive));
			_directories = new Dictionary<string, IDirectoryInfo>(new PathComparer());
			_root = new ArchiveDirectoryInfo("", this, parent: null, root: null);
			_directories.Add(_root.FullName, _root);
			_files = new Dictionary<string, ArchiveFileInfo>(new PathComparer());

			BuildIndex();
		}

		#region IDisposable

		public void Dispose()
		{
			_archive?.Dispose();
		}

		#endregion

		private void BuildIndex()
		{
			foreach (var entry in _archive.Entries)
				if (entry.FullName.EndsWith("/") ||
				    entry.FullName.EndsWith("\\"))
					GetOrAddDirectory(entry.FullName);
				else
					TryAddFile(entry);
		}

		private void TryAddFile(ZipArchiveEntry entry)
		{
			var parentPath = Path.GetDirectoryName(entry.FullName);
			var directory = GetOrAddDirectory(parentPath);

			if (!_files.ContainsKey(entry.FullName))
			{
				var fileInfo = new ArchiveFileInfo(entry, this, directory);
				_files.Add(fileInfo.FullPath, fileInfo);
			}
		}

		private IDirectoryInfo GetOrAddDirectory(string fullPath)
		{
			if (fullPath.EndsWith("/") ||
			    fullPath.EndsWith("\\"))
				fullPath = fullPath.Remove(fullPath.Length - 1, count: 1);

			if (!_directories.TryGetValue(fullPath, out var directory))
			{
				var parentPath = Path.GetDirectoryName(fullPath);
				IDirectoryInfo parent;
				if (parentPath == null)
					parent = _root;
				else
					parent = GetOrAddDirectory(parentPath);

				directory = new ArchiveDirectoryInfo(fullPath, this, parent, _root);
				_directories.Add(fullPath, directory);
			}

			return directory;
		}

		private ArchiveFileInfo GetFileInfoEx(string fileName)
		{
			if (!_files.TryGetValue(fileName, out var fileInfo))
			{
				var directoryPath = Path.GetDirectoryName(fileName);
				if (!_directories.ContainsKey(directoryPath))
					throw new DirectoryNotFoundException($"The given directory does not exist: {fileName}");

				throw new FileNotFoundException("The given file is not part of the archive", fileName);
			}

			return fileInfo;
		}

		[Pure]
		private static string GetRelativePathTo(string basePath, string otherPath)
		{
			var fakeRoot = "A:\\";
			var p1 = Path.Combine(fakeRoot, basePath);
			var p2 = Path.Combine(fakeRoot, otherPath);

			var path1 = new Uri(p1);
			var path2 = new Uri(p2);
			var diff = path1.MakeRelativeUri(path2);
			var relPath = diff.OriginalString;
			return relPath;
		}

		#region Implementation of IFilesystem

		public IDirectoryInfo CreateDirectory(string path)
		{
			throw new NotImplementedException();
		}

		public void DeleteDirectory(string path)
		{
			throw new NotImplementedException();
		}

		public void DeleteDirectory(string path, bool recursive)
		{
			throw new NotImplementedException();
		}

		public bool DirectoryExists(string path)
		{
			throw new NotImplementedException();
		}

		public IDirectoryInfo GetDirectoryInfo(string path)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<string> EnumerateFiles(string path,
		                                            string searchPattern = null,
		                                            SearchOption searchOption = SearchOption.TopDirectoryOnly,
		                                            bool tolerateNonExistantPath = false)
		{
			return EnumerateFilesEx(path, searchPattern, searchOption, tolerateNonExistantPath)
			       .Select(x => x.FullPath)
			       .ToList();
		}

		public IReadOnlyList<string> EnumerateDirectories(string path)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

		public DateTime FileCreationTimeUtc(string fullPath)
		{
			throw new NotImplementedException();
		}

		public DateTime FileLastAccessTimeUtc(string fullPath)
		{
			throw new NotImplementedException();
		}

		public DateTime FileLastWriteTimeUtc(string fullPath)
		{
			throw new NotImplementedException();
		}

		public IFileInfo GetFileInfo(string fileName)
		{
			return GetFileInfoEx(fileName);
		}

		public bool FileExists(string path)
		{
			throw new NotImplementedException();
		}

		public long FileLength(string path)
		{
			throw new NotImplementedException();
		}

		public bool IsFileReadOnly(string path)
		{
			throw new NotImplementedException();
		}

		public Stream CreateFile(string path)
		{
			throw new NotImplementedException();
		}

		public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return GetFileInfoEx(path).OpenRead();
		}

		public Stream OpenRead(string path)
		{
			return GetFileInfoEx(path).OpenRead();
		}

		public Stream OpenWrite(string path)
		{
			throw new NotImplementedException();
		}

		public void Write(string path, Stream stream)
		{
			throw new NotImplementedException();
		}

		public void WriteAllBytes(string path, byte[] bytes)
		{
			throw new NotImplementedException();
		}

		public void WriteAllText(string path, string contents)
		{
			throw new NotImplementedException();
		}

		public void WriteAllText(string path, string contents, Encoding encoding)
		{
			throw new NotImplementedException();
		}

		public byte[] ReadAllBytes(string path)
		{
			throw new NotImplementedException();
		}

		public string ReadAllText(string path)
		{
			throw new NotImplementedException();
		}

		public string ReadAllText(string path, Encoding encoding)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<string> ReadAllLines(string path)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<string> ReadAllLines(string path, Encoding encoding)
		{
			throw new NotImplementedException();
		}

		public void CopyFile(string sourceFileName, string destFileName)
		{
			throw new NotImplementedException();
		}

		public void DeleteFile(string path)
		{
			throw new NotImplementedException();
		}

		public IFilesystemWatchdog Watchdog
		{
			get { throw new NotImplementedException(); }
		}

		public string CurrentDirectory
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public IDirectoryInfo Current
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<IDirectoryInfo> Roots
		{
			get
			{
				return new[]
				{
					_root
				};
			}
		}

		#endregion

		public IEnumerable<ArchiveFileInfo> EnumerateFilesEx(string path,
		                                                     string searchPattern = null,
		                                                     SearchOption searchOption = SearchOption.TopDirectoryOnly,
		                                                     bool tolerateNonExistantPath = false)
		{
			if (!_directories.TryGetValue(path, out var directory))
			{
				if (tolerateNonExistantPath)
					return Enumerable.Empty<ArchiveFileInfo>();

				throw new DirectoryNotFoundException($"The given path does not exist: {path}");
			}

			return EnumerateFilesEx(directory, searchPattern, searchOption);
		}

		private IEnumerable<ArchiveFileInfo> EnumerateFilesEx(IDirectoryInfo directory,
		                                                      string searchPattern = null,
		                                                      SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			foreach (var fileInfo in _files.Values)
			{
				if (Matches(directory, fileInfo, searchPattern, searchOption))
					yield return fileInfo;
			}
		}

		private bool Matches(IDirectoryInfo directory, ArchiveFileInfo file, string searchPattern, SearchOption searchOption)
		{
			if (searchOption == SearchOption.TopDirectoryOnly)
			{
				if (!ReferenceEquals(file.Directory, directory))
				{
					return false;
				}
			}
			else
			{
				if (!IsParentOf(directory, file))
					return false;
			}

			if (searchPattern != null)
			{
				var regexPattern = Regex.Escape(searchPattern).Replace("\\*", ".*?");
				var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
				var fileName = file.Name;
				if (!regex.IsMatch(fileName))
					return false;
			}

			return true;
		}

		private bool IsParentOf(IDirectoryInfo directory, ArchiveFileInfo file)
		{
			var parent = file.Directory;
			while (parent != null)
			{
				if (ReferenceEquals(parent, directory))
					return true;
				if (ReferenceEquals(parent, _root))
					return false;

				parent = parent.Parent;
			}

			return false;
		}
	}
}