using System;

namespace Content
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class UiControlDisplaySpec : System.Attribute
	{
		#region Fields

		public string displayName = "<name>";

		#endregion
	}
}
