using System.Diagnostics;
using NUnit.Framework;

namespace Tailviewer.Normalizer.Systemtests
{
	public sealed class Executor
	{
		public static ExecutionResult Execute(StartInfo info)
		{
			using (var process = new Process {StartInfo = CreateStartInfo(info)})
			{
				TestContext.Progress.WriteLine("CWD: {0}", info.WorkingDirectory);
				TestContext.Progress.WriteLine("EXEC: \"{0}\" {1}", info.Program, info.Arguments);

				process.Start();
				process.WaitForExit();

				var result = new ExecutionResult();
				result.ReturnCode = process.ExitCode;
				result.Output = process.StandardOutput.ReadToEnd();

				TestContext.Progress.WriteLine("OUT:\r\n{0}", result.Output);

				return result;
			}
		}

		private static ProcessStartInfo CreateStartInfo(StartInfo info)
		{
			var startInfo = new ProcessStartInfo(info.Program, info.Arguments)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			return startInfo;
		}
	}
}