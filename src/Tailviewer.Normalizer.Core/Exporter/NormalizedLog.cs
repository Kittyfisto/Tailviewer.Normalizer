using System.Collections.Generic;

namespace Tailviewer.Normalizer.Core.Exporter
{
	/// <summary>
	///     This class represents the data that is eventually serialized to a suitable format of the user's choice.
	/// </summary>
	public sealed class NormalizedLog
	{
		public NormalizationOptions Options { get; set; }
		public List<LogFileReport> Files { get; set; }
		public List<LogEvent> Events { get; set; }
	}
}