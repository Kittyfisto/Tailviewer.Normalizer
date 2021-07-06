using System;
using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.Normalizer.Core.Database
{
	public interface IReader
		: IDisposable
	{
		IEnumerable<IReadOnlyLogEntry> Query(ILogEntryFilter filter);
	}
}