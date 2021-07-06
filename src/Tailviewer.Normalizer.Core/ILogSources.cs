using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.Analyzer.Core
{
	/// <summary>
	/// Represents one or more <see cref="ILogSource"/>s which may be enumerated on-demand.
	/// </summary>
	public interface IAnalyzerSources
	{
		IEnumerable<ILogSource> Enumerate(LogSourceFilter filter);
	}
}