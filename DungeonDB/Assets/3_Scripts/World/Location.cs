using System;
using System.Collections.Generic;
using UnityEngine;
using Content;

[Serializable]
[ContentElement(contentType = ContentType.World_Location, nameKeyField = "name")]
public class Location
{
	#region Fields

	[Header("Descriptives:")]
	public string name = "new Location";
	public string tags = string.Empty;
	public string description = string.Empty;
	public string appearance = string.Empty;
	public string trivia = string.Empty;
	public List<string> relatedContentNames = new List<string>();

	[Header("Coordinates:")]
	public Vector3 position = Vector3.zero;
	public Vector3 dimensions = Vector3.one;

	[Header("Connections:")]
	[UiControlDisplaySpec(displayName = "Parent Location")]
	[UiContentAccessorSpec(ContentType.World_Location, UiControlContentBinding.LoadFromDatabase, true)]
	public ContentAccessor parentLocation = ContentAccessor.Empty;
	[UiContentAccessorSpec(ContentType.World_Location, UiControlContentBinding.LoadFromDatabase, false)]
	public List<ContentAccessor> connectedLocations = new List<ContentAccessor>();

	[Header("Story:")]
	[UiContentAccessorSpec(ContentType.Story_Event, UiControlContentBinding.LoadFromDatabase, true)]
	public List<ContentAccessor> events = new List<ContentAccessor>();

	#endregion
}
