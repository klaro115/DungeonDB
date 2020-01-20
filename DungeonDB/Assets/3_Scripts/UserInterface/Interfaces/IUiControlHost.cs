using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IUiControlHost
{
	#region Methods

	bool NotifyControlChanged(UiControl control);

	#endregion
}
