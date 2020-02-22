using System;
using System.Collections.Generic;
using UnityEngine;

using Content;
using Story;
using System.IO;
using Content.Types;

namespace Projects
{
	public static class ProjectCreator
	{
		#region Methods

		public static bool CreateNewProject(string campaignName, string parentDirectory, ContentLoadSource defaultSaveLocation, DateTime ingameStartTime, out Campaign outCampaign)
		{
			// Create the campaign directories and folder structure:
			if (!CreateProjectDirectories(campaignName, parentDirectory, out string projectRootDir))
			{
				outCampaign = null;
				return false;
			}

			// Create a project object, for later serialization to file:
			Project project = new Project()
			{
				name = campaignName,
				rootPath = projectRootDir,
			};

			// Set the new project as active, thus updating content root directories to the new project's:
			if (!ProjectManager.LoadProject(project))
			{
				Debug.LogError("[ProjectCreator] Error! Failed to load and activate newly created project!");
				outCampaign = null;
				return false;
			}

			// Create basic contents for the project and its main campaign:
			string storylineName = $"{campaignName} Timeline";
			StoryEvent startEvent = new StoryEvent()
			{
				name = $"{campaignName} beginning",
				description = "The player characters first meet in the taproom of their hostel, attracted by a scream of terror. " +
					"Seconds later, the woman at the origin of the scream collapses; blood running from her eyes, she dies on the spot. " +
					"On her hand, a strange marking has appeared, strangely similar to the symbol Baal, the god of death.",
				startTime = ingameStartTime,
				storyline = new ContentAccessor(storylineName),
			};
			ContentAccessor startEventCA = new ContentAccessor(startEvent.name, startEvent);
			ContentLoader.SaveContent(startEventCA, out ContentRefAndHandle startEventRaH, defaultSaveLocation);

			StoryLine storyline = new StoryLine()
			{
				name = storylineName,
				description = $"This is the main storyline of the '{campaignName}' campaign, telling the tale of a group " +
					"of unlikely companions banding together to vanquish the great evil that is threatening their home.",
				startTime = ingameStartTime,
				endTime = ingameStartTime + new TimeSpan(1, 0, 0, 0),
				events = new List<ContentAccessor>(new ContentAccessor[] { startEventCA }),
			};
			ContentAccessor storylineCA = new ContentAccessor(storylineName, storyline);
			ContentLoader.SaveContent(storylineCA, out ContentRefAndHandle storylineRaH, defaultSaveLocation);

			Campaign campaign = new Campaign()
			{
				name = campaignName,
				description = "A grand adventure involving great heroes, vile foes, and an excessive number of murder hobos.",
				mainStoryline = new ContentAccessor(storyline.name, storyline),
			};
			ContentAccessor campaignCA = new ContentAccessor(campaignName, campaign);
			ContentLoader.SaveContent(campaignCA, out ContentRefAndHandle campaignRaH, defaultSaveLocation);

			WorldMap worldMap = new WorldMap()
			{
				name = $"{campaignName} World Map",
				description = "The home world of our great heroes, and the place they persistently haunt in quest for murder and loot.",
			};
			ContentAccessor worldMapCA = new ContentAccessor(worldMap.name, worldMap);
			ContentLoader.SaveContent(worldMapCA, out ContentRefAndHandle worldMapRaH, defaultSaveLocation);

			// Link the above contents to the project:
			project.mainCampaign = campaignCA;
			project.mainWorldMap = worldMapCA;
			ContentAccessor projectCA = new ContentAccessor(project.name, project);

			// Save project to file:
			ContentLoader.SaveContent(projectCA, out ContentRefAndHandle projectRaH, ContentLoadSource.File);

			// Output the newly created and populated campaign object and return success:
			outCampaign = campaign;
			return true;
		}

		private static bool CreateProjectDirectories(string projectName, string parentDirectory, out string outProjectRootPath)
		{
			// Verify parameters:
			if (string.IsNullOrWhiteSpace(projectName))
			{
				Debug.LogError("[ProjectCreator] Error! Project name may not be null or blank!");
				outProjectRootPath = null;
				return false;
			}
			if (string.IsNullOrWhiteSpace(parentDirectory) || !Directory.Exists(parentDirectory))
			{
				Debug.LogError($"[ProjectCreator] Error! Cannot create project directories at parent directory '{parentDirectory ?? string.Empty}' does not exist!");
				outProjectRootPath = null;
				return false;
			}

			// Create the root project folder:
			if (parentDirectory[parentDirectory.Length - 1] != '/' && parentDirectory[parentDirectory.Length - 1] != '\\')
				parentDirectory += '/';

			string projectDir = Path.Combine(parentDirectory, $"{projectName}/");
			CreateDirectory(projectDir);

			// Create subdirectories for each content category:
			Array categoryValues = Enum.GetValues(typeof(ContentType));
			foreach (object categoryObj in categoryValues)
			{
				ContentType category = (ContentType)categoryObj;
				string categorySubPath = ContentHelper.GetContentCategorySubPath(category);
				string categoryPath = Path.Combine(projectDir, categorySubPath);

				if (!CreateDirectory(categoryPath))
				{
					Debug.LogError($"[ProjectCreator] Error! Failed to create directory for content category '{category}' at path '{categoryPath}'!");
				}
			}

			outProjectRootPath = projectDir;
			return true;
		}

		private static bool CreateDirectory(string directoryPath)
		{
			if (string.IsNullOrEmpty(directoryPath)) return false;

			// If the directory already exists, stop right there:
			if (Directory.Exists(directoryPath)) return true;

			// Try creating the new directory:
			try
			{
				Directory.CreateDirectory(directoryPath);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ProjectCreator] ERROR! An exception occurred when trying to create directory at path '{directoryPath}'!\nException message: {ex.Message}");
				return false;
			}
		}

		#endregion
	}
}
