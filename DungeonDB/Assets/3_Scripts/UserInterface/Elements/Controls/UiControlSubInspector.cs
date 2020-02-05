using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiControlSubInspector : UiControl, IUiInspector
{
	#region Fields

	public bool allowCollapseLists = true;
	public bool allowCollapseNested = true;
	private bool isCollapsed = false;
	public bool startCollapsed = true;

	private UiInspector rootHost = null;
	private UiInspectorMode mode = UiInspectorMode.IList;

	private object target = null;
	private Type targetType = null;

	public RectTransform contentParent = null;
	private List<UiInspector.TargetControl> controls = null;
	public Button buttonCollapse = null;
	private Text buttonCollapseTxt = null;
	private Color buttonCollapseColorNormal = new Color(1, 1, 1, 1);
	public Color buttonCollapseColorActive = new Color(1, 0.65f, 0, 1);

	private float baseHeight = 0.0f;
	private float extraHeightPerCtrl = 0.0f;
	public float baseHeightExtra = 10.0f;

	#endregion
	#region Properties

	public override Type ValueType => targetType;
	public object ControlTarget => target;
	public Type ControlTargetType => targetType;

	public UiInspector RootHost => rootHost;
	public IUiInspector Host => host;
	public RectTransform ContentParent => contentParent;

	public int ControlCount => controls != null ? controls.Count : 0;
	private int ListElementCount => target != null && target is IList list && list != null ? list.Count : 0;
	public UiInspectorMode InspectorMode => mode;

	#endregion
	#region Methods

	protected override void Start()
	{
		if (uiLabel == null) uiLabel = GetComponentInChildren<Text>();
		if (contentParent == null) contentParent = transform as RectTransform;
		if (buttonCollapse == null) buttonCollapse = GetComponentInChildren<Button>(false);
		if (buttonCollapse != null)
		{
			buttonCollapseColorNormal = buttonCollapse.image.color;
			buttonCollapseTxt = buttonCollapse.GetComponentInChildren<Text>();
		}

		// Gather some height and layouting values:
		VerticalLayoutGroup verticalLayout = contentParent.GetComponent<VerticalLayoutGroup>();
		if (verticalLayout != null)
		{
			baseHeight += verticalLayout.padding.vertical;
			extraHeightPerCtrl += verticalLayout.spacing;
		}

		base.Start();

		if (startCollapsed) SetCollapsed(true);
	}

	public bool Initialize(UiInspector _rootHost, IUiInspector _host, object _hostTarget, UiInspector.TargetControl _control)
	{
		if (_rootHost == null || _host == null || _hostTarget == null || _control == null) return false;

		// Set host hierarchy references:
		rootHost = _rootHost;
		host = _host;
		//if (host == this) Debug.Log("WTF???");
		controlName = _control?.Name ?? "???";

		// Get the value of the host's control, which will serve as this control's target:
		if (!_control.GetValue(_hostTarget, out target))
		{
			string hostTargetTypeName = _hostTarget?.GetType().ToString() ?? string.Empty;
			Debug.LogError($"[UiControlSubInspector] Error! Subinspector '{controlName}' of target type '{hostTargetTypeName}' failed to get target object from host's target control!");
			return false;
		}
		// Get the target's supposed type:
		if (!_control.GetType(out targetType))
		{
			string hostTargetTypeName = _hostTarget?.GetType().ToString() ?? string.Empty;
			Debug.LogError($"[UiControlSubInspector] Error! Subinspector '{controlName}' of target type '{hostTargetTypeName}' failed to get type info from host's target control!");
			return false;
		}

		// Calculate the control's base height:
		baseHeight = base.CalculateControlHeight();

		// Figure out if target is an IList type or just an arbitrary object type:
		if (TypeTools.ImplementsInterface(targetType, typeof(IList))) mode = UiInspectorMode.IList;
		else mode = UiInspectorMode.NestedElement;

		// Return success:
		UpdateContents();
		return true;
	}

	public override bool IsValueTypeComptible(Type other)
	{
		if (targetType == null) return false;
		if (targetType.IsValueType && other == null) return false;
		return true;
	}
	
	public override bool SetValue(object newValue)
	{
		target = newValue;
		rawValue = newValue;
		if (target != null && targetType != target.GetType()) targetType = target.GetType();

		UpdateContents();
		return true;
	}

	public override void UpdateContents()
	{
		UpdateLabelContents();

		// Have the controls rebuilt if either they haven't been initialized or the represented list's element count changed:
		if (controls == null || (mode == UiInspectorMode.IList && controls.Count != ListElementCount))
		{
			UiInspector.RebuildContents(this, ref controls);
		}

		// Update regular inspector contents:
		UiInspector.UpdateContents(this, ref controls);

		// Update the control's physical height to encompass all child controls:
		float totalHeight = CalculateControlHeight();
		RectTransform rect = transform as RectTransform;
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

		// Hide the collapse button if we're in a mode that does not allow it:
		if (buttonCollapse != null)
		{
			if ((mode == UiInspectorMode.IList && !allowCollapseLists) || (mode == UiInspectorMode.NestedElement && !allowCollapseNested))
				buttonCollapse.gameObject.SetActive(false);
		}
	}

	public bool NotifyControlChanged(UiControl control)
	{
		if (control == null || controls == null) return false;

		UiInspector.TargetControl tc = controls.Find(o => o.control == control);
		if (tc == null)
		{
			Debug.LogError($"[UiControlSubInspector] Error! Subinspector '{controlName}' with target '{target}' of type '{targetType}' does not contain control '{control.controlName}'!");
			return false;
		}

		// Try setting the property/field values on target after reading it from UI control:
		bool success = false;
		try
		{
			success = tc.SetValue(target, control.RawValue);
			if (success && tc.IsContentBindingSource)
			{
				UiInspector.LoadContentBindingValues(this, tc, null);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"[UiControlSubInspector] ERROR! An exception was caught while trying to set a control's value to target member!\nException message: {ex.Message}");
			return false;
		}

		// Notify the host that this list or nested object has been changed:
		return success && host != null ? host.NotifyControlChanged(this) : success;
	}

	public override float CalculateControlHeight()
	{
		float totalHeight = baseHeight + baseHeightExtra;
		if (controls != null && !isCollapsed)
		{
			foreach (UiInspector.TargetControl tc in controls)
			{
				if (tc != null && tc.control != null)
				{
					float controlHeight = tc.control.CalculateControlHeight();
					totalHeight += controlHeight + extraHeightPerCtrl;
				}
			}
		}
		testLastTotalHeight = totalHeight;
		return totalHeight;
	}

	public bool SetCollapsed(bool toggle)
	{
		// Only react to collapsing when in a mode that allows it:
		if ((mode == UiInspectorMode.IList && allowCollapseLists) ||
			(mode == UiInspectorMode.NestedElement && allowCollapseNested))
		{
			// Set the new state:
			isCollapsed = toggle;

			// Hide contents and update collapsing controls:
			if (contentParent != null && contentParent != transform)
				contentParent.gameObject.SetActive(!isCollapsed);
			if (buttonCollapse != null)
			{
				buttonCollapse.image.color = isCollapsed ? buttonCollapseColorActive : buttonCollapseColorNormal;
				if (buttonCollapseTxt != null) buttonCollapseTxt.text = isCollapsed ? "+" : "-";
			}
		}

		// Update the control's physical height to encompass all child controls:
		float totalHeight = CalculateControlHeight();
		RectTransform rect = transform as RectTransform;
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

		// Force a layout rebuild by turning the game object off and on again:
		gameObject.SetActive(false);
		gameObject.SetActive(true);
		// ^Note: Seriously, why is there no easy and reliable way of doing this? This works but is highly janky and likely redoes the layouting twice...

		// Notify other subinspectors up the hierarchy to fix their layout as well:
		if (host != null && host is UiControlSubInspector hostSI) hostSI.SetCollapsed(hostSI.isCollapsed);

		// Return success:
		return true;
	}

	public void CallbackCollapseControl()
	{
		SetCollapsed(!isCollapsed);
	}

	#endregion
}
