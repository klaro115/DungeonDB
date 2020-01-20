﻿using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class UiControl : MonoBehaviour
{
	#region Fields

	private object rawValue = null;

	public string controlName = string.Empty;
	public IUiControlHost host = null;
	public Text uiLabel = null;

	#endregion
	#region Properties

	public virtual object RawValue { get => rawValue; set => SetValue(value); }
	public abstract Type ValueType { get; }

	#endregion
	#region Methods

	protected virtual void Start()
	{
		if (uiLabel == null) uiLabel = GetComponentInChildren<Text>(false);

		UpdateContents();
	}

	public bool IsValueTypeComptible(Type other)
	{
		Type valueType = ValueType;
		if (valueType.IsValueType && other == null) return false;
		return other == valueType || other.IsSubclassOf(valueType);
	}

	public virtual bool SetValue<T>(T newValue)
	{
		// Verify type compatibility:
		if (!IsValueTypeComptible(typeof(T)))
		{
			Debug.LogError($"Error! Invalid type for new value of control '{controlName}': {typeof(T)} vs. {ValueType}");
			return false;
		}

		// Use the default setter; override if you need special behaviour:
		return SetValue((object)newValue);
	}
	public bool SetValue(object newValue)
	{
		// Verify type compatibility:
		Type newValueType = newValue?.GetType();
		if (!IsValueTypeComptible(newValueType))
		{
			Debug.LogError($"Error! Invalid type for new value of control '{controlName}': {newValueType?.ToString() ?? "null"} vs. {ValueType}");
			return false;
		}

		// Set the new value:
		if (!SetValue_internal(newValue))
		{
			Debug.LogError($"Error! Could not set value in control '{controlName}': {newValue} ({ValueType})");
			return false;
		}

		// Update representation on UI elements:
		UpdateContents();
		return true;
	}
	protected virtual bool SetValue_internal(object newValue)
	{
		// NOTE: Remember to override this to store the value in the control's actual native/represented data type!
		rawValue = newValue;
		return true;
	}

	public abstract void UpdateContents();

	public virtual void UpdateLabelContents()
	{
		if (uiLabel != null) uiLabel.text = controlName ?? "null";
	}

	#endregion
}