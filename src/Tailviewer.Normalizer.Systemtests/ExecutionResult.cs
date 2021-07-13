namespace Tailviewer.Normalizer.Systemtests
{
	public sealed class ExecutionResult
	{
		/// <summary>
		///     The output written to stdout of the program.
		/// </summary>
		public string Output;

		/// <summary>
		///     The return code of the invocation of the program.
		/// </summary>
		public int ReturnCode;
	}
}