using System;

[Flags]
public enum UiControlLevel
{
	Any			= 0x7fffffff,

	Rough		= 1,
	Normal		= 2,
	Detailed	= 4,
}
