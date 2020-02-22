using System;
using System.Reflection;

namespace Content
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class ContentElementAttribute : Attribute
	{
		#region Fields

		public string nameKeyField = "name";
		public ContentType contentType = ContentType.Custom;
		public Type customType = null;

		#endregion
		#region Methods

		public Type GetContentType()
		{
			return ContentHelper.GetContentType(contentType) ?? customType;
		}

		public bool GetNameKey(object content, out string outNameKey)
		{
			if (content != null && !string.IsNullOrEmpty(nameKeyField))
			{
				Type type = content.GetType();

				object nameKeyObj = null;
				FieldInfo field = type.GetField(nameKeyField);
				if (field != null) nameKeyObj = field.GetValue(content);
				else if (field == null)
				{
					PropertyInfo prop = type.GetProperty(nameKeyField);
					if (prop != null && prop.CanRead) nameKeyObj = prop.GetValue(content);
				}
				outNameKey = nameKeyObj != null ? nameKeyObj as string : null;
				return outNameKey != null;
			}

			outNameKey = string.Empty;
			return false;
		}

		#endregion
	}
}
