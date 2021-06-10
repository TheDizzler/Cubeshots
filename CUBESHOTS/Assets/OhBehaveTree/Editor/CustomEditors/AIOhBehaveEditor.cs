using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools.CustomEditors
{
	[CustomEditor(typeof(OhBehaveAI))]
	[CanEditMultipleObjects]
	public class AIOhBehaveEditor : Editor
	{
		private OhBehaveAI instance;



		public void OnEnable()
		{
			instance = (OhBehaveAI)target;
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", target, typeof(OhBehaveAI), false);
			if (string.IsNullOrEmpty(instance.jsonFilepath))
				EditorGUILayout.DelayedTextField("Tree file", "!NO FILE SELECTED!");
			else
				EditorGUILayout.DelayedTextField("Tree file", Path.GetFileName(instance.jsonFilepath));
			GUI.enabled = true;


			if (!string.IsNullOrEmpty(instance.jsonFilepath))
			{
				if (GUILayout.Button("Change json file"))
				{ // If FileChooser is opened here, will get EditorLayout Error.
					// make sure the right target is in focus? (might not be necessary)
					EditorWindow.GetWindow<OhBehaveEditorWindow>().OpenFileChooser(instance);
				}

				// do something here to verify tree is well-formed. If not, display angry button.
			}
			else
			{
				if (GUILayout.Button("No json file loaded"))
				{
					Debug.LogError("Not yet implemented");
				}
			}

			var windows = Resources.FindObjectsOfTypeAll<OhBehaveEditorWindow>();
			if (windows == null || windows.Length == 0)
			{
				if (GUILayout.Button("Open AIOhBehaveEditor"))
					EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(instance);
			}
			else
			{
				if (GUILayout.Button("Create New AI Tree"))
				{
					CreateNewJson();
				}
			}

			var actionSource = instance.GetComponent<OhBehaveActions>();
			//if (instance.sharedMethods != null && instance.sharedMethods.Count > 0)
			{
				// Create the dropdown in the inspector for the found methods
				EditorGUILayout.Popup("Action List", 0, instance.GetMethodNames());
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void CreateNewJson()
		{
			EditorWindow.GetWindow<OhBehaveEditorWindow>().OpenSaveFilePanel(instance);
		}
	}
}