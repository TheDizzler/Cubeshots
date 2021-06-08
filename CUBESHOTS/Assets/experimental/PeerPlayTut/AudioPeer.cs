using AtomosZ.Cubeshots.AudioTools;
using AtomosZ.Cubeshots.MusicCommander;
using UnityEngine;

namespace AtomosZ.ExperimentalStuff.AudioTools
{
	/// <summary>
	/// Audio Spectrum
	/// Deep Bass: 20Hz to 60Hz
	/// Bass: 60Hz to 250Hz
	/// Low Midrange: 250Hz to 500Hz
	/// Midrange: 500Hz to 2000Hz
	/// Upper Midrange: 2000Hz to 4000Hz
	/// Presence: 4000Hz to 6000Hz
	/// Brilliance: 6000Hz to 20000Hz
	/// 
	/// Audio volume in dB:
	///		1) pass array to GetOutputData, 
	///		2) sum all squared sample values,
	///		3) calculate the average and get its square root; 
	///		(now we have the RMS value,)
	///		4) convert to dB with: 20 * Log10(rmsValue / refValue)
	///		(adjust Ref Value in the Inspector if you need a different 0 dB reference)
	///			this gives the average sound level of the last (arraySize / sampleRate) ms
	///			ex: (1024 / 48000) = 21.3ms
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class AudioPeer : MonoBehaviour
	{
		private float[] spectrumLeft;
		private float[] spectrumRight;

		public Color[] bandColor;

		[HideInInspector]
		public float[] audioBand8;
		[HideInInspector]
		public float[] audioBandBuffer8;

		[HideInInspector]
		public float[] audioBand64;
		[HideInInspector]
		public float[] audioBandBuffer64;

		public float[] freqBand8;
		public float[] bandBuffer8;
		private float[] bufferDecrease8;
		private float[] freqBandHighest8;

		private float[] freqBand64;
		private float[] bandBuffer64;
		private float[] bufferDecrease64;
		private float[] freqBandHighest64;




		[HideInInspector]
		public float amplitude;
		[HideInInspector]
		public float amplitudeBuffer;

		[Tooltip("Must be a power of 2")]
		[Range(64, 8192)]
		public int spectrumBands = 512;
		private AudioSource source;
		public InstantiateCubes cubes;
		public float audioProfile;

		private float amplitudeHighest;

		private MusicalCommander musicCommander;


		void Start()
		{
			source = GetComponent<AudioSource>();
			musicCommander = GetComponent<MusicalCommander>();

			audioBand8 = new float[8];
			audioBand64 = new float[64];
			audioBandBuffer8 = new float[8];
			audioBandBuffer64 = new float[64];
			freqBandHighest8 = new float[8];
			freqBandHighest64 = new float[64];
			spectrumLeft = new float[spectrumBands];
			spectrumRight = new float[spectrumBands];
			freqBand8 = new float[8];
			freqBand64 = new float[64];
			bandBuffer8 = new float[8];
			bandBuffer64 = new float[64];
			bufferDecrease8 = new float[8];
			bufferDecrease64 = new float[64];
			AudioProfile();
			//cubes.Init(spectrumBands);
		}

		private void AudioProfile()
		{
			for (int i = 0; i < 8; ++i)
				freqBandHighest8[i] = audioProfile;
			for (int i = 0; i < 64; ++i)
				freqBandHighest64[i] = audioProfile;
		}

		void Update()
		{
			GetSpectrumAudioSource();
			MakeFrequencyBands8();
			MakeFrequencyBands64();
			BandBuffer();
			CreateAudioBands();
			GetAmplitude();


			for (int i = 1; i < spectrumLeft.Length - 1; i++)
			{
				Debug.DrawLine(new Vector3(i - 1, spectrumLeft[i] + 10, 0), new Vector3(i, spectrumLeft[i + 1] + 10, 0), Color.red);
				Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrumLeft[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrumLeft[i]) + 10, 2), Color.cyan);
				Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrumLeft[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrumLeft[i] - 10, 1), Color.green);
				Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrumLeft[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrumLeft[i]), 3), Color.blue);
			}
		}



		private void GetSpectrumAudioSource()
		{
			source.GetSpectrumData(spectrumLeft, 0, FFTWindow.Blackman);
			source.GetSpectrumData(spectrumRight, 1, FFTWindow.Blackman);
		}

		private void GetAmplitude()
		{
			float currentAmp = 0;
			float currentAmpBuffer = 0;
			for (int i = 0; i < 8; ++i)
			{
				currentAmp += audioBand8[i];
				currentAmpBuffer += audioBandBuffer8[i];
			}

			if (currentAmp > amplitudeHighest)
				amplitudeHighest = currentAmp;

			amplitude = currentAmp / amplitudeHighest;
			amplitudeBuffer = currentAmpBuffer / amplitudeHighest;
		}


		void BandBuffer()
		{
			for (int g = 0; g < 8; ++g)
			{
				if (freqBand8[g] > bandBuffer8[g])
				{
					bandBuffer8[g] = freqBand8[g];
					bufferDecrease8[g] = .005f;
				}

				if (freqBand8[g] < bandBuffer8[g])
				{
					bandBuffer8[g] -= bufferDecrease8[g];
					bufferDecrease8[g] = (bandBuffer8[g] - freqBand8[g]) * .125f;
				}
			}

			for (int g = 0; g < 64; ++g)
			{
				if (freqBand64[g] > bandBuffer64[g])
				{
					bandBuffer64[g] = freqBand64[g];
					bufferDecrease64[g] = .005f;
				}

				if (freqBand64[g] < bandBuffer64[g])
				{
					bandBuffer64[g] -= bufferDecrease64[g];
					bufferDecrease64[g] = (bandBuffer64[g] - freqBand64[g]) * .125f;
				}
			}
		}

		private void CreateAudioBands()
		{
			for (int i = 0; i < 8; ++i)
			{
				if (freqBand8[i] > freqBandHighest8[i])
					freqBandHighest8[i] = freqBand8[i];

				if (freqBandHighest8[i] == 0)
				{
					audioBand8[i] = 0;
					audioBandBuffer8[i] = 0;
				}
				else
				{
					audioBand8[i] = freqBand8[i] / freqBandHighest8[i];
					audioBandBuffer8[i] = Mathf.Max(0, bandBuffer8[i] / freqBandHighest8[i]);
				}
			}

			for (int i = 0; i < 64; ++i)
			{
				if (freqBand64[i] > freqBandHighest64[i])
					freqBandHighest64[i] = freqBand64[i];

				if (freqBandHighest64[i] == 0)
				{
					audioBand64[i] = 0;
					audioBandBuffer64[i] = 0;
				}
				else
				{
					audioBand64[i] = freqBand64[i] / freqBandHighest64[i];
					audioBandBuffer64[i] = Mathf.Max(0, bandBuffer64[i] / freqBandHighest64[i]);
				}
			}
		}



		private void MakeFrequencyBands8()
		{
			/** 22050 / 512 = 43Hz per sample
			 * 
			 * 0: 2 bands = 86Hz (0Hz - 86Hz)
			 * 1: 4 bands = 172Hz (87Hz -258Hz)
			 * 2: 8 bands = 344Hz (259Hz - 602Hz)
			 * 3: 16 bands = 688Hz (603Hz - 1290Hz)
			 * 4: 32 bands = 1376Hz (1291Hz - 2666Hz)
			 * 5: 64 bands = 2752Hz (2667Hz - 5418Hz)
			 * 6: 128 bands = 5504Hz (5419Hz - 10922Hz)
			 * 7: 256 bands = 11008Hz (10923Hz - 21930Hz)
			 * 
			 * 
			 * 22050 / 1024 = 21.5Hz per sample
			 * 
			 * 0: 2 band = 21.5Hz (0Hz - 43Hz)
			 * 1: 4 band = 43Hz (44Hz - 87Hz)
			 * 2: 8 band = 86Hz (88Hz - 174Hz)
			 * 3: 16 band = 172Hz (175Hz - 347Hz)
			 * 4: 32 band = 344Hz (348Hz - 692Hz)
			 * 5: 64 band = 688Hz (693Hz - 1381Hz)
			 * 6: 128 band = 1376Hz (1382Hz - 2758Hz)
			 * 7: 256 band = 2752Hz (2759Hz - 5511Hz)
			 * 8: 512 band = 5504Hz (5512Hz - 11016Hz)
			 * 9: 1024 band = 11008Hz (11017Hz - 22025Hz)
			 */

			int count = 0;
			for (int i = 0; i < 8; ++i)
			{
				int sampleCount = (int)Mathf.Pow(2, i) * 2;

				if (i == 7)
				{
					sampleCount += 2;
				}

				float avg = 0;
				for (int j = 0; j < sampleCount; ++j)
				{
					avg += (spectrumLeft[count] + spectrumRight[count]) * (count + 1);
					++count;
				}

				avg /= count;

				freqBand8[i] = avg;
			}
		}

		private void MakeFrequencyBands64()
		{
			/** 22050 / 512 = 43Hz per sample
			 * 
			 * 0-15: 1 band	=     16	688Hz	(    0Hz -   688Hz)
			 * 16-31: 2 bands =   32	1376Hz	(  689Hz -  2065Hz)
			 * 32-39: 4 bands =   32	1376Hz	( 2066Hz -  3442Hz)
			 * 40-47: 6 bands =	  48	2065Hz	( 3443Hz -  5508Hz)
			 * 48-55: 16 bands = 128	5504Hz	( 5509Hz - 11013Hz)
			 * 56-63: 32 bands = 256	11008Hz (11014Hz - 22022Hz)
			 *					 ---
			 *					 512
			 */

			int count = 0;
			int sampleCount = 1;
			int power = 0;
			for (int i = 0; i < 64; ++i)
			{
				if (i == 16 || i == 32 || i == 40 || i == 48 || i == 56)
				{
					++power;
					sampleCount = (int)Mathf.Pow(2, power);
					if (power == 3)
						sampleCount -= 2;
				}

				float avg = 0;
				for (int j = 0; j < sampleCount; ++j)
				{
					avg += (spectrumLeft[count] + spectrumRight[count]) * (count + 1);
					++count;
				}

				avg /= count;

				freqBand64[i] = avg * 100;
			}
		}


		void OnValidate()
		{
			if ((spectrumBands & (spectrumBands - 1)) != 0)
			{
				spectrumBands = (int)Mathf.Pow(
					2, Mathf.Round(Mathf.Log(spectrumBands) / Mathf.Log(2)));
			}
		}
	}
}