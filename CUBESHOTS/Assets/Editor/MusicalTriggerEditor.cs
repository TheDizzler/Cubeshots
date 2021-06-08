using UnityEditor;
using UnityEngine;
using static AtomosZ.Cubeshots.MusicCommander.MusicalCommander;

namespace AtomosZ.Cubeshots.MusicCommander.EditorTools
{
	[CustomPropertyDrawer(typeof(TriggerEvent))]
	public class MusicalTriggerDrawer : PropertyDrawer
	{
		private bool isFoldout = true;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight;
			if (isFoldout)
			{
				height += EditorGUIUtility.singleLineHeight * 3.5f;
				int listSize = property.FindPropertyRelative("callback")
					.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize;
				height += Mathf.Max(1, listSize) * EditorGUIUtility.singleLineHeight * 2.61f;
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			isFoldout = EditorGUI.BeginFoldoutHeaderGroup(labelRect, isFoldout, label);
			if (isFoldout)
			{
				var startPos = position.x;
				position.y += EditorGUIUtility.singleLineHeight;
				var type = property.FindPropertyRelative("type");
				EditorGUI.PropertyField(
					new Rect(position.x, position.y, 90, EditorGUIUtility.singleLineHeight),
					type, GUIContent.none);

				switch (type.enumValueIndex)
				{
					case (int)TriggerType.SpectrumBand:
					{
						EditorGUI.PropertyField(
							new Rect(position.x + 90, position.y, 90, EditorGUIUtility.singleLineHeight),
							property.FindPropertyRelative("spectrumBand"), GUIContent.none);
					}
					break;
					case (int)TriggerType.Frequency:
					{
						EditorGUI.PropertyField(
							new Rect(position.x + 90, position.y, 90, EditorGUIUtility.singleLineHeight),
							property.FindPropertyRelative("frequency"), GUIContent.none);
					}
					break;

					case (int)TriggerType.Beat:
						EditorGUI.PropertyField(
							new Rect(position.x + 90, position.y, 90, EditorGUIUtility.singleLineHeight),
							property.FindPropertyRelative("beatLength"), GUIContent.none);
						break;

				}
				EditorGUI.PropertyField(
					new Rect(startPos,
						position.y + EditorGUIUtility.singleLineHeight, position.width,
						EditorGUIUtility.singleLineHeight),
					property.FindPropertyRelative("callback"), GUIContent.none);
			}

			EditorGUI.EndFoldoutHeaderGroup();
		}
	}
}