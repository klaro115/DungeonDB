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
	public Location location = null;
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

	#endregion
}
