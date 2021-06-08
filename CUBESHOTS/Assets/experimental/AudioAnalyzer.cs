using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using DSPLib;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class AudioAnalyzer : MonoBehaviour
{
	[Tooltip("Must be mulitple of 2")]
	[Range(64, 8192)]
	public const int spectrumSampleSize = 256;

	public AudioSource source;
	public int amp = 20;

	public PeakFinder hz80;
	public PeakFinder hz120;
	public PeakFinder hz240;
	public PeakFinder hz480;
	public PeakFinder full;

	private SpectralFluxAnalyzer preProcessedSpectralFluxAnalyzer;
	private float[] multiChannelSamples;
	private float[] currentSpectrum = new float[spectrumSampleSize];

	private float[] currentSample = new float[spectrumSampleSize];
	private Vector3 dotSize = new Vector3(.1f, .1f);
	private int numChannels;
	private int numTotalSamples;
	private float clipLength;
	private int sampleRate;
	private Coroutine analysisCoroutine;
	private bool analysisDone;


	void Start()
	{
		PlayNext();
	}


	//void OnDrawGizmos()
	//{
	//	if (currentSpectrum != null)
	//	{
	//		for (int i = 0; i < currentSpectrum.Length; ++i)
	//		{
	//			Gizmos.DrawCube(new Vector3(i * .1f, currentSpectrum[i] * amp, 0), dotSize);
	//		}
	//	}
	//}

	private IEnumerator FullAnalysisCoroutine()
	{
		ThreadStart fullSpectrumAnalysis = GetFullSpectrumThreaded;
		Thread bgThread = new Thread(fullSpectrumAnalysis);
		float threadStart = Time.time;
		Debug.Log("Starting Background Thread");
		bgThread.Start();

		while (!analysisDone)
		{
			yield return null;
		}

		Debug.Log("Background Thread Completed in " + (Time.time - threadStart) + "s");
		AnalysisComplete();
	}

	public void PlayNext()
	{
		analysisDone = false;
		source.Stop();

		preProcessedSpectralFluxAnalyzer = new SpectralFluxAnalyzer();
		// Need all audio samples.  If in stereo, samples will return with left and right channels interweaved
		// [L,R,L,R,L,R]
		multiChannelSamples = new float[source.clip.samples * source.clip.channels];
		numChannels = source.clip.channels;
		numTotalSamples = source.clip.samples;
		clipLength = source.clip.length;

		// We are not evaluating the audio as it is being played by Unity, so we need the clip's sampling rate
		sampleRate = source.clip.frequency;

		source.clip.GetData(multiChannelSamples, 0);

		analysisCoroutine = StartCoroutine(FullAnalysisCoroutine());
	}

	public void GetFullSpectrumThreaded()
	{
		try
		{
			// We only need to retain the samples for combined channels over the time domain
			float[] preProcessedSamples = new float[numTotalSamples];

			int numProcessed = 0;
			float combinedChannelAverage = 0f;
			for (int i = 0; i < multiChannelSamples.Length; i++)
			{
				combinedChannelAverage += multiChannelSamples[i];

				// Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
				if ((i + 1) % numChannels == 0)
				{
					preProcessedSamples[numProcessed] = combinedChannelAverage / numChannels;
					numProcessed++;
					combinedChannelAverage = 0f;
				}
			}

			Debug.Log("Combine Channels done");
			Debug.Log(preProcessedSamples.Length);

			// Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
			int iterations = preProcessedSamples.Length / spectrumSampleSize;

			FFT fft = new FFT();
			fft.Initialize(spectrumSampleSize);

			Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
			double[] sampleChunk = new double[spectrumSampleSize];
			for (int i = 0; i < iterations; i++)
			{
				// Grab the current 1024 chunk of audio sample data
				System.Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

				// Apply our chosen FFT Window
				double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, spectrumSampleSize);
				double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
				double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

				// Perform the FFT and convert output (complex numbers) to Magnitude
				Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
				double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
				scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

				// These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
				float curSongTime = GetTimeFromIndex(i) * spectrumSampleSize;

				// Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
				preProcessedSpectralFluxAnalyzer.analyzeSpectrum(
					System.Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime);
			}

			Debug.Log("Spectrum Analysis done");
			analysisDone = true;
		}
		catch (System.Exception e)
		{
			// Catch exceptions here since the background thread won't always surface the exception to the main thread
			Debug.Log(e.ToString());
		}
	}


	private void AnalysisComplete()
	{
		analysisCoroutine = null;
		//source.Play();
	}

	/// <summary>
	/// initial test
	/// </summary>
	public void Analyze()
	{
		Debug.Log(sizeof(float) * spectrumSampleSize);
		// Channel 0 contains the average of the stereo samples,
		// combining every 2 stereo samples into 1 mono sample.
		source.GetSpectrumData(currentSpectrum, 0, FFTWindow.BlackmanHarris);

		//int freq = song.frequency;
		// hz per bin = freq / 2 / spectrumArraySize
		// if freq == 48000Hz, 1024 sample size == 23.44Hz
		// if freq == 44100Hz, 1024 sample size == 21.53Hz

		float targetFrequency = 234f;
		float hertzPerBin = AudioSettings.outputSampleRate * .5f / spectrumSampleSize;
		int targetIndex = (int)(targetFrequency / hertzPerBin);

		string outString = "";
		for (int i = targetIndex - 3; i <= targetIndex + 3; i++)
		{
			outString += string.Format("| Bin {0} : {1}Hz : {2} |   ", i, i * hertzPerBin, currentSpectrum[i]);
		}

		Debug.Log(outString);
	}

	void Update()
	{
		// GetSpectrumData gets relative amplitude data
		source.GetSpectrumData(currentSpectrum, 0, FFTWindow.BlackmanHarris);
		// GetOutputData gets amplitude
		//source.GetOutputData(currentSpectrum, 0);
		//AnalyzeSpectrum(currentSpectrum, source.time);

		if (source.isPlaying)
		{
			int indexToPlot = GetIndexFromTime(source.time) / spectrumSampleSize;
			if (indexToPlot >= preProcessedSpectralFluxAnalyzer.spectralFluxSamples.Count)
				return;
			hz80.FindPeak(preProcessedSpectralFluxAnalyzer.spectralFluxSamples[indexToPlot], 0);
			hz120.FindPeak(preProcessedSpectralFluxAnalyzer.spectralFluxSamples[indexToPlot], 1);
			hz240.FindPeak(preProcessedSpectralFluxAnalyzer.spectralFluxSamples[indexToPlot], 2);
			hz480.FindPeak(preProcessedSpectralFluxAnalyzer.spectralFluxSamples[indexToPlot], 3);
			full.FindPeak(preProcessedSpectralFluxAnalyzer.spectralFluxSamples[indexToPlot], -1);
		}
	}


	private float GetTimeFromIndex(int index)
	{
		return (1f / sampleRate) * index;
	}

	private int GetIndexFromTime(float curTime)
	{
		float lengthPerSample = clipLength / numTotalSamples;
		return Mathf.FloorToInt(curTime / lengthPerSample);
	}

}
