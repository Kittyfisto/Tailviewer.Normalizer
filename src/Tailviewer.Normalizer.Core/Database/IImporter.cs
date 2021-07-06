using System;
using System.Collections.Generic;
using System.IO;
using Tailviewer.Api;

namespace Tailviewer.Normalizer.Core.Database
{
	public interface IImporter
		: IDisposable
	{
		void Import(IFileInfo source, IEnumerable<IReadOnlyLogEntry> logEntries);
		void Commit();
	}
}