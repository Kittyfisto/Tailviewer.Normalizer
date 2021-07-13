using System;
using Tailviewer.Api;

namespace Tailviewer.Normalizer.Core.Exporter
{
	public sealed class LogEvent
	{
		public LogEvent()
		{}

		public LogEvent(IReadOnlyLogEntry logEntry)
		{
			Line = logEntry.LineNumber;
			FileName = logEntry.GetValue(CustomColumns.SourceFileName);
			FullFilePath = logEntry.GetValue(CustomColumns.FullSourceFilePath);
			Timestamp = logEntry.Timestamp;
			Level = logEntry.LogLevel.ToString().ToLower();
			RawMessage = logEntry.RawContent;
		}

		private static string ToString(DateTime? timestamp)
		{
			if (timestamp == null)
				return null;

			return timestamp.Value.ToString("s");
		}

		public int Line { get; set; }
		public string FileName { get; set; }
		public string FullFilePath { get; set; }
		public DateTime? Timestamp { get; set; }
		public string Level { get; set; }
		public string RawMessage { get; set; }
	}
}