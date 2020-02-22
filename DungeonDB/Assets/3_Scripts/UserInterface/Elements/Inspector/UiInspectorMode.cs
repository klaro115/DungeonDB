using System;

namespace UI.Inspector
{
	public enum UiInspectorMode
	{
		IList,          // The subinspector depicts a list within the host inspector.
		NestedElement,  // The subinspector depicts a class type object nested in the host inspector's target.
	}
}
