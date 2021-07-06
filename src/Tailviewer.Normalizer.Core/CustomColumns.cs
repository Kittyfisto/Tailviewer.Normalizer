using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Normalizer.Core
{
	public static class CustomColumns
	{
		public static readonly IColumnDescriptor<string> FullSourceFilePath;
		public static readonly IColumnDescriptor<string> SourceFileName;

		static CustomColumns()
		{
			FullSourceFilePath = new CustomColumnDescriptor<string>("full_source_file_path");
			SourceFileName = new CustomColumnDescriptor<string>("source_file_name");
		}
	}
}
