namespace Tailviewer.Normalizer.Core.Exporter
{
	/// <summary>
	/// Holds the various options that were given to the normalizer (via commandline).
	/// </summary>
	public sealed class NormalizationOptions
	{
		public string Source { get; set; }
		public bool Recursive { get; set; }
		public string FileFilter { get; set; }
	}
}