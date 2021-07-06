using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Tailviewer.Api;
using Tailviewer.Normalizer.Core.Database.Sqlite.Schema;

namespace Tailviewer.Normalizer.Core.Database.Sqlite
{
	public sealed class Importer
		: IImporter
	{
		private readonly SQLiteTransaction _transaction;

		private readonly SQLiteCommand _insertLogSourceCommand;
		private readonly SQLiteParameter _logSourceId;
		private readonly SQLiteParameter _logSourceFileName;
		private readonly SQLiteParameter _logSourceFullFilePath;

		private readonly SQLiteCommand _insertLogEntryCommand;
		private readonly SQLiteParameter _logLine;
		private readonly SQLiteParameter _logEntrySource;
		private readonly SQLiteParameter _logEntryTimestamp;
		private readonly SQLiteParameter _logEntryLevel;
		private readonly SQLiteParameter _logEntryRawContent;

		private long _nextLogSourceId;

		public Importer(SQLiteConnection connection)
		{
			_transaction = connection.BeginTransaction();
			_insertLogSourceCommand = new SQLiteCommand($"insert into {LogSourceTable.Name} ({LogSourceTable.IdColumn}, {LogSourceTable.FileNameColumn}, {LogSourceTable.FullFilePathColumn}) values (?, ?, ?)", connection);
			_insertLogSourceCommand.Parameters.Add(_logSourceId = new SQLiteParameter(DbType.Int64));
			_insertLogSourceCommand.Parameters.Add(_logSourceFileName = new SQLiteParameter(DbType.String));
			_insertLogSourceCommand.Parameters.Add(_logSourceFullFilePath = new SQLiteParameter(DbType.String));
			_nextLogSourceId = 0;

			_insertLogEntryCommand = new SQLiteCommand($"insert into {LogEntriesTable.Name} ({LogEntriesTable.LogLineColumn}, {LogEntriesTable.LogSourceColumn}, {LogEntriesTable.TimestampColumn}, {LogEntriesTable.LogLevelColumn}, {LogEntriesTable.RawContentColumn}) values (?, ?, ?, ?, ?)", connection);
			_insertLogEntryCommand.Parameters.Add(_logLine = new SQLiteParameter(DbType.Int64));
			_insertLogEntryCommand.Parameters.Add(_logEntrySource = new SQLiteParameter(DbType.Int64));
			_insertLogEntryCommand.Parameters.Add(_logEntryTimestamp = new SQLiteParameter(DbType.String));
			_insertLogEntryCommand.Parameters.Add(_logEntryLevel = new SQLiteParameter(DbType.String));
			_insertLogEntryCommand.Parameters.Add(_logEntryRawContent = new SQLiteParameter(DbType.String));
		}

		public void Import(IFileInfo source, IEnumerable<IReadOnlyLogEntry> logEntries)
		{
			var dataSourceId = AddDataSource(source);
			_logEntrySource.Value = dataSourceId;

			foreach (var logEntry in logEntries)
			{
				var timestamp = logEntry.Timestamp;
				_logLine.Value = logEntry.LineNumber;
				_logEntryTimestamp.Value = LogEntriesTable.FormatTimestamp(timestamp);
				_logEntryLevel.Value = LogEntriesTable.FormatLevel(logEntry.LogLevel);
				_logEntryRawContent.Value = logEntry.RawContent;
				_insertLogEntryCommand.ExecuteNonQuery();
			}
		}

		private long AddDataSource(IFileInfo source)
		{
			var currentLogSourceId = _nextLogSourceId;
			++_nextLogSourceId;

			_logSourceId.Value = currentLogSourceId;
			_logSourceFileName.Value = source.Name;
			_logSourceFullFilePath.Value = source.FullPath;
			_insertLogSourceCommand.ExecuteNonQuery();

			return currentLogSourceId;
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		#region IDisposable

		public void Dispose()
		{
			_insertLogSourceCommand.Dispose();
			_insertLogEntryCommand.Dispose();
			_transaction.Dispose();
		}

		#endregion
	}
}