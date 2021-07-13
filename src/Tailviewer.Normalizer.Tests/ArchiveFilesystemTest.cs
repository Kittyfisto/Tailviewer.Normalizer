using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Normalizer.Core.Sources;

namespace Tailviewer.Normalizer.Tests
{
	[TestFixture]
	public sealed class ArchiveFilesystemTest
	{
		[Test]
		public void TestArchiveNormalizeFilePath()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			archive.CreateEntry("foo/bar.txt");

			var fs = new ArchiveFilesystem(archive);
			var file = fs.GetFileInfo("foo\\bar.txt");
			file.Should().NotBeNull();
			file.FullPath.Should().Be("foo\\bar.txt");
			file.Name.Should().Be("bar.txt");
			file.DirectoryName.Should().Be("foo");

			var files = fs.EnumerateFiles("", searchOption: SearchOption.AllDirectories);
			files.Should().Equal(new object[] {"foo\\bar.txt"});
		}

		[Test]
		public void TestArchiveOneEmptyFile()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			var entry = archive.CreateEntry("foo.txt");

			var fs = new ArchiveFilesystem(archive);
			fs.Roots.Should().HaveCount(expected: 1);
			fs.EnumerateFiles("").Should().Equal(new object[] {"foo.txt"});
			var stream = fs.OpenRead("foo.txt");
			var reader = new StreamReader(stream);
			reader.ReadToEnd().Should().BeEmpty();
		}

		[Test]
		public void TestArchiveOneFile()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			archive.CreateEntry("foo\\bar.txt");

			var fs = new ArchiveFilesystem(archive);
			var file = fs.GetFileInfo("foo\\bar.txt");
			file.Should().NotBeNull();
			file.FullPath.Should().Be("foo\\bar.txt");
			file.Name.Should().Be("bar.txt");
			file.DirectoryName.Should().Be("foo");

			var files = fs.EnumerateFiles("", searchOption: SearchOption.AllDirectories);
			files.Should().Equal(new object[] {"foo\\bar.txt"});
		}

		[Test]
		public void TestArchiveOneLineFile()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			var entry = archive.CreateEntry("foo.txt");
			using (var writer = new StreamWriter(entry.Open()))
			{
				writer.Write("Hello, World!");
			}

			var fs = new ArchiveFilesystem(archive);
			fs.Roots.Should().HaveCount(expected: 1);
			fs.EnumerateFiles("").Should().Equal(new object[] {"foo.txt"});
			using (var reader = new StreamReader(fs.OpenRead("foo.txt")))
			{
				reader.ReadToEnd().Should().Be("Hello, World!");
			}

			// We should be able to read the same file again without problems
			using (var reader = new StreamReader(fs.OpenRead("foo.txt")))
			{
				reader.ReadToEnd().Should().Be("Hello, World!");
			}
		}

		[Test]
		public void TestEmptyArchiveEnumerateFiles()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			var fs = new ArchiveFilesystem(archive);
			fs.Roots.Should().HaveCount(expected: 1);
			fs.EnumerateFiles("").Should().BeEmpty();
		}

		[Test]
		public void TestEmptyArchiveOpenRead1()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			var fs = new ArchiveFilesystem(archive);
			new Action(() => fs.OpenRead("foo.txt")).Should().Throw<FileNotFoundException>();
		}

		[Test]
		public void TestEmptyArchiveOpenRead2()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			var fs = new ArchiveFilesystem(archive);
			new Action(() => fs.OpenRead("foo\\bar.txt")).Should().Throw<DirectoryNotFoundException>();
		}

		[Test]
		public void TestEmptyArchiveRoot()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			var fs = new ArchiveFilesystem(archive);
			fs.Roots.Should().HaveCount(expected: 1);
			var root = fs.Roots.First();
			root.Parent.Should().BeNull();
			root.Root.Should().Be(root);
			root.FullName.Should().Be("");
			root.Exists.Should().BeTrue();
			root.Name.Should().Be("");
		}

		[Test]
		public void TestOpenRead_EmptyPath()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			var fs = new ArchiveFilesystem(archive);
			new Action(() => fs.OpenRead("")).Should().Throw<ArgumentException>();
		}

		[Test]
		public void TestOpenRead_NullPath()
		{
			var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
			var fs = new ArchiveFilesystem(archive);
			new Action(() => fs.OpenRead(path: null)).Should().Throw<ArgumentNullException>();
		}
	}
}