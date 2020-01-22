using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
			UiControlLevelSpec levelSpec = fieldInfo?.GetCustomAttribute<UiControlLevelSpec>();
			controlLevel = levelSpec != null ? levelSpec.levels : UiControlLevel.Any;
		}
		public TargetControl(PropertyInfo _propInfo)
		{
			fieldInfo = null;
			propInfo = _propInfo;
			UiControlLevelSpec levelSpec = propInfo?.GetCustomAttribute<UiControlLevelSpec>();
			controlLevel = levelSpec != null ? levelSpec.levels : UiControlLevel.Any;
		}

		#endregion
		#region Fields

		public UiControl control = null;
		public readonly FieldInfo fieldInfo = null;
		public readonly PropertyInfo propInfo = null;
		public readonly UiControlLevel controlLevel = UiControlLevel.Any;

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

	public enum TargetTypes
	{
		Custom,

		Scalar,
		Text,
		Vectors,
		Color,
		DateTime,
	}

	[Serializable]
	public class ControlPrefab
	{
		public ControlPrefab(TargetTypes _targetTypes, UiControl _prefab = null, Type _type = null, UiControlLevel _controlLevel = UiControlLevel.Any)
		{
			targetTypes = _targetTypes;
			controlLevel = _controlLevel;
			typeName = string.Empty;
			prefab = _prefab;
			type = _type;
		}
		public ControlPrefab(string _typeName, UiControl _prefab = null, Type _type = null, UiControlLevel _controlLevel = UiControlLevel.Any)
		{
			targetTypes = TargetTypes.Custom;
			controlLevel = _controlLevel;
			typeName = _typeName;
			prefab = _prefab;
			type = _type;
		}

		public TargetTypes targetTypes = TargetTypes.Custom;
		public UiControlLevel controlLevel = UiControlLevel.Any;
		public string typeName = string.Empty;
		public UiControl prefab = null;
		public Type type = null;
	}

	#endregion
	#region Fields

	private object target = null;
	private Type targetType = null;

	public RectTransform controlParent = null;
	private List<TargetControl> controls = null;

	public ControlPrefab[] prefabs = new ControlPrefab[]
	{
		new ControlPrefab(TargetTypes.Scalar),
		new ControlPrefab(TargetTypes.Text),
		new ControlPrefab(TargetTypes.Vectors),
		new ControlPrefab(TargetTypes.Color),
		new ControlPrefab(TargetTypes.DateTime),
	};

	#endregion
	#region Properties

	public object Target { get => target; set => SetTarget(value); }

	public int ControlCount => controls != null ? controls.Count : 0;

	#endregion
	#region Methods

	void Start()
    {
		if (controlParent == null) controlParent = transform as RectTransform;

		// Fetch types by name right away and only once:
		if (prefabs != null && prefabs.Length != 0)
		{
			Assembly assembly = Assembly.GetCallingAssembly();
			foreach (ControlPrefab cp in prefabs)
			{
				if (cp.targetTypes == TargetTypes.Custom && cp.type == null)
				{
					cp.type = assembly.GetType(cp.typeName, false, true);
				}
			}
		}

		// Set the initial target to null:
		SetTarget(null);

		//TEST
		SetTarget(new StoryEvent()
		{
			name = "Birthday",
			description = "It's always somebody's birthday somewhere on this planet.",
			locationName = "Right here",
			startTime = new DateTime(2020, 2, 15),
			endTime = new DateTime(2020, 2, 16),
			timeline = new List<StoryMoment>(new StoryMoment[1]
			{
				new StoryMoment() { name = "Partyyyyyy", locationName = "Everywhere", time = new DateTime(2020, 2, 15, 22, 0, 0) }
			})
		});
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
		FieldInfo[] fields = targetType.GetFields();
		if (fields != null)
		{
			foreach (FieldInfo field in fields)
			{
				controls.Add(new TargetControl(field));
			}
		}
		// Gather all public getter-setter properties:
		PropertyInfo[] properties = targetType.GetProperties();
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

			// Using the control's type, find the right control for this type:
			UiControl prefab = GetControlPrefabForType(tcType, tc.controlLevel);
			if (prefab != null)
			{
				// Instantiate the control's prefab:
				GameObject newControlGO = Instantiate<GameObject>(prefab.gameObject, controlParent);
				tc.control = newControlGO.GetComponent<UiControl>();
				tc.control.controlName = tc.Name;

				// Do some type-specific initialization:
				if (tc.control is UiControlNumber cn) cn.SetDataTypeFromSystemType(tcType);
				//...
			}
			else Debug.Log($"[UiInspector] Warning: No prefab found for control '{tc.Name}' of type '{tcType}'!");
			// ^Note: The actual value and contents of the control will be set later, no need to do that here.
		}

		// Not that all controls are in place, display the target's attributes/values:
		UpdateContents();
	}

	private UiControl GetControlPrefabForType(Type type, UiControlLevel requestedLevel)
	{
		if (type == null || prefabs == null) return null;

		// Select a target depending on the given type:
		TargetTypes target = TargetTypes.Custom;
		if (TypeTools.IsScalarType(type)) target = TargetTypes.Scalar;
		else if (TypeTools.IsTextType(type)) target = TargetTypes.Text;
		else if (TypeTools.IsVectorType(type, out int dims)) target = TargetTypes.Vectors;
		else if (TypeTools.IsColorType(type)) target = TargetTypes.Color;
		else if (type == typeof(DateTime)) target = TargetTypes.DateTime;
		//...

		// Search our prefab array for a fitting target/type match:
		UiControl prefab = null;
		foreach (ControlPrefab cp in prefabs)
		{
			if (cp.targetTypes == target)
			{
				bool isCompatible = false;
				if (target != TargetTypes.Custom) isCompatible = true;
				else
				{
					if (cp.type != null && cp.type == type) isCompatible = true;
					else if (cp.type == null && string.Compare(cp.typeName, type.Name, StringComparison.InvariantCultureIgnoreCase) == 0) isCompatible = true;
				}
				if (isCompatible)
				{
					prefab = cp.prefab;
					if ((requestedLevel & cp.controlLevel) != 0) break;
				}
			}
		}
		return prefab;
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
