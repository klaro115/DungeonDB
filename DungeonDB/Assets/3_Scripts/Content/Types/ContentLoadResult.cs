using System;

namespace Content
{
	[Serializable]
	public struct ContentLoadResult
	{
		#region Constructors

		public ContentLoadResult(object _content, bool _isSuccess, bool _loadedFromFile)
		{
			content = _content;
			contentType = _content?.GetType();
			isSuccess = _isSuccess;
			loadedFromFile = _loadedFromFile;
		}
		public ContentLoadResult(ContentHandle handle, bool _loadedFromFile)
		{
			content = handle.content;
			contentType = handle.contentType;
			isSuccess = true;
			loadedFromFile = _loadedFromFile;
		}

		#endregion
		#region Fields

		public bool isSuccess;
		public bool loadedFromFile;
		public object content;
		public Type contentType;

		#endregion
		#region Properties

		public bool IsFailure => !isSuccess;

		public static ContentLoadResult Failure => new ContentLoadResult(null, false, false);

		#endregion
	}
}
