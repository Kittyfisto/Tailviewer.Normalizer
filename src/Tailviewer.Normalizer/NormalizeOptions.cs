using CommandLine;

namespace Tailviewer.Normalizer
{
	[Verb("normalize", HelpText = "Normalizes a source of one or more log files into a structured format containing all log events from all sources")]
	public sealed class NormalizeOptions
	{
		[Value(0, MetaName = "source",
		       HelpText = "The path of a log file, an archive or a folder of log files",
		       Required = true)]
		public string Source { get; set; }
		
		[Option('r', "recursive",
		        Default = false,
		        HelpText = "When set to true, then the analyzer will recursively go into subfolders and archives to find all log files contained in the source. Otherwise only top level log files are analyzed.")]
		public bool Recursive { get; set; }

		[Option("file_filter",
		        Default = "*.txt;*.log",
		        HelpText = "A semicolon-separated list of wildcard filters which is applied to filenames to filter out which log files to analyze")]
		public string FileFilter { get; set; }

		[Option("log_entry_filter",
		        HelpText = "A Tailviewer filter expression which is applied to the log events prior to exporting them to the output.")]
		public string LogEntryFilter { get; set; }

		[Option('o', "output",
				Required = true,
		        HelpText = "A relative or absolute path to the output file in which all sources are merged into one")]
		public string Output { get; set; }
	}
}