using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Content;
using UI.Inspector;

namespace UI
{
	[RequireComponent(typeof(RectTransform))]
	public class UiInspector : MonoBehaviour, IUiInspector
	{
		#region Fields

		public UiPane hostPane = null;

		public bool adjustHeightToFitContents = false;

		private object target = null;
		private Type targetType = null;

		public RectTransform controlParent = null;
		private List<TargetControl> controls = null;

		public ControlPrefab[] prefabs = new ControlPrefab[]
		{
			new ControlPrefab(TargetTypes.Scalar),
			new ControlPrefab(TargetTypes.Text),
			new ControlPrefab(TargetTypes.Vectors),
			new ControlPrefab(TargetTypes.Color),
			new ControlPrefab(TargetTypes.DateTime),
		};

		private float baseHeight = 0.0f;
		private float extraHeightPerCtrl = 0.0f;

		#endregion
		#region Properties

		public object Target { get => target; set => SetTarget(value); }

		public object ControlTarget => target;
		public Type ControlTargetType => targetType;

		public int ControlCount => controls != null ? controls.Count : 0;
		public UiInspectorMode InspectorMode => UiInspectorMode.NestedElement;

		public IUiHostElement HostElement => hostPane;
		public UiInspector RootHost => this;
		public IUiInspector Host => null;
		public RectTransform ContentParent => controlParent;

		#endregion
		#region Methods

		void Start()
		{
			if (controlParent == null) controlParent = transform as RectTransform;

			// Gather some height and layouting values:
			VerticalLayoutGroup verticalLayout = controlParent.GetComponent<VerticalLayoutGroup>();
			if (verticalLayout != null)
			{
				baseHeight += verticalLayout.padding.vertical;
				extraHeightPerCtrl += verticalLayout.spacing;
			}

			// Fetch types by name right away and only once:
			if (prefabs != null && prefabs.Length != 0)
			{
				Assembly assembly = Assembly.GetCallingAssembly();
				foreach (ControlPrefab cp in prefabs)
				{
					if (cp.targetTypes == TargetTypes.Custom && cp.type == null)
					{
						cp.type = assembly.GetType(cp.typeName, false, true);
					}
				}
			}

			// Set the initial target to null:
			SetTarget(null);

			//TEST
			SetTarget(new StoryEvent()
			{
				name = "Birthday",
				description = "It's always somebody's birthday somewhere on this planet.",
				location = new ContentAccessor("My Place")
				{
					//content = new Location()
					//{
					//	name = "My Place",
					//	description = "The place where my house lives, you know, where my bed sleeps.",
					//	position = new Vector3(10, 0, 100),
					//},
				},
				startTime = new DateTime(2020, 2, 15),
				endTime = new DateTime(2020, 2, 16),
				timeline = new List<StoryMoment>(new StoryMoment[1]
				{
					new StoryMoment() { name = "Partyyyyyy", location = new ContentAccessor("Everywhere"), time = new DateTime(2020, 2, 15, 22, 0, 0) }
				})
			});
		}

		public bool SetTarget(object newTarget)
		{
			// Target remains unchanged, simply update controls' values to be safe:
			if (newTarget == target)
			{
				UpdateContents(this, ref controls);
				return true;
			}

			// Set the new target object and type:
			Type prevTargetType = targetType;
			target = newTarget;
			targetType = target?.GetType();

			// If the new target is null or a value type, purge all controls:
			if (target == null || (targetType != null && targetType.IsValueType))
			{
				// ^Note: Why are value types not supported? Because I don't wanna deal with that pointer nightmare.
				ClearContents(controls);
				return true;
			}

			// Rebuild UI controls if the type has changed:
			if (prevTargetType != targetType) RebuildContents(this, ref controls);
			// Just update controls' values if the target's type remained the same:
			else UpdateContents(this, ref controls);

			// After updating or rebuilding contents, we may want to adjust this inspector's height to properly fit everything:
			if (adjustHeightToFitContents)
			{
				float totalHeight = CalculateTotalHeight();
				RectTransform rect = transform as RectTransform;
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
			}

			return true;
		}
	
