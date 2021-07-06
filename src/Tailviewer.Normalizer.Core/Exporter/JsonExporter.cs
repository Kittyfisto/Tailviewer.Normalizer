using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Tailviewer.Api;
using Tailviewer.Normalizer.Core.Database;

namespace Tailviewer.Normalizer.Core.Exporter
{
	public sealed class JsonExporter
		: IExporter
	{
		#region Implementation of IExporter

		public void ExportTo(ILogEntryDatabase database, string filePath)
		{
			using (StreamWriter writer = File.CreateText(filePath))
			using (var reader = database.CreateReader())
			{
				JsonSerializer serializer = new JsonSerializer();

				foreach (var logEntry in reader.Query(null))
				{
					serializer.Serialize(writer, CreateJson(logEntry));
					writer.WriteLine();
				}
			}
		}

		private object CreateJson(IReadOnlyLogEntry logEntry)
		{
			var ret = new Dictionary<string, object>();
			ret.Add("Line", logEntry.LineNumber);
			ret.Add("FileName", logEntry.GetValue(CustomColumns.SourceFileName));
			ret.Add("FullFilePath", logEntry.GetValue(CustomColumns.FullSourceFilePath));
			ret.Add("Timestamp", ToString(logEntry.Timestamp));
			ret.Add("Level", logEntry.LogLevel.ToString().ToLower());
			ret.Add("RawMessage", logEntry.RawContent);
			return ret;
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
		}

		#endregion

		private static string ToString(DateTime? timestamp)
		{
			if (timestamp == null)
				return null;

			return timestamp.Value.ToString("s");
		}
	}
}
