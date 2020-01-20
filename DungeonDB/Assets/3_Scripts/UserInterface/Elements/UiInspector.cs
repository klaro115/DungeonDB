using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiInspector : MonoBehaviour, IUiControlHost
{
	#region Types

	class TargetControl
	{
		#region Constructors

		public TargetControl(FieldInfo _fieldInfo)
		{
			fieldInfo = _fieldInfo;
			propInfo = null;
		}
		public TargetControl(PropertyInfo _propInfo)
		{
			fieldInfo = null;
			propInfo = _propInfo;
		}

		#endregion
		#region Fields

		public UiControl control = null;
		public readonly FieldInfo fieldInfo = null;
		public readonly PropertyInfo propInfo = null;

		public bool IsField => fieldInfo != null;
		public bool IsProperty => fieldInfo == null && propInfo != null;
		public string Name => fieldInfo != null ? fieldInfo.Name : (propInfo != null ? propInfo.Name : string.Empty);

		#endregion
		#region Methods

		public bool GetValue(object target, out object outValue)
		{
			if (IsField) outValue = fieldInfo.GetValue(target);
			else if (IsProperty) outValue = propInfo.GetValue(target);
			else
			{
				outValue = null;
				return false;
			}
			return true;
		}
		public bool SetValue(object target, object value)
		{
			if (IsField) fieldInfo.SetValue(target, value);
			else if (IsProperty) propInfo.SetValue(target, value);
			else return false;
			return true;
		}
		public bool GetType(out Type outType)
		{
			if (IsField) outType = fieldInfo.FieldType;
			else if (IsProperty) outType = propInfo.PropertyType;
			else
			{
				outType = null;
				return false;
			}
			return true;
		}
		
		#endregion
	}

	#endregion
	#region Fields

	private object target = null;
	private Type targetType = null;

	private List<TargetControl> controls = null;

	#endregion
	#region Properties

	public object Target { get => target; set => SetTarget(value); }

	public int ControlCount => controls != null ? controls.Count : 0;

	#endregion
	#region Methods

	void Start()
    {
		// Set the initial target to null:
		SetTarget(null);
    }

	public bool SetTarget(object newTarget)
	{
		// Target remains unchanged, simply update controls' values to be safe:
		if (newTarget == target)
		{
			UpdateContents();
			return true;
		}

		// Set the new target object and type:
		Type prevTargetType = targetType;
		target = newTarget;
		targetType = target?.GetType();

		// If the new target is null or a value type, purge all controls:
		if (target == null || (targetType != null && targetType.IsValueType))
		{
			// ^Note: Why are value types not supported? Because I don't wanna deal with that pointer nightmare.
			ClearContents();
			return true;
		}

		// Rebuild UI controls if the type has changed:
		if (prevTargetType != targetType) RebuildContents();
		// Just update controls' values if the target's type remained the same:
		else UpdateContents();

		return true;
	}
	
	public void ClearContents()
	{
		if (controls != null)
		{
			foreach (TargetControl tc in controls)
			{
				Destroy(tc.control.gameObject);
				tc.control = null;
			}
			controls.Clear();
		}
	}

	public void RebuildContents()
	{
		if (controls == null) controls = new List<TargetControl>();

		// Purge all previous contents and controls:
		ClearContents();

		// Stop right there if the target is null:
		if (target == null || targetType == null) return;

		// Gather all public fields:
		FieldInfo[] fields = targetType.GetFields(BindingFlags.Public);
		if (fields != null)
		{
			foreach (FieldInfo field in fields)
			{
				controls.Add(new TargetControl(field));
			}
		}
		// Gather all public getter-setter properties:
		PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Public);
		if (fields != null)
		{
			foreach (PropertyInfo prop in properties)
			{
				if (prop.CanRead && prop.CanWrite)
				{
					controls.Add(new TargetControl(prop));
				}
			}
		}

		// Create the actual UI controls representing the members we just gathered:
		foreach(TargetControl tc in controls)
		{
			if (!tc.GetType(out Type tcType)) continue;

			if (TypeTools.IsIntegerType(tcType) || TypeTools.IsFloatType(tcType))
			{
				tc.control = new UiControlNumber();
				(tc.control as UiControlNumber).SetDataTypeFromSystemType(tcType);
			}
			// TODO: add further types here.
		}

		// Not that all controls are in place, display the target's attributes/values:
		UpdateContents();
	}

	public void UpdateContents()
	{
		if (target == null) return;

		// Target is non-null but no controls have been set? Rebuild controls now:
		if (controls == null)
		{
			RebuildContents();
			return;
		}

		// Try fetching values from properties and fields and passing these to the controls:
		try
		{
			foreach (TargetControl tc in controls)
			{
				if (tc.control == null) continue;
				if (tc.GetValue(target, out object tcValue))
				{
					tc.control.SetValue(tcValue);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"ERROR! An exception was caught while trying to update inspector's UI controls!\nException message: {ex.Message}");
			return;
		}
	}

	public bool NotifyControlChanged(UiControl control)
	{
		if (control == null) return false;
		if (controls == null) return false;

		TargetControl tc = controls.Find(o => o.control == control);
		if (tc == null) return false;

		// Try setting the property/field values on target after reading it from UI control:
		try
		{
			return tc.SetValue(target, control.RawValue);
		}
		catch (Exception ex)
		{
			Debug.LogError($"ERROR! An exception was caught while trying to set a control's value to target member!\nException message: {ex.Message}");
			return false;
		}
	}

	#endregion
}