		public static void ClearContents(List<TargetControl> controls)
		{
			if (controls != null)
			{
				foreach (TargetControl tc in controls)
				{
					Destroy(tc.control.gameObject);
					tc.control = null;
				}
				controls.Clear();
			}
		}

		public static bool LoadContentBindingValues(IUiInspector inspector, TargetControl bindingSource, string bindingNameKey)
		{
			if (inspector == null || bindingSource == null) return false;
			if (!bindingSource.IsContentBindingSource) return false;

			object target = inspector.ControlTarget;

			// If the given name key is null, assume it needs to be retrieved from binding source first:
			if (bindingNameKey == null)
			{
				if (!bindingSource.GetType(out Type bindSrcType) || !TypeTools.IsTextType(bindSrcType)) return false;
				if (!bindingSource.GetValue(target, out object bindSrcValue)) return false;
				if (!TypeTools.GetTextTypeToString(bindSrcValue, out bindingNameKey)) return false;
			}

			// Fetch the actual content using the sourced name key and using whichever binding method was requested:
			object content = null;
			bool contentWasLoaded = false;
			switch (bindingSource.contentBindingTargets[0].bindingType)
			{
				case UiControlContentBinding.LoadFromDatabase:
					ContentLoadResult loadResult = ContentLoader.LoadContent(bindingNameKey);
					contentWasLoaded = loadResult.IsFailure;
					content = loadResult.content;
					break;
				case UiControlContentBinding.CreateDefault:
					// TODO: Create a blank instance of the targets' value type. (typically once needed to ensure a control's value is garanteed non-null)
					break;
				default:
					break;
			}

			int successCounter = 0;
			foreach (ContentBindingHandle bindingHandle in bindingSource.contentBindingTargets)
			{
				TargetControl tc = bindingHandle.bindingTarget;

				// Briefly verify the loaded content's type:
				if (!tc.GetType(out Type tcType) && tcType == null) continue;
				if (tcType.IsValueType)
				{
					if (content == null || content.GetType() != tcType) continue;
				}
				else
				{
					if (content != null && !content.GetType().IsAssignableFrom(tcType)) continue;
				}

				// Try assigning the loaded content to the target's member:
				if (tc.SetValue(target, content))
				{
					if (tc.control != null) successCounter++;
				}
			}
			return contentWasLoaded && successCounter == bindingSource.contentBindingTargets.Count;
		}

		//TEST/TEMP
		private static void TESTTraceBinding(IUiInspector insp, TargetControl src)
		{
			if (insp == null || src == null) return;

			IUiInspector current = insp;
			string txt = $"CTRL: {src.Name}\tINSP";
			while (current != null)
			{
				string inspName = current is UiControlSubInspector subInsp ? subInsp.controlName : "Root";
				txt += $" => {inspName}";
				current = current.Host;
			}
			Debug.Log($"TEST: {txt}");
		}
		//TEST/TEMP

		private static void InitializeContentBinding(IUiInspector inspector, object target, TargetControl tc, List<TargetControl> controls)
		{
			if (inspector == null || target == null || controls == null) return;

			// Try fetching the binding source from a local target control via the underlying member's name:
			TargetControl bindingSource = controls.Find(o => string.Compare(o.Name, tc.setup.contentNameSource, true) == 0);
			if (bindingSource == null)
			{
				Debug.Log($"[UiInspector] Error! No binding source found for control '{tc.Name}' of inspector '{(inspector is UiControlSubInspector si ? si.controlName : "Root")}'");
				return;
			}

			// Only text-based types are a valid binding name source, as their values may be used as keys for data retrieval:
			if (!bindingSource.GetType(out Type srcType) || !TypeTools.IsTextType(srcType))
			{
				Debug.LogError($"[UiInspector] Error! Only text type target controls may be used as content binding name sources! Found {srcType?.ToString() ?? "null"}");
				return;
			}

			// Add this field as a content binding target to the source if that hasn't been done yet:
			bindingSource.AddContentBindingTarget(tc);

			// Skip any targets that already have a bound object reference loaded and set:
			if (tc.GetValue(target, out object currentValue) && currentValue != null) return;

			// Fetch the content name key from the above source:
			if (!bindingSource.GetValue(target, out object nameObj))
			{
				Debug.LogError($"[UiInspector] Error! Failed to get name key from content name source '{tc.setup.contentNameSource}'!");
				return;
			}
			if (!TypeTools.GetTextTypeToString(nameObj, out string nameKey) || string.IsNullOrWhiteSpace(nameKey))
			{
				Debug.Log($"[UiInspector] Error! No name key can be read for binding source '{tc.Name}' of inspector '{(inspector is UiControlSubInspector si ? si.controlName : "Root")}'");
				return;
			}

			// Load actual contents:
			LoadContentBindingValues(inspector, bindingSource, nameKey);
		}

