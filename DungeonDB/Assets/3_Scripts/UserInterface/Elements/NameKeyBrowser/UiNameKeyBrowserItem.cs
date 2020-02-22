using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NameKeyBrowser
{
	[RequireComponent(typeof(RectTransform))]
	public class UiNameKeyBrowserItem : MonoBehaviour
	{
		#region Fields

		public UiNameKeyBrowser browser = null;

		private int index = -1;
		public Image uiIcon = null;
		public Text uiName = null;
		public Text uiType = null;
		public Toggle uiLoaded = null;

		#endregion
		#region Properties

		public int Index => index;

		#endregion
		#region Methods

		public void SetContent(UiNameKeyBrowser _browser, UiNameKeyBrowser.Item item, UiNameKeyBrowser.TypeStyle style, int _index)
		{
			// No actual content? Deactivate self, object will remain in browser's object pool for later use:
			if (browser == null || item == null || _index < 0)
			{
				index = -1;
				gameObject.SetActive(false);
				return;
			}

			browser = _browser;
			index = _index;
			item.uiItem = this;

			// If some UI reference hasn't been set, try fetching it anew:
			if (uiIcon == null) uiIcon = GetComponentInChildren<Image>();
			if (uiName == null || uiType == null)
			{
				Text[] uiTexts = GetComponentsInChildren<Text>();
				if (uiTexts != null)
				{
					uiType = uiTexts[0];
					if (uiTexts.Length > 1) uiType = uiTexts[1];
				}
			}
			if (uiLoaded == null) uiLoaded = GetComponentInChildren<Toggle>();

			// Update UI elements:
			if (uiName != null) uiName.text = item.Name;
			if (style != null)
			{
				if (uiIcon != null) uiIcon.overrideSprite = style.icon;
				if (uiType != null) uiType.text = style.displayName;
			}
			else if (uiType != null) uiType.text = item.type.ToString();
			if (uiLoaded != null) uiLoaded.isOn = item.content.IsLoaded;
		}

		public void CallbackButtonClicked()
		{
			if (browser != null && index >= 0) browser.NotifyItemSelected(index);
		}

		#endregion
	}
}
