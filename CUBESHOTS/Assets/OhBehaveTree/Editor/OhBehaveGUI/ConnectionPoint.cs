using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public enum ConnectionPointType { In, Out }

	/// <summary>
	/// This class is for UI logic with the visual representation of the node only.
	/// A connection point should have no knowledge of the points connected to it,
	/// only the window it is attached to.
	/// </summary>
	public class ConnectionPoint
	{
		public Rect rect;
		public ConnectionPointType type;
		public NodeWindow nodeWindow;
		public Action<ConnectionPoint> OnClickConnectionPoint;
		public bool isCreatingNewConnection;

		private GUIStyle unConnectedstyle;
		private GUIStyle connectedStyle;
		private OhBehaveTreeBlueprint blueprint;
		private bool isHovering;
		private Color hoverBGColor;
		private bool isConnected = false;


		public ConnectionPoint(NodeWindow node, ConnectionPointType type, Action<ConnectionPoint> OnClickConnectionPoint)
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			blueprint = ohBehave.treeBlueprint;
			this.nodeWindow = node;
			this.type = type;
			if (type == ConnectionPointType.In)
				unConnectedstyle = OhBehaveEditorWindow.InPointStyle;
			else
				unConnectedstyle = OhBehaveEditorWindow.OutPointStyle;
			this.OnClickConnectionPoint = OnClickConnectionPoint;

			connectedStyle = new GUIStyle();
			connectedStyle.normal.background = unConnectedstyle.hover.background;

			rect = new Rect(0, 0, unConnectedstyle.normal.background.width, unConnectedstyle.normal.background.height);
		}

		public void ProcessEvents(Event e)
		{
			Rect windowRect = nodeWindow.GetRect();
			rect.x = windowRect.x + windowRect.width * .5f - rect.width * .5f;

			switch (type)
			{
				case ConnectionPointType.In:
					rect.y = windowRect.y - rect.height / 2;
					break;

				case ConnectionPointType.Out:
					rect.y = windowRect.y - rect.height / 2 + windowRect.height;
					break;
			}

			if (rect.Contains(e.mousePosition))
			{
				isHovering = true;

				if (blueprint.IsValidConnection(this))
				{
					hoverBGColor = Color.green;
				}
				else
				{
					hoverBGColor = Color.red;
				}

				if (e.button == 0)
				{
					if (e.type == EventType.MouseDown)
					{
						blueprint.StartPointSelected(this);
						isCreatingNewConnection = true;
						e.Use();
					}
					else if (e.type == EventType.MouseUp)
					{
						blueprint.EndPointSelected(this);
						e.Use();
					}
				}
				else if (e.button == 1)
				{
					if (e.type == EventType.MouseUp)
					{
						CreateContextMenu();
						e.Use();
					}
				}
			}
			else
				isHovering = false;
		}



		public bool DrawConnectionTo(List<int> childrenIndices, out int[] newChildOrder)
		{
			if (childrenIndices == null || childrenIndices.Count == 0)
			{
				newChildOrder = new int[0];
				return false;
			}

			newChildOrder = new int[childrenIndices.Count];

			isConnected = true;

			Vector2 lineStart = rect.center + ConnectionControls.lineOffset;
			Handles.DrawAAPolyLine(ConnectionControls.lineThickness, rect.center, lineStart);
			Vector2 furthestLeft = lineStart;
			Vector2 furthestRight = lineStart;

			Dictionary<float, int> nodeLocationIndex = new Dictionary<float, int>();
			List<float> order = new List<float>();
			int i = 0;
			foreach (int nodeIndex in childrenIndices)
			{
				if (nodeIndex == OhBehaveTreeBlueprint.ROOT_INDEX)
				{
					Debug.LogError("WTF: trying to draw a connection to the root.");
					continue;
				}
				if (blueprint.GetNodeObjectByIndex(nodeIndex) == null)
				{
					Debug.LogWarning("Child index " + nodeIndex + " invalid. Delayed removal?");
					continue;
				}

				ConnectionPoint otherPoint = blueprint.GetNodeObjectByIndex(nodeIndex).GetWindow().inPoint;
				otherPoint.isConnected = true;

				Vector2 downLineStart = new Vector2(otherPoint.rect.center.x, lineStart.y);
				if (downLineStart.x > furthestRight.x)
					furthestRight.x = downLineStart.x;
				else if (downLineStart.x < furthestLeft.x)
					furthestLeft.x = downLineStart.x;

				Handles.DrawAAPolyLine(ConnectionControls.lineThickness,
					downLineStart, otherPoint.rect.center);

				Handles.Label((downLineStart + otherPoint.rect.center) / 2, i.ToString(), EditorStyles.boldLabel);
				order.Add(downLineStart.x);
				nodeLocationIndex[downLineStart.x] = nodeIndex;
				++i;
			}

			Handles.DrawAAPolyLine(ConnectionControls.lineThickness,
				furthestLeft, furthestRight);

			if (Event.current.type == EventType.Repaint)
				return false;

			order.Sort();

			bool reordered = false;
			for (i = 0; i < order.Count; ++i)
			{
				newChildOrder[i] = nodeLocationIndex[order[i]];
				if (newChildOrder[i] != childrenIndices[i])
				{
					reordered = true;
				}
			}

			// this should be safe to delete. Keep it around for a little while just in case.
			for (i = 0; i < newChildOrder.Length; ++i)
			{
				for (int j = 0; j < newChildOrder.Length; ++j)
				{
					if (i == j)
						continue;
					if (newChildOrder[i] == newChildOrder[j])
					{
						Debug.LogWarning("found dups: " + Event.current.type + ". If was re-ording list, should be safe to ignore.");
						return false;
					}
				}
			}

			return reordered;
		}


		public void OnGUI()
		{
			Color clr = GUI.backgroundColor;
			if (isHovering || isCreatingNewConnection)
				GUI.backgroundColor = hoverBGColor;

			GUI.Label(rect, "", isConnected ? connectedStyle : unConnectedstyle);

			GUI.backgroundColor = clr;

			isConnected = false;
		}


		private void CreateContextMenu()
		{
			switch (type)
			{
				case ConnectionPointType.In:
					if (!nodeWindow.TryGetParentName(out string parentName))
					{
						blueprint.CreateParentContextMenu(nodeWindow.nodeObject, false);
						return;
					}

					var genericMenu = new GenericMenu();
					genericMenu.AddItem(new GUIContent("Remove Connection to " + parentName), false,
						() => DisconnectParent(nodeWindow.parent));
					genericMenu.ShowAsContext();
					break;

				case ConnectionPointType.Out:
					var children = nodeWindow.GetChildren();
					if (children != null && children.Count > 0)
					{
						var disconnectMenu = new GenericMenu();
						disconnectMenu.allowDuplicateNames = true;
						for (int i = 0; i < children.Count; ++i)
						{
							var childNode = blueprint.GetNodeObjectByIndex(children[i]);
							disconnectMenu.AddItem(new GUIContent(
								"Remove Connection to " + "(" + i + ") " + childNode.displayName),
								false, () => DisconnectChild(childNode));
						}

						disconnectMenu.ShowAsContext();
					}
					break;
			}
		}

		private void DisconnectChild(NodeEditorObject childNode)
		{
			NodeEditorObject.DisconnectNodes(nodeWindow.nodeObject, childNode);
		}

		/// <summary>
		/// Called when Remove Connection selected from context menu.
		/// </summary>
		private void DisconnectParent(IParentNodeWindow parent)
		{
			NodeEditorObject.DisconnectNodes(((NodeWindow)parent).nodeObject, nodeWindow.nodeObject);
		}
	}
}