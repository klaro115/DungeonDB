using System;
using UnityEngine;

[Serializable]
public class StoryMoment
{
	#region Fields

	[Header("Descriptives:")]
	public string name = "new Moment";
	public string description = string.Empty;

	[Header("Coordinates:")]
	public string locationName = string.Empty;
	[NonSerialized]
	[UiControlLevelSpec(UiControlLevel.Any, UiControlContentBinding.LoadFromDatabase, "locationName")]
	public Location location = null;
	[UiControlLevelSpec(UiControlLevel.Normal)]
	public DateTime time = new DateTime();

	#endregion
	#region Methods

	public TimeSpan GetTimeUntil(DateTime current)
	{
		return time - current;
	}

	public void DelayMoment(TimeSpan delay)
	{
		time += delay;
	}

	public override string ToString()
	{
		return $"{name ?? string.Empty} ({time.ToShortTimeString()})";
	}

	#endregion
}
