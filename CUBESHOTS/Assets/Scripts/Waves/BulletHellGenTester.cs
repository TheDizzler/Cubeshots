using AtomosZ.Cubeshots.Waves;
using AtomosZ.Cubeshots.UITools;
using TMPro;
using UnityEngine;

namespace AtomosZ.Cubeshots
{
	public class BulletHellGenTester : BulletHellPattern
	{
		public SliderController fireDelaySlider;
		public SliderController volleyCountSlider;
		public SliderController volleySpreadSlider;
		public SliderController spinRateSlider;
		public SliderController spinAccelerationSlider;
		public SliderController maxSpinRateSlider;
		public SliderController reboundTimerSlider;
		public SliderController reboundAngleSlider;
		public SliderController curveSlider;
		public ToggleController spreadTypeToggle;
		public TMP_Dropdown reboundTypeDropdown;
		public TextMeshProUGUI spinometer;
		public TextMeshProUGUI reboundTimer;
		public TextMeshProUGUI currentAngleText;


		public override void Init()
		{
			base.Init();

			fireDelaySlider.Init(delegate
			{
				FireRateChanged();
			}, .1f, 1f, fireDelay, false);

			volleyCountSlider.Init(delegate
			{
				VolleyCountChanged();
			}, 1, 20, bulletsPerVolley, true);

			volleySpreadSlider.Init(delegate
			{
				VolleySpreadChanged();
			}, 1, 359, volleySpread, true);

			spinRateSlider.Init(delegate
			{
				SpinRateChanged();
			}, 0, 100, spinRate, false);

			spinAccelerationSlider.Init(delegate
			{
				SpinAccelerationChanged();
			}, 0, 50, spinAcceleration, false);

			maxSpinRateSlider.Init(delegate
			{
				MaxSpinChanged();
			}, 0, 100, maxSpinRate, false);

			spreadTypeToggle.Init(delegate
			{
				SpreadTypeChanged();
			}, isEvenAngleSpread);

			reboundTimerSlider.Init(delegate
			{
				ReboundTimerChanged();
			}, 0, 10, reboundTime, false);

			reboundAngleSlider.Init(delegate
			{
				ReboundAngleChanged();
			}, 5, 179f, reboundAngle, false);

			curveSlider.Init(delegate
			{
				CurveChanged();
			}, -359f, 359f, bulletCurve, false);

			reboundTypeDropdown.options.Clear();
			reboundTypeDropdown.onValueChanged.AddListener(delegate
			{
				ReboundTypeChanged();
			});
			foreach (ReboundType reboundType in System.Enum.GetValues(typeof(ReboundType)))
				reboundTypeDropdown.options.Add(new TMP_Dropdown.OptionData(reboundType.ToString()));
			reboundTypeDropdown.SetValueWithoutNotify((int)reboundType);
		}

		private void FireRateChanged()
		{
			fireDelay = fireDelaySlider.GetValue();
		}

		private void VolleyCountChanged()
		{
			bulletsPerVolley = (int)volleyCountSlider.GetValue();
		}

		private void VolleySpreadChanged()
		{
			volleySpread = volleySpreadSlider.GetValue();
		}

		private void SpinRateChanged()
		{
			spinRate = spinRateSlider.GetValue();
			currentSpinRate = spinRate;
		}

		private void SpinAccelerationChanged()
		{
			spinAcceleration = spinAccelerationSlider.GetValue();
		}

		private void MaxSpinChanged()
		{
			maxSpinRate = maxSpinRateSlider.GetValue();
		}

		private void ReboundTimerChanged()
		{
			reboundTime = reboundTimerSlider.GetValue();
		}

		private void ReboundTypeChanged()
		{
			reboundType = (ReboundType)reboundTypeDropdown.value;
			if (reboundType == ReboundType.Angle)
			{
				startDirectionAngle = nextDirectionAngle;
			}
		}

		private void ReboundAngleChanged()
		{
			reboundAngle = reboundAngleSlider.GetValue();
		}

		private void CurveChanged()
		{
			bulletCurve = curveSlider.GetValue();
		}

		private void SpreadTypeChanged()
		{
			isEvenAngleSpread = spreadTypeToggle.GetValue();
			spreadTypeToggle.SetText(isEvenAngleSpread ? "Spread divided into angle" : "Angle is spread");
		}

		public override void TriggerRebound()
		{
			if (reboundType == ReboundType.ManualTrigger)
				reboundTriggered = true;
		}


		public override void Update()
		{
			base.Update();
			reboundTimer.text = "Rebound Time: " + timeToRebound.ToString("0.00s") + "s";
			currentAngleText.text = "Angle: " + Vector3.Angle(nextDirectionAngle, startDirectionAngle).ToString("000.0");
				spinometer.text = "Spin Rate: " + currentSpinRate.ToString(".000");
		}
	}
}