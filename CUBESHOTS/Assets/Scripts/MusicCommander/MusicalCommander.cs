
using System.Collections;
using System.Collections.Generic;
using AtomosZ.Cubeshots.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AtomosZ.Cubeshots.MusicCommander
{
	public enum BeatLength
	{
		/// <summary>
		/// Half beat.
		/// </summary>
		Eighth,
		/// <summary>
		/// One beat.
		/// </summary>
		Quarter,
		/// <summary>
		/// Two beats.
		/// </summary>
		Half,
		/// <summary>
		/// Four beats (usually).
		/// </summary>
		Full,
	}

	public class MusicalCommander : MonoBehaviour
	{
		public static int SAMPLE_SIZE = 1025;
		public static int SPECTRUM_SIZE = 2048;
		public static int NOTES_ANALYZED = 115;

		[System.Serializable]
		public class TriggerEvent
		{
			public TriggerType type;
			public SpectrumBand spectrumBand;
			public float frequency;
			public BeatLength beatLength;
			public UnityEvent callback;
		}

		public enum TriggerType
		{
			Beat,
			SpectrumBand,
			Frequency,
			SongStart,
		}


		/// <summary>
		/// Audio Spectrum
		/// Bass: 0Hz to 250Hz
		/// Low Midrange: 250Hz to 500Hz
		/// Midrange: 500Hz to 2000Hz
		/// Upper Midrange: 2000Hz to 4000Hz
		/// Presence: 4000Hz to 6000Hz
		/// Brilliance: 6000Hz to 20000Hz
		/// </summary>
		public enum SpectrumBand
		{
			DeepBass,
			Bass,
			LowMidRange,
			MidRange,
			UpperMidRange,
			Presence,
			Brilliance,
		}

		public enum SpectrumBand4
		{
			Bass,
			LowMid,
			Mid,
			High
		}

		public static MusicalCommander instance;

		public Scene level;
		public Transform bulletStore;
		public Transform spectralAnalyzer;
		public GameObject freqBarPrefab;

		public float BPM;
		public TextMeshProUGUI bpmText;
		public float spectrumNormalizeScale = 10;

		private AudioSource source;
		private float[] samplesRight, samplesLeft;
		private float[] noteFrequencies;
		public int beatCheckFrequencyLowerLimit;
		public int beatCheckFrequencyUpperLimit;
		private List<UnityEvent> songStartEvents;
		private Dictionary<SpectrumBand, List<UnityEvent>> spectrumEvents;
		private Dictionary<float, List<UnityEvent>> frequencyEvents;
		private Dictionary<BeatLength, List<UnityEvent>> beatEvents;

		private float[] spectrumLeft;
		private float[] spectrumRight;
		[HideInInspector]
		public float[] spectrumBandsLeft;
		[HideInInspector]
		public float[] spectrumBandsRight;
		[HideInInspector]
		public float[] spectrumBands4Left;
		[HideInInspector]
		public float[] spectrumBands4Right;

		public float[] spectrumHighs = new float[7];
		public float[] spectrumHighs4 = new float[4];

		public float[] spectrumByNotes;
		public float[] spectrumByNotesLeft;
		public float[] spectrumByNotesRight;


		private System.Tuple<int, int>[] bandRanges;
		private System.Tuple<int, int>[] bandRanges4;
		private float timeSinceLastBeat = float.MinValue;

		private int energyWindowSize = 43;
		private CircularBuffer<float> energyBuffer;
		private float hertzPerBin;
		private float timeLastBeat;
		public float minBeatCooldown = .1f;
		public float minBeatEnergy = .0005f;

		public float C = 1.4f;
		public float currentEnergy;
		private bool songStarted;
		private float timeSongStarted;
		private int beatCount;
		private float totalTime;
		private float quarterNoteLength;
		private float eighthNoteLength;
		private float halfNoteLength;
		private float fullNoteLength;
		private int quarterBeatCount;
		private bool playedEighth;
		private Coroutine startCoroutine;
		private int bar;

		void Awake()
		{
			instance = this;

			if (SceneManager.sceneCount == 1)
				SceneManager.LoadScene("Level", LoadSceneMode.Additive);

			source = GetComponent<AudioSource>();
			samplesLeft = new float[SAMPLE_SIZE];
			samplesRight = new float[SAMPLE_SIZE];
			spectrumLeft = new float[SPECTRUM_SIZE];
			spectrumRight = new float[SPECTRUM_SIZE];
			spectrumBandsLeft = new float[System.Enum.GetNames(typeof(SpectrumBand)).Length];
			spectrumBandsRight = new float[System.Enum.GetNames(typeof(SpectrumBand)).Length];
			bandRanges = new System.Tuple<int, int>[System.Enum.GetNames(typeof(SpectrumBand)).Length];

			spectrumByNotes = new float[NOTES_ANALYZED];
			spectrumByNotesLeft = new float[NOTES_ANALYZED];
			spectrumByNotesRight = new float[NOTES_ANALYZED];
			energyBuffer = new CircularBuffer<float>(energyWindowSize);

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
			hertzPerBin = AudioSettings.outputSampleRate * .5f / SPECTRUM_SIZE;
			int deepBassHigh = 2;   // 86Hz     // (int)Mathf.Floor(80 / hertzPerBand);
			int bassHigh = 6;       // 258Hz    // (int)Mathf.Floor(250 / hertzPerBand);
			int lowMidHigh = 12;    // 876Hz	// (int)Mathf.Floor(500 / hertzPerBand);
			int midHigh = 48;       // 2,064Hz	// (int)Mathf.Floor(2000 / hertzPerBand);
			int upperMidHigh = 96;  // 4,128Hz	// (int)Mathf.Floor(4000 / hertzPerBand);
			int presenceHigh = 192; // 8,256Hz  // (int)Mathf.Floor(6000 / hertzPerBand);
			int brillianceHigh = 512;

			bandRanges[(int)SpectrumBand.DeepBass] = new System.Tuple<int, int>(0, deepBassHigh);
			bandRanges[(int)SpectrumBand.Bass] = new System.Tuple<int, int>(deepBassHigh, bassHigh);
			bandRanges[(int)SpectrumBand.LowMidRange] = new System.Tuple<int, int>(bassHigh, lowMidHigh);
			bandRanges[(int)SpectrumBand.MidRange] = new System.Tuple<int, int>(lowMidHigh, midHigh);
			bandRanges[(int)SpectrumBand.UpperMidRange] = new System.Tuple<int, int>(midHigh, upperMidHigh);
			bandRanges[(int)SpectrumBand.Presence] = new System.Tuple<int, int>(upperMidHigh, presenceHigh);
			bandRanges[(int)SpectrumBand.Brilliance] = new System.Tuple<int, int>(presenceHigh, brillianceHigh);


			spectrumBands4Left = new float[System.Enum.GetNames(typeof(SpectrumBand4)).Length];
			spectrumBands4Right = new float[System.Enum.GetNames(typeof(SpectrumBand4)).Length];
			bandRanges4 = new System.Tuple<int, int>[System.Enum.GetNames(typeof(SpectrumBand4)).Length];

			bandRanges4[(int)SpectrumBand4.Bass] = new System.Tuple<int, int>(0, 4);
			bandRanges4[(int)SpectrumBand4.LowMid] = new System.Tuple<int, int>(4, 12);
			bandRanges4[(int)SpectrumBand4.Mid] = new System.Tuple<int, int>(12, 48);
			bandRanges4[(int)SpectrumBand4.High] = new System.Tuple<int, int>(48, 512);

			Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			Vector3 analyzerLeft = mainCamera.ViewportToWorldPoint(
				new Vector3(0, .5f, -mainCamera.transform.position.z + spectralAnalyzer.localPosition.z));
			Vector3 analyzerRight = mainCamera.ViewportToWorldPoint(
				new Vector3(1, .5f, -mainCamera.transform.position.z + spectralAnalyzer.localPosition.z));

			float distance = Vector3.Distance(analyzerLeft, analyzerRight);
			float barSize = distance / NOTES_ANALYZED;



			noteFrequencies = new float[NOTES_ANALYZED];
			for (int i = 0; i < NOTES_ANALYZED; ++i)
			{
				noteFrequencies[i] = Mathf.Pow(2, (i - 49) / 12f) * 440;
				//Debug.Log(i + " : " + noteFrequencies[i]);

				ScaleCubeNote cube = Instantiate(freqBarPrefab, spectralAnalyzer).GetComponent<ScaleCubeNote>();
				cube.transform.position = new Vector3(
					analyzerLeft.x + cube.transform.localScale.x * .5f + i * barSize,
						2, analyzerLeft.z);
				cube.noteIndex = i;
				cube.frequency = noteFrequencies[i];
				cube.channel = 0;

				cube = Instantiate(freqBarPrefab, spectralAnalyzer).GetComponent<ScaleCubeNote>();
				cube.transform.position = new Vector3(
					analyzerLeft.x + cube.transform.localScale.x * .5f + i * barSize,
						-2, analyzerLeft.z);
				cube.noteIndex = i;
				cube.frequency = noteFrequencies[i];
				cube.channel = 1;
			}

			//Debug.Log(hertzPerBin + " Hz/bin");
			beatCheckFrequencyLowerLimit = (int)Mathf.Floor(20 / hertzPerBin);
			beatCheckFrequencyUpperLimit = (int)Mathf.Floor(174 / hertzPerBin);

			songStartEvents = new List<UnityEvent>();
			frequencyEvents = new Dictionary<float, List<UnityEvent>>();
			spectrumEvents = new Dictionary<SpectrumBand, List<UnityEvent>>();
			for (int i = 0; i < System.Enum.GetNames(typeof(SpectrumBand)).Length; ++i)
				spectrumEvents[(SpectrumBand)i] = new List<UnityEvent>();
			beatEvents = new Dictionary<BeatLength, List<UnityEvent>>();
			foreach (var beatLength in System.Enum.GetValues(typeof(BeatLength)))
				beatEvents[(BeatLength)beatLength] = new List<UnityEvent>();


			quarterNoteLength = 60f / BPM;
			eighthNoteLength = quarterNoteLength / 2;
			halfNoteLength = quarterNoteLength * 2;
			fullNoteLength = quarterNoteLength * 4;
		}


		public void RegisterForEvents(TriggerEvent triggerEvent)
		{
			switch (triggerEvent.type)
			{
				case TriggerType.Beat:
					beatEvents[triggerEvent.beatLength].Add(triggerEvent.callback);
					break;

				case TriggerType.SpectrumBand:
					spectrumEvents[triggerEvent.spectrumBand].Add(triggerEvent.callback);
					break;

				case TriggerType.SongStart:
					songStartEvents.Add(triggerEvent.callback);
					break;

				case TriggerType.Frequency:
					if (!frequencyEvents.ContainsKey(triggerEvent.frequency))
						frequencyEvents.Add(triggerEvent.frequency, new List<UnityEvent>());
					frequencyEvents[triggerEvent.frequency].Add(triggerEvent.callback);
					break;
			}
		}


		public void LoadSong()
		{
			source.Stop();
			songStarted = false;
			timeSinceLastBeat = float.MinValue;
			timeSongStarted = float.MaxValue;
			playedEighth = false;
			quarterBeatCount = 0;
			bar = 0;
			if (startCoroutine != null)
				StopCoroutine(startCoroutine);
			startCoroutine = StartCoroutine(StartSong());
		}

		private IEnumerator StartSong()
		{
			yield return new WaitForSeconds(.5f);
			foreach (var startcallback in songStartEvents)
				startcallback.Invoke();
			source.Play();
		}

		void Update()
		{
			if (source.isPlaying)
			{
				//AnalyzeSample();
				AnalyzeSpectrum();
				AnalyzeEnergy();

				CalculateBeats();
			}
		}

		private void CalculateBeats()
		{
			timeSinceLastBeat += Time.deltaTime;

			bool updateText = false;
			if (!playedEighth && timeSinceLastBeat >= eighthNoteLength)
			{
				playedEighth = true;
				foreach (var beatEvent in beatEvents[BeatLength.Eighth])
					beatEvent.Invoke();
				updateText = true;
			}

			if (timeSinceLastBeat >= quarterNoteLength)
			{
				foreach (var beatEvent in beatEvents[BeatLength.Quarter])
					beatEvent.Invoke();
				timeSinceLastBeat -= quarterNoteLength;

				foreach (var beatEvent in beatEvents[BeatLength.Eighth])
					beatEvent.Invoke();
				playedEighth = false;
				if (++quarterBeatCount > 4)
				{
					quarterBeatCount = 1;
					foreach (var beatEvent in beatEvents[BeatLength.Half])
						beatEvent.Invoke();
					foreach (var beatEvent in beatEvents[BeatLength.Full])
						beatEvent.Invoke();
					++bar;
				}
				else if (quarterBeatCount == 3)
					foreach (var beatEvent in beatEvents[BeatLength.Half])
						beatEvent.Invoke();
				updateText = true;
			}

			if (updateText)
			{
				bpmText.text = "Bar: " + (bar + 1) + "   Beat: " + (quarterBeatCount);
			}
		}

		private void AnalyzeEnergy()
		{
			currentEnergy = 0;
			for (int i = beatCheckFrequencyLowerLimit; i < beatCheckFrequencyUpperLimit; ++i)
			{
				currentEnergy += spectrumLeft[i] /** spectrumLeft[i]*/
					+ spectrumRight[i] /** spectrumRight[i]*/;
			}

			if (energyBuffer.Enqueue(currentEnergy))
			{
				float avg = 0;
				for (int i = 0; i < energyWindowSize; ++i)
				{
					avg += energyBuffer.buffer[i];
				}

				avg /= energyWindowSize;

				//float varE = 0;
				//for (int i = 0; i < energyWindowSize; ++i)
				//{
				//	varE += Mathf.Pow(avg - energyBuffer.buffer[i], 2);
				//}

				//varE /= energyWindowSize;
				//float C = -.0000015f * varE + 1.5142857f;
				if (currentEnergy > minBeatEnergy && currentEnergy > avg * C)
				{
					if (!songStarted)
					{
						songStarted = true;
						Debug.Log("song start!");
						timeSongStarted = Time.time;
						timeSinceLastBeat = quarterNoteLength;
						playedEighth = false;
						quarterBeatCount = 0;
						bar = 0;
					}

					if (Time.time - timeLastBeat > minBeatCooldown)
					{
						++beatCount;
						totalTime = Time.time - timeSongStarted;
						float totalAvgBPM = 60 / (totalTime / beatCount);
						float bpm = 60 / (Time.time - timeLastBeat);
						timeLastBeat = Time.time;
						//bpmText.text = "Total AVG BPM: " + totalAvgBPM
						//	+ " --- BPM from last: " + bpm
						//	+ " bpm from " + beatCount + " beats";
					}
				}
			}
		}

		/// Audio volume in dB:
		///		1) pass array to GetOutputData, 
		///		2) sum all squared sample values,
		///		3) calculate the average and get its square root; 
		///		(now we have the RMS value,)
		///		4) convert to dB with: 20 * Log10(rmsValue / refValue)
		///		(adjust Ref Value in the Inspector if you need a different 0 dB reference)
		///			this gives the average sound level of the last (arraySize / sampleRate) ms
		///			ex: (1024 / 48000) = 21.3ms
		private void AnalyzeSample()
		{
			source.GetOutputData(samplesLeft, 0);
			source.GetOutputData(samplesRight, 1);
			float sumLeft = 0;
			float sumRight = 0;
			for (int i = 0; i < SAMPLE_SIZE; ++i)
			{
				sumLeft += samplesLeft[i] * samplesLeft[i];
				sumRight += samplesRight[i] * samplesRight[i];
			}

			float rmsLeft = Mathf.Sqrt(sumLeft / SAMPLE_SIZE);
			float rmsRight = Mathf.Sqrt(sumRight / SAMPLE_SIZE);

			float dbValueLeft = 20 * Mathf.Log10(rmsLeft * 10);
			float dbValueRight = 20 * Mathf.Log10(rmsRight * 10);
		}

		private void AnalyzeSpectrum()
		{
			source.GetSpectrumData(spectrumLeft, 0, FFTWindow.BlackmanHarris);
			source.GetSpectrumData(spectrumRight, 1, FFTWindow.BlackmanHarris);

			foreach (SpectrumBand band in System.Enum.GetValues(typeof(SpectrumBand)))
			{
				int bandIndex = (int)band;
				var bandRange = bandRanges[bandIndex];
				spectrumBandsLeft[bandIndex] = Mathf.NegativeInfinity;
				spectrumBandsRight[bandIndex] = Mathf.NegativeInfinity;
				for (int i = bandRange.Item1; i < bandRange.Item2; ++i)
				{
					spectrumBandsLeft[bandIndex]
						= Mathf.Max(spectrumBandsLeft[bandIndex], spectrumLeft[i]);
					spectrumBandsRight[bandIndex]
						= Mathf.Max(spectrumBandsRight[bandIndex], spectrumRight[i]);
				}

				spectrumHighs[bandIndex] = spectrumBandsLeft[bandIndex] * spectrumNormalizeScale;
			}

			foreach (SpectrumBand4 band in System.Enum.GetValues(typeof(SpectrumBand4)))
			{
				int bandIndex = (int)band;
				var bandRange = bandRanges4[bandIndex];
				spectrumBands4Left[bandIndex] = Mathf.NegativeInfinity;
				spectrumBands4Right[bandIndex] = Mathf.NegativeInfinity;

				for (int i = bandRange.Item1; i < bandRange.Item2; ++i)
				{
					spectrumBands4Left[bandIndex]
						= Mathf.Max(spectrumBands4Left[bandIndex], spectrumLeft[i]);
					spectrumBands4Right[bandIndex]
						= Mathf.Max(spectrumBands4Right[bandIndex], spectrumRight[i]);
				}

				spectrumHighs4[bandIndex] = spectrumBands4Left[bandIndex] * spectrumNormalizeScale;
			}

			for (int i = 0; i < NOTES_ANALYZED; ++i)
			{
				float targetFrequency = noteFrequencies[i];
				int targetIndex = Mathf.RoundToInt(targetFrequency / hertzPerBin);
				spectrumByNotes[i] = (spectrumLeft[targetIndex] + spectrumRight[targetIndex]) * .5f;
				spectrumByNotesLeft[i] = spectrumLeft[targetIndex];
				spectrumByNotesRight[i] = spectrumRight[targetIndex];
			}
		}
	}
}