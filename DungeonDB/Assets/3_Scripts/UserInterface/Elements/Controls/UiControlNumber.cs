using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UiControlNumber : UiControl
	{
		#region Types

		[Serializable]
		public enum DataType
		{
			Byte,
			Short,
			Int,
			Long,
			Single,
			Double,
		}

		#endregion
		#region Fields

		private DataType numberType = DataType.Single;
		private Type valueType = typeof(int);
		private double value = 0.0;

		public InputField uiField = null;

		#endregion
		#region Properties

		public double Value { get => value; set => SetValue((object)value); }
		public override Type ValueType => valueType;
		public DataType NumberType { get => numberType; set => SetDataType(value); }

		#endregion
		#region Methods

		protected override void Start()
		{
			if (uiField == null) uiField = GetComponentInChildren<InputField>(true);

			base.Start();
		}

		public bool SetDataTypeFromSystemType(Type type)
		{
			if (type == null) return false;
			if (!TypeTools.IsScalarType(type)) return false;

			valueType = type;
			if (type == typeof(byte)) numberType = DataType.Byte;
			else if (type == typeof(short)) numberType = DataType.Short;
			else if (type == typeof(int)) numberType = DataType.Int;
			else if (type == typeof(long)) numberType = DataType.Long;
			else if (type == typeof(float)) numberType = DataType.Single;
			else if (type == typeof(double)) numberType = DataType.Double;
			else return false;
			return true;
		}
		public void SetDataType(DataType newType)
		{
			numberType = newType;
			switch (newType)
			{
				case DataType.Byte:
					valueType = typeof(byte);
					break;
				case DataType.Short:
					valueType = typeof(short);
					break;
				case DataType.Long:
					valueType = typeof(long);
					break;
				case DataType.Single:
					valueType = typeof(float);
					break;
				case DataType.Double:
					valueType = typeof(double);
					break;
				default:
					valueType = typeof(int);
					break;
			}
		}
	
		public bool GetValue<T>(out object outValue) where T : struct
		{
			Type type = typeof(T);
			if (type == typeof(int)) outValue = (int)value;
			else if (type == typeof(byte)) outValue = (byte)value;
			else if (type == typeof(short)) outValue = (short)value;
			else if (type == typeof(long)) outValue = (long)value;
			else if (type == typeof(float)) outValue = (float)value;
			else if (type == typeof(double)) outValue = value;
			else
			{
				outValue = null;
				return false;
			}
			return true;
		}
		public override bool SetValue(object newValue)
		{
			if (newValue != null && TypeTools.IsScalarType(newValue.GetType()))
			{
				rawValue = newValue;
				value = (double)newValue;

				UpdateContents();
				return true;
			}
			return false;
		}

		public override void UpdateContents()
		{
			UpdateLabelContents();

			if (uiField != null)
			{
				uiField.inputType = InputField.InputType.Standard;
				uiField.lineType = InputField.LineType.SingleLine;
				uiField.textComponent.alignment = TextAnchor.MiddleRight;
				switch (numberType)
				{
					case DataType.Byte:
					case DataType.Short:
					case DataType.Long:
						uiField.contentType = InputField.ContentType.IntegerNumber;
						break;
					case DataType.Single:
					case DataType.Double:
						uiField.contentType = InputField.ContentType.DecimalNumber;
						break;
					default:
						break;
				}
				uiField.text = value.ToString();
			}
		}

		public void CallbackValueChanged(double newValue)
		{
			if (SetValue(newValue) && host != null) host.NotifyControlChanged(this);
		}
	
		#endregion
	}
}
