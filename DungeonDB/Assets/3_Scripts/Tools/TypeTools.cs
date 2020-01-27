using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TypeTools
{
	#region Fields

	private static StringBuilder textBuilder = null;

	#endregion
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
		return type != null && (type == typeof(float) || type == typeof(double));
	}
	public static bool IsScalarType(Type type)
	{
		return IsIntegerType(type) || IsFloatType(type);
	}

	public static bool IsVectorType(Type type, out int outDimensions)
	{
		outDimensions = 0;
		if (type == null) return false;

		if (type == typeof(Vector2) || type == typeof(Vector2Int)) outDimensions = 2;
		else if (type == typeof(Vector3) || type == typeof(Vector3Int)) outDimensions = 3;
		else if (type == typeof(Vector4)) outDimensions = 4;

		return outDimensions > 1;
	}

	public static bool IsTextType(Type type)
	{
		return type != null && (type == typeof(string) || type == typeof(StringBuilder) || ImplementsInterface(type, typeof(ICollection<char>)));
	}

	public static bool IsColorType(Type type)
	{
		return type != null && (type == typeof(Color) || type == typeof(Color32));
	}

	public static bool ImplementsInterface(Type type, Type interfaceType)
	{
		if (type == null || interfaceType == null) return false;
		return type == interfaceType || interfaceType.IsAssignableFrom(type);
	}

	public static int[] GetTypeDimensions(Type type)
	{
		if (type == null) return null;

		if (type == typeof(Vector2) || type == typeof(Vector2Int)) return new int[1] { 1 };
		if (type == typeof(Vector3) || type == typeof(Vector3Int)) return new int[2] { 1, 1 };
		if (type == typeof(Vector4)) return new int[3] { 1, 1, 1 };
		if (type == typeof(Matrix4x4)) return new int[4] { 4, 4, 4, 4 };
		if (typeof(IEnumerable).IsAssignableFrom(type))
		{
			return new int[1] { -1 };
		}

		return new int[1] { 1 };
	}
	public static int[] GetObjectDimensions(object obj)
	{
		if (obj == null) return null;

		Type type = obj.GetType();
		int[] dimensions = GetTypeDimensions(type);
		if (dimensions == null) return null;
		if (dimensions.Length > 1 || dimensions[0] >= 0) return dimensions;

		if (obj is string txt) dimensions[0] = txt.Length;
		else if (obj is StringBuilder builder) dimensions[0] = builder.Length;
		else if (obj is Array a)
		{
			dimensions = new int[a.Rank];
			for (int i = 0; i < dimensions.Length; ++i) dimensions[i] = a.GetUpperBound(i) + 1;
		}
		else if (obj is ICollection coll) dimensions[0] = coll.Count;
		else if (obj is IEnumerable ie)
		{
			int counter = 0;
			foreach (IEnumerator i in ie) counter++;
			dimensions[0] = counter;
		}
		return dimensions;
	}

	public static bool GetTextTypeToString(object obj, out string outTxt)
	{
		outTxt = null;
		if (obj == null) return false;

		if (obj is string objTxt) outTxt = objTxt;
		else if (obj is StringBuilder builder) outTxt = builder.ToString();
		else if (obj.GetType().IsAssignableFrom(typeof(ICollection<char>)))
		{
			ICollection<char> coll = (ICollection<char>)obj;
			if (textBuilder == null) textBuilder = new StringBuilder(coll.Count);
			else textBuilder.Clear();
			foreach (char c in coll)
				textBuilder.Append(c);
			outTxt = textBuilder.ToString();
		}
		else return false;
		return true;
	}

	#endregion
}
