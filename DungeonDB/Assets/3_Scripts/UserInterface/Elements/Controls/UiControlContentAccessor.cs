using System;
using UnityEngine;
using Content;
using UI.Inspector;
using System.Reflection;

namespace UI
{
	public class UiControlContentAccessor : UiControl, IUiSubInspector
	{
		#region Fields

		private UiInspector rootHost = null;

		private ContentAccessor accessor = null;
		private UiContentAccessorSpec accessorSpec = null;
		private TargetControl nameKey = null;
		private TargetControl subInspector = null;

		#endregion
		#region Properties

		public object ControlTarget => accessor;
		public override Type ValueType => accessor.GetValueType(accessorSpec != null ? accessorSpec.contentType : ContentType.Unknown) ?? typeof(object);
		public Type ControlTargetType => typeof(ContentAccessor);

		public IUiHostElement HostElement => host ?? rootHost;
		public UiInspector RootHost => host?.RootHost;
		public IUiInspector Host => host;
		public RectTransform ContentParent => transform as RectTransform;

		public int ControlCount => 2;
		public UiInspectorMode InspectorMode => UiInspectorMode.NestedElement;

		private bool DisplayContent => accessorSpec == null || (accessorSpec != null && accessorSpec.displayContent);

		#endregion
		#region Methods

		protected override void Start()
		{
			base.Start();
		}

		public bool Initialize(UiInspector _rootHost, IUiInspector _host, object _hostTarget, TargetControl _control)
		{
			if (_rootHost == null || _host == null || _hostTarget == null || _control == null) return false;

			// Set host hierarchy references:
			rootHost = _rootHost;
			host = _host;
			controlName = _control?.Name ?? "???";

			// Make sure this type of control is only ever used to represent content accessors:
			if (_control.type != TargetControl.TCType.ContentAccessor)
			{
				Debug.LogError($"[UiControlContentAccessor] Error! This control can only represent members of target type 'ContentAccessor', found type '{_control.type}'!");
				return false;
			}
			accessor = _control.accessor;
			accessorSpec = _control.fieldInfo?.GetCustomAttribute<UiContentAccessorSpec>();

			// Initialize local target controls for the accessor's 2 visible user-members and add the necessary bindings from text to content field:
			if (nameKey == null) nameKey = new TargetControl(typeof(ContentAccessor).GetField("nameKey"));
			if (nameKey.control == null) nameKey.control = GetComponentInChildren<UiControlText>(true);

			if (subInspector == null) subInspector = new TargetControl(typeof(ContentAccessor).GetField("content"));
			if (subInspector.control == null) subInspector.control = GetComponentInChildren<UiControlSubInspector>(true);

			// Update name key control and the subinspector:
			nameKey.control.controlName = _control.GetDisplayName();
			nameKey.control.controlLevel = UiControlLevel.Detailed;
			nameKey.control.host = this;
			if (nameKey.control is UiControlText ctrlTxt) ctrlTxt.isNameKeyField = true;

			nameKey.AddContentBindingTarget(subInspector);
			nameKey.control.SetValue(accessor.nameKey);

			subInspector.control.controlName = _control.GetDisplayName();
			subInspector.control.controlLevel = _control.setup.level;
			subInspector.control.host = this;

			if (DisplayContent)
			{
				(subInspector.control as UiControlSubInspector).Initialize(rootHost, this, ControlTarget, subInspector);
				subInspector.control.SetValue(accessor.content);
			}
			else
			{
				(subInspector.control as UiControl).gameObject.SetActive(false);
			}

			// Return success:
			UpdateContents();
			return true;
		}

		public override bool SetValue(object newValue)
		{
			if (newValue != null && newValue is ContentAccessor newAccessor)
			{
				accessor = newAccessor;
				rawValue = accessor;

				if (nameKey != null) nameKey.control.SetValue(accessor.nameKey);
				return subInspector != null ? subInspector.control.SetValue(accessor.content) : true;
			}
			return false;
		}

		public override void UpdateContents()
		{
			// Update child controls:
			if (nameKey?.control != null) nameKey.control.UpdateContents();
			if (subInspector?.control != null && DisplayContent) subInspector.control.UpdateContents();

			// Update the control's physical height to encompass all child controls:
			float totalHeight = CalculateControlHeight();
			RectTransform rect = transform as RectTransform;
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
		}

		public bool NotifyControlChanged(UiControl control)
		{
			if (control == null) return false;

			TargetControl tc = null;
			if (control == nameKey?.control) tc = nameKey;
			else if (control == subInspector?.control) tc = subInspector;
			else return false;

			// Try setting the property/field values on target after reading it from UI control:
			bool success = false;
			bool bindingSuccessful = false;
			try
			{
				success = tc.SetValue(accessor, control.RawValue);
				if (success && tc.IsContentBindingSource)
				{
					bindingSuccessful = UiInspector.LoadContentBindingValues(this, tc, accessor.nameKey);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"[UiControlSubInspector] ERROR! An exception was caught while trying to set a control's value to target member!\nException message: {ex.Message}");
				return false;
			}

			// If the bound content was not found or could not be loaded, the key must have been invalid, so reset that:
			if (!bindingSuccessful)
			{
				nameKey.control.SetValue(string.Empty);
			}

			// Notify the host that this list or nested object has been changed:
			return success && host != null ? host.NotifyControlChanged(this) : success;
		}

		public override float CalculateControlHeight()
		{
			float totalHeight = 0.0f;
			if (nameKey?.control != null) totalHeight += nameKey.control.CalculateControlHeight();
			if (subInspector?.control != null && DisplayContent) totalHeight += subInspector.control.CalculateControlHeight();
			return totalHeight;
		}

		public UiEnvironment GetEnvironment()
		{
			return rootHost != null ? rootHost.GetEnvironment() : host?.GetEnvironment();
		}

		#endregion
	}
}
