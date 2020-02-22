using System;
using System.Collections.Generic;
using UnityEngine;

using Content;

namespace Projects
{
	public static class ProjectManager
	{
		#region Fields

		private static Project activeProject = null;

		#endregion
		#region Properties

		public static Project ActiveProject => activeProject;
		public static string ProjectRootPath => activeProject?.rootPath ?? string.Empty;

		#endregion
		#region Methods

		public static bool LoadProject(Project project)
		{
			if (project == null)
			{
				Debug.LogError("[ProjectManager] Error! Cannot load and set as active a null project instance!");
				return false;
			}

			// If the new project is different from the previously active one, deactivate that first:
			if (activeProject != null && activeProject != project)
			{
				DeactivateProject();
			}

			// Activate and load contents of the new project:
			return ActivateProject(project);
		}

		private static bool ActivateProject(Project project)
		{
			if (project == null) return false;

			activeProject = project;
			activeProject.LoadBaseContents();

			return true;
		}

		private static void DeactivateProject()
		{
			if (activeProject == null) return;

			// TODO: [important] Unload all previous contents and reset content loaders!

			//...

			activeProject = null;
		}

		#endregion
	}
}
