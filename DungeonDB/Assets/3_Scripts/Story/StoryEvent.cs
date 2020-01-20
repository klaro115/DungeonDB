using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryEvent
{
	#region Fields

	[Header("Descriptives:")]
	public string name = "new Event";
	public string tags = string.Empty;
	public string description = string.Empty;
	public string trivia = string.Empty;
	public List<string> relatedContentNames = new List<string>();

	[Header("Coordinates:")]
	public string locationName = string.Empty;
	[NonSerialized]
	public Location location = null;
	public DateTime startTime = new DateTime();
	public DateTime endTime = new DateTime();
	public List<StoryMoment> timeline = new List<StoryMoment>();

	#endregion
	#region Properties

	public TimeSpan Duration => endTime > startTime ? endTime - startTime : TimeSpan.Zero;
	public DateTime MiddleTime => startTime + TimeSpan.FromMilliseconds(Duration.TotalMilliseconds / 2);

	public int TimelineSize => timeline != null ? timeline.Count : 0;

	#endregion
	#region Methods

	public TimeSpan GetTimeUntil(DateTime current, StoryTimeReference referenceMoment = StoryTimeReference.StartTime)
	{
		switch (referenceMoment)
		{
			case StoryTimeReference.StartTime:
				return startTime - current;
			case StoryTimeReference.EndTime:
				return endTime - current;
			case StoryTimeReference.MiddleTime:
				return MiddleTime - current;
			case StoryTimeReference.FirstTimelineMoment:
				StoryMoment ftm = timeline != null && timeline.Count != 0 ? timeline[0] : null;
				return (ftm != null ? ftm.time : startTime) - current;
			case StoryTimeReference.LastTimelineMoment:
				StoryMoment ltm = timeline != null && timeline.Count != 0 ? timeline[timeline.Count - 1] : null;
				return (ltm != null ? ltm.time : startTime) - current;
			default:
				return startTime - current;
		}
	}

	public void DelayEvent(TimeSpan delay)
	{
		startTime += delay;
		endTime += delay;
		if (timeline != null)
		{
			foreach (StoryMoment moment in timeline)
			{
				moment.DelayMoment(delay);
			}
		}
	}

	public DateTime MatchStartTimeToTimeline()
	{
		if (timeline == null || timeline.Count == 0) return startTime;
		StoryMoment moment = timeline[0];
		if (moment != null) startTime = moment.time;
		return startTime;
	}
	public DateTime MatchEndTimeToTimeline()
	{
		if (timeline == null || timeline.Count == 0) return endTime;
		StoryMoment moment = timeline[timeline.Count - 1];
		if (moment != null) endTime = moment.time;
		return endTime;
	}

	public bool FindMoment(string momentName, out StoryMoment outMoment)
	{
		outMoment = null;
		if (momentName == null || timeline == null) return false;

		outMoment = timeline.Find(o => string.Compare(momentName, o.name, StringComparison.InvariantCultureIgnoreCase) == 0);
		return outMoment != null;
	}

	#endregion
}
