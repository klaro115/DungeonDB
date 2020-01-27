using System;

[Flags]
public enum UiControlLevel
{
	Any			= 0x7fffffff,	// A level that is suitable for all control of this type.
	DontShow	= 0,			// Don't generate any visible controls for this member.

	Broad		= 1,			// Needs a very broad and global control, for a larger scope or greater content volumes. (ex.: text editors; calendars)
	Normal		= 2,			// Your generic default control for this type. (ex.: short descriptions; meeting times)
	Detailed	= 4,			// Needs a very detailed and specific control, for a narrow scope and minute adjustments. (ex.: name fields; precision timers)
}
