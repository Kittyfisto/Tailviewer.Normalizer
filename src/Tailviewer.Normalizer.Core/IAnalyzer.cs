using Tailviewer.Normalizer.Core.Sources;

namespace Tailviewer.Normalizer.Core
{
	public interface IApplication
	{
		
	}

	public sealed class Application
		: IApplication
	{

	}

	public sealed class Analyzer
	{

	}

	public interface IAnalyzer
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		Result Analyze(IAnalyzerSources source, Configuration configuration);
	}
}