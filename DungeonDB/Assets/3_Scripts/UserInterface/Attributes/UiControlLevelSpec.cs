using System;
using UI;

namespace Content
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class UiControlLevelSpec : System.Attribute
	{
		#region Constructors

		public UiControlLevelSpec(UiControlLevel _levels)
		{
			levels = _levels;
			contentBinding = UiControlContentBinding.None;
			contentBindingSourceName = null;
			bindingBehaviour = UiControlBindingBehaviour.AlwaysShow;
		}
		public UiControlLevelSpec(UiControlLevel _levels,
			string _contentBindingSrcName,
			UiControlContentBinding _contentBinding = UiControlContentBinding.LoadFromDatabase,
			UiControlBindingBehaviour _bindingBehaviour = UiControlBindingBehaviour.HideIfNoBinding)
		{
			levels = _levels;
			contentBinding = _contentBinding;
			contentBindingSourceName = _contentBindingSrcName;
			bindingBehaviour = _bindingBehaviour;
		}

		#endregion
		#region Fields

		public UiControlLevel levels;
		public UiControlContentBinding contentBinding = UiControlContentBinding.None;
		public string contentBindingSourceName = null;
		public UiControlBindingBehaviour bindingBehaviour = UiControlBindingBehaviour.AlwaysShow;

		#endregion
		#region Properties

		public UiControlSetup Setup => new UiControlSetup(levels, contentBinding, contentBinding == UiControlContentBinding.None ? null : contentBindingSourceName);

		#endregion
	}
}
