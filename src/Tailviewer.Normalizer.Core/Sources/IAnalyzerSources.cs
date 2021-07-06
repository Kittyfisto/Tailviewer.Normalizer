using System;
using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.Normalizer.Core.Sources
{
	/// <summary>
	/// Represents one or more <see cref="ILogSource"/>s which may be enumerated on-demand.
	/// </summary>
	public interface IAnalyzerSources
		: IDisposable
	{
		IEnumerable<IAnalyzerSource> Enumerate(LogSourceFilter filter);
	}
}