using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Tailviewer.Normalizer.Core.Exporter;

namespace Tailviewer.Normalizer.Systemtests
{
	[TestFixture]
	public sealed class NormalizerTest
	{
		private string _normalizerPath;
		private string _normalizerFilePath;
		private string _testData;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_normalizerPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_testData = Path.Combine(_normalizerPath, "..", "..", "testdata");
			_normalizerFilePath = Path.Combine(_normalizerPath, "Tailviewer.Normalizer.exe");
		}

		[Test]
		public void Normalizing_a_non_existing_directory_returns_2()
		{
			var result = Normalize("C:\\no_such_directory", "result.json", recursive: true);
			result.ReturnCode.Should().Be(-2, "because we've tried to analyse a folder which does not exist");
			result.Output.Should().Contain("ERROR: No such file or directory: C:\\no_such_directory");
		}

		[Test]
		public void A_zip_file_with_one_log_can_be_normalized_into_a_json_document()
		{
			var source = Path.Combine(_testData, "source_one_file_in_archive.zip");
			var json = "single_zip_result.json";
			var result = Normalize(source, json, recursive: true);
			result.ReturnCode.Should().Be(0);
			result.Output.Should().NotContain("ERROR:");

			var actualContent = ReadOutput(json);
			actualContent.Options.Source.Should().Be(source);
			actualContent.Options.Recursive.Should().BeTrue();
			actualContent.Options.FileFilter.Should().Be("*.txt;*.log", "because by default, the normalizer will only inspect files ending in .txt or .log");
			actualContent.Events.Should().HaveCount(9996);
			actualContent.Events[0].RawMessage.Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
			actualContent.Events[9995].RawMessage.Should().Be("2015-10-07 19:51:01,813 [8092, 5] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Invocation of RPC #3319 finished");
		}

		private ExecutionResult Normalize(string source, string output, bool recursive)
		{
			if (File.Exists(output))
				File.Delete(output);

			var builder = new StringBuilder();
			builder.AppendFormat("\"{0}\"", source);
			builder.AppendFormat(" --output \"{0}\"", output);
			if (recursive)
				builder.AppendFormat(" --recursive");

			return Run(builder.ToString(), _normalizerPath);
		}

		private ExecutionResult Run(string commandline, string workingDirectory)
		{
			var startInfo = new StartInfo
			{
				Program = _normalizerFilePath,
				Arguments = commandline,
				WorkingDirectory = workingDirectory
			};
			return Executor.Execute(startInfo);
		}

		/// <summary>
		/// Reads the file written by the normalizer back into memory and returns an object representing its data.
		/// </summary>
		/// <param name="outputFilePath"></param>
		/// <returns></returns>
		private NormalizedLog ReadOutput(string outputFilePath)
		{
			using (var reader = new StreamReader(outputFilePath))
			using (var jsonReader = new JsonTextReader(reader))
			{
				JsonSerializer serializer = new JsonSerializer();
				var result = serializer.Deserialize<NormalizedLog>(jsonReader);
				return result;
			}
		}
	}
}