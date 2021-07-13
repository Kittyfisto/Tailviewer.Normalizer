namespace Tailviewer.Normalizer.Core.Exporter
{
	/// <summary>
	///     Represents information about a single tailviewer plugin (*.tvp) and how or if at all it was involved
	///     in the normalization process.
	/// </summary>
	public sealed class PluginReport
	{
		/// <summary>
		///     The full file path to the plugin file on disk.
		/// </summary>
		public string FullFilePath { get; set; }

		/// <summary>
		///     Whether or not the plugin could be loaded.
		/// </summary>
		public bool Loaded { get; set; }

		/// <summary>
		/// The error message (if any) that occured while loading the plugin.
		/// </summary>
		public string Error { get; set; }
	}
}