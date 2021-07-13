using System;
using System.Collections.Generic;
using Tailviewer.Normalizer.Core.Database;

namespace Tailviewer.Normalizer.Core.Exporter
{
	public interface IExporter
		: IDisposable
	{
		int ExportTo(NormalizationOptions options, 
		             IReadOnlyList<LogFileReport> logFileReports,
		             IReadOnlyList<PluginReport> plugins,
		             ILogEntryDatabase database,
		             string filePath);
	}
}