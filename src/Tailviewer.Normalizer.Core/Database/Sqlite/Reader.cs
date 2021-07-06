using System.Collections.Generic;
using System.Data.SQLite;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Normalizer.Core.Database.Sqlite.Schema;

namespace Tailviewer.Normalizer.Core.Database.Sqlite
{
	public sealed class Reader
		: IReader
	{
		private readonly SQLiteConnection _connection;

		public Reader(SQLiteConnection connection)
		{
			_connection = connection;
		}

		#region Implementation of IReader

		public IEnumerable<IReadOnlyLogEntry> Query(ILogEntryFilter filter)
		{
			var sources = ReadSources();
			var logEntriesCommand = new SQLiteCommand($"select {LogEntriesTable.LineNumberColumn}, {LogEntriesTable.LogSourceColumn}, {LogEntriesTable.TimestampColumn}, {LogEntriesTable.LogLevelColumn}, {LogEntriesTable.RawContentColumn} from {LogEntriesTable.Name} order by {LogEntriesTable.TimestampColumn} asc", _connection);
			using (var reader = logEntriesCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return CreateLogEntry(reader, sources);
				}
			}
		}

		private IReadOnlyDictionary<long, LogSource> ReadSources()
		{
			var logSourcesCommand =
				new
					SQLiteCommand($"select {LogSourceTable.IdColumn}, {LogSourceTable.FileNameColumn}, {LogSourceTable.FullFilePathColumn} from {LogSourceTable.Name}", _connection);

			var ret = new Dictionary<long, LogSource>();
			using (var reader = logSourcesCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var id = reader.GetInt64(0);
					var name = GetString(reader, 1);
					var path = GetString(reader, 2);
					ret.Add(id, new LogSource{FileName = name, FullFilePath = path});
				}
			}

			return ret;
		}

		struct LogSource
		{
			public string FileName;
			public string FullFilePath;
		}

		private IReadOnlyLogEntry CreateLogEntry(SQLiteDataReader reader,
		                                         IReadOnlyDictionary<long, LogSource> sources)
		{
			var lineNumber = reader.GetInt32(0);
			var logSourceId = reader.GetInt64(1);
			var timestamp = GetString(reader, 2);
			var level = GetString(reader, 3);
			var rawContent = GetString(reader, 4);
			var logSource = sources[logSourceId];

			return new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>
			{
				{Columns.Index, lineNumber - 1 },
				{Columns.LineNumber, lineNumber },
				{Columns.Timestamp, LogEntriesTable.TryParseTimestamp(timestamp) },
				{Columns.LogLevel, LogEntriesTable.TryParseLevel(level) },
				{Columns.RawContent, rawContent },
				{CustomColumns.FullSourceFilePath, logSource.FullFilePath },
				{CustomColumns.SourceFileName, logSource.FileName }
			});
		}

		private string GetString(SQLiteDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
				return null;

			return reader.GetString(index);
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
		}

		#endregion
	}
}