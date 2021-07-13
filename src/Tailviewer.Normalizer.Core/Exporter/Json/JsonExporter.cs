using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Tailviewer.Normalizer.Core.Database;

namespace Tailviewer.Normalizer.Core.Exporter.Json
{
	public sealed class JsonExporter
		: IExporter
	{
		#region Implementation of IExporter

		public int ExportTo(NormalizationOptions options, ILogEntryDatabase database, string filePath)
		{
			int logEntryCount = 0;

			using (StreamWriter streamWriter = File.CreateText(filePath))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			using (var reader = database.CreateReader())
			{
				jsonWriter.IndentChar = ' ';
				jsonWriter.Indentation = 4;
				jsonWriter.Formatting = Formatting.Indented;
				jsonWriter.DateFormatString = "s";

				JsonSerializer serializer = new JsonSerializer();

				var jsonObject = new NormalizedLog
				{
					Options = options,
					Events = new List<LogEvent>()
				};
				foreach (var logEntry in reader.Query(null))
				{
					jsonObject.Events.Add(new LogEvent(logEntry));
					++logEntryCount;
				}
				serializer.Serialize(jsonWriter, jsonObject);
			}

			return logEntryCount;
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
		}

		#endregion
	}
}
