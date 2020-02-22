using System;

namespace UI
{
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

		// Content accessors:
		ContentAccessor,

		// Sub-Inspector types:
		IList,
		NestedObject,
	}
}
