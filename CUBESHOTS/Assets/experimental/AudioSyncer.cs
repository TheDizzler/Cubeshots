using TMPro;
using UnityEngine;

namespace AtomosZ.Cubeshots.AudioTools
{
	public class AudioSyncer : MonoBehaviour
	{
		public AudioSpectrum.SpectrumBand spectrumBandTarget;
		/// <summary>
		/// What spectrum value will trigger beat.
		/// </summary>
		public float bias;
		/// <summary>
		/// Minimum interval between each beat.
		/// </summary>
		public float timeStep;
		/// <summary>
		/// How long for visualization to hit peak.
		/// </summary>
		public float timeToBeat;
		/// <summary>
		/// How long for visualization to return to rest.
		/// </summary>
		public float restSmoothTime;

		public TextMeshPro text;
		protected bool isBeat;

		private float previousAudioValue;
		private float audioValue;
		private float timer;
		private int targetIndex;


		void Start()
		{
			text.text = System.Enum.GetName(typeof(AudioSpectrum.SpectrumBand), spectrumBandTarget);
		}

		public virtual void OnBeat()
		{
			timer = 0;
			isBeat = true;
		}

		public virtual void OnUpdate()
		{
			previousAudioValue = audioValue;
			audioValue = AudioSpectrum.spectrumBandValues[(int)spectrumBandTarget];

			if (timer > timeStep)
			{
				if ((previousAudioValue <= bias && audioValue > bias)
						//|| (previousAudioValue > bias && audioValue <= bias)
						)
				{
					OnBeat();
				}
			}

			timer += Time.deltaTime;
		}

		void Update()
		{
			OnUpdate();
		}
	}
}