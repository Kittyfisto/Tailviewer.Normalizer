using System.Collections.Generic;

namespace Tailviewer.Normalizer.Core.Exporter
{
	/// <summary>
	/// Represents information about the normalizer application itself (mainly its version number).
	/// </summary>
	public sealed class ApplicationReport
	{
		/// <summary>
		/// 
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string TailviewerApiVersion { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public List<PluginReport> Plugins { get; set; }
	}
}