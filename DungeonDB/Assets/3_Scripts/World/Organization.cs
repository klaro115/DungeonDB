using System;
using System.Collections.Generic;
using UnityEngine;
using Content;

[Serializable]
[ContentElement(contentType = ContentType.World_Organization, nameKeyField = "name")]
public class Organization
{
	#region Fields

	[Header("Descriptives:")]
	public string name = string.Empty;
	public string tags = string.Empty;
	public string description = string.Empty;
	public string trivia = string.Empty;
	public List<string> relatedContentNames = new List<string>();

	[Header("Coordinates:")]
	[UiControlDisplaySpec(displayName = "Headquarters")]
	public string headquartersName = string.Empty;
	[NonSerialized]
	[UiControlLevelSpec(UiControlLevel.Any, "headquartersName", UiControlContentBinding.LoadFromDatabase)]
	public Location headquarters = null;

	public string storylineName = string.Empty;
	[NonSerialized]
	[UiControlLevelSpec(UiControlLevel.DontShow, "storylineName", UiControlContentBinding.LoadFromDatabase)]
	public StoryLine storyline = null;

	#endregion
}
