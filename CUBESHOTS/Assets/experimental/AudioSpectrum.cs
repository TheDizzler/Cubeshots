using UnityEngine;

namespace AtomosZ.Cubeshots.AudioTools
{
	public class AudioSpectrum : MonoBehaviour
	{
		/// <summary>
		/// Audio Spectrum
		/// Deep Bass: 0Hz to 60Hz
		/// Bass: 60Hz to 250Hz
		/// Low Midrange: 250Hz to 500Hz
		/// Midrange: 500Hz to 2000Hz
		/// Upper Midrange: 2000Hz to 4000Hz
		/// Presence: 4000Hz to 6000Hz
		/// Brilliance: 6000Hz to 20000Hz
		/// </summary>


		public static float[] spectrumBandValues;
		private System.Tuple<int, int> bassBand;
		private System.Tuple<int, int> lowMidBand;
		private System.Tuple<int, int> midBand;
		private System.Tuple<int, int> upperMidBand;
		private System.Tuple<int, int> presenceBand;
		private System.Tuple<int, int> brillianceBand;

		public enum SpectrumBand
		{
			Bass,
			LowMidRange,
			MidRange,
			UpperMidRange,
			Presence,
			Brilliance,
		}

		[Tooltip("Must be a power of 2")]
		[Range(64, 8192)]
		public int spectrumSize = 128;
		public int spectrumMultiplier = 100;


		private float[] audioSpectrum;


		void Start()
		{
			audioSpectrum = new float[spectrumSize];

			float hertzPerBand = AudioSettings.outputSampleRate * .5f / spectrumSize;

			spectrumBandValues = new float[System.Enum.GetNames(typeof(SpectrumBand)).Length];

			bassBand = new System.Tuple<int, int>(0, (int)(250 / hertzPerBand));
			lowMidBand = new System.Tuple<int, int>((int)(251 / hertzPerBand), (int)(500 / hertzPerBand));
			midBand = new System.Tuple<int, int>((int)(501 / hertzPerBand), (int)(2000 / hertzPerBand));
			upperMidBand = new System.Tuple<int, int>((int)(2001 / hertzPerBand), (int)(4000 / hertzPerBand));
			presenceBand = new System.Tuple<int, int>((int)(4001 / hertzPerBand), (int)(6000 / hertzPerBand));
			brillianceBand = new System.Tuple<int, int>((int)(6001 / hertzPerBand),
				(int)(AudioSettings.outputSampleRate * .5f / hertzPerBand));
		}

		private Vector3 dotSize = new Vector3(.1f, .1f);
		void OnDrawGizmos()
		{
			if (audioSpectrum != null)
			{
				for (int i = 0; i < audioSpectrum.Length; ++i)
				{
					Gizmos.DrawCube(new Vector3(i * .1f, audioSpectrum[i] * spectrumMultiplier, 0), dotSize);
				}
			}
		}

		void Update()
		{
			AudioListener.GetSpectrumData(audioSpectrum, 0, FFTWindow.Hamming);

			if (audioSpectrum != null && audioSpectrum.Length > 0)
			{
				spectrumBandValues[(int)SpectrumBand.Bass] = GetHighestIn(bassBand) * spectrumMultiplier;
				spectrumBandValues[(int)SpectrumBand.LowMidRange] = GetHighestIn(lowMidBand) * spectrumMultiplier;
				spectrumBandValues[(int)SpectrumBand.MidRange] = GetHighestIn(midBand) * spectrumMultiplier;
				spectrumBandValues[(int)SpectrumBand.UpperMidRange] = GetHighestIn(upperMidBand) * spectrumMultiplier;
				spectrumBandValues[(int)SpectrumBand.Presence] = GetHighestIn(presenceBand) * spectrumMultiplier;
				spectrumBandValues[(int)SpectrumBand.Brilliance] = GetHighestIn(brillianceBand) * spectrumMultiplier;
			}
		}

		private float GetHighestIn(System.Tuple<int, int> bandLimits)
		{
			float highest = 0;
			for (int i = bandLimits.Item1; i < bandLimits.Item2; ++i)
				if (audioSpectrum[i] > highest)
					highest = audioSpectrum[i];
			return highest;
		}

		private float GetAverageIn(System.Tuple<int, int> bandLimits)
		{
			float sum = 0;
			for (int i = bandLimits.Item1; i < bandLimits.Item2; ++i)
			{
				sum += audioSpectrum[i];
			}

			return sum / (bandLimits.Item2 - bandLimits.Item1 - 1);
		}


		private void OnValidate()
		{
			if ((spectrumSize & (spectrumSize - 1)) != 0)
			{
				spectrumSize = (int)Mathf.Pow(2, Mathf.Round(Mathf.Log(spectrumSize) / Mathf.Log(2)));
			}
		}
	}
}