using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Normalizer.Core.Parsing;

namespace Tailviewer.Normalizer.Tests
{
	[TestFixture]
	public sealed class LogFileParserTest
	{
		private InMemoryFilesystem _filesystem;
		private LogFileParser _parser;

		[SetUp]
		public void Setup()
		{
			_filesystem = new InMemoryFilesystem();
			_parser = new LogFileParser(_filesystem);
		}

		[Test]
		public void TestParseNonExistingFile()
		{
			_parser.Parse("foo.txt").Should().BeEmpty();
		}

		[Test]
		public void TestParseEmptyFile()
		{
			_filesystem.WriteAllText("foo.log", "");
			_parser.Parse("foo.log").Should().BeEmpty();
		}

		[Test]
		public void TestParseOneLine()
		{
			const string path = "M:\\foo.log";
			_filesystem.WriteAllText(path, "Hello, World!");
			var logEntries = _parser.Parse(path);
			logEntries.Should().HaveCount(1);
			var logEntry = logEntries.First();
			logEntry.LineNumber.Should().Be(1);
			logEntry.Timestamp.Should().Be(null);
			logEntry.LogLevel.Should().Be(LevelFlags.Other);
			logEntry.RawContent.Should().Be("Hello, World!");
		}
	}
}
