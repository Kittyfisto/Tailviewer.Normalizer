using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Normalizer.Core.Database;
using Tailviewer.Normalizer.Core.Database.Sqlite;
using Tailviewer.Normalizer.Core.Exporter;
using Tailviewer.Normalizer.Core.Exporter.Json;
using Tailviewer.Normalizer.Core.Parsing;
using Tailviewer.Normalizer.Core.Sources;

namespace Tailviewer.Normalizer
{
	public sealed class NormalizeApplication
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly NormalizeOptions _options;
		private readonly Regex _fileNameRegex;
		private readonly SqliteLogEntryDatabase _database;

		public NormalizeApplication(NormalizeOptions options)
		{
			_options = options;

			var regex = string.Join("|", options.FileFilter.Split(';')
			                                     .Select(x => "(" + Regex.Escape(x).Replace("\\*", ".*?") + ")"));
			_fileNameRegex = new Regex(regex);

			_database = SqliteLogEntryDatabase.CreateInMemory();
		}

		public int Run()
		{
			ImportSourceIntoDatabase();

			return 0;
		}

		private void ImportSourceIntoDatabase()
		{
			using (var importer = _database.CreateImporter())
			using (var scheduler = new DefaultTaskScheduler())
			using (var filesystem = new CompoundFilesystem(new Filesystem(scheduler)))
			using (var parser = new LogFileParser(filesystem))
			using (var exporter = new JsonExporter())
			{
				var allFiles = FindFiles(filesystem);
				int numExported;

				if (allFiles.Any())
				{
					Log.InfoFormat("Found {0} files in total, applying filename filter '{1}'...", allFiles.Count, _options.FileFilter);

					var filteredFiles = FilterFiles(filesystem, allFiles);
					if (filteredFiles.Any())
					{
						Log.InfoFormat("{0} out of {1} files match filter, importing file(s) into database...",
						               filteredFiles.Count,
						               allFiles.Count);
						foreach (var file in filteredFiles)
						{
							ImportSource(importer, parser, file);
						}

						importer.Commit();

						Log.InfoFormat("All files imported, exporting to '{0}'...", _options.Output);

						numExported = exporter.ExportTo(CreateOptions(_options), CreateReport(allFiles, filteredFiles), _database, _options.Output);

					}
					else
					{
						Log.WarnFormat("The file_filter \"{0}\" excludes all {1} file(s) from the source!", _options.FileFilter, allFiles.Count);

						importer.Commit();
						numExported = exporter.ExportTo(CreateOptions(_options), CreateReport(allFiles, filteredFiles), _database, _options.Output);
					}
				}
				else
				{
					Log.WarnFormat("The source \"{0}\" does not contain a single file!", _options.Source);

					importer.Commit();
					numExported = exporter.ExportTo(CreateOptions(_options), CreateReport(new string[0], new IFileInfo[0]), _database, _options.Output);
				}

				Log.InfoFormat("Exported {0} log entries to '{1}'!", numExported, _options.Output);
			}
			
		}

		private IReadOnlyList<LogFileReport> CreateReport(IReadOnlyList<string> allFiles,
		                                                  IReadOnlyList<IFileInfo> readOnlyList)
		{
			var ret = new List<LogFileReport>(allFiles.Count);
			foreach (var file in allFiles)
			{
				var report = new LogFileReport
				{
					FullFilePath = file,
					Included = readOnlyList.Any(x => Equals(x.FullPath, file))
				};
				ret.Add(report);
			}

			return ret;
		}

		private NormalizationOptions CreateOptions(NormalizeOptions options)
		{
			return new NormalizationOptions
			{
				Source = options.Source,
				FileFilter = options.FileFilter,
				Recursive = options.Recursive
			};
		}

		private void ImportSource(IImporter importer, LogFileParser parser, IFileInfo file)
		{
			Log.InfoFormat("{0}: Parsing ({1} bytes)...", file.FullPath, file.Length);
			var logEntries = parser.Parse(file.FullPath);

			var mergedLogEntries = GroupByLogEntry(logEntries);

			Log.InfoFormat("{0}: Parsed {1} lines into {2} log entries, importing into database...", file.FullPath, logEntries.Count, mergedLogEntries.Count);
			importer.Import(file, mergedLogEntries);
		}

		private IReadOnlyList<IReadOnlyLogEntry> GroupByLogEntry(IReadOnlyList<IReadOnlyLogEntry> logEntries)
		{
			var group = new List<IReadOnlyLogEntry>();
			var ret = new List<IReadOnlyLogEntry>();
			for (int i = 0; i < logEntries.Count; ++i)
			{
				var current = logEntries[i];
				if (group.Count > 0)
				{
					if (!BelongsToGroup(current, group))
					{
						ret.Add(CreateLogEntry(group));
						group.Clear();
					}
				}
				group.Add(current);
			}

			if (group.Count > 0)
				ret.Add(CreateLogEntry(group));

			return ret;
		}

		private bool BelongsToGroup(IReadOnlyLogEntry current, List<IReadOnlyLogEntry> @group)
		{
			var first = group[0];
			if (first.LogEntryIndex == current.LogEntryIndex)
				return true;

			if (first.Timestamp != null && current.Timestamp == null)
				return true;

			if (first.LogLevel != LevelFlags.None && current.LogLevel == LevelFlags.None)
				return true;

			return false;
		}

		private IReadOnlyLogEntry CreateLogEntry(IReadOnlyList<IReadOnlyLogEntry> @group)
		{
			var first = group[0];
			var values = new Dictionary<IColumnDescriptor, object>();
			foreach (var column in first.Columns)
			{
				values.Add(column, first.GetValue(column));
			}

			var rawContent = new StringBuilder();
			foreach (var logEntry in group)
			{
				if (rawContent.Length > 0)
					rawContent.AppendLine();
				rawContent.Append(logEntry.RawContent);
			}

			values[Columns.RawContent] = rawContent.ToString();
			var mergedLogEntry = new ReadOnlyLogEntry(values);
			return mergedLogEntry;
		}

		private IReadOnlyList<string> FindFiles(CompoundFilesystem filesystem)
		{
			Log.DebugFormat("Inspecting source '{0}'...", _options.Source);

			if (filesystem.FileExists(_options.Source))
			{
				if (CompoundFilesystem.IsArchive(_options.Source))
				{
					Log.InfoFormat("Source is an archive, enumerating contents...");
					var allFiles = filesystem.EnumerateFiles(_options.Source, null,
					                                         _options.Recursive
						                                         ? SearchOption.AllDirectories
						                                         : SearchOption.TopDirectoryOnly);
					return allFiles;
				}

				Log.InfoFormat("Source is a single file...");
				return new []{_options.Source};
			}

			if (filesystem.DirectoryExists(_options.Source))
			{
				Log.InfoFormat("Source is a directory, enumerating contents...");
				var allFiles = filesystem.EnumerateFiles(_options.Source, null,
				                                         _options.Recursive
					                                         ? SearchOption.AllDirectories
					                                         : SearchOption.TopDirectoryOnly);
				return allFiles;
			}

			throw new FileNotFoundException($"No such file or directory: {_options.Source}");
		}

		private IReadOnlyList<IFileInfo> FilterFiles(IFilesystem filesystem, IEnumerable<string> allFiles)
		{
			return allFiles.Where(x => _fileNameRegex.IsMatch(Path.GetFileName(x)))
			               .Select(x => filesystem.GetFileInfo(x))
			               .ToList();
		}

		#region IDisposable

		public void Dispose()
		{
			_database.Dispose();
		}

		#endregion
	}
}