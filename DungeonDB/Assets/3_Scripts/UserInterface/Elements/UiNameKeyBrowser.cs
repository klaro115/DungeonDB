using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Content;
using Content.Types;
using UI.NameKeyBrowser;
using UI.Internal;

namespace UI
{
	[RequireComponent(typeof(RectTransform))]
	public class UiNameKeyBrowser : MonoBehaviour, IUiPopup
	{
		#region Types

		public class Item
		{
			public ContentRefAndHandle content = null;
			public ContentType type = ContentType.Unknown;

			public UiNameKeyBrowserItem uiItem = null;

			public string Name => content?.NameKey ?? "NULL";
		}

		[Serializable]
		public class TypeStyle
		{
			public TypeStyle(ContentType _type, string _displayName)
			{
				type = _type;
				displayName = _displayName ?? type.ToString();
			}

			public ContentType type = ContentType.Unknown;
			public string displayName = string.Empty;
			public Sprite icon = null;
		}

		#endregion
		#region Fields

		private bool isActive = false;

		private IUiNameKeySource activeSource = null;
		private int currentSelectionIndex = -1;

		public UiEnvironment environment = null;
		public RectTransform contentParent = null;
		public Button buttonConfirm = null;
		public InputField searchbar = null;

		private List<Item> items = new List<Item>();
		private List<Item> filteredItems = new List<Item>();
		[SerializeField]
		private UiNameKeyBrowserItem itemPrefab = null;
		private List<UiNameKeyBrowserItem> uiItems = new List<UiNameKeyBrowserItem>();

		[SerializeField]
		private TypeStyle[] contentTypeStyles = new TypeStyle[]
		{
			new TypeStyle(ContentType.Unknown, "???"),

			new TypeStyle(ContentType.World_Location, "Location"),
			new TypeStyle(ContentType.World_Organization, "Organization"),

			new TypeStyle(ContentType.Story_Line, "Storyline"),
			new TypeStyle(ContentType.Story_Event, "Event"),
			new TypeStyle(ContentType.Story_Moment, "Moment"),

			new TypeStyle(ContentType.Creature, "Creature"),
			//...

			new TypeStyle(ContentType.Custom, "Custom"),
		};

		private ContentType filterCategory = ContentType.Unknown;
		private Type filterType = null;

		//...

		#endregion
		#region Properties

		public bool IsActive => isActive;

		public int ItemCount => items != null ? items.Count : 0;

		#endregion
		#region Methods

		private void Start()
		{
			if (contentParent == null) contentParent = transform as RectTransform;
			if (searchbar == null) searchbar = GetComponentInChildren<InputField>();

			if (environment == null) environment = UiEnvironment.Main;

			Activate(null);
		}

		public bool Activate(IUiNameKeySource newSource)
		{
			Debug.Log("TEST: Name key browser opened.");

			// Sending a null source or retransmitting the previous source is interpreted as a command to abort:
			if (newSource == null || newSource == activeSource)
			{
				Deactivate();
				return true;
			}

			// Store the source of the current query:
			activeSource = newSource;

			// Enable this GO:
			gameObject.SetActive(true);

			// Reset browser states:
			ResetStates();

			// Update UI contents:
			UpdateContents();

			isActive = true;
			if (environment != null) environment.NotifyPopupActivityChanged(this);

			return true;
		}

		public void Deactivate()
		{
			gameObject.SetActive(false);

			currentSelectionIndex = -1;
			activeSource = null;
			isActive = false;

			if (environment != null) environment.NotifyPopupActivityChanged(this);
		}

		private void ResetStates()
		{
			// Reset current selection:
			NotifyItemSelected(-1);

			// Reset search fields:
			if (searchbar != null) searchbar.text = string.Empty;
		}

		public void SearchItems(string searchQuery)
		{
			if (items == null) UpdateItemsList();

			if (filteredItems == null) filteredItems = new List<Item>();
			else filteredItems.Clear();

			// Display all items of no filters were set:
			bool searchQueryIsBlank = string.IsNullOrWhiteSpace(searchQuery);
			if (searchQueryIsBlank && filterCategory == ContentType.Unknown && filterType == null)
			{
				filteredItems.AddRange(items);
				return;
			}

			// Filter out any matching items:
			filteredItems.AddRange(from i in items where CompareItem(i, searchQuery, searchQueryIsBlank) select i);
		}

		private bool CompareItem(Item item, string searchQuery, bool skipNameSearch)
		{
			if (item == null) return false;

			// Filter by content type:
			bool isLoaded = item.content.IsLoaded;
			if (filterType != null)
			{
				if (isLoaded && item.content.handle.contentType != filterType)
					return false;
				else if (!isLoaded && item.content.reference.contentTypeName != null && string.Compare(item.content.reference.contentTypeName, filterType.Name, true) != 0)
					return false;
			}
			// Filter by content category:
			if (filterCategory != ContentType.Unknown && item.content.Category != filterCategory) return false;

			// Return true for any elements with the requested name:
			return skipNameSearch ? true : string.Compare(item.Name, searchQuery, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		private void UpdateItemsList()
		{
			if (items == null) items = new List<Item>();
			else items.Clear();

			// First of all, get a collection of *all* content in this project:
			ContentRefAndHandle[] allContents = ContentLoader.GetAllContentReferences();
			if (allContents == null || allContents.Length == 0) return;

			foreach (ContentRefAndHandle crh in allContents)
			{
				if (crh == null || !crh.IsFetched) continue;

				Item newItem = new Item()
				{
					content = crh,
					type = crh.Category,
					uiItem = null,
				};
				items.Add(newItem);
			}
		}

		public void UpdateContents()
		{
			UpdateItemsList();

			if (uiItems == null) uiItems = new List<UiNameKeyBrowserItem>();

			int itemCount = filteredItems != null ? filteredItems.Count : 0;
			int excessCount = Mathf.Max(uiItems.Count - itemCount, 0);
			int itemsCovered = Mathf.Min(itemCount, uiItems.Count);

			for (int i = 0; i < itemCount; ++i)
			{
				Item item = filteredItems[i];
				TypeStyle style = (from s in contentTypeStyles where s.type == item.type select s).FirstOrDefault() ?? contentTypeStyles[0];
				UiNameKeyBrowserItem uiItem = null;
				if (i < itemsCovered)
				{
					item.uiItem = uiItems[i];
				}
				else
				{
					uiItem = Instantiate<UiNameKeyBrowserItem>(itemPrefab, contentParent);
					uiItems.Add(uiItem);
				}
				item.uiItem = uiItem;
				item.uiItem.SetContent(this, item, style, i);
			}
			for (int i = 0; i < excessCount; ++i)
			{
				int index = itemCount + i;
				uiItems[index].gameObject.SetActive(false);
			}
		}

		public void NotifyItemSelected(int index)
		{
			currentSelectionIndex = index;

			bool indexIsValid = index >= 0 && index < ItemCount;
			if (buttonConfirm != null) buttonConfirm.interactable = indexIsValid && activeSource != null;
		}

		public void CallbackButtonConfirm()
		{
			if (activeSource != null && currentSelectionIndex >= 0 && currentSelectionIndex < items.Count)
			{
				activeSource.SetNameKey(items[currentSelectionIndex].Name);
			}

			Deactivate();
		}
		public void CallbackButtonCancel()
		{
			Deactivate();
		}

		public void CallbackSearchItems()
		{
			if (searchbar != null)
			{
				string query = searchbar.text ?? string.Empty;
				SearchItems(query);
				UpdateContents();
			}
		}

		#endregion
	}
}
