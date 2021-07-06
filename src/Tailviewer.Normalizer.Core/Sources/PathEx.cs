using System.Diagnostics.Contracts;
using System.Text;

namespace Tailviewer.Normalizer.Core.Sources
{
	public static class PathEx
	{
		[Pure]
		public static string Normalize(string path)
		{
			var builder = new StringBuilder(path);
			builder.Replace('/', '\\');
			return builder.ToString();
		}
	}
}
