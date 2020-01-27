using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class UiControlLevelSpec : System.Attribute
{
	#region Constructors

	public UiControlLevelSpec(UiControlLevel _levels)
	{
		levels = _levels;
		contentBinding = UiControlContentBinding.None;
		contentBindingSourceName = null;
	}
	public UiControlLevelSpec(UiControlLevel _levels, UiControlContentBinding _contentBinding, string _contentBindingSrcName)
	{
		levels = _levels;
		contentBinding = _contentBinding;
		contentBindingSourceName = _contentBindingSrcName;
	}

	#endregion
	#region Fields

	public UiControlLevel levels;
	public UiControlContentBinding contentBinding = UiControlContentBinding.None;
	public string contentBindingSourceName = null;

	#endregion
	#region Properties

	public UiControlSetup Setup => new UiControlSetup(levels, contentBinding, contentBinding == UiControlContentBinding.None ? null : contentBindingSourceName);

	#endregion
}
