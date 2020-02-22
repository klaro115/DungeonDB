using System;
using System.Collections.Generic;
using UnityEngine;
using Content;

[Serializable]
[ContentElement(contentType = ContentType.Story_Line, nameKeyField = "name")]
public class StoryLine : IContentItem
{
	#region Fields

	[Header("Descriptives:")]
	public string name = "new Storyline";
	public string tags = string.Empty;
	public string description = string.Empty;
	public List<string> relatedContentNames = new List<string>();

	[Header("Coordinates:")]
	[UiControlLevelSpec(UiControlLevel.Any, bindingBehaviour = UiControlBindingBehaviour.HideIfNoBinding, contentBinding = UiControlContentBinding.LoadFromDatabase)]
	public List<ContentAccessor> events = new List<ContentAccessor>();
	[UiControlLevelSpec(UiControlLevel.Broad)]
	public DateTime startTime = new DateTime();
	[UiControlLevelSpec(UiControlLevel.Broad)]
	public DateTime endTime = new DateTime();

	#endregion
	#region Properties

	public TimeSpan Duration => endTime > startTime ? endTime - startTime : TimeSpan.Zero;
	public DateTime MiddleTime => startTime + TimeSpan.FromMilliseconds(Duration.TotalMilliseconds / 2);

	public int EventCount => events != null ? events.Count : 0;

	#endregion
	#region Methods

	public void LoadAllContents()
	{
		if (events != null)
		{
			foreach (ContentAccessor eventCA in events)
			{
				eventCA.TryLoadContent(out object eventObj);
			}
		}
	}
	public bool GetEvent(int index, out StoryEvent outEvent)
	{
		outEvent = null;
		if (events == null || index < 0 || index >= events.Count) return false;

		ContentAccessor eventCA = events[index];
		if (!eventCA.TryLoadContent(out object eventObj)) return false;

		outEvent = eventObj as StoryEvent;
		return true;
	}

	public void DelayEvent(TimeSpan delay)
	{
		startTime += delay;
		endTime += delay;
		if (events != null)
		{
			for (int i = 0; i < events.Count; ++i)
			{
				if (GetEvent(i, out StoryEvent se) && se != null) se.DelayEvent(delay);
			}
		}
	}

	public DateTime MatchStartTimeToEvents()
	{
		if (!GetEvent(0, out StoryEvent se)) return startTime;
		if (se != null) startTime = se.startTime;
		return startTime;
	}
	public DateTime MatchEndTimeToEvents()
	{
		if (!GetEvent(0, out StoryEvent se)) return endTime;
		if (se != null) endTime = se.endTime;
		return endTime;
	}

	public bool FindEvent(string eventName, out StoryEvent outEvent)
	{
		outEvent = null;
		if (eventName == null || events == null) return false;

		ContentAccessor eventCA = events.Find(o => string.Compare(eventName, o.nameKey, StringComparison.InvariantCultureIgnoreCase) == 0);
		if (eventCA != null && eventCA.TryLoadContent(out object eventObj))
		{
			outEvent = eventObj as StoryEvent;
			return true;
		}
		return false;
	}
	public bool FindEventMoment(string momentName, out StoryMoment outMoment)
	{
		outMoment = null;
		if (momentName == null || events == null) return false;

		LoadAllContents();
		foreach (ContentAccessor eventCA in events)
		{
			if (eventCA.content != null && eventCA.content is StoryEvent se && se.FindMoment(momentName, out outMoment)) return true;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{name} ({startTime.ToShortDateString()}-{endTime.ToShortDateString()})";
	}

	#endregion
}
