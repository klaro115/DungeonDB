using System;
using UnityEngine;

namespace Content
{
	/// <summary>
	/// Instead of using a separate string member as name key source, and a second field for the loaded content controlled via binding,
	/// use this type instead, decorated with the same 'UiControlLevelSpec' attribute as the target, though leaving the binding source
	/// name blank (as it will be overwritten by a special behaviour). When changing the name key field's control, the associated content
	/// will be loaded and displayed correctly without the clutter of having dozens of attributes for every other field.
	/// </summary>
	[Serializable]
	public class ContentAccessor
	{
		#region Fields

		public ContentAccessor(string _nameKey, object _content = null)
		{
			nameKey = !string.IsNullOrEmpty(_nameKey) ? _nameKey : "Content Name";
			content = _content;
		}

		#endregion
		#region Fields

		public string nameKey = string.Empty;
		[NonSerialized]
		public object content = null;

		#endregion
		#region Properties

		public static ContentAccessor Empty => new ContentAccessor(string.Empty, null);

		#endregion
		#region Methods

		public bool SetValue(object newValue)
		{
			if (newValue != null)
			{
				Type requiredType = GetValueType();
				if (requiredType != null && newValue.GetType() != requiredType)
				{
					Debug.LogError($"[ContentAccessor] Error! Cannot assign content of type '{newValue.GetType()}' when the required type is '{requiredType}'!");
					return false;
				}
			}
			content = newValue;
			return true;
		}
		public object GetValue()
		{
			return content;
		}

		public Type GetValueType(ContentType contentType = ContentType.Unknown)
		{
			return content != null ? content.GetType() : ContentHelper.GetContentType(contentType);
		}

		/// <summary>
		/// Safe shorthand for casting the loaded content directly to the desired type.
		/// </summary>
		/// <typeparam name="T">The loaded content's expected type.</typeparam>
		/// <param name="outContent">Output the content object after casting it to the expected type; outputs null if type is mismatched or null.</param>
		/// <returns>False if the type could not be cast or was null, true if the type was cast and output to the expected type.</returns>
		public bool GetContent<T>(out T outContent) where T : class
		{
			if (content != null && content is T typedContent)
			{
				outContent = typedContent;
				return true;
			}
			outContent = null;
			return false;
		}

		public bool TryLoadContent()
		{
			if (content != null) return true;

			ContentLoadResult result = ContentLoader.LoadContent(nameKey);
			content = result.content;
			return result.isSuccess;
		}

		public bool TryLoadContent(out object outContent)
		{
			bool isSuccess = true;
			if (content == null)
			{
				ContentLoadResult result = ContentLoader.LoadContent(nameKey);
				isSuccess = result.isSuccess;
				content = result.content;
			}

			outContent = content;
			return isSuccess;
		}

		public bool TryLoadContent<T>(out T outContent)
		{
			if (TryLoadContent(out object contentObj))
			{
				if (contentObj == null)
				{
					outContent = default(T);
					return true;
				}
				else if (contentObj is T content)
				{
					outContent = content;
					return true;
				}
			}
			outContent = default(T);
			return false;
		}

		#endregion
	}
}
