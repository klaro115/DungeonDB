using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UiPane : MonoBehaviour
{
	#region Fields

	public UiPane parent = null;
	public UiPane[] neighbors = null;
	public RectTransform contentParent = null;
	public GameObject reopenControl = null;

	public bool startOpen = true;

	public UiPaneClosingMode closingMode = UiPaneClosingMode.None;
	public UiOrientation orientation = UiOrientation.Vertical;
	public UiScalingMode scalingMode = UiScalingMode.Fixed;
	public UiScalingRole scalingRole = UiScalingRole.LeftMaster;
	public bool retainLastScaleAsDefault = false;
	public float defaultScale = 300.0f;

	private static Vector3[] worldCorners = new Vector3[4];

	#endregion
	#region Properties

	public bool IsActive => gameObject.activeSelf;
	public float Size => GetCurrentSize(transform as RectTransform);

	#endregion
	#region Methods
		
	private void Start()
	{
		if (contentParent == null) contentParent = transform as RectTransform;
		if (retainLastScaleAsDefault && IsActive) defaultScale = Size;

		if (startOpen) Open();
		else Close();
	}

	private RectTransform.Axis GetScalingAxis()
	{
		return orientation == UiOrientation.Horizontal ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
	}
	private RectTransform.Edge GetScalingEdge(UiScalingCoordinator coordinator = UiScalingCoordinator.Command)
	{
		if (orientation == UiOrientation.Horizontal)
			return coordinator == UiScalingCoordinator.LeftMaster ? RectTransform.Edge.Left : RectTransform.Edge.Right;
		else
			return coordinator == UiScalingCoordinator.LeftMaster ? RectTransform.Edge.Top : RectTransform.Edge.Bottom;
	}
	private float GetCurrentSize(RectTransform rect)
	{
		rect.GetWorldCorners(worldCorners);
		return orientation == UiOrientation.Horizontal ? worldCorners[2].x - worldCorners[0].x : worldCorners[1].y - worldCorners[0].y;
	}
	private void SetNewSlaveAnchors(RectTransform rect, float newScale, bool scaleLeftSide)
	{
		Vector2 anchorMin = rect.anchorMin;
		Vector2 anchorMax = rect.anchorMax;
		if (orientation == UiOrientation.Horizontal)
		{
			if (scaleLeftSide) anchorMin.x = newScale;
			else anchorMax.x = 1.0f - newScale;
		}
		else
		{
			if (scaleLeftSide) anchorMin.y = newScale;
			else anchorMax.y = 1.0f - newScale;
		}
		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
	}

	public void Resize(float newMasterSize, UiScalingCoordinator scalingCoordinator, float prevMasterSize = 0.0f)
	{
		// Gather some reference values:
		RectTransform rect = transform as RectTransform;
		float newScale = newMasterSize;
		float previousSize = GetCurrentSize(rect);

		// Determine the pane's new scale based on its role:
		if (scalingRole == UiScalingRole.CenterSlave)
		{
			// Don't allow coordinator to manipulate slave panes directly, they need to go through a master first:
			if (scalingCoordinator == UiScalingCoordinator.Command) return;

			// Adjust slave scale:
			switch (scalingMode)
			{
				case UiScalingMode.AdjustWidth:
					rect.SetSizeWithCurrentAnchors(GetScalingAxis(), newScale);
					break;
				case UiScalingMode.AdjustAnchors:
					SetNewSlaveAnchors(rect, newScale, scalingCoordinator == UiScalingCoordinator.LeftMaster);
					break;
				case UiScalingMode.AdjustPadding:
					float newSize = GetCurrentSize(rect) + prevMasterSize - newScale;
					rect.SetInsetAndSizeFromParentEdge(GetScalingEdge(scalingCoordinator), newScale, newSize);
					break;
				default:
					break;
			}
		}
		else
		{
			// Adjust master scale:
			switch (scalingMode)
			{
				case UiScalingMode.AdjustWidth:
					rect.SetSizeWithCurrentAnchors(GetScalingAxis(), newScale);
					break;
				case UiScalingMode.AdjustAnchors:
					//todo
					break;
				case UiScalingMode.AdjustPadding:
					rect.SetInsetAndSizeFromParentEdge(GetScalingEdge(), newScale, GetCurrentSize(rect) - newScale);
					break;
				default:
					break;
			}

			// Adjust slave neighbors to the master's new size:
			if (neighbors != null)
			{
				UiScalingCoordinator coordinatorRole = scalingRole == UiScalingRole.LeftMaster ? UiScalingCoordinator.LeftMaster : UiScalingCoordinator.RightMaster;
				foreach (UiPane neighbor in neighbors)
				{
					if (neighbor.scalingRole == UiScalingRole.CenterSlave)
					{
						neighbor.Resize(newMasterSize, coordinatorRole, previousSize);
					}
				}
			}
		}
		// If requested, adopt the new scale as default for closing/reopening:
		if (retainLastScaleAsDefault && newScale > 0)
		{
			defaultScale = newScale;
		}
	}

	public void Activate()
	{
		gameObject.SetActive(true);
	}
	public void Deactivate()
	{
		gameObject.SetActive(false);
	}

	public void Close()
	{
		bool closed = true;
		switch (closingMode)
		{
			case UiPaneClosingMode.ResizeToZero:
				Resize(0.0f, UiScalingCoordinator.Command);
				Deactivate();
				break;
			case UiPaneClosingMode.Deactivate:
				Deactivate();
				break;
			default:
				closed = false;
				break;
		}
		if (closed && reopenControl != null) reopenControl.SetActive(true);
	}
	public void Open()
	{
		bool opened = true;
		switch (closingMode)
		{
			case UiPaneClosingMode.ResizeToZero:
				Activate();
				Resize(defaultScale, UiScalingCoordinator.Command);
				break;
			case UiPaneClosingMode.Deactivate:
				Activate();
				break;
			default:
				opened = false;
				break;
		}
		if (opened && reopenControl != null) reopenControl.SetActive(false);
	}

	#endregion
}
