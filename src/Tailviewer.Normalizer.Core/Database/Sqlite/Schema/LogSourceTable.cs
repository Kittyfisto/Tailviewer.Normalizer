using System.Data.SQLite;

namespace Tailviewer.Normalizer.Core.Database.Sqlite.Schema
{
	public static class LogSourceTable
	{
		public static readonly string Name = "LogSources";
		public static readonly string IdColumn = "Id";
		public static readonly string FullFilePathColumn = "FullFilePath";
		public static readonly string FileNameColumn = "FileName";

		public static void Create(SQLiteConnection connection)
		{
			var sql = $"create table {Name} (" +
			          $"{IdColumn} INTEGER PRIMARY KEY," +
			          $"{FullFilePathColumn} TEXT," +
					  $"{FileNameColumn} TEXT" +
			")";
			var command = new SQLiteCommand(sql, connection);
			command.ExecuteNonQuery();
		}
	}
}