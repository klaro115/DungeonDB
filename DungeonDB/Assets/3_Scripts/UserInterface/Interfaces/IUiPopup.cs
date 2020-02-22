using System;
namespace UI.Internal
{
	public interface IUiPopup
	{
		#region Properties

		bool IsActive { get; }

		#endregion
		#region Methods

		void Deactivate();

		#endregion
	}
}
