using System;

namespace UI.Inspector
{
	interface IUiSubInspector : IUiInspector
	{
		#region Methods

		bool Initialize(UiInspector _rootHost, IUiInspector _host, object _hostTarget, TargetControl _control);

		#endregion
	}
}
