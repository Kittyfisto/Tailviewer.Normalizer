using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;

namespace Tailviewer.Normalizer.Core.Sources
{
	/// <summary>
	///     An <see cref="IFilesystem" /> implementation which is capable of treating archives (zip, tar, you name it) as
	///     simple folders, allowing
	///     seamless access to their files (recursively, if needed).
	/// </summary>
	public sealed class CompoundFilesystem
		: IFilesystem
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		sealed class CompoundFileInfo
			: IFileInfo
		{
			private readonly CompoundFilesystem _compoundFilesystem;
			private readonly string _fullPath;
			private readonly string _pathRelativeToArchive;
			private readonly ArchiveFilesystem _archive;
			private readonly IFileInfo _archiveFileInfo;

			public CompoundFileInfo(CompoundFilesystem compoundFilesystem,
			                        string fullPath,
			                        string pathRelativeToArchive,
			                        ArchiveFilesystem archive)
			{
				_compoundFilesystem = compoundFilesystem;
				_fullPath = fullPath;
				_pathRelativeToArchive = pathRelativeToArchive;
				_archive = archive;
				_archiveFileInfo = _archive.GetFileInfo(_pathRelativeToArchive);
			}

			#region Implementation of IFileInfo

			public Stream Create()
			{
				throw new NotImplementedException();
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			public IDirectoryInfo Directory
			{
				get
				{
					return _compoundFilesystem.GetDirectoryInfo(DirectoryName);
				}
			}

			public string DirectoryName
			{
				get
				{
					return Path.GetDirectoryName(_fullPath);
				}
			}

			public string Name
			{
				get
				{
					return Path.GetFileName(_fullPath);
				}
			}

			public string FullPath
			{
				get
				{
					return _fullPath;
				}
			}

			public long Length
			{
				get
				{
					return _archiveFileInfo.Length;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return _archiveFileInfo.IsReadOnly;
				}
			}

			public bool Exists
			{
				get
				{
					return _archiveFileInfo.Exists;
				}
			}

			public DateTime CreationTimeUtc
			{
				get
				{
					return _archiveFileInfo.CreationTimeUtc;
				}
			}

			public DateTime LastAccessTimeUtc
			{
				get
				{
					return _archiveFileInfo.LastAccessTimeUtc;
				}
			}

			public DateTime LastWriteTimeUtc
			{
				get
				{
					return _archiveFileInfo.LastWriteTimeUtc;
				}
			}

			#endregion
		}

		sealed class ArchiveDirectoryInfo
			: IDirectoryInfo
			, IDisposable
		{
			private readonly CompoundFilesystem _compoundFilesystem;
			private readonly string _path;
			private readonly ArchiveFilesystem _archive;

			public ArchiveFilesystem Archive
			{
				get { return _archive; }
			}

			public ArchiveDirectoryInfo(CompoundFilesystem compoundFilesystem,
			                            string path,
			                            ArchiveFilesystem archive)
			{
				_compoundFilesystem = compoundFilesystem;
				_path = path;
				_archive = archive;
			}

			#region Implementation of IDirectoryInfo

			public bool FileExists(string filename)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<IFileInfo> EnumerateFiles()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
			{
				throw new NotImplementedException();
			}

			public void Create()
			{
				throw new NotImplementedException();
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			public IDirectoryInfo CreateSubdirectory(string path)
			{
				throw new NotImplementedException();
			}

			public IDirectoryInfo Root
			{
				get
				{
					return _compoundFilesystem.GetDirectoryInfo(Path.GetPathRoot(_path));
				}
			}

			public IDirectoryInfo Parent
			{
				get
				{
					return _compoundFilesystem.GetDirectoryInfo(Path.GetDirectoryName(_path));
				}
			}

			public string Name
			{
				get
				{
					return Path.GetFileName(_path);
				}
			}

			public string FullName
			{
				get
				{
					return _path;
				}
			}

			public bool Exists
			{
				get
				{
					return true;
				}
			}

			#endregion

			#region IDisposable

			public void Dispose()
			{
				_archive.Dispose();
			}

			#endregion

			public IFileInfo GetFile(string pathRelativeToArchive)
			{
				return new CompoundFileInfo(_compoundFilesystem,
				                            Path.Combine(_path, pathRelativeToArchive),
				                            pathRelativeToArchive,
				                            _archive);
			}

			public IDirectoryInfo GetDirectory(string pathRelativeToArchive)
			{
				return new ArchiveDirectoryInfo(_compoundFilesystem,
				                                Path.Combine(_path, pathRelativeToArchive),
				                                _archive);
			}
		}

		private readonly Dictionary<string, ArchiveDirectoryInfo> _archives;
		private readonly IFilesystem _fileSystem;

		public CompoundFilesystem(IFilesystem fileSystem)
		{
			_fileSystem = fileSystem;
			_archives = new Dictionary<string, ArchiveDirectoryInfo>(new PathComparer());
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
			return GetDirectoryInfo(path).Exists;
		}

		public IDirectoryInfo GetDirectoryInfo(string path)
		{
			if (TryGetArchive(path, out var archive, out var pathRelativeToArchive))
			{
				return archive.GetDirectory(pathRelativeToArchive);
			}

			return _fileSystem.GetDirectoryInfo(path);
		}

		public IReadOnlyList<string> EnumerateFiles(string path,
		                                            string searchPattern = null,
		                                            SearchOption searchOption = SearchOption.TopDirectoryOnly,
		                                            bool tolerateNonExistantPath = false)
		{
			var rawFiles = new Stack<string>();
			if (IsArchive(path))
			{
				rawFiles.Push(path);
			}
			else
			{
				foreach (var file in _fileSystem.EnumerateFiles(path, searchPattern, searchOption,
				                                                tolerateNonExistantPath))
				{
					rawFiles.Push(file);
				}
			}

			var allFiles = new List<string>();

			while (rawFiles.Count > 0)
			{
				var rawFile = rawFiles.Pop();
				if (searchOption == SearchOption.AllDirectories && TryOpenArchive(rawFile, out var archive))
				{
					var archiveFiles =
						archive.Archive.EnumerateFiles("", searchPattern, SearchOption.AllDirectories,
						                       tolerateNonExistantPath);
					foreach (var archiveFile in archiveFiles)
					{
						var fullFilePath = Path.Combine(rawFile, archiveFile);
						rawFiles.Push(fullFilePath);
					}
				}
				else
				{
					allFiles.Add(rawFile);
				}
			}

			return allFiles;
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
			return GetFileInfo(fullPath).CreationTimeUtc;
		}

		public DateTime FileLastAccessTimeUtc(string fullPath)
		{
			return GetFileInfo(fullPath).LastAccessTimeUtc;
		}

		public DateTime FileLastWriteTimeUtc(string fullPath)
		{
			return GetFileInfo(fullPath).LastWriteTimeUtc;
		}

		public IFileInfo GetFileInfo(string fileName)
		{
			if (TryGetArchive(fileName, out var archive, out var pathRelativeToArchive))
			{
				return archive.GetFile(pathRelativeToArchive);
			}

			return _fileSystem.GetFileInfo(fileName);
		}

		public bool FileExists(string path)
		{
			try
			{
				return GetFileInfo(path).Exists;
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception: {0}", e);
				return false;
			}
		}

		public long FileLength(string path)
		{
			return GetFileInfo(path).Length;
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
			if (TryGetArchive(path, out var archive, out var pathRelativeToArchive))
			{
				return archive.Archive.Open(pathRelativeToArchive, mode, access, share);
			}

			return _fileSystem.Open(path, mode, access, share);
		}

		public Stream OpenRead(string path)
		{
			if (TryGetArchive(path, out var archive, out var pathRelativeToArchive))
			{
				return archive.Archive.OpenRead(pathRelativeToArchive);
			}

			return _fileSystem.OpenRead(path);
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
			using (var reader = new StreamReader(OpenRead(path)))
			{
				return ReadAllLines(reader);
			}
		}

		public IReadOnlyList<string> ReadAllLines(string path, Encoding encoding)
		{
			using (var reader = new StreamReader(OpenRead(path), encoding))
			{
				return ReadAllLines(reader);
			}
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
			get { throw new NotImplementedException(); }
		}

		#endregion

		private IReadOnlyList<string> ReadAllLines(TextReader reader)
		{
			var lines = new List<string>();
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				lines.Add(line);
			}

			return lines;
		}

		private bool TryOpenArchive(string filePath, out ArchiveDirectoryInfo archive)
		{
			if (IsArchive(filePath))
			{
				try
				{
					archive = OpenArchive(filePath);
					return true;
				}
				catch(Exception e)
				{
					Log.WarnFormat("Caught exception while trying to open archive '{0}': {1}", filePath, e);
				}
			}

			archive = null;
			return false;
		}

		private ArchiveDirectoryInfo OpenArchive(string filePath)
		{
			// Let's see if we have this archive already opened
			if (_archives.TryGetValue(filePath, out var directoryInfo))
			{
				return directoryInfo;
			}

			// Since it's not open already, we should go and obtain a stream onto the file.
			// We may need to recursively open archives to do so.
			var archives = SplitBySubArchives(filePath);
			ArchiveDirectoryInfo parentArchive = null;
			for(int i = 0; i < archives.Count - 1; ++i)
			{
				parentArchive = OpenArchive(archives[i]);
			}

			Stream stream;
			if (parentArchive != null)
			{
				var relativePath = filePath.Substring(parentArchive.FullName.Length + 1);
				stream = parentArchive.Archive.OpenRead(relativePath);
			}
			else
			{
				stream = _fileSystem.OpenRead(filePath);
			}

			try
			{
				var archive = new ZipArchive(stream);
				var archiveFilesystem = new ArchiveFilesystem(archive);
				directoryInfo = new ArchiveDirectoryInfo(this, filePath, archiveFilesystem);
				_archives.Add(filePath, directoryInfo);
				return directoryInfo;
			}
			catch(Exception)
			{
				stream.Dispose();
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fullPath"></param>
		/// <param name="archive"></param>
		/// <param name="pathRelativeToArchive"></param>
		/// <returns></returns>
		private bool TryGetArchive(string fullPath, out ArchiveDirectoryInfo archive, out string pathRelativeToArchive)
		{
			var archives = SplitBySubArchives(fullPath);
			if (archives.Count > 0)
			{
				var lastArchive = archives[archives.Count - 1];
				archive = OpenArchive(lastArchive);
				if (lastArchive.Length >= fullPath.Length)
				{
					pathRelativeToArchive = "";
				}
				else
				{
					pathRelativeToArchive = fullPath.Substring(lastArchive.Length + 1);
				}
				return true;
			}

			archive = null;
			pathRelativeToArchive = null;
			return false;
		}

		/// <summary>
		/// Splits the given 
		/// </summary>
		/// <param name="fullPath"></param>
		/// <returns></returns>
		[Pure]
		private static IReadOnlyList<string> SplitBySubArchives(string fullPath)
		{
			var archives = new List<string>();

			var folders = SplitByFolders(fullPath);
			for (int i = 0; i < folders.Count; ++i)
			{
				var fileOrFolder = folders[i];
				if (IsArchive(fileOrFolder))
				{
					// Path.Combine sucks when the root ends with a :, hence we have to do it ourselves...
					var subPath = string.Join(Path.DirectorySeparatorChar.ToString(), folders.Take(i+1).ToArray());
					archives.Add(subPath);
				}
			}

			return archives;
		}

		/// <summary>
		/// Splits the given 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[Pure]
		private static IReadOnlyList<string> SplitByFolders(string path)
		{
			return path.Split(new[] {'/', '\\'});
		}

		[Pure]
		public static bool IsArchive(string fileName)
		{
			var extension = Path.GetExtension(fileName);
			switch (extension)
			{
				case ".zip":
					return true;
			}

			return false;
		}

		#region IDisposable

		public void Dispose()
		{
			foreach (var archive in _archives.Values)
			{
				archive.Dispose();
			}
		}

		#endregion
	}
}