using System;
using Tailviewer.Normalizer.Core.Database;

namespace Tailviewer.Normalizer.Core.Exporter
{
	public interface IExporter
		: IDisposable
	{
		void ExportTo(ILogEntryDatabase database, string filePath);
	}
}