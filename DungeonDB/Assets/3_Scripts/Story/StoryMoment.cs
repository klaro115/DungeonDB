using System;
using UnityEngine;
using Content;

[Serializable]
[ContentElement(contentType = ContentType.Story_Moment, nameKeyField = "name")]
public class StoryMoment : IContentItem
{
	#region Fields

	[Header("Descriptives:")]
	public string name = "new Moment";
	public string description = string.Empty;

	[Header("Coordinates:")]
	[UiContentAccessorSpec(ContentType.World_Location, UiControlContentBinding.LoadFromDatabase, false)]
	public ContentAccessor location = ContentAccessor.Empty;

	[UiControlLevelSpec(UiControlLevel.Normal)]
	public DateTime time = new DateTime();

	#endregion
	#region Methods

	public void LoadAllContents()
	{
		if (location != null) location.TryLoadContent(out object locationObj);
	}

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
