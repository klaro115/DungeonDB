using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class UiControlLevelSpec : System.Attribute
{
	#region Constructors

	public UiControlLevelSpec(UiControlLevel _levels)
	{
		levels = _levels;
	}

	#endregion
	#region Fields

	public UiControlLevel levels;

	#endregion
}
