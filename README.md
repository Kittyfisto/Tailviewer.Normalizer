# Tailviewer.Normalizer

This project is a commandline application which allows a user to normalize a set of log sources (files, etc...) into a single log file with a defined schema.  
Individual log files are parsed and interpreted using the Tailviewer framework.  
Custom log sources (not readily understood by Tailviewer) may be integrated by implementing a Tailviewer Log Source plugin or by using readily available
plugins for the desired log source.

## Use Case

You want to post process / analyze a set of log files, but they're not standardised / from various sources and you don't want to write the code to normalize the log files on the
fly.  
  
Step 1: You use the Tailviewer.Normalizer CLI to pre-process the data into a single file (for example a json file)  
Step 2: You analyze / process the data with the tools of your choice, only having to parse the output from Step 1  

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

# Output Formats

For now, only JSON is supported.

## Json

The entire / all log source file(s) is normalized into a singular json file. The resulting document consists of:

- A structured view onto the arguments given to the normalizer (input, recursive true/false, filters, etc..)
- A list of all files which were discovered during normalization (and if they were included or not, for example because their filename did not match the file_filter)
- A list of noteworthy warnings and/or errors which occured during normalization (TODO)
- A list of log events

See the following example json file:

```
{
    "Options": {
        "Source": "F:\\Logs\\logs.zip",
        "Recursive": true,
        "FileFilter": "foo*.log"
    },
    "Files": [
        {
            "FullFilePath": "F:\\Logs\\logs.zip\\foo_20210511_152116_0.log",
            "Included": true
        },
        {
            "FullFilePath": "F:\\Logs\\logs.zip\\dmp.bin",
            "Included": false
        }
    ],
    "Events": [
        {
            "Line": 1,
            "FileName": "foo_20210511_152116_0.log",
            "FullFilePath": "F:\\Logs\\logs.zip\\foo_20210511_152116_0.log",
            "Timestamp": "2021-05-11T15:21:16",
            "Level": "other",
            "RawMessage": "2021-05-11 15:21:16 Hello"
        },
        {
            "Line": 2,
            "FileName": "foo_20210511_152116_0.log",
            "FullFilePath": "F:\\Logs\\logs.zip\\foo_20210511_152116_0.log",
            "Timestamp": "2021-05-11T15:21:16",
            "Level": "other",
            "RawMessage": "2021-05-11 15:21:16 World"
        },
    ]
}
```

Description of the Event attributes in detail:

- Line: The line number of the first line of the log entry, e.g. `42`
- FileName: The name of the log file the log entry was extracted from, e.g. `foo.log`
- FullFilePath: The entire path of the log file the log entry was extracted from, e.g. `C:\bar.zip\foo.log`
- Level: The log level of the entry, e.g. `fatal, error, warning, info, debug, trace` or `other` if the level couldn't be detected
- Timestamp: The timestamp of the log entry in sortable format or null (See: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Sortable), e.g. `2021-05-11T15:28:21`
- RawMessage: The actual content of the log entry. May consist of multiple lines in case Tailviewer (or a plugin) detected that multiple lines form a singular log entry, e.g. `foo\r\nbar`

# Examples

## Normalize an entire folder tree

The following commandline call shows how to use the normalizer to parse an entire subtree of log files (ending in .log) into a single json file.
The resulting log file is automatically sorted by timestamp.

```
Tailviewer.Normalizer.exe C:\logs --recursive --file_filter *.log -o C:\condensed.json
```

