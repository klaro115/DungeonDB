using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TypeTools
{
	#region Methods

	public static bool IsIntegerType(Type type)
	{
		if (type == null) return false;
		return type == typeof(byte) ||
			type == typeof(short) ||
			type == typeof(ushort) ||
			type == typeof(int) ||
			type == typeof(uint) ||
			type == typeof(long) ||
			type == typeof(ulong);
	}
	public static bool IsFloatType(Type type)
	{
		if (type == null) return false;
		return type == typeof(float) || type == typeof(double);
	}

	#endregion
}
