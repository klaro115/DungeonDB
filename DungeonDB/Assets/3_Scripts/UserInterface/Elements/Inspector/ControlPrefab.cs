using System;

namespace UI.Inspector
{
	[Serializable]
	public class ControlPrefab
	{
		#region Constructors

		public ControlPrefab(TargetTypes _targetTypes, UiControl _prefab = null, Type _type = null, UiControlLevel _controlLevel = UiControlLevel.Any)
		{
			targetTypes = _targetTypes;
			controlLevel = _controlLevel;
			typeName = string.Empty;
			prefab = _prefab;
			type = _type;
		}
		public ControlPrefab(string _typeName, UiControl _prefab = null, UiControlLevel _controlLevel = UiControlLevel.Any)
		{
			targetTypes = TargetTypes.Custom;
			controlLevel = _controlLevel;
			typeName = _typeName;
			prefab = _prefab;
			type = null;
		}

		#endregion
		#region Fields

		public TargetTypes targetTypes = TargetTypes.Custom;
		public UiControlLevel controlLevel = UiControlLevel.Any;
		public string typeName = string.Empty;
		public UiControl prefab = null;
		public Type type = null;

		#endregion
	}
}
