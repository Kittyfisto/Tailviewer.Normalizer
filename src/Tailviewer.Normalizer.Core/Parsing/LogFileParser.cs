using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Core;

namespace Tailviewer.Normalizer.Core.Parsing
{
	/// <summary>
	///    Responsible for parsing a log file into a sequence of <see cref="IReadOnlyLogEntry"/>s.
	/// </summary>
	public sealed class LogFileParser
		: IDisposable
	{
		private readonly ManualTaskScheduler _taskScheduler;
		private readonly IServiceContainer _services;
		private readonly PluginArchiveLoader _pluginArchiveLoader;
		private readonly PluginCache _pluginCache;
		private readonly IRawFileLogSourceFactory _rawLogSourceFactory;
		private readonly LogSourceParserPlugin _logSourceParserPlugin;

		public LogFileParser(IFilesystem filesystem)
		{
			_taskScheduler = new ManualTaskScheduler();
			var pluginPaths = new[]
			{
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tailviewer",
				             "Downloads", "Plugins")
			};
			_rawLogSourceFactory = new TextLogSourceFactory(filesystem, _taskScheduler);
			_pluginArchiveLoader = new PluginArchiveLoader(filesystem, pluginPaths);
			_pluginCache = new PluginCache(_pluginArchiveLoader);

			_services = new ServiceContainer();
			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<IFilesystem>(filesystem);
			_services.RegisterInstance<IPluginLoader>(_pluginCache);
			
			_logSourceParserPlugin = new LogSourceParserPlugin(_services);
			_services.RegisterInstance<ILogSourceParserPlugin>(_logSourceParserPlugin);

			_services.RegisterInstance<IRawFileLogSourceFactory>(_rawLogSourceFactory);
			_services.RegisterInstance<ILogFileFormatMatcher>(new LogFileFormatMatcher(_services));
		}

		public IReadOnlyList<IReadOnlyLogEntry> Parse(string fileName)
		{
			using (var logSource = FileLogSourceFactory.OpenRead(_services, fileName))
			{
				ParseSource(logSource);
				return ReadToEnd(logSource);
			};
		}

		private static IReadOnlyList<IReadOnlyLogEntry> ReadToEnd(ILogSource logSource)
		{
			var count = logSource.GetProperty(Properties.LogEntryCount);
			var buffer = new LogBufferArray(count, new IColumnDescriptor[]
			{
				Columns.LineNumber,
				Columns.LogEntryIndex,
				Columns.Timestamp,
				Columns.LogLevel,
				Columns.RawContent
			});
			logSource.GetEntries(new LogSourceSection(0, count), buffer);
			return MergeLogEntries(buffer);
		}

		private static IReadOnlyList<IReadOnlyLogEntry> MergeLogEntries(LogBufferArray source)
		{
			var buffer = new LogBufferList(source.Columns);
			var currentEntry = new List<IReadOnlyLogEntry>();
			LogEntryIndex index = 0;
			foreach(var logEntry in source)
			{
				if (logEntry.LogEntryIndex == index)
				{
					currentEntry.Add(logEntry);
				}
				else
				{
					buffer.Add(Merge(currentEntry));
					currentEntry.Clear();

					currentEntry.Add(logEntry);
					index = logEntry.LogEntryIndex;
				}
			}

			if (currentEntry.Count > 0)
			{
				buffer.Add(Merge(currentEntry));
			}

			return buffer;
		}

		private static IReadOnlyLogEntry Merge(IReadOnlyList<IReadOnlyLogEntry> currentEntry)
		{
			if (currentEntry.Count == 1)
			{
				return currentEntry.First();
			}

			var rawContent = new StringBuilder();
			foreach (var logEntry in currentEntry)
			{
				rawContent.AppendLine(logEntry.RawContent);
			}

			var mergedLogEntry = new LogEntry(currentEntry.First())
			{
				RawContent = rawContent.ToString()
			};
			return mergedLogEntry;
		}

		private void ParseSource(ILogSource logSource)
		{
			const int maxTries = 10;
			for (int i = 0; i < maxTries; ++i)
			{
				_taskScheduler.RunOnce();

				var progress = logSource.GetProperty(Properties.PercentageProcessed);
				if (progress >= Percentage.HundredPercent)
				{
					break;
				}
			}
		}

		#region IDisposable

		public void Dispose()
		{
		}

		#endregion
	}
}
