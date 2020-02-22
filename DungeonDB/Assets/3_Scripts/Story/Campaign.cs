using System;
using System.Collections.Generic;
using UnityEngine;

using Content;

namespace Story
{
	[Serializable]
	public class Campaign
	{
		#region Fields

		[Header("Descriptives:")]
		public string name = "Campaign";
		public string tags = string.Empty;
		public string description = string.Empty;
		public string trivia = string.Empty;

		[Header("Story:")]
		[UiControlLevelSpec(UiControlLevel.DontShow, bindingBehaviour = UiControlBindingBehaviour.HideIfNoBinding, contentBinding = UiControlContentBinding.LoadFromDatabase)]
		public ContentAccessor mainStoryline = new ContentAccessor(string.Empty);

		[Header("Cast:")]
		[UiControlLevelSpec(UiControlLevel.DontShow, bindingBehaviour = UiControlBindingBehaviour.HideIfNoBinding, contentBinding = UiControlContentBinding.LoadFromDatabase)]
		List<ContentAccessor> playerCharacters = new List<ContentAccessor>();
		[UiControlLevelSpec(UiControlLevel.DontShow, bindingBehaviour = UiControlBindingBehaviour.HideIfNoBinding, contentBinding = UiControlContentBinding.LoadFromDatabase)]
		List<ContentAccessor> storyCharacters = new List<ContentAccessor>();

		#endregion
		#region Methods

		public void LoadBaseContents()
		{
			if (mainStoryline != null && mainStoryline.TryLoadContent<StoryLine>(out StoryLine msl))
			{
				msl.LoadAllContents();
			}

			if (playerCharacters != null)
			{
				foreach (ContentAccessor playerCA in playerCharacters)
				{
					if (playerCA != null) playerCA.TryLoadContent();
				}
			}
			if (storyCharacters != null)
			{
				foreach (ContentAccessor npcCA in storyCharacters)
				{
					if (npcCA != null) npcCA.TryLoadContent();
				}
			}
		}

		#endregion
	}
}
