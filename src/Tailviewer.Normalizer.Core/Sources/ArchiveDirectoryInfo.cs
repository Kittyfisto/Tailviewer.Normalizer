using System;
using System.Collections.Generic;
using System.IO;

namespace Tailviewer.Normalizer.Core.Sources
{
	public sealed class ArchiveDirectoryInfo
		: IDirectoryInfo
	{
		private readonly ArchiveFilesystem _filesystem;
		private readonly IDirectoryInfo _root;
		private readonly IDirectoryInfo _parent;
		private readonly string _path;
		private readonly string _name;

		public ArchiveDirectoryInfo(string path,
		                            ArchiveFilesystem filesystem,
		                            IDirectoryInfo parent,
		                            IDirectoryInfo root)
		{
			_path = path;
			_name = path != "" ? Path.GetDirectoryName(path) : path;
			_filesystem = filesystem;
			_parent = parent;
			_root = root ?? this;
		}

		#region Implementation of IDirectoryInfo

		public bool FileExists(string filename)
		{
			return _filesystem.FileExists(Path.Combine(_path, filename));
		}

		public IEnumerable<IFileInfo> EnumerateFiles()
		{
			return _filesystem.EnumerateFilesEx(_path);
		}

		public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
		{
			return _filesystem.EnumerateFilesEx(_path, searchPattern);
		}

		public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
			return _filesystem.EnumerateFilesEx(_path, searchPattern, searchOption);
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
			get { return _root; }
		}

		public IDirectoryInfo Parent
		{
			get { return _parent; }
		}

		public string Name
		{
			get { return _name; }
		}

		public string FullName
		{
			get { return _path; }
		}

		public bool Exists
		{
			get { return true; }
		}

		#endregion
	}
}