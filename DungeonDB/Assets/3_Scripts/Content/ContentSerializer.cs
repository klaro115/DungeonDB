using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

namespace Content
{
	public static class ContentSerializer
	{
		#region Methods

		/************************************************************************************************/
		// FILE ACCESS:

		public static bool ReadTextFile(string filePath, out string txt)
		{
			txt = null;
			if (string.IsNullOrEmpty(filePath)) return false;
			if (!File.Exists(filePath))
			{
				Debug.LogError($"[ContentSerializer] Error! The file at path '{filePath}' does not exist!");
				return false;
			}

			try
			{
				txt = File.ReadAllText(filePath);
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ContentSerializer] ERROR! An exception was met when trying to read text contents from file at path '{filePath}'!\nException message: {ex.Message}");
				txt = null;
				return false;
			}
			return true;
		}
		public static bool WriteTextFile(string filePath, string txt)
		{
			if (string.IsNullOrEmpty(filePath) || txt == null) return false;

			try
			{
				File.WriteAllText(filePath, txt);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ContentSerializer] ERROR! An exception was met when trying to write text contents to file at path '{filePath}'!\nException message: {ex.Message}");
				return false;
			}
		}

		public static bool ReadBinaryFile(string filePath, out byte[] bytes)
		{
			bytes = null;
			if (string.IsNullOrEmpty(filePath)) return false;
			if (!File.Exists(filePath))
			{
				Debug.LogError($"[ContentSerializer] Error! The file at path '{filePath}' does not exist!");
				return false;
			}

			try
			{
				bytes = File.ReadAllBytes(filePath);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ContentSerializer] ERROR! An exception was met when trying to read binary contents from file at path '{filePath}'!\nException message: {ex.Message}");
				bytes = null;
				return false;
			}
		}
		public static bool WriteBinaryFile(string filePath, byte[] bytes)
		{
			if (string.IsNullOrEmpty(filePath) || bytes == null) return false;
			
			try
			{
				File.WriteAllBytes(filePath, bytes);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ContentSerializer] ERROR! An exception was met when trying to write binary contents to file at path '{filePath}'!\nException message: {ex.Message}");
				return false;
			}
		}

		/************************************************************************************************/
		// SERIALIZATION & DESERIALIZATION:

		public static bool DeserializeJson(string jsonTxt, Type contentType, out object content)
		{
			content = null;
			if (jsonTxt == null || contentType == null) return false;

			// No json-formatted text? The object must be null:
			if (jsonTxt.Length == 0) return true;

			// Try deserializing object:
			try
			{
				content = JsonUtility.FromJson(jsonTxt, contentType);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ContentSerializer] ERROR! An exception was met when trying to deserialize type '{contentType}' from JSON!\nException message: {ex.Message}");
				return false;
			}
		}
		public static bool SerializeJson(object content, out string jsonTxt)
		{
			jsonTxt = null;
			if (content == null) return false;

			// Try serializing object:
			try
			{
				jsonTxt = JsonUtility.ToJson(content, true);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ContentSerializer] ERROR! An exception was met when trying to serialize type '{content.GetType()}' to JSON!\nException message: {ex.Message}");
				return false;
			}
		}

		public static bool DeserializeXml(string xmlTxt, Type contentType, out object content)
		{
			content = null;
			if (xmlTxt == null || contentType == null) return false;

			// No json-formatted text? The object must be null:
			if (xmlTxt.Length == 0) return true;

			// Try deserializing object:
			try
			{
				XmlSerializer serializer = new XmlSerializer(contentType);
				using (StringReader reader = new StringReader(xmlTxt))
				{
					content = serializer.Deserialize(reader);
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ContentSerializer] ERROR! An exception was met when trying to deserialize type '{contentType}' from JSON!\nException message: {ex.Message}");
				return false;
			}
		}

		public static bool DeserializeBinary(byte[] bytes, Type contentType, out object content)
		{
			content = null;
			if (bytes == null) return false;

			// No byte data? The object must be null:
			if (bytes.Length == 0) return true;

			// Try deserializing object:
			BinaryFormatter formatter = new BinaryFormatter();
			try
			{
				// Create a byte stream to perform binary formatting from:
				using (MemoryStream stream = new MemoryStream(bytes))
				{
					content = formatter.Deserialize(stream);
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[ContentSerializer] ERROR! An exception was met when trying to deserialize '{contentType}' from binary data!\nException message: {ex.Message}");
				return false;
			}
		}

		#endregion
	}
}
