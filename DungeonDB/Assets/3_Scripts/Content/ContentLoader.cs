using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Content
{
	public static class ContentLoader
	{
		#region Fields

		private static Dictionary<string, ContentReference> contentDict = null;		// Dictionary containing all detected content in the project.
		private static Dictionary<string, ContentHandle> contentLoadedDict = null;	// Dictionary containing 

		#endregion
		#region Properties

		public static int ContentLoadedCount => contentLoadedDict != null ? contentLoadedDict.Count : 0;

		#endregion
		#region Methods

		public static void FetchAllContentReferences()
		{
			// TODO [Critical]: Browse our directories, fetch databases, and enter all files and entries into 'contentDict'!
		}

		public static ContentLoadResult LoadContent(string nameKey)
		{
			if (string.IsNullOrEmpty(nameKey))
			{
				Debug.LogError("[ContentLoader] Error! Content name key may not be null or empty!");
				return ContentLoadResult.Failure;
			}

			// All name keys must be lower case and culture-independent:
			nameKey = nameKey.ToLowerInvariant();

			// Check if the content has already been loaded before, if so, just return that:
			if (contentLoadedDict == null) contentLoadedDict = new Dictionary<string, ContentHandle>();
			else if (contentLoadedDict.TryGetValue(nameKey, out ContentHandle contentHandle))
			{
				// Briefly check the content handle's validity:
				if (!contentHandle.IsValid())
				{
					Debug.LogError($"[ContentLoader] Error! Content handle '{contentHandle}' with key '{nameKey}' is invalid and cannot be loaded!");
					return ContentLoadResult.Failure;
				}

				// Return the content:
				return new ContentLoadResult(contentHandle, false);
			}

			// If the content has not been loaded yet, let's see if we can find a file reference to it:
			if (contentDict == null) contentDict = new Dictionary<string, ContentReference>();
			if (contentDict.TryGetValue(nameKey, out ContentReference contentRef))
			{
				// Try loading the content from file or database as indicated in its reference:
				if (!LoadContentFromReference(contentRef, out ContentHandle newHandle)) return ContentLoadResult.Failure;

				// Register the newly loaded content and return it:
				contentLoadedDict.Add(nameKey, newHandle);
				return new ContentLoadResult(newHandle, true);
			}

			// No content could be found, print an message and return failure:
			Debug.Log($"[ContentLoader] Could not find any content matching the name '{nameKey}'! A file or database entry with that key does not exist.");
			return ContentLoadResult.Failure;
		}

		private static bool LoadContentFromReference(ContentReference contentRef, out ContentHandle outHandle)
		{
			// Verify parameters and basic validity of the content reference:
			outHandle = null;
			if (contentRef == null)
			{
				Debug.LogError("[ContentLoader] Error! Cannot load content from null reference!");
				return false;
			}
			if (!contentRef.IsValid())
			{
				Debug.LogError($"[ContentLoader] Error! Content reference '{contentRef}' is invalid, cannot load content!");
				return false;
			}

			// Load the content from whichever source it is located in:
			switch (contentRef.source)
			{
				case ContentLoadSource.File:
					// Content object was serialized to file, read and deserialize it:
					return ContentFileLoader.LoadContent(contentRef, out outHandle);
				case ContentLoadSource.Database:
					// Content was stored in a database (SQL or other), retrieve it:
					return LoadContentFromDatabase(contentRef, out outHandle);
				default:
					// Unidentified source, log an error message and return failure:
					Debug.LogError($"[ContentLoader] Error! Content source '{contentRef.source}' is unknown, cannot load content for reference '{contentRef}'!");
					break;
			}
			return false;
		}

		private static bool LoadContentFromDatabase(ContentReference contentRef, out ContentHandle outHandle)
		{
			outHandle = null;
			if (contentRef == null) return false;

			// TODO

			object content = null;

			outHandle = new ContentHandle(contentRef.nameKey, content);
			return true;
		}

		#endregion
	}
}
