using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class NodeStyle
	{
		public static Color RootColor = new Color(1, .65f, 1, .75f);
		public static Color SequenceColor = new Color(1, .92f, .016f, .75f);
		public static Color SelectorColor = new Color(1, .65f, 0, .75f);
		public static Color LeafColor = new Color(0, 1, 0, .75f);
		public static Color InverterColor = new Color(0, .75f, .16f, .75f);


		public static GUIStyle LeafLabelStyle
		{
			get
			{
				var leafLabelStyle = new GUIStyle();
				Texture2D tex = new Texture2D(2, 2);
				var fillColorArray = tex.GetPixels32();

				for (var i = 0; i < fillColorArray.Length; ++i)
				{
					fillColorArray[i] = Color.green;
				}

				tex.SetPixels32(fillColorArray);
				tex.Apply();
				leafLabelStyle.normal.background = tex;
				leafLabelStyle.alignment = TextAnchor.UpperCenter;

				return leafLabelStyle;
			}
		}

		public static GUIStyle InverterLabelStyle
		{
			get
			{
				var leafLabelStyle = new GUIStyle();
				Texture2D tex = new Texture2D(2, 2);
				var fillColorArray = tex.GetPixels32();

				for (var i = 0; i < fillColorArray.Length; ++i)
				{
					fillColorArray[i] = Color.red;
				}

				tex.SetPixels32(fillColorArray);
				tex.Apply();
				leafLabelStyle.normal.background = tex;
				leafLabelStyle.alignment = TextAnchor.UpperCenter;

				return leafLabelStyle;
			}
		}

		public static GUIStyle SequencerLabelStyle
		{
			get
			{
				var sequencerLabelStyle = new GUIStyle();
				Texture2D tex = new Texture2D(2, 2);
				var fillColorArray = tex.GetPixels32();

				for (var i = 0; i < fillColorArray.Length; ++i)
				{
					fillColorArray[i] = Color.blue;
				}

				tex.SetPixels32(fillColorArray);
				tex.Apply();
				sequencerLabelStyle.normal.background = tex;
				sequencerLabelStyle.alignment = TextAnchor.UpperCenter;

				return sequencerLabelStyle;
			}
		}

		public static GUIStyle SelectorLabelStyle
		{
			get
			{
				var selectorLabelStyle = new GUIStyle();
				Texture2D tex = new Texture2D(2, 2);
				var fillColorArray = tex.GetPixels32();

				for (var i = 0; i < fillColorArray.Length; ++i)
				{
					fillColorArray[i] = Color.cyan;
				}

				tex.SetPixels32(fillColorArray);
				tex.Apply();
				selectorLabelStyle.normal.background = tex;
				selectorLabelStyle.alignment = TextAnchor.UpperCenter;

				return selectorLabelStyle;
			}
		}

		public GUIStyle defaultStyle, selectedStyle;
		public Vector2 size;

		private Texture2D texture2D;



		public void Init(Vector2 rectSize)
		{
			CreateStyles();
			size = rectSize;
		}

		private void CreateStyles()
		{
			defaultStyle = new GUIStyle(EditorStyles.helpBox);
			defaultStyle.normal.textColor = new Color(0, 0, 0, 0);
			defaultStyle.alignment = TextAnchor.UpperCenter;

			selectedStyle = new GUIStyle(EditorStyles.helpBox);
			selectedStyle.normal.textColor = new Color(0, 0, 0, 0);
			selectedStyle.alignment = TextAnchor.UpperCenter;
		}
	}
}