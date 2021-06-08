using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AtomosZ.Cubeshots.UITools
{
	public class ToggleController : MonoBehaviour
	{
		public TextMeshProUGUI text;
		private Toggle toggle;


		public void Init(UnityAction<bool> callback, bool isOn)
		{
			toggle = GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(callback);
			toggle.isOn = isOn;
		}

		public bool GetValue()
		{
			return toggle.isOn;
		}

		public void SetText(string newText)
		{
			text.text = newText;
		}

		internal void Init(Action<bool> p, object spinRebound)
		{
			throw new NotImplementedException();
		}
	}
}