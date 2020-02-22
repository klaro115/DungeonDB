using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UiControlCalendar : UiControl
	{
		#region Fields

		private DateTime value = DateTime.Now;

		public Text labelMonth = null;
		public Text labelYear = null;
		public RectTransform gridParent = null;
		private Button[,] grid = null;
		private Text[,] gridDates = null;

		public Color selectedDateColor = new Color(1, 0.5f, 0, 1);

		public string[] monthNames = new string[12]
		{
			"January",
			"February",
			"March",
			"April",
			"May",
			"June",
			"July",
			"August",
			"September",
			"October",
			"November",
			"December"
		};

		#endregion
		#region Properties

		public DateTime Value { get => value; set => SetValue((object)value); }
		public override Type ValueType => typeof(DateTime);

		#endregion
		#region Methods

		protected override void Start()
		{
			if (gridParent == null) gridParent = transform as RectTransform;
			Button[] allButtons = gridParent.GetComponentsInChildren<Button>(true);
			if (allButtons != null && allButtons.Length >= 42)
			{
				grid = new Button[6, 7];
				gridDates = new Text[grid.GetLength(0), grid.GetLength(1)];
				for (int i = 0; i < Mathf.Min(42, allButtons.Length); ++i)
				{
					int weekIndex = i / 7;
					int dayIndex = i % 7;
					Button button = allButtons[i];
					grid[weekIndex, dayIndex] = button;
					gridDates[weekIndex, dayIndex] = button.GetComponentInChildren<Text>(true);
				}
			}

			base.Start();
		}
	
		public override bool SetValue(object newValue)
		{
			if (newValue != null && newValue is DateTime dt)
			{
				value = dt;
				rawValue = dt;

				UpdateContents();
				return true;
			}
			return false;
		}

		public override void UpdateContents()
		{
			UpdateLabelContents();

			if (grid != null)
			{
				// Get Monday-based index of the day of the week the value represents, as well as its week and month indices:
				DateTime firstOfMonth = new DateTime(value.Year, value.Month, 1);
				int weekDayIndexFirst = ((int)firstOfMonth.DayOfWeek + 6) % 7;
				int weekDayIndexValue = ((int)value.DayOfWeek + 6) % 7;
				int weekOfMonthIndex = (value.Day + weekDayIndexFirst - 1) / 7;
				int lengthOfMonth = DateTime.DaysInMonth(value.Year, value.Month);
				int dayOfMonth = value.Day - 1;

				// Update the grid buttons representing the days of the week:
				for (int week = 0; week < 6; ++week)
				{
					for (int day = 0; day < 7; ++day)
					{
						int index = week * 7 + day;
						int gridDayOfMonth = index - weekDayIndexFirst;
						bool isPrevMonth = week == 0 && gridDayOfMonth < 0;
						bool isNextMonth = week >= 4 && gridDayOfMonth >= lengthOfMonth;
						bool isSameMonth = !isPrevMonth && !isNextMonth;
						bool isSelectedDate = gridDayOfMonth == dayOfMonth;

						Button button = grid[week, day];
						button.interactable = isSameMonth;
						button.image.color = isSelectedDate ? selectedDateColor : Color.white;
						Text buttonLabel = gridDates[week, day];
						if (buttonLabel != null)
						{
							buttonLabel.enabled = isSameMonth;
							if (isSameMonth)
							{
								buttonLabel.text = (gridDayOfMonth + 1).ToString();
								buttonLabel.fontStyle = isSelectedDate ? FontStyle.Bold : FontStyle.Normal;
							}
						}
					}
				}
			}
			// Update month and year labels:
			if (labelMonth != null) labelMonth.text = monthNames?[value.Month - 1] ?? string.Empty;
			if (labelYear != null) labelYear.text = value.Year.ToString();
		}

		private void SetValue_callback(DateTime newValue)
		{
			if (SetValue(newValue) && host != null) host.NotifyControlChanged(this);
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
		public void CallbackSelectGridDate(int gridIndex)
		{
			int weekIndex = gridIndex / 7;
			int dayOfWeekIndex = gridIndex % 7;
			DateTime firstOfMonth = new DateTime(value.Year, value.Month, 1);
			int weekDayIndexFirst = ((int)firstOfMonth.DayOfWeek + 6) % 7;
			int weekDayIndexValue = ((int)value.DayOfWeek + 6) % 7;
			int gridDayOfMonth = gridIndex - weekDayIndexFirst + 1;
			DateTime newValue = new DateTime(value.Year, value.Month, gridDayOfMonth, value.Hour, value.Minute, value.Second, value.Millisecond, value.Kind);
			SetValue_callback(newValue);
		}

		#endregion
	}
}
