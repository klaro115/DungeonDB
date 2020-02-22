using System;

namespace UI.Inspector
{
	[Serializable]
	public class ContentBindingHandle
	{
		public ContentBindingHandle(TargetControl _bindingTarget, UiControlContentBinding _bindingType)
		{
			bindingTarget = _bindingTarget;
			bindingType = _bindingType;
		}

		[NonSerialized]
		public TargetControl bindingTarget = null;
		public UiControlContentBinding bindingType = UiControlContentBinding.None;

		public override string ToString()
		{
			return bindingTarget != null ? $"{bindingTarget.Name} ({bindingType})" : "<missing>";
		}
	}
}
