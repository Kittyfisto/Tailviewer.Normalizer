using System;
using System.Collections.Generic;
using System.IO;

namespace Tailviewer.Normalizer.Core.Sources
{
	public sealed class FolderSource
		: IAnalyzerSources
	{
		private readonly IFilesystem _filesystem;
		private readonly string _root;

		public FolderSource(IFilesystem filesystem, string root)
		{
			_filesystem = filesystem;
			_root = root;
		}

		#region Implementation of ILogSources

		public IEnumerable<IAnalyzerSource> Enumerate(LogSourceFilter filter)
		{
			var files = _filesystem.EnumerateFiles(_root, null, SearchOption.AllDirectories);
			foreach (var file in files)
			{
				yield return CreateSource(file);
			}
		}

		#endregion

		private IAnalyzerSource CreateSource(string file)
		{
			return new FileSource(file, _filesystem);
		}

		#region IDisposable

		public void Dispose()
		{
			if (_filesystem is IDisposable disposable)
				disposable.Dispose();
		}

		#endregion
	}
}