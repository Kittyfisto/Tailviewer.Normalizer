# Tailviewer.Normalizer

This project is a commandline application which allows a user to normalize a set of log sources (files, etc...) into a single log file with a defined schema.  
Individual log files are parsed and interpreted using the Tailviewer framework.  
Custom log sources (not readily understood by Tailviewer) may be integrated by implementing a Tailviewer Log Source plugin or by using readily available
plugins for the desired log source.

## Use Case

You want to post process / analyze a set of log files, but they're not standardised / from various sources and you don't want to write the code to normalize the log files on the
fly.  
  
Step 1: You use the Tailviewer.Normalizer CLI to pre-process the data into a single file (for example a json file) with a defined schema.  
Step 2: You analyze / process the data with the tools of your choice, only having to parse the output from Step 1, which has a defined schema.

## Commandline Documentation

```
  -r, --recursive       (Default: false) When set to true, then the analyzer will recursively go into subfolders and
                        archives to find all log files contained in the source. Otherwise only top level log files are
                        analyzed.

  --file_filter         (Default: *.txt;*.log) A semicolon-separated list of wildcard filters which is applied to
                        filenames to filter out which log files to analyze

  --log_entry_filter    A Tailviewer filter expression which is applied to the log events prior to exporting them to
                        the output.

  -o, --output          A relative or absolute path to the output file in which all sources are merged into one

  --help                Display this help screen.

  --version             Display version information.

  source (pos. 0)       Required. The path of a log file, an archive or a folder of log files
```

# Examples

## Normalize an entire folder tree

```
Tailviewer.Normalizer.exe C:\logs --recursive --file_filter *.log -o C:\condensed.json
```