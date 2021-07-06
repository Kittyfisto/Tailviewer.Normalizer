using System;
using System.IO;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Normalizer.Core.Sources;

namespace Tailviewer.Normalizer.Tests
{
	[TestFixture]
	public sealed class CompoundFilesystemTest
	{
		public string TestData;
		private CompoundFilesystem FileSystem;

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			TestData = Path.GetFullPath(Path.Combine(AssemblyDirectory, "..", "..", "testdata"));
		}

		[SetUp]
		public void SetUp()
		{
			FileSystem = new CompoundFilesystem(new Filesystem(new ManualTaskScheduler()));
		}

		[Test]
		public void TestEnumerateFiles_EmptySource()
		{
			var files = FileSystem.EnumerateFiles(Path.Combine(TestData, "source_empty"));
			files.Should().BeEmpty();
		}

		[Test]
		public void TestEnumerateFiles_OneTextLog()
		{
			var source = Path.Combine(TestData, "source_one_text_log");
			var files = FileSystem.EnumerateFiles(source);
			files.Should().BeEquivalentTo(new[]
			{
				Path.Combine(source, "1Line.txt")
			});
		}

		[Test]
		public void TestEnumerateFiles_MultipleTextLogs()
		{
			var source = Path.Combine(TestData, "source_multiple_text_logs");
			var files = FileSystem.EnumerateFiles(source);
			files.Should().BeEquivalentTo(new[]
			{
				Path.Combine(source, "2LogEntries.txt"),
				Path.Combine(source, "3LogEntries.txt")
			});
		}

		[Test]
		public void TestEnumerateFiles_OneArchiveOneFileInside()
		{
			var source = Path.Combine(TestData, "source_one_archive_one_file_inside");
			var files = FileSystem.EnumerateFiles(source, searchOption: SearchOption.AllDirectories);
			files.Should().BeEquivalentTo(new[]
			{
				Path.Combine(source, "logs.zip", "1Mb.txt")
			});
		}

		[Test]
		public void TestEnumerateFiles_OneArchiveAllLogsInside_NoFilter1()
		{
			var source = Path.Combine(TestData, "source_all_logs");
			var zip = "all_logs.zip";
			var files = FileSystem.EnumerateFiles(source, searchOption: SearchOption.AllDirectories);
			files.Should().HaveCount(32);
			files.Should().Contain(Path.Combine(source, zip, "Encodings\\utf8_w_bom.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Encodings\\utf16_be_bom.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Encodings\\utf16_le_bom.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Encodings\\windows_1252.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Formats\\Serilog\\Serilog.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Multiline\\Log1.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Multiline\\Log2.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Timestamps\\ddd MMM dd HH_mm_ss.fff yyyy.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Timestamps\\HH_mm_ss.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Timestamps\\HH_mm_ss;s.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Timestamps\\yyyy MMM dd HH_mm_ss.fff.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Timestamps\\yyyy-MM-dd HH_mm_ss.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Timestamps\\yyyy-MM-dd HH_mm_ss_fff.txt"));
			files.Should().Contain(Path.Combine(source, zip, "1Line.txt"));
			files.Should().Contain(Path.Combine(source, zip, "1Line_WithNewline.txt"));
			files.Should().Contain(Path.Combine(source, zip, "1Mb.txt"));
			files.Should().Contain(Path.Combine(source, zip, "1Mb_1Line.txt"));
			files.Should().Contain(Path.Combine(source, zip, "1Mb_2Lines.txt"));
			files.Should().Contain(Path.Combine(source, zip, "2Lines.txt"));
			files.Should().Contain(Path.Combine(source, zip, "2LogEntries.txt"));
			files.Should().Contain(Path.Combine(source, zip, "2MB.txt"));
			files.Should().Contain(Path.Combine(source, zip, "3LogEntries.txt"));
			files.Should().Contain(Path.Combine(source, zip, "20Mb.txt"));
			files.Should().Contain(Path.Combine(source, zip, "dawwddwa.txt"));
			files.Should().Contain(Path.Combine(source, zip, "DifferentLevels.txt"));
			files.Should().Contain(Path.Combine(source, zip, "Empty.txt"));
			files.Should().Contain(Path.Combine(source, zip, "LevelCounts.txt"));
			files.Should().Contain(Path.Combine(source, zip, "TestLive1.txt"));
			files.Should().Contain(Path.Combine(source, zip, "TestLive2.txt"));
			files.Should().Contain(Path.Combine(source, zip, "utf8 - Copy.txt"));
			files.Should().Contain(Path.Combine(source, zip, "utf8.txt"));
			files.Should().Contain(Path.Combine(source, zip, "version.xml"));
		}

		[Test]
		[Description("Verifies that it's possible to treat a zip-archive as a directory and directly enumerate it")]
		public void TestEnumerateFiles_OneArchiveAllLogsInside_NoFilter2()
		{
			var source = Path.Combine(TestData, "source_all_logs", "all_logs.zip");
			var files = FileSystem.EnumerateFiles(source, searchOption: SearchOption.AllDirectories);
			files.Should().HaveCount(32);
			files.Should().Contain(Path.Combine(source, "Encodings\\utf8_w_bom.txt"));
			files.Should().Contain(Path.Combine(source, "Encodings\\utf16_be_bom.txt"));
			files.Should().Contain(Path.Combine(source, "Encodings\\utf16_le_bom.txt"));
			files.Should().Contain(Path.Combine(source, "Encodings\\windows_1252.txt"));
			files.Should().Contain(Path.Combine(source, "Formats\\Serilog\\Serilog.txt"));
			files.Should().Contain(Path.Combine(source, "Multiline\\Log1.txt"));
			files.Should().Contain(Path.Combine(source, "Multiline\\Log2.txt"));
			files.Should().Contain(Path.Combine(source, "Timestamps\\ddd MMM dd HH_mm_ss.fff yyyy.txt"));
			files.Should().Contain(Path.Combine(source, "Timestamps\\HH_mm_ss.txt"));
			files.Should().Contain(Path.Combine(source, "Timestamps\\HH_mm_ss;s.txt"));
			files.Should().Contain(Path.Combine(source, "Timestamps\\yyyy MMM dd HH_mm_ss.fff.txt"));
			files.Should().Contain(Path.Combine(source, "Timestamps\\yyyy-MM-dd HH_mm_ss.txt"));
			files.Should().Contain(Path.Combine(source, "Timestamps\\yyyy-MM-dd HH_mm_ss_fff.txt"));
			files.Should().Contain(Path.Combine(source, "1Line.txt"));
			files.Should().Contain(Path.Combine(source, "1Line_WithNewline.txt"));
			files.Should().Contain(Path.Combine(source, "1Mb.txt"));
			files.Should().Contain(Path.Combine(source, "1Mb_1Line.txt"));
			files.Should().Contain(Path.Combine(source, "1Mb_2Lines.txt"));
			files.Should().Contain(Path.Combine(source, "2Lines.txt"));
			files.Should().Contain(Path.Combine(source, "2LogEntries.txt"));
			files.Should().Contain(Path.Combine(source, "2MB.txt"));
			files.Should().Contain(Path.Combine(source, "3LogEntries.txt"));
			files.Should().Contain(Path.Combine(source, "20Mb.txt"));
			files.Should().Contain(Path.Combine(source, "dawwddwa.txt"));
			files.Should().Contain(Path.Combine(source, "DifferentLevels.txt"));
			files.Should().Contain(Path.Combine(source, "Empty.txt"));
			files.Should().Contain(Path.Combine(source, "LevelCounts.txt"));
			files.Should().Contain(Path.Combine(source, "TestLive1.txt"));
			files.Should().Contain(Path.Combine(source, "TestLive2.txt"));
			files.Should().Contain(Path.Combine(source, "utf8 - Copy.txt"));
			files.Should().Contain(Path.Combine(source, "utf8.txt"));
			files.Should().Contain(Path.Combine(source, "version.xml"));
		}

		[Test]
		public void TestOpenRead_OneArchiveOneFileInside()
		{
			var path = Path.Combine(TestData, "source_one_archive_one_file_inside", "logs.zip", "1Mb.txt");
			var lines = FileSystem.ReadAllLines(path);
			lines.Should().HaveCount(9996);
			lines[0].Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
			lines[lines.Count- 1].Should().Be("2015-10-07 19:51:01,813 [8092, 5] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Invocation of RPC #3319 finished");
		}

		[Test]
		public void TestGetDirectoryInfo_OneArchiveOneFileInside()
		{
			var source = Path.Combine(TestData, "source_one_archive_one_file_inside");
			var path = Path.Combine(source, "logs.zip");
			var directory = FileSystem.GetDirectoryInfo(path);
			directory.Should().NotBeNull();
			directory.FullName.Should().Be(path);
			directory.Name.Should().Be("logs.zip");
			directory.Exists.Should().BeTrue();
			directory.Parent.Should().NotBeNull();
			directory.Root.Should().NotBeNull();
		}

		[Test]
		public void TestGetFileInfo_OneArchiveOneFileInside()
		{
			var source = Path.Combine(TestData, "source_one_archive_one_file_inside");
			var path = Path.Combine(source, "logs.zip", "1MB.txt");
			var file = FileSystem.GetFileInfo(path);
			file.Should().NotBeNull();
			file.FullPath.Should().Be(path);
		}
	}
}