using System;

public enum TargetTypes
{
	// Custom types, using custom controls:
	Custom,

	// Known value types:
	Scalar,
	Text,
	Vectors,
	Color,
	DateTime,

	// Sub-Inspector types:
	IList,
	NestedObject,
}
