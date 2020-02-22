using System;

namespace Content
{
	[Serializable]
	public class ContentReference
	{
		#region Constructors

		public ContentReference(string _nameKey, string _contentTypeName, ContentLoadSource _source, string _sourcePath)
		{
			nameKey = _nameKey?.ToLowerInvariant();
			contentTypeName = !string.IsNullOrEmpty(_contentTypeName) ? _contentTypeName : "Object";
			source = _source;
			sourcePath = _sourcePath;
		}
		public ContentReference(string _nameKey, Type _contentType, ContentLoadSource _source, string _sourcePath)
		{
			nameKey = _nameKey?.ToLowerInvariant();
			contentTypeName = _contentType != null ? _contentType.Name : "Object";
			source = _source;
			sourcePath = _sourcePath;
		}
		public ContentReference(object content, ContentLoadSource _source, string _sourcePath)
		{
			Type contentType = content?.GetType();
			if (contentType != null)
			{
				object[] attributes = contentType.GetCustomAttributes(typeof(ContentElementAttribute), true);
				ContentElementAttribute contentAttr = attributes != null && attributes.Length != 0 ? attributes[0] as ContentElementAttribute : null;
				if (contentAttr != null && contentAttr.GetNameKey(content, out nameKey))
				{
					contentTypeName = contentAttr.GetContentType()?.Name ?? string.Empty;
				}
			}
			source = _source;
			sourcePath = _sourcePath;
		}

		#endregion
		#region Fields

		public string nameKey = string.Empty;
		public string contentTypeName = string.Empty;
		public ContentLoadSource source = ContentLoadSource.File;
		public string sourcePath = null;

		#endregion
		#region Methods

		public bool IsValid()
		{
			// The content's name key and type name may never be null or blank:
			if (string.IsNullOrWhiteSpace(nameKey) || string.IsNullOrWhiteSpace(contentTypeName)) return false;

			// Depending on the source that content is to be loaded from, its path must also be non-null:
			switch (source)
			{
				case ContentLoadSource.File:
					if (string.IsNullOrWhiteSpace(sourcePath)) return false;
					break;
				case ContentLoadSource.Database:
					// todo [later] Implement database key validation of some kind...
					break;
				default:
					return false;
			}
			return true;
		}

		public override string ToString()
		{
			return $"{nameKey ?? "NULL"} (type={contentTypeName ?? "NULL"}, src={source})";
		}

		#endregion
	}
}
