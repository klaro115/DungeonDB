using System;

public enum UiControlContentBinding
{
	None,				// No automated/dynamic content binding, just use local value or populate using default value.

	LoadFromDatabase,	// Load the content from main database using a given content name as content key.
	//...
}
