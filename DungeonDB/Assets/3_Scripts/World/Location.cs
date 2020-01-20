using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
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
	public string parentName = string.Empty;
	[NonSerialized]
	public Location parent = null;
	public List<string> connectedLocationNames = new List<string>();
	[NonSerialized]
	public List<Location> connectedLocations = null;

	[Header("Story:")]
	public List<string> eventNames = new List<string>();
	[NonSerialized]
	public List<StoryEvent> events = null;

	#endregion
}