		private static void PopulateControls_Object(IUiInspector inspector, object target, Type targetType, List<TargetControl> controls)
		{
			if (inspector == null || target == null || targetType == null || controls == null) return;

			// Gather all public fields:
			FieldInfo[] fields = targetType.GetFields();
			if (fields != null)
			{
				foreach (FieldInfo field in fields)
				{
					controls.Add(new TargetControl(field, target));
				}
			}
			// Gather all public getter-setter properties:
			PropertyInfo[] properties = targetType.GetProperties();
			if (fields != null)
			{
				foreach (PropertyInfo prop in properties)
				{
					if (prop.CanRead && prop.CanWrite)
					{
						controls.Add(new TargetControl(prop));
					}
				}
			}

			// Iterate over all control targets we found and preload any database contents here:
			foreach (TargetControl tc in controls)
			{
				if (tc.fieldInfo == null && tc.propInfo == null) continue;

				// Skip any targets with normal localized content:
				UiControlSetup setup = tc.setup;
				if (setup.contentBinding == UiControlContentBinding.None || string.IsNullOrWhiteSpace(setup.contentNameSource)) continue;

				// Initialize content bindings and try loading a value for this control:
				InitializeContentBinding(inspector, inspector.ControlTarget, tc, controls);
			}
		}

		private static void PopulateControls_List(IUiInspector inspector, IList list, List<TargetControl> controls)
		{
			if (inspector == null || list == null || controls == null) return;

			// Create a control for each item in the list:
			for (int i = 0; i < list.Count; ++i)
			{
				object itemObj = list[i];
				if (itemObj == null) continue;

				// Add a target control for each non-null item in the list:
				controls.Add(new TargetControl(list, i));
			}
		}

		public static void RebuildContents(IUiInspector inspector, ref List<TargetControl> controls)
		{
			if (inspector == null) return;
			if (controls == null) controls = new List<TargetControl>();

			// Purge all previous contents and controls:
			ClearContents(controls);

			// Stop right there if the target is null:
			object target = inspector.ControlTarget;

			// Populate controls for the inspector's target:
			switch (inspector.InspectorMode)
			{
				case UiInspectorMode.IList:
					PopulateControls_List(inspector, target as IList, controls);
					break;
				default:
					PopulateControls_Object(inspector, target, inspector.ControlTargetType, controls);
					break;
			}

			// Create the actual UI controls representing the members we just gathered:
			foreach (TargetControl tc in controls)
			{
				if (!tc.GetType(out Type tcType)) continue;

				// Using the control's type, find the right control for this type:
				UiControl prefab = inspector.RootHost.GetControlPrefabForType(tcType, tc);
				if (prefab != null)
				{
					// Instantiate the control's prefab:
					GameObject newControlGO = Instantiate<GameObject>(prefab.gameObject, inspector.ContentParent);
					tc.control = newControlGO.GetComponent<UiControl>();
					tc.control.controlName = tc.GetDisplayName();
					tc.control.controlLevel = tc.setup.level;
					tc.control.host = inspector;

					// Do some type-specific initialization:
					if (tc.control is UiControlNumber cn) cn.SetDataTypeFromSystemType(tcType);
					if (tc.control is IUiSubInspector subInspector) subInspector.Initialize(inspector.RootHost, inspector, inspector.ControlTarget, tc);
					//...
				}
				else Debug.Log($"[UiInspector] Warning: No prefab found for control '{tc.Name}' of type '{tcType}'!");
				// ^Note: The actual value and contents of the control will be set later, no need to do that here.
			}

			// Purge null or unassigned controls:
			controls.RemoveAll(o => o == null || o.control == null);

			// Not that all controls are in place, display the target's attributes/values:
			UpdateContents(inspector, ref controls);
		}

