using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class NodeEditPopup : EditorWindow
	{
		public static NodeEditPopup instance;

		private static NodeEditorObject editing;
		private static string newname;
		private static string newdesc;
		
		private Event currentEvent;
		private bool mouseDown;

		public static void Init(NodeEditorObject node)
		{
			if (instance == null)
			{
				instance = ScriptableObject.CreateInstance<NodeEditPopup>();
				instance.position = new Rect(
					GUIUtility.GUIToScreenPoint(Event.current.mousePosition),
					new Vector2(250, 150));
				instance.ShowPopup();
			}
			else
			{
				instance.Repaint();
			}

			editing = node;
			newname = editing.displayName;
			newdesc = editing.description;
		}

		public void Hide()
		{
			this.Close();
			instance = null;
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Name:");
			newname = EditorGUILayout.TextField(newname);

			EditorGUILayout.LabelField("Name:");
			newdesc = EditorGUILayout.TextField(newdesc);

			if (GUILayout.Button("OK"))
			{
				editing.displayName = newname;
				editing.description = newdesc;
				Hide();
			}
			if (GUILayout.Button("Cancel"))
			{
				Hide();
			}
		}
	}
}