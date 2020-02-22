using System;

namespace UI
{
	public interface IUiHostElement
	{
		#region Properties

		IUiHostElement HostElement { get; }

		#endregion
		#region Methods

		UiEnvironment GetEnvironment();

		#endregion
	}
}
