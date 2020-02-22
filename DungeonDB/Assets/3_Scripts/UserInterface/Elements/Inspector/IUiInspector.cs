using System;
using UnityEngine;
using UI.Inspector;

namespace UI
{
	public interface IUiInspector : IUiControlHost, IUiHostElement
	{
		#region Properties

		UiInspector RootHost { get; }
		IUiInspector Host { get; }
		RectTransform ContentParent { get; }

		int ControlCount { get; }
		UiInspectorMode InspectorMode { get; }

		#endregion
	}
}
