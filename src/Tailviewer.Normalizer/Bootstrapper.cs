using System;
using System.IO;
using CommandLine;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Tailviewer.Normalizer
{
	class Bootstrapper
	{
		static int Main(string[] args)
		{
			try
			{
				SetupLogging();
			}
			catch (Exception e)
			{
				Console.WriteLine("Caught unexpected exception while trying to setup logging: {0}", e);
				return -1;
			}

			try
			{
				return Run(args);
			}
			catch (IOException e)
			{
				Console.WriteLine("ERROR: {0}", e.Message);
				return -2;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return -3;
			}
		}

		private static void SetupLogging()
		{
			var hierarchy = (Hierarchy) LogManager.GetRepository();

			var patternLayout = new PatternLayout
			{
				ConversionPattern = "%date{HH:mm:ss,fff} %-5level %message%newline"
			};
			patternLayout.ActivateOptions();
			var consoleAppender = new ConsoleAppender
			{
				Layout = patternLayout
			};
			consoleAppender.ActivateOptions();
			hierarchy.Root.AddAppender(consoleAppender);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}

		private static int Run(string[] args)
		{
			var result = Parser.Default.ParseArguments<NormalizeOptions>(args);
			return result.MapResult(
			                        (NormalizeOptions options) => new NormalizeApplication(options).Run(),
			                        _ => -2);
		}
	}
}
