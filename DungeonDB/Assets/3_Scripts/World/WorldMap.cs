using System;
using System.Collections.Generic;
using UnityEngine;

using Content;

[Serializable]
public class WorldMap
{
	#region Fields

	[Header("Descriptives:")]
	public string name = "new Map";
	public string tags = string.Empty;
	public string description = string.Empty;
	public string trivia = string.Empty;

	[Header("Coordinates:")]
	public Vector3 position = Vector3.zero;
	public Vector3 dimensions = Vector3.one;

	[Header("Locations:")]
	public List<ContentAccessor> majorLocations = new List<ContentAccessor>();

	#endregion
	#region Methods

	public void LoadBaseContents()
	{
		if (majorLocations != null)
		{
			foreach (ContentAccessor locationCA in majorLocations)
			{
				if (locationCA != null) locationCA.TryLoadContent();
			}
		}
	}

	#endregion
}
