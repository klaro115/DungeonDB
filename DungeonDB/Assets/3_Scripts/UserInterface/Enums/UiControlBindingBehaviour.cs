using System;

namespace Content
{
	public enum UiControlBindingBehaviour
	{
		AlwaysShow			= 1,	// Always display the control, even if teh current content binding yielded no results.
		HideIfNoBinding		= 2,	// Hide this control whenever no content could be loaded, unhide if the binding yields a valid result.
	}
}
