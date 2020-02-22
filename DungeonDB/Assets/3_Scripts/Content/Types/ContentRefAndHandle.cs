using System;

namespace Content.Types
{
	public class ContentRefAndHandle
	{
		#region Constructors

		public ContentRefAndHandle(ContentReference _reference, ContentHandle _handle = null)
		{
			reference = _reference;
			handle = _handle;
		}

		#endregion
		#region Fields

		public ContentReference reference = null;
		public ContentHandle handle = null;

		#endregion
		#region Properties

		public bool IsFetched => reference != null;
		public bool IsLoaded => reference != null && handle != null;

		public string NameKey => handle?.nameKey ?? reference?.nameKey ?? "NULL";
		public ContentType Category => handle != null ? handle.category : ContentType.Unknown;

		#endregion
	}
}
