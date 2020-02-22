using System;
using System.Collections.Generic;
using UnityEngine;

using UI.Internal;

namespace UI
{
	public class UiEnvironment : MonoBehaviour
	{
		#region Fields

		public Canvas canvas = null;

		[Header("Workspaces:")]
		public UiWorkspace workspace = null;

		[Header("Popups & Modal Windows:")]
		public UiNameKeyBrowser nameKeyBrowser = null;

		private List<IUiPopup> activePopups = null;


		private static UiEnvironment main = null;

		#endregion
		#region Properties

		public bool AreWorkspacesLocked => activePopups != null && activePopups.Count != 0;

		public static UiEnvironment Main => main;

		#endregion
		#region Methods

		private void Awake()
		{
			if (main == null) main = this;
		}

		public bool NotifyPopupActivityChanged(IUiPopup newPopup)
		{
			if (activePopups == null) activePopups = new List<IUiPopup>();
			if (newPopup == null) return false;

			// Add or remove the popup from list of active popups:
			bool changed = false;
			if (newPopup.IsActive)
			{
				if (!activePopups.Contains(newPopup))
				{
					activePopups.Add(newPopup);
					changed = true;
				}
			}
			else
			{
				changed = activePopups.Remove(newPopup);
			}

			// Lock workspaces if any of the popups are still active:
			if (workspace != null) workspace.SetWorkspaceLocked(AreWorkspacesLocked);

			// Return success:
			return changed;
		}

		public void DeactivateAllPopups()
		{
			if (activePopups == null) return;

			foreach (IUiPopup popup in activePopups)
			{
				if (popup.IsActive) popup.Deactivate();
			}
			activePopups.Clear();
		}

		#endregion
	}
}
