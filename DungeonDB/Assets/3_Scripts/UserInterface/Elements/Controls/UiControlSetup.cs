using System;

namespace UI
{
	public class UiControlSetup
	{
		#region Constructors

		public UiControlSetup() { }

		public UiControlSetup(UiControlLevel _level, UiControlContentBinding _contentBinding = UiControlContentBinding.None, string _contentNameSrc = null)
		{
			level = _level;
			contentBinding = _contentBinding;
			contentNameSource = _contentNameSrc;
		}

		#endregion
		#region Fields

		public UiControlLevel level = UiControlLevel.Any;
		public UiControlContentBinding contentBinding = UiControlContentBinding.None;
		public string contentNameSource = null;

		#endregion
	}
}
