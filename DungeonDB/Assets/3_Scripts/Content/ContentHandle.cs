using System;

namespace Content
{
	[Serializable]
	public class ContentHandle
	{
		#region Constructors

		public ContentHandle(string _nameKey, object _content)
		{
			nameKey = _nameKey?.ToLowerInvariant();
			content = _content;
			contentType = _content?.GetType();
		}

		#endregion
		#region Fields

		public string nameKey = string.Empty;
		public object content = null;
		public Type contentType = null;
		//...

		#endregion
		#region Methods

		public bool IsValid()
		{
			if (string.IsNullOrWhiteSpace(nameKey)) return false;
			if (content == null || contentType == null) return false;
			//...

			return true;
		}

		#endregion
	}
}
