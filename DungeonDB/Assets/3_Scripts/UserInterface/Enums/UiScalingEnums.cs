using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum UiScalingMode
{
	Fixed,
	AdjustWidth,
	AdjustAnchors,
	AdjustPadding,
}

public enum UiScalingRole
{
	LeftMaster,
	CenterSlave,
	RightMaster
}

public enum UiScalingCoordinator
{
	Command,
	LeftMaster,
	RightMaster,
}
