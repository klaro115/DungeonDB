using System;

[Serializable]
public enum StoryTimeReference
{
	StartTime,				// The time an event starts.
	EndTime,				// The time when the event ends.
	MiddleTime,				// The mean time; exactly in the middle of the event's timespan.

	FirstTimelineMoment,	// The time of the first moment in the event's timeline.
	LastTimelineMoment		// The time of the last moment in the event's timeline.
}
