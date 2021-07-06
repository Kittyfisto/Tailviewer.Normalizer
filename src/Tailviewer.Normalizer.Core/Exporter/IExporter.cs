using System;
using Tailviewer.Normalizer.Core.Database;

namespace Tailviewer.Normalizer.Core.Exporter
{
	public interface IExporter
		: IDisposable
	{
		int ExportTo(ILogEntryDatabase database, string filePath);
	}
}