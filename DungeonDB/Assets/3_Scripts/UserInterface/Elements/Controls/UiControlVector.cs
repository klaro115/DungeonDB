using System;
using UnityEngine;
using UnityEngine.UI;

public class UiControlVector : UiControl
{
	#region Types

	public enum VectorType
	{
		Vector2		= 2,
		Vector3		= 3,
		Vector4		= 4
	}

	#endregion
	#region Fields

	private VectorType vectorType = VectorType.Vector2;
	private Type valueType = null;
	private Vector4 value = Vector4.zero;

	public InputField fieldX = null;
	public InputField fieldY = null;
	public InputField fieldZ = null;
	public InputField fieldW = null;

	#endregion
	#region Properties

	public override Type ValueType => valueType;

	#endregion
	#region Methods

	protected override void Start()
	{
		// Fetch and initialize input fields:
		InputField[] inputFields = GetComponentsInChildren<InputField>(true);
		if (inputFields != null)
		{
			// Assign fields to their respective axis:
			if (fieldX == null && inputFields.Length > 0) fieldX = inputFields[0];
			if (fieldY == null && inputFields.Length > 1) fieldX = inputFields[1];
			if (fieldZ == null && inputFields.Length > 2) fieldX = inputFields[2];
			if (fieldW == null && inputFields.Length > 3) fieldX = inputFields[3];

			// Set the layout and data formats for all inputs:
			foreach(InputField field in inputFields)
			{
				field.inputType = InputField.InputType.Standard;
				field.contentType = InputField.ContentType.DecimalNumber;
				field.textComponent.alignment = TextAnchor.MiddleRight;
			}
		}

		base.Start();
	}

	private static Vector4 ParseToVector4(object obj, int dimensions)
	{
		switch (dimensions)
		{
			case 2:
				if (obj is Vector2 v2) return new Vector4(v2.x, v2.y, 0, 0);
				else if (obj is Vector2Int v2i) return new Vector4(v2i.x, v2i.y, 0, 0);
				break;
			case 3:
				if (obj is Vector3 v3) return new Vector4(v3.x, v3.y, v3.z, 0);
				else if (obj is Vector3Int v3i) return new Vector4(v3i.x, v3i.y, v3i.z, 0);
				break;
			case 4:
				if (obj is Vector4 v4) return v4;
				break;
			default:
				break;
		}
		return Vector4.zero;
	}
	private static object ParseToObject(Vector4 value, VectorType type)
	{
		switch (type)
		{
			case VectorType.Vector2:
				return new Vector2(value.x, value.y);
			case VectorType.Vector3:
				return new Vector3(value.x, value.y, value.z);
			default:
				return value;
		}
	}

	public override bool SetValue(object newValue)
	{
		if (newValue != null && TypeTools.IsVectorType(newValue.GetType(), out int dimensions))
		{
			vectorType = (VectorType)dimensions;
			value = ParseToVector4(newValue, dimensions);
			valueType = newValue.GetType();
			rawValue = newValue;

			UpdateContents();
			return true;
		}
		return false;
	}

	private void UpdateContentsAxisField(InputField field, float axisValue, bool hasThisAxis)
	{
		if (field != null)
		{
			field.gameObject.SetActive(hasThisAxis);
			if (hasThisAxis) field.text = axisValue.ToString();
		}
	}
	public override void UpdateContents()
	{
		UpdateLabelContents();

		int axisCount = (int)vectorType;
		UpdateContentsAxisField(fieldX, value.x, true);
		UpdateContentsAxisField(fieldY, value.y, true);
		UpdateContentsAxisField(fieldZ, value.z, axisCount > 2);
		UpdateContentsAxisField(fieldW, value.w, axisCount > 3);
	}

	private bool SetAxisValue(int axisIndex, float newValue)
	{
		if (axisIndex == 0) value.x = newValue;
		else if (axisIndex == 1) value.y = newValue;
		else if (axisIndex == 2) value.z = newValue;
		else if (axisIndex == 3) value.w = newValue;
		else
		{
			Debug.LogError($"[UiControlVector] Error! Cannot set value of unidentified axis {axisIndex}!");
			return false;
		}
		object newValueRawType = ParseToObject(value, vectorType);
		return SetValue(newValueRawType);
	}

	public void CallbackValueChanged(int axisIndex)
	{
		if (axisIndex < 0 || axisIndex >= (int)vectorType) return;

		// Fetch the new value from the input field corresponding to that axis:
		string newAxisTxt = string.Empty;
		if (axisIndex == 0 && fieldX != null) newAxisTxt = fieldX.text;
		else if (axisIndex == 1 && fieldY != null) newAxisTxt = fieldY.text;
		else if (axisIndex == 2 && fieldZ != null) newAxisTxt = fieldZ.text;
		else if (axisIndex == 3 && fieldW != null) newAxisTxt = fieldW.text;
		
		// Try parsing the input field's contents to float format:
		float newAxisValue = 0.0f;
		try
		{
			newAxisValue = System.Convert.ToSingle(newAxisTxt);
		}
		catch (Exception ex)
		{
			Debug.LogError($"[UiControlVector] ERROR! Could not parse string '{newAxisTxt}' for axis {axisIndex} to float type!\nException message: {ex.Message}");
			return;
		}

		// Set the new axis value on the vector:
		if (SetAxisValue(axisIndex, newAxisValue) && host != null) host.NotifyControlChanged(this);
	}

	#endregion
}
