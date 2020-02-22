using System;
using System.Collections.Generic;
using UnityEngine;

using Content;
using Story;

namespace Projects
{
	[Serializable]
	public class Project
	{
		#region Fields

		[Header("Descriptives:")]
		public string name = "new Project";
		public string description = string.Empty;
		[NonSerialized]
		public string rootPath = string.Empty;
		public ContentFileLoader.FileFormat preferredFileFormat = ContentFileLoader.FileFormat.Json;

		[Header("Main Contents:")]
		[UiContentAccessorSpec(ContentType.Campaign, UiControlContentBinding.LoadFromDatabase, true)]
		public ContentAccessor mainCampaign = new ContentAccessor(string.Empty);
		[UiContentAccessorSpec(ContentType.World_Map, UiControlContentBinding.LoadFromDatabase, false)]
		public ContentAccessor mainWorldMap = new ContentAccessor(string.Empty);

		#endregion
		#region Methods

		public void LoadBaseContents()
		{
			if (mainCampaign != null && mainCampaign.TryLoadContent<Campaign>(out Campaign mc))
			{
				mc.LoadBaseContents();
			}
			if (mainWorldMap != null && mainWorldMap.TryLoadContent<WorldMap>(out WorldMap mwm))
			{
				mwm.LoadBaseContents();
			}
		}

		#endregion
	}
}
