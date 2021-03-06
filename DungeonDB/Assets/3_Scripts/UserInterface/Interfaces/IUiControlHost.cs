﻿using System;

namespace UI
{
	public interface IUiControlHost
	{
		#region Methods

		object ControlTarget { get; }
		Type ControlTargetType { get; }

		bool NotifyControlChanged(UiControl control);

		#endregion
	}
}
