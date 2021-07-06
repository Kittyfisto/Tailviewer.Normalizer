using System;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Globalization;
using log4net.Core;
using Tailviewer.Api;

namespace Tailviewer.Normalizer.Core.Database.Sqlite.Schema
{
	public static class LogEntriesTable
	{
		public static readonly string Name = "LogEntries";
		public static readonly string IdColumn = "Id";
		public static readonly string LineNumberColumn = "LineNumber";
		public static readonly string LogSourceColumn = "Source";
		public static readonly string LogLevelColumn = "Level";
		public static readonly string TimestampColumn = "Timestamp";
		public static readonly string RawContentColumn = "RawContent";

		public static void Create(SQLiteConnection connection)
		{
			var sql = $"CREATE TABLE {Name}(" +
			          $"{IdColumn} INTEGER PRIMARY KEY, " +
			          $"{LineNumberColumn} INTEGER, " +
			          $"{LogSourceColumn} INTEGER, " +
					  $"{LogLevelColumn} TEXT, " +
			          $"{TimestampColumn} TEXT, " +
					  $"{RawContentColumn} TEXT" +
			          ")";
			using (var command = new SQLiteCommand(sql, connection))
			{
				command.ExecuteNonQuery();
			}

			sql = $"create index {Name}_{TimestampColumn}_Index " +
			      $"on {Name}({TimestampColumn})";
			using (var command = new SQLiteCommand(sql, connection))
			{
				command.ExecuteNonQuery();
			}
		}

		[Pure]
		public static string FormatTimestamp(DateTime? timestamp)
		{
			return timestamp?.ToString("s", CultureInfo.InvariantCulture);
		}

		[Pure]
		public static DateTime? TryParseTimestamp(string timestamp)
		{
			if (!DateTime.TryParseExact(timestamp, "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
				return null;

			return result;
		}

		[Pure]
		public static string FormatLevel(LevelFlags level)
		{
			return level.ToString();
		}

		[Pure]
		public static LevelFlags TryParseLevel(string level)
		{
			if (!Enum.TryParse(level, out LevelFlags actualLevel))
				return LevelFlags.None;

			return actualLevel;
		}
	}
}