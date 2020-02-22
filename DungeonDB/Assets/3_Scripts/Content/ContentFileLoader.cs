using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

using Projects;

namespace Content
{
	public static class ContentFileLoader
	{
		#region Types

		public enum FileFormat
		{
			Json,
			Xml,
			Csv,
			Binary,

			None
		}
		public class FileFormatId
		{
			public FileFormatId(FileFormat _format, string _extension)
			{
				format = _format;
				extension = _extension;
			}

			public FileFormat format = FileFormat.Json;
			public string extension = null;
		}

		#endregion
		#region Fields

		private static readonly FileFormatId[] fileExtensions = new FileFormatId[]
		{
			new FileFormatId(FileFormat.Json, ".json"),
			new FileFormatId(FileFormat.Xml, ".xml"),
			new FileFormatId(FileFormat.Csv, ".csv"),
			new FileFormatId(FileFormat.Binary, ".bin"),
			new FileFormatId(FileFormat.Binary, ".data"),
		};
		
		private static readonly char[] pathSeparators = new char[2] { '/', '\\' };

		#endregion
		#region Methods

		private static string GetFileNameFromPath(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return string.Empty;

			int lastPartIndex = filePath.LastIndexOfAny(pathSeparators);
			if (lastPartIndex < 0) return filePath;

			return filePath.Substring(Math.Max(lastPartIndex + 1, filePath.Length - 1));
		}
		private static string GetFileExtension(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return string.Empty;

			int extIndex = filePath.LastIndexOf('.');
			if (extIndex < 0) return string.Empty;

			return filePath.Substring(extIndex);
		}

		private static bool VerifyContentReference(ContentReference contentRef, out FileFormat outFileFormat)
		{
			outFileFormat = FileFormat.None;
			if (contentRef == null) return false;
			
			// Verify the content's file path:
			if (string.IsNullOrWhiteSpace(contentRef.sourcePath))
			{
				Debug.LogError($"[ContentFileLoader] Error! Source path URI of content reference '{contentRef}' may not be null or blank!");
				return false;
			}

			string sourcePathExtension = GetFileExtension(contentRef.sourcePath)?.ToLowerInvariant();
			if (string.IsNullOrEmpty(sourcePathExtension))
			{
				Debug.LogError($"[ContentFileLoader] Error! Source path URI of content '{contentRef}' has no file extension, cannot determine type!\nPath: {contentRef.sourcePath}");
				return false;
			}
			foreach (FileFormatId fileExt in fileExtensions)
			{
				if (string.CompareOrdinal(fileExt.extension, sourcePathExtension) == 0)
				{
					outFileFormat = fileExt.format;
					break;
				}
			}
			return true;
		}

		public static string CreateSourcePath(ContentHandle handle)
		{
			if (handle == null || !handle.IsValid())
			{
				Debug.LogError("[ContentFileLoader] Error! Cannot create file source path from null or invalid content handle!");
				return string.Empty;
			}

			string rootPath = ProjectManager.ActiveProject?.rootPath ?? string.Empty;
			string subPath = ContentHelper.GetContentCategorySubPath(handle.category);
			string contentDirectory = Path.Combine(rootPath, subPath);

			FileFormat fileFormat = ProjectManager.ActiveProject.preferredFileFormat;
			string fileExtension = (from ext in fileExtensions where ext.format == fileFormat select ext.extension).FirstOrDefault();
			string fileName = $"{handle.nameKey}{fileExtension ?? string.Empty}";

			return Path.Combine(contentDirectory, fileName);
		}

		public static bool LoadContent(ContentReference contentRef, out ContentHandle outHandle)
		{
			outHandle = null;
			if (contentRef == null) return false;

			if (!VerifyContentReference(contentRef, out FileFormat sourceFileFormat)) return false;

			// Get the content's actual data type:
			Assembly assembly = Assembly.GetCallingAssembly();
			Type contentType = assembly.GetType(contentRef.contentTypeName, false, true);
			if (contentType == null)
			{
				Debug.LogError($"[ContentFileLoader] Error! Could not find any type of the name '{contentRef.contentTypeName}' for content reference '{contentRef}'!");
				return false;
			}

			// Read and deserialize content from whatever format:
			bool success = false;
			object content = null;
			switch (sourceFileFormat)
			{
				case FileFormat.Json:
					if (ContentSerializer.ReadTextFile(contentRef.sourcePath, out string contentJson))
						success = ContentSerializer.DeserializeJson(contentJson, contentType, out content);
					break;
				case FileFormat.Xml:
					if (ContentSerializer.ReadTextFile(contentRef.sourcePath, out string contentXml))
						success = ContentSerializer.DeserializeXml(contentXml, contentType, out content);
					break;
				case FileFormat.Csv:
					// TODO
					break;
				case FileFormat.Binary:
					if (ContentSerializer.ReadBinaryFile(contentRef.sourcePath, out byte[] contentBytes))
						success = ContentSerializer.DeserializeBinary(contentBytes, contentType, out content);
					break;
				default:
					Debug.LogError($"[ContentFileLoader] Error! Content source file has unidentified or unknown file format '{sourceFileFormat}'!");
					return false;
			}

			// On a failure, stop here:
			if (!success)
			{
				Debug.LogError($"[ContentFileLoader] Error! Failed to read and deserialize content at path '{contentRef.sourcePath}' ({sourceFileFormat})!");
				return false;
			}

			// Output loaded content and return success:
			outHandle = new ContentHandle(contentRef.nameKey, content);
			return true;
		}

		public static bool SaveContent(ContentReference contentRef, ContentHandle handle)
		{
			if (contentRef == null || handle == null)
			{
				Debug.LogError("[ContentFileLoader] Error! Cannot save content to file using null content reference or handle!");
				return false;
			}
			if (!VerifyContentReference(contentRef, out FileFormat fileFormat)) return false;

			// Serialize and write content to whatever format:
			bool success = false;
			switch (fileFormat)
			{
				case FileFormat.Json:
					if (ContentSerializer.SerializeJson(handle.content, out string jsonTxt))
						success = ContentSerializer.WriteTextFile(contentRef.sourcePath, jsonTxt);
					break;
				case FileFormat.Xml:
					if (ContentSerializer.SerializeXml(handle.content, out string xmlTxt))
						success = ContentSerializer.WriteTextFile(contentRef.sourcePath, xmlTxt);
					break;
				case FileFormat.Csv:
					// TODO
					break;
				case FileFormat.Binary:
					if (ContentSerializer.SerializeBinary(handle.content, out byte[] contentBytes))
						success = ContentSerializer.WriteBinaryFile(contentRef.sourcePath, contentBytes);
					break;
				default:
					Debug.LogError($"[ContentFileLoader] Error! Content source file has unidentified or unknown file format '{fileFormat}'!");
					return false;
			}

			// On a failure, print an error:
			if (!success)
			{
				Debug.LogError($"[ContentFileLoader] Error! Failed to serialize and write content at path '{contentRef.sourcePath}' ({fileFormat})!");
			}
			return success;
		}

		#endregion
	}
}
