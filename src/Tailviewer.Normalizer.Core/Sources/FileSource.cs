using System;
using System.IO;

namespace Tailviewer.Normalizer.Core.Sources
{
	public sealed class FileSource
		: IAnalyzerSource
	{
		private readonly string _path;
		private readonly IFilesystem _filesystem;

		public FileSource(string path, IFilesystem filesystem)
		{
			_path = path;
			_filesystem = filesystem;
		}

		#region Implementation of IAnalyzerSource

		public string Path
		{
			get { throw new NotImplementedException(); }
		}

		public string FileName
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}