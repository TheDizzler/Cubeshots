using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public static class ConnectionControls
	{
		public static readonly Vector2 lineOffset = new Vector2(0, 40);
		public static readonly float lineThickness = 8;

		private static ConnectionPoint selectedInPoint;
		private static ConnectionPoint selectedOutPoint;


		internal static void OnClickInPoint(ConnectionPoint inPoint)
		{
			selectedInPoint = inPoint;
			if (selectedOutPoint != null)
			{
				if (selectedOutPoint.nodeWindow != selectedInPoint.nodeWindow)
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		internal static void OnClickOutPoint(ConnectionPoint outPoint)
		{
			selectedOutPoint = outPoint;

			if (selectedInPoint != null)
			{
				if (selectedOutPoint.nodeWindow != selectedInPoint.nodeWindow)
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		private static void CreateConnection()
		{
			((CompositeNodeWindow)selectedOutPoint.nodeWindow).CreateChildConnection(selectedInPoint.nodeWindow);
		}

		private static void ClearConnectionSelection()
		{
			selectedInPoint = null;
			selectedOutPoint = null;
		}
	}
}