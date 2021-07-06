using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Normalizer.Core.Database.Sqlite;

namespace Tailviewer.Normalizer.Tests
{
	[TestFixture]
	public sealed class LogEntryDatabaseTest
	{
		private SqliteLogEntryDatabase Database;

		[SetUp]
		public void Setup()
		{
			Database = SqliteLogEntryDatabase.CreateInMemory();
		}

		[Test]
		public void TestImportOneLogEntry()
		{
			using (var importer = Database.CreateImporter())
			{
				var source = new Mock<IFileInfo>();
				importer.Import(source.Object, new []{new LogEntry
				{
					LineNumber = 1,
					Timestamp = new DateTime(2021, 05, 14, 15, 49, 0),
					LogLevel = LevelFlags.Debug,
					Message = "Hello, World!"
				}});
				importer.Commit();
			}
		}
	}
}
