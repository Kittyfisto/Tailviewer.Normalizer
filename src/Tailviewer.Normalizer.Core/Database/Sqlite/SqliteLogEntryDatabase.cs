using System;
using System.Data.SQLite;
using Tailviewer.Normalizer.Core.Database.Sqlite.Schema;

namespace Tailviewer.Normalizer.Core.Database.Sqlite
{
	public sealed class SqliteLogEntryDatabase
		: ILogEntryDatabase
		, IDisposable
	{
		private readonly SQLiteConnection _connection;

		private SqliteLogEntryDatabase(SQLiteConnection connection)
		{
			_connection = connection;
		}

		public static SqliteLogEntryDatabase CreateInMemory()
		{
			var builder = new SQLiteConnectionStringBuilder {DataSource = ":memory:"};
			return Create(builder);
		}

		private static SqliteLogEntryDatabase Create(SQLiteConnectionStringBuilder builder)
		{
			var connection = new SQLiteConnection(builder.ConnectionString);
			try
			{
				connection.Open();
				return CreateTables(connection);
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
		}

		private static SqliteLogEntryDatabase CreateTables(SQLiteConnection connection)
		{
			LogSourceTable.Create(connection);
			LogEntriesTable.Create(connection);
			return new SqliteLogEntryDatabase(connection);
		}

		public IImporter CreateImporter()
		{
			return new Importer(_connection);
		}

		public IReader CreateReader()
		{
			return new Reader(_connection);
		}

		#region IDisposable

		public void Dispose()
		{
			_connection?.Dispose();
		}

		#endregion
	}
}