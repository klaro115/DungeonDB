using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryLine
{
	#region Fields

	[Header("Descriptives:")]
	public string name = "new Storyline";
	public string tags = string.Empty;
	public string description = string.Empty;
	public List<string> relatedContentNames = new List<string>();

	[Header("Coordinates:")]
	public List<string> eventNames = new List<string>();
	[NonSerialized]
	public List<StoryEvent> events = null;
	[UiControlLevelSpec(UiControlLevel.Broad)]
	public DateTime startTime = new DateTime();
	[UiControlLevelSpec(UiControlLevel.Broad)]
	public DateTime endTime = new DateTime();

	#endregion
	#region Properties

	public TimeSpan Duration => endTime > startTime ? endTime - startTime : TimeSpan.Zero;
	public DateTime MiddleTime => startTime + TimeSpan.FromMilliseconds(Duration.TotalMilliseconds / 2);

	public int EventCount => events != null ? events.Count : (eventNames != null ? eventNames.Count : 0);

	#endregion
	#region Methods

	public void DelayEvent(TimeSpan delay)
	{
		startTime += delay;
		endTime += delay;
		if (events != null)
		{
			foreach (StoryEvent se in events)
			{
				se.DelayEvent(delay);
			}
		}
	}

	public DateTime MatchStartTimeToEvents()
	{
		if (events == null || events.Count == 0) return startTime;
		StoryEvent se = events[0];
		if (se != null) startTime = se.startTime;
		return startTime;
	}
	public DateTime MatchEndTimeToEvents()
	{
		if (events == null || events.Count == 0) return endTime;
		StoryEvent se = events[events.Count - 1];
		if (se != null) endTime = se.endTime;
		return endTime;
	}

	public bool FindEvent(string eventName, out StoryEvent outEvent)
	{
		outEvent = null;
		if (eventName == null || events == null) return false;

		outEvent = events.Find(o => string.Compare(eventName, o.name, StringComparison.InvariantCultureIgnoreCase) == 0);
		return outEvent != null;
	}
	public bool FindEventMoment(string momentName, out StoryMoment outMoment)
	{
		outMoment = null;
		if (momentName == null || events == null) return false;

		foreach (StoryEvent se in events)
		{
			if (se.FindMoment(momentName, out outMoment)) return true;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{name} ({startTime.ToShortDateString()}-{endTime.ToShortDateString()})";
	}

	#endregion
}
