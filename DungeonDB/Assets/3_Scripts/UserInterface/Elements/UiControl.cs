using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public abstract class UiControl : MonoBehaviour
	{
		#region Fields

		protected object rawValue = null;

		public string controlName = string.Empty;
		public UiControlLevel controlLevel = UiControlLevel.Normal;
		public IUiInspector host = null;
		public Text uiLabel = null;

		protected static StringBuilder labelBuilder = null;

		protected static readonly Vector3[] screenCorners = new Vector3[4];

		//TEST
		public float testLastTotalHeight = 0.0f;

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

		public virtual bool IsValueTypeComptible(Type other)
		{
			Type valueType = ValueType;
			if (valueType == null) return false;
			if (valueType.IsValueType && other == null) return false;
			return other == valueType || other.IsSubclassOf(valueType);
		}

		public virtual bool SetValue(object newValue)
		{
			// Verify type compatibility:
			Type newValueType = newValue?.GetType();
			if (!IsValueTypeComptible(newValueType))
			{
				Debug.LogError($"Error! Invalid type for new value of control '{controlName}': {newValueType?.ToString() ?? "null"} vs. {ValueType}");
				return false;
			}

			// Set the new value:
			rawValue = newValue;

			// Update representation on UI elements:
			UpdateContents();
			return true;
		}

		public abstract void UpdateContents();

		public virtual void UpdateLabelContents(string statusTxt = null)
		{
			if (uiLabel != null)
			{
				FormatLabelText(controlName);
				uiLabel.text = labelBuilder.ToString();
			}
		}

		public static StringBuilder FormatLabelText(string name)
		{
			if (labelBuilder == null) labelBuilder = new StringBuilder(128);
			else labelBuilder.Clear();

			string rawTxt = name ?? "???";
			bool prevCharWasLower = false;
			bool isFirstChar = true;
			foreach (char c in rawTxt)
			{
				// Separate words based on camelCase upper-to-lower changes:
				bool isUpper = char.IsUpper(c);
				bool isLetter = char.IsLetter(c);
				if (isUpper && prevCharWasLower) labelBuilder.Append(' ');
				// Capitalize the first character if it is a letter:
				if (isFirstChar && isLetter)
					labelBuilder.Append(char.ToUpper(c));
				else
					labelBuilder.Append(c);
				// Reset camelCase flag:
				prevCharWasLower = !isUpper && isLetter;
				isFirstChar = false;
			}
			return labelBuilder;
		}

		public virtual float CalculateControlHeight()
		{
			RectTransform rect = transform as RectTransform;
			rect.GetLocalCorners(screenCorners);
			float height = Mathf.Abs(screenCorners[2].y - screenCorners[0].y);
			testLastTotalHeight = height;
			return height;
		}

		#endregion
	}
}
