using System;
using System.IO;

namespace Tailviewer.Normalizer.Core.Sources
{
	public sealed class SeekableReadOnlyStream
		: Stream
	{
		private readonly MemoryStream _buffer;
		private readonly Stream _data;

		public SeekableReadOnlyStream(Stream data)
		{
			_buffer = new MemoryStream();
			_data = data;
			_data.CopyTo(_buffer);
			_buffer.Position = 0;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_data.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Overrides of Stream

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _buffer.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _buffer.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override long Length
		{
			get
			{
				return _buffer.Length;
			}
		}

		public override long Position
		{
			get
			{
				return _buffer.Position;
			}
			set
			{
				_buffer.Position = value;
			}
		}

		#endregion
	}
}