using System;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(CanvasGroup))]
	public class UiWorkspace : MonoBehaviour, IUiHostElement
	{
		#region Types

		public enum Space
		{
			Main,
			TabLeft,
			TabRight,
			Bottom
		}

		#endregion
		#region Fields

		public UiEnvironment environment = null;
		private CanvasGroup canvasGroup = null;

		public UiPane mainSpace = null;
		public UiPane tabLeft = null;
		public UiPane tabRight = null;
		public UiPane tabBottom = null;

		#endregion
		#region Properties

		public IUiHostElement HostElement => null;

		public CanvasGroup CanvasGroup => canvasGroup;

		#endregion
		#region Methods

		private void Awake()
		{
			if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

			if (environment == null) environment = UiEnvironment.Main;
		}

		public GameObject AddModuleToSpace(GameObject newModule, Space space, bool newModuleIsPrefab)
		{
			if (newModule == null) return null;
			// Only allow the addition of UI type elements:
			if (!(newModule.transform is RectTransform))
			{
				Debug.LogError("[UiWorkspace] Error! Please only add game objects with a RectTransform as modules to a workspace!");
				return null;
			}

			// Select the right to add this module to:
			UiPane hostSpace = null;
			switch (space)
			{
				case Space.Main:
					hostSpace = mainSpace;
					break;
				case Space.TabLeft:
					hostSpace = tabLeft;
					break;
				case Space.TabRight:
					hostSpace = tabRight;
					break;
				case Space.Bottom:
					hostSpace = tabBottom;
					break;
				default:
					break;
			}
			if (hostSpace == null) return null;

			// If the given module is a prefab rather than a ready-to-go scene instance:
			if (newModuleIsPrefab)
			{
				newModule = Instantiate(newModule, hostSpace.contentParent);
			}
			// The new module is ready-to-go, just make it a child of the the host space:
			else
			{
				newModule.transform.SetParent(hostSpace.contentParent);
				hostSpace.contentParent.ForceUpdateRectTransforms();
			}
			return newModule;
		}

		public UiEnvironment GetEnvironment() => environment;

		public void SetWorkspaceLocked(bool isLocked)
		{
			if (canvasGroup != null) canvasGroup.interactable = !isLocked;
		}

		#endregion
	}
}
