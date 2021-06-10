using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class InverterNodeWindow : NodeWindow, IParentNodeWindow
	{
		public InverterNodeWindow(NodeEditorObject nodeObj) : base(nodeObj) { }

		public override bool ProcessEvents(Event e)
		{
			inPoint.ProcessEvents(e);
			outPoint.ProcessEvents(e);
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

				if (nodeObject.HasChildren() && nodeObject.GetChildren().Count == 1)
					GUILayout.Label("NOT " + treeBlueprint.GetNodeObjectByIndex(nodeObject.GetChildren()[0]).displayName);
				else
					GUILayout.Label("Dangeling Inverter");
				if (Event.current.type == EventType.Repaint)
				{
					Rect lastrect = GUILayoutUtility.GetLastRect();
					nodeObject.windowRect.height = lastrect.yMax + 10;
				}
			}
			GUILayout.EndArea();
			GUI.backgroundColor = clr;

			inPoint.OnGUI();
			outPoint.OnGUI();

			outPoint.DrawConnectionTo(GetChildren(), out int[] newChildOrder); // inverter can only have one child so we could simplify this
		}

		public override void UpdateChildrenList()
		{
			// I don't think anything special needs to be done
		}

		public void CreateChildConnection(NodeWindow newChild)
		{
			newChild.SetParentWindow(this);
		}
	}
}