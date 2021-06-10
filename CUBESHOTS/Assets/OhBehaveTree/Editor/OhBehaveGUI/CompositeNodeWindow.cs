using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public abstract class CompositeNodeWindow : NodeWindow, IParentNodeWindow
	{
		private ReorderableList childNodesReorderable;

		public CompositeNodeWindow(NodeEditorObject node) : base(node) { }


		public override bool ProcessEvents(Event e)
		{
			if (inPoint != null)
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

				EditorGUILayout.BeginHorizontal();
				{
					NodeType newType = (NodeType)EditorGUILayout.EnumPopup(nodeObject.nodeType);
					if (newType != nodeObject.nodeType)
					{
						nodeObject.ChangeNodeType(newType);
					}

					bool newRandom = GUILayout.Toggle(nodeObject.isRandom, "Random");
					if (newRandom != nodeObject.isRandom)
					{
						treeBlueprint.save = true;
						nodeObject.isRandom = newRandom;
					}
				}
				EditorGUILayout.EndHorizontal();


				if (childNodesReorderable != null)
				{
					//try
					//{
						childNodesReorderable.DoLayoutList();
					//}
					//catch (System.Exception)
					//{// only happens on Repaint after deleting a node. I think we can safely ignore it.
					//}
				}
				else if (Event.current.type == EventType.Repaint)
				{
					CreateChildList();
				}

				if (Event.current.type == EventType.Repaint)
				{
					Rect lastrect = GUILayoutUtility.GetLastRect();
					nodeObject.windowRect.height = lastrect.yMax;
				}
			}
			GUILayout.EndArea();

			GUI.backgroundColor = clr;

			if (inPoint != null) // only a root node would not have an inpoint
				inPoint.OnGUI();
			outPoint.OnGUI();
		}


		public override void DrawConnectionWires()
		{
			if (outPoint.DrawConnectionTo(GetChildren(), out int[] newChildOrder))
			{
				nodeObject.NewChildOrder(newChildOrder);
			}
		}


		public override void UpdateChildrenList()
		{
			CreateChildList();
		}


		public void CreateChildConnection(NodeWindow newChild)
		{
			newChild.SetParentWindow(this);
		}


		private void CreateChildList()
		{
			if (nodeObject.HasChildren())
			{
				var children = nodeObject.GetChildren();
				if (children.Count == 0)
				{
					return;
				}

				ReorderableItem[] nodeItems = new ReorderableItem[children.Count];
				for (int i = 0; i < children.Count; ++i)
				{
					var node = treeBlueprint.GetNodeObjectByIndex(children[i]);
					if (node == null)
					{
						Debug.LogError("Missing child error");
						nodeItems[i] = new ReorderableItem(OhBehaveTreeBlueprint.MISSING_INDEX, "MISSING CHILD");
					}
					else
					{
						nodeItems[i] = new ReorderableItem(node.index, node.displayName);
					}

				}

				childNodesReorderable = new ReorderableList(nodeItems, typeof(ReorderableItem), true, true, false, false);
				childNodesReorderable.onReorderCallback = ChildrenReordered;
				childNodesReorderable.drawElementCallback = DrawListItem;
				childNodesReorderable.drawHeaderCallback = DrawHeader;
				GUI.changed = true;
			}
			else
				childNodesReorderable = new ReorderableList(new List<int>(), typeof(int), false, true, false, false);
		}

		private void ChildrenReordered(ReorderableList newOrderList)
		{
			List<int> orgOrder = nodeObject.GetChildren();
			List<int> newOrder = new List<int>();
			for (int i = 0; i < newOrderList.count; ++i)
			{
				newOrder.Add(((ReorderableItem)newOrderList.list[i]).index);
			}

			nodeObject.ReorderPhysicalChildren(newOrder);
		}

		private void DrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, nodeObject.nodeType == NodeType.Sequence ? "Execute All" : "Execute until Success");
		}

		private void DrawListItem(Rect rect, int index, bool isActive, bool isFocused)
		{
			ReorderableItem item = (ReorderableItem)childNodesReorderable.list[index];
			EditorGUI.LabelField(rect, new GUIContent(item.displayName + " (index: " + item.index + ")"));
		}
	}

	[System.Serializable]
	public class ReorderableItem
	{
		public int index;
		public string displayName;

		public ReorderableItem(int index, string displayName)
		{
			this.index = index;
			this.displayName = displayName;
		}
	}
}