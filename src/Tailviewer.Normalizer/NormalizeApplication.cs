using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Tailviewer.Normalizer.Core.Database;
using Tailviewer.Normalizer.Core.Database.Sqlite;
using Tailviewer.Normalizer.Core.Exporter;
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
		private IImporter _importer;

		public NormalizeApplication(NormalizeOptions options)
		{
			_options = options;

			var regex = string.Join("|", options.FileFilter.Split(';')
			                                     .Select(x => "(" + Regex.Escape(x).Replace("\\*", ".*?") + ")"));
			_fileNameRegex = new Regex(regex);

			_database = SqliteLogEntryDatabase.CreateInMemory();
			_importer = _database.CreateImporter();
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
				Log.InfoFormat("Found {0} files in total, applying filename filter '{1}'...", allFiles.Count, _options.FileFilter);

				var filteredFiles = FilterFiles(filesystem, allFiles);
				Log.InfoFormat("{0} out of {1} files match filter, importing file(s) into database...",
				               filteredFiles.Count,
				               allFiles.Count);
				foreach (var file in filteredFiles)
				{
					ImportSource(importer, parser, file);
				}

				importer.Commit();

				Log.InfoFormat("All files imported, exporting to '{0}'...", _options.Output);

				exporter.ExportTo(_database, _options.Output);
			}
		}

		private void ImportSource(IImporter importer, LogFileParser parser, IFileInfo file)
		{
			Log.InfoFormat("Parsing {0} ({1} bytes)...", file.FullPath, file.Length);
			var logEntries = parser.Parse(file.FullPath);

			Log.InfoFormat("Parsed {0} log entries, importing into database...", logEntries.Count);
			importer.Import(file, logEntries);
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
		}

		#endregion
	}
}