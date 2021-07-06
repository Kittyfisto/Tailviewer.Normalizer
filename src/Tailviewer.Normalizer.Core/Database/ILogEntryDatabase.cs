namespace Tailviewer.Normalizer.Core.Database
{
	public interface ILogEntryDatabase
	{
		IImporter CreateImporter();
		IReader CreateReader();
	}
}