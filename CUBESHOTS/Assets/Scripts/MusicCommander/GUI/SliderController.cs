using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AtomosZ.Cubeshots.UITools
{
	public class SliderController : MonoBehaviour
	{
		public TextMeshProUGUI text;
		private Slider slider;


		public void Init(UnityAction<float> callback, float minValue, float maxValue, float currentValue, bool ints)
		{
			slider = GetComponent<Slider>();
			slider.onValueChanged.AddListener(callback);
			slider.minValue = minValue;
			slider.maxValue = maxValue;
			slider.value = currentValue;
			slider.wholeNumbers = ints;
			if (ints)
				text.text = slider.value.ToString("");
			else
				text.text = slider.value.ToString(".000");
		}

		public float GetValue()
		{
			if (slider.wholeNumbers)
				text.text = slider.value.ToString("");
			else
				text.text = slider.value.ToString(".000");
			return slider.value;
		}
	}
}