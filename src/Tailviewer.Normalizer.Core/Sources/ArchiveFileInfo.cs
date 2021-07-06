using System;
using System.IO;
using System.IO.Compression;

namespace Tailviewer.Normalizer.Core.Sources
{
	public sealed class ArchiveFileInfo
		: IFileInfo
	{
		private readonly IDirectoryInfo _directory;
		private readonly ZipArchiveEntry _entry;
		private readonly ArchiveFilesystem _filesystem;
		private readonly string _fullPath;

		public ArchiveFileInfo(ZipArchiveEntry entry,
		                       ArchiveFilesystem filesystem,
		                       IDirectoryInfo directory)
		{
			_entry = entry;
			_fullPath = PathEx.Normalize(entry.FullName);
			_filesystem = filesystem;
			_directory = directory;
		}

		public Stream OpenRead()
		{
			return new SeekableReadOnlyStream(_entry.Open());
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
			get { return _directory; }
		}

		public string DirectoryName
		{
			get { return _directory.FullName; }
		}

		public string Name
		{
			get { return _entry.Name; }
		}

		public string FullPath
		{
			get { return _fullPath; }
		}

		public long Length
		{
			get { return _entry.Length; }
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public bool Exists
		{
			get { return true; }
		}

		public DateTime CreationTimeUtc
		{
			get
			{
				return _entry.LastWriteTime.DateTime;
			}
		}

		public DateTime LastAccessTimeUtc
		{
			get
			{
				return _entry.LastWriteTime.DateTime;
			}
		}

		public DateTime LastWriteTimeUtc
		{
			get
			{
				return _entry.LastWriteTime.DateTime;
			}
		}

		#endregion
	}
}