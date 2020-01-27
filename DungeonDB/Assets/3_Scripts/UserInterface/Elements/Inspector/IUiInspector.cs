using System;
using System.Collections.Generic;
using UnityEngine;

public interface IUiInspector : IUiControlHost
{
	#region Properties

	UiInspector RootHost { get; }
	IUiInspector Host { get; }
	RectTransform ContentParent { get; }

	int ControlCount { get; }
	UiInspectorMode InspectorMode { get; }

	#endregion
}
