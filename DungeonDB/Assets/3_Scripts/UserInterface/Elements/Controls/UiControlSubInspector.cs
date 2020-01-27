using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiControlSubInspector : UiControl, IUiInspector
{
	#region Fields

	private UiInspector rootHost = null;
	private UiInspectorMode mode = UiInspectorMode.IList;

	private object target = null;
	private Type targetType = null;

	public RectTransform contentParent = null;
	private List<UiInspector.TargetControl> controls = null;

	private float baseHeight = 0.0f;
	private float extraHeightPerCtrl = 0.0f;

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

		// Gather some height and layouting values:
		VerticalLayoutGroup verticalLayout = contentParent.GetComponent<VerticalLayoutGroup>();
		if (verticalLayout != null)
		{
			baseHeight += verticalLayout.padding.vertical;
			extraHeightPerCtrl += verticalLayout.spacing;
		}

		base.Start();
	}

	public bool Initialize(UiInspector _rootHost, IUiInspector _host, object _hostTarget, UiInspector.TargetControl _control)
	{
		if (_rootHost == null || _host == null || _hostTarget == null || _control == null) return false;

		// Set host hierarchy references:
		rootHost = _rootHost;
		host = _host;
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

		Debug.Log($"TEST: Updating contents for sub inspector '{controlName}'");

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
	}

	public bool NotifyControlChanged(UiControl control)
	{
		if (control == null || controls == null) return false;

		UiInspector.TargetControl tc = controls.Find(o => o.control == control);
		if (tc == null) return false;

		// Try setting the property/field values on target after reading it from UI control:
		try
		{
			if (!tc.SetValue(target, control.RawValue)) return false;
		}
		catch (Exception ex)
		{
			Debug.LogError($"ERROR! An exception was caught while trying to set a control's value to target member!\nException message: {ex.Message}");
			return false;
		}

		// Notify the host that this list or nested object has been changed:
		return host != null ? host.NotifyControlChanged(this) : true;
	}

	public override float CalculateControlHeight()
	{
		float totalHeight = baseHeight;
		if (controls != null)
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
		return totalHeight;
	}

	#endregion
}
