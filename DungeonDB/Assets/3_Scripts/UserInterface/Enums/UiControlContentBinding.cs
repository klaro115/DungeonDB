using System;

public enum UiControlContentBinding
{
	None,				// No automated/dynamic content binding, just use local value or populate using default value.

	LoadFromDatabase,	// Load the content from main database using a given content name as content key.
	CreateDefault,		// Create the default value for the control's type; Type must have a parameter-less constructor.
	//...
}
