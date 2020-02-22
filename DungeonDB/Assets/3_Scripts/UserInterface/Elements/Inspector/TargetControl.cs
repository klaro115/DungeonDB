using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Content;

namespace UI.Inspector
{
	[Serializable]
	public class TargetControl
	{
		#region Types

		public enum TCType
		{
			Field,
			Property,
			List,
			ContentAccessor,

			None
		}

		#endregion
		#region Constructors

		public TargetControl(FieldInfo _fieldInfo, object _target = null)
		{
			fieldInfo = _fieldInfo;
			if (fieldInfo != null && _target != null && fieldInfo.FieldType == typeof(ContentAccessor))
			{
				type = TCType.ContentAccessor;
				accessor = fieldInfo.GetValue(_target) as ContentAccessor;
			}
			else
			{
				type = TCType.Field;
			}
			UiControlLevelSpec levelSpec = fieldInfo?.GetCustomAttribute<UiControlLevelSpec>();
			setup = levelSpec != null ? levelSpec.Setup : new UiControlSetup();
		}
		public TargetControl(PropertyInfo _propInfo)
		{
			type = TCType.Property;
			propInfo = _propInfo;
			UiControlLevelSpec levelSpec = propInfo?.GetCustomAttribute<UiControlLevelSpec>();
			setup = levelSpec != null ? levelSpec.Setup : new UiControlSetup();
		}
		public TargetControl(IList _list, int _listIndex)
		{
			type = TCType.List;
			list = _list;
			listIndex = _listIndex;
			UiControlLevelSpec levelSpec = propInfo?.GetCustomAttribute<UiControlLevelSpec>();
			setup = levelSpec != null ? levelSpec.Setup : new UiControlSetup();
		}

		#endregion
		#region Fields

		public UiControl control = null;

		public readonly TCType type = TCType.None;
		public readonly FieldInfo fieldInfo = null;
		public readonly PropertyInfo propInfo = null;
		public readonly IList list = null;
		public readonly ContentAccessor accessor = null;
		public int listIndex = 0;

		public readonly UiControlSetup setup = new UiControlSetup();
		public List<ContentBindingHandle> contentBindingTargets = null;

		#endregion
		#region Properties

		public bool IsContentBindingSource => contentBindingTargets != null && contentBindingTargets.Count != 0;

		public string Name
		{
			get
			{
				if (fieldInfo != null) return fieldInfo.Name;
				if (propInfo != null) return propInfo.Name;
				if (list != null) return $"{GetNumberString(listIndex + 1)} Entry:";
				return string.Empty;
			}
		}
		public string GetDisplayName()
		{
			if (fieldInfo != null) return fieldInfo.GetCustomAttribute<UiControlDisplaySpec>()?.displayName ?? Name;
			if (propInfo != null) return propInfo.GetCustomAttribute<UiControlDisplaySpec>()?.displayName ?? Name;
			return Name;
		}

		#endregion
		#region Methods

		private static string GetNumberString(int x)
		{
			switch (x % 10)
			{
				case 1:
					return $"{x}st";
				case 2:
					return $"{x}nd";
				case 3:
					return $"{x}rd";
				default:
					return $"{x}th";
			}
		}

		public bool GetValue(object target, out object outValue)
		{
			switch (type)
			{
				case TCType.Field:
					outValue = fieldInfo.GetValue(target);
					return true;
				case TCType.Property:
					outValue = propInfo.GetValue(target);
					return true;
				case TCType.List:
					if (listIndex >= 0 && listIndex < list.Count)
					{
						outValue = list[listIndex];
						return true;
					}
					break;
				case TCType.ContentAccessor:
					outValue = accessor.GetValue();
					return true;
				default:
					break;
			}
			outValue = null;
			return false;
		}

		public bool SetValue(object target, object value)
		{
			switch (type)
			{
				case TCType.Field:
					fieldInfo.SetValue(target, value);
					break;
				case TCType.Property:
					propInfo.SetValue(target, value);
					break;
				case TCType.List:
					if (listIndex < 0 || listIndex >= list.Count) return false;
					list[listIndex] = value;
					break;
				case TCType.ContentAccessor:
					if (!accessor.SetValue(value)) return false;
					break;
				default:
					return false;
			}
			return control != null ? control.SetValue(value) : true;
		}

		public bool GetType(out Type outType)
		{
			switch (type)
			{
				case TCType.Field:
					outType = fieldInfo?.FieldType;
					return true;
				case TCType.Property:
					outType = propInfo?.PropertyType;
					return true;
				case TCType.List:
					if (list.Count > 0)
					{
						outType = list[0]?.GetType();
						return outType != null;
					}
					break;
				case TCType.ContentAccessor:
					outType = typeof(ContentAccessor);
					return outType != null;
				default:
					break;
			}
			outType = null;
			return false;
		}

		public bool AddContentBindingTarget(TargetControl bindingTarget)
		{
			if (bindingTarget == null || bindingTarget == this) return false;

			if (contentBindingTargets == null) contentBindingTargets = new List<ContentBindingHandle>();
			else if (contentBindingTargets.Find(o => o.bindingTarget == bindingTarget) != null) return false;

			contentBindingTargets.Add(new ContentBindingHandle(bindingTarget, bindingTarget.setup.contentBinding));

			if (control != null && control is UiControlText ctrlTxt) ctrlTxt.isNameKeyField = true;
			return true;
		}

		#endregion
	}
}
