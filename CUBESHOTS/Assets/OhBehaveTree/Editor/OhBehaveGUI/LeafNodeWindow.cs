using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class LeafNodeWindow : NodeWindow
	{
		private GUIStyle foldoutStyle;

		public LeafNodeWindow(NodeEditorObject node) : base(node) { }


		public override bool ProcessEvents(Event e)
		{
			inPoint.ProcessEvents(e);
			bool saveNeeded = false;
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						LeftClick(e);
					}
					else if (e.button == 1)
					{
						RightClick(e);
					}
					break;
				case EventType.MouseUp:
					if (isDragged)
					{
						saveNeeded = true;
						e.Use();
					}
					isDragged = false;
					break;
				case EventType.MouseDrag:
					if (e.button == 0 && isDragged)
					{
						Drag(e.delta);
						e.Use();
					}
					break;
				case EventType.KeyDown:
					if (isSelected && e.keyCode == KeyCode.Delete)
					{
						treeBlueprint.DeleteNode(nodeObject);
					}
					break;
			}

			return saveNeeded;
		}


		public override void OnGUI()
		{
			if (refreshConnection)
			{
				RefreshConnection();
			}

			Color clr = GUI.backgroundColor;

			if (isSelected)
				GUI.backgroundColor = bgColor;

			var content = new GUIContent("Name: " + nodeName, nodeObject.description);


			GUILayout.BeginArea(GetRect(), content, currentStyle);
			{
				CreateTitleBar();

				NodeType newType = (NodeType)EditorGUILayout.EnumPopup(nodeObject.nodeType);
				if (newType != nodeObject.nodeType)
				{
					nodeObject.ChangeNodeType(newType);
				}

				if (!isValid)
				{
					foldoutStyle = OhBehaveEditorWindow.invalidFoldoutStyle;
				}
				else
					foldoutStyle = OhBehaveEditorWindow.normalFoldoutStyle;

				bool newExpanded = EditorGUILayout.Foldout(nodeObject.isExpanded, new GUIContent("Actions"), true, foldoutStyle);
				if (newExpanded != nodeObject.isExpanded)
				{
					GUI.changed = true;
					nodeObject.isExpanded = newExpanded;
				}


				if (newExpanded)
				{
					GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
					if (!isValid)
						labelStyle.normal.textColor = Color.red;

					EditorGUILayout.LabelField("What do actions?");
					if (string.IsNullOrEmpty(nodeObject.actionName))
					{
						EditorGUILayout.LabelField("Action:", labelStyle);
						EditorGUILayout.LabelField("\tNo Methods Set", labelStyle);
					}
					else
					{
						EditorGUILayout.LabelField("Action:");
						EditorGUILayout.LabelField("\t" + nodeObject.actionName);
					}
				}

				if (Event.current.type == EventType.Repaint)
				{
					Rect lastrect = GUILayoutUtility.GetLastRect();
					nodeObject.windowRect.height = lastrect.yMax + 10;
				}
			}
			GUILayout.EndArea();

			GUI.backgroundColor = clr;

			inPoint.OnGUI();
		}

		public override void UpdateChildrenList()
		{
			Debug.LogError("Leaf nodes should NOT have children!");
		}
	}
}