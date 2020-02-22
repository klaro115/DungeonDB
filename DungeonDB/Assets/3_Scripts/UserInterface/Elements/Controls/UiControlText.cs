using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Content;

namespace UI
{
	public class UiControlText : UiControl, IUiNameKeySource
	{
		#region Fields

		public bool isNameKeyField = false;
		public string value = string.Empty;

		public float baseHeight = 0.0f;
		public float lineHeightAdd = 22.0f;

		public InputField uiField = null;
		public Button uiOpenKeyBrowser = null;

		private static StringBuilder valueBuilder = null;

		#endregion
		#region Properties

		public string Value { get => value; set => SetValue(value); }
		public override Type ValueType => typeof(string);

		#endregion
		#region Methods

		protected override void Start()
		{
			if (uiField == null) uiField = GetComponentInChildren<InputField>(true);
			if (uiOpenKeyBrowser == null) uiOpenKeyBrowser = GetComponentInChildren<Button>(true);

			base.Start();
		}

		public override bool SetValue(object newValue)
		{
			if (newValue != null && !TypeTools.IsTextType(newValue.GetType()))
			{
				Debug.LogError($"Error! Value of control '{controlName}' must be a text type! (string, StringBuilder, and ICollection<char> only)");
				return false;
			}

			// Directly set or convert the new value:
			if (newValue is string txt) value = txt;
			else if (newValue is StringBuilder builder) value = builder.ToString();
			else if (newValue is ICollection<char> coll)
			{
				if (valueBuilder == null) valueBuilder = new StringBuilder(coll.Count);
				else valueBuilder.Clear();
				foreach (char c in coll) valueBuilder.Append(c);
				value = valueBuilder.ToString();
			}
			rawValue = newValue;

			// Update representation on UI elements:
			UpdateContents();
			return true;
		}

		public override void UpdateContents()
		{
			UpdateLabelContents();

			if (uiField != null)
			{
				// Determine how many lines to provide based on the control's detail level:
				int lineCount = 1;
				switch (controlLevel)
				{
					case UiControlLevel.Broad:
						lineCount = 5;
						break;
					case UiControlLevel.Normal:
						lineCount = 2;
						break;
					default:
						lineCount = 1;
						break;
				}
				uiField.lineType = lineCount > 1 ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;

				// Calculate the control's new height and apply that:
				float newHeight = baseHeight + lineCount * lineHeightAdd;
				RectTransform rect = transform as RectTransform;
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
				//rect.anchorMax = new Vector2(rect.anchorMax.x, 1.0f);

				// Update content on that input field:
				uiField.text = value;
			}

			if (uiOpenKeyBrowser != null) uiOpenKeyBrowser.gameObject.SetActive(isNameKeyField);
		}

		public void CallbackTextChanged()
		{
			if (uiField != null && SetValue(uiField.text) && host != null) host.NotifyControlChanged(this);
		}
		public void CallbackOpenKeyBrowser()
		{
			if (isNameKeyField)
			{
				UiNameKeyBrowser nameKeyBrowser = host?.GetEnvironment()?.nameKeyBrowser;
				if (nameKeyBrowser != null) nameKeyBrowser.Activate(this);
			}
		}

		public bool SetNameKey(string newNameKey)
		{
			if (!isNameKeyField) return false;

			// Set the new value on UI:
			if (uiField != null) uiField.text = newNameKey ?? string.Empty;

			// Then pretend it was changed via direct user input:
			CallbackTextChanged();
			return true;
		}

		#endregion
	}
}
