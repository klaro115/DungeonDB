using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiControlTime : UiControl
{
	#region Fields

	private DateTime value = DateTime.Now;

	public Text labelDate = null;

	public InputField inputHour = null;
	public InputField inputMinute = null;
	public InputField inputSecond = null;
	public InputField inputMilliSecond = null;

	#endregion
	#region Properties

	public override Type ValueType => typeof(DateTime);

	#endregion
	#region Methods

	protected override void Start()
	{
		if (inputHour == null || inputMinute == null)
		{
			InputField[] inputFields = GetComponentsInChildren<InputField>(true);
			if (inputFields != null)
			{
				if (inputFields.Length > 0) inputHour = inputFields[0];
				if (inputFields.Length > 1) inputMinute = inputFields[1];
				if (inputFields.Length > 2) inputSecond = inputFields[2];
				if (inputFields.Length > 3) inputMilliSecond = inputFields[3];
			}
		}

		base.Start();
	}

	public override bool SetValue(object newValue)
	{
		if (newValue != null && newValue is DateTime dt)
		{
			value = dt;
			rawValue = newValue;

			UpdateContents();
			return true;
		}
		return false;
	}

	public override void UpdateContents()
	{
		UpdateLabelContents();

		// Update input fields for time:
		if (inputHour != null) inputHour.text = value.Hour.ToString();
		if (inputMinute != null) inputMinute.text = value.Minute.ToString();
		if (inputSecond != null) inputSecond.text = value.Second.ToString();

		// Update the date label:
		if (labelDate != null) labelDate.text = value.ToShortDateString();
	}

	private int ParseStringToInt(string txt)
	{
		if (string.IsNullOrEmpty(txt)) return 0;
		try
		{
			return System.Convert.ToInt32(txt);
		}
		catch (Exception ex)
		{
			Debug.Log($"[UiControlTime] ERROR! An exception was caught while trying to parse string '{txt}' to integer!\nException message: {ex.Message}");
			return 0;
		}
	}

	private void SetValue_callback(DateTime newValue)
	{
		if (SetValue(newValue) && host != null) host.NotifyControlChanged(this);
	}

	public void CallbackHourChanged()
	{
		int newHour = Mathf.Clamp(ParseStringToInt(inputHour?.text), 0, 23);
		DateTime newValue = new DateTime(value.Year, value.Month, value.Day, newHour, value.Minute, value.Second, value.Millisecond, value.Kind);
		SetValue_callback(newValue);
	}
	public void CallbackMinuteChanged()
	{
		int newMinute = Mathf.Clamp(ParseStringToInt(inputMinute?.text), 0, 59);
		DateTime newValue = new DateTime(value.Year, value.Month, value.Day, value.Hour, newMinute, value.Second, value.Millisecond, value.Kind);
		SetValue_callback(newValue);
	}
	public void CallbackSecondChanged()
	{
		int newSecond = Mathf.Clamp(ParseStringToInt(inputSecond?.text), 0, 59);
		DateTime newValue = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, newSecond, value.Millisecond, value.Kind);
		SetValue_callback(newValue);
	}
	public void CallbackMilliSecondChanged()
	{
		int newMilliSecond = Mathf.Clamp(ParseStringToInt(inputMilliSecond?.text), 0, 999);
		DateTime newValue = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, newMilliSecond, value.Kind);
		SetValue_callback(newValue);
	}

	public void CallbackIncrementDay(int direction)
	{
		DateTime newValue = value.AddDays(direction);
		SetValue_callback(newValue);
	}
	public void CallbackIncrementMonth(int direction)
	{
		DateTime newValue = value.AddMonths(direction);
		SetValue_callback(newValue);
	}
	public void CallbackIncrementYear(int direction)
	{
		DateTime newValue = value.AddYears(direction);
		SetValue_callback(newValue);
	}

	#endregion
}