		private UiControl GetControlPrefabForType(Type type, TargetControl tc)
		{
			if (type == null || tc == null || prefabs == null) return null;

			// Don't generate any controls for hidden members:
			if (tc.setup.level == UiControlLevel.DontShow) return null;

			// Select a target depending on the given type:
			TargetTypes target = TargetTypes.Custom;
			if (TypeTools.IsScalarType(type)) target = TargetTypes.Scalar;
			else if (TypeTools.IsTextType(type)) target = TargetTypes.Text;
			else if (TypeTools.IsVectorType(type, out int dims)) target = TargetTypes.Vectors;
			else if (TypeTools.IsColorType(type)) target = TargetTypes.Color;
			else if (type == typeof(DateTime)) target = TargetTypes.DateTime;
			else if (type.IsAssignableFrom(typeof(IList))) target = TargetTypes.IList;
			else if (tc.type == TargetControl.TCType.ContentAccessor) target = TargetTypes.ContentAccessor;
			//...
			else if (type.IsClass) target = TargetTypes.NestedObject;

			// Search our prefab array for a fitting target/type match:
			UiControl prefab = null;
			foreach (ControlPrefab cp in prefabs)
			{
				if (cp.targetTypes == target)
				{
					bool isCompatible = false;
					if (target != TargetTypes.Custom) isCompatible = true;
					else
					{
						if (cp.type != null && cp.type == type) isCompatible = true;
						else if (cp.type == null && string.Compare(cp.typeName, type.Name, StringComparison.InvariantCultureIgnoreCase) == 0) isCompatible = true;
					}
					if (isCompatible)
					{
						prefab = cp.prefab;
						if ((tc.setup.level & cp.controlLevel) != 0) break;
					}
				}
			}
			return prefab;
		}

		public static void UpdateContents(IUiInspector inspector, ref List<TargetControl> controls)
		{
			if (inspector == null) return;

			object target = inspector.ControlTarget;
			if (target == null) return;

			// Target is non-null but no controls have been set? Rebuild controls now:
			if (controls == null)
			{
				RebuildContents(inspector, ref controls);
				return;
			}

			// Try fetching values from properties and fields and passing these to the controls:
			try
			{
				foreach (TargetControl tc in controls)
				{
					if (tc == null || tc.control == null) continue;
					if (tc.GetValue(target, out object tcValue))
					{
						tc.control.SetValue(tcValue);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"[UiInspector] ERROR! An exception was caught while trying to update inspector's UI controls!\nException message: {ex.Message}");
				return;
			}
		}

		public bool NotifyControlChanged(UiControl control)
		{
			if (control == null || controls == null) return false;

			TargetControl tc = controls.Find(o => o.control == control);
			if (tc == null)
			{
				Debug.LogError($"[UiInspector] Error! Inspector with target '{target}' of type '{targetType}' does not contain control '{control.controlName}'!");
				return false;
			}

			// Try setting the property/field values on target after reading it from UI control:
			try
			{
				bool success = tc.SetValue(target, control.RawValue);
				if (success && tc.IsContentBindingSource)
				{
					LoadContentBindingValues(this, tc, null);
				}
				return success;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[UiInspector] ERROR! An exception was caught while trying to set a control's value to target member!\nException message: {ex.Message}");
				return false;
			}
		}

		public float CalculateTotalHeight()
		{
			float totalHeight = baseHeight;
			if (controls != null)
			{
				foreach (TargetControl tc in controls)
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

		public UiEnvironment GetEnvironment()
		{
			return hostPane?.GetEnvironment();
		}

		#endregion
	}
}
