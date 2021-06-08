using System.Collections.Generic;
using UnityEngine;

public class SpectralFluxInfo
{
	public float time;
	public float spectralFlux;
	public float threshold;
	public float prunedSpectralFlux;
	public bool isPeak;
	public float[] spectrum;

	public FrequencyFlux[] fluxes;


	public class FrequencyFlux
	{
		public float threshold;
		public float frequency;
		public float spectralFlux;
		public float prunedSpectralFlux;
		public bool isPeak;

		public void Prune()
		{
			prunedSpectralFlux = Mathf.Max(0f, spectralFlux - threshold);
		}
	}
}

public class SpectralFluxAnalyzer
{
	// Sensitivity multiplier to scale the average threshold.
	// In this case, if a rectified spectral flux sample is > 1.5 times the average, it is a peak
	float thresholdMultiplier = 1.9f;

	// Number of samples to average in our window
	int thresholdWindowSize = 30;

	public List<SpectralFluxInfo> spectralFluxSamples;

	float[] curSpectrum;
	float[] prevSpectrum;
	private float hertzPerBand;
	int indexToProcess;

	int[] analysisFreqs = new int[] { 80, 120, 240, 480 };

	public SpectralFluxAnalyzer()
	{
		spectralFluxSamples = new List<SpectralFluxInfo>();

		// Start processing from middle of first window and increment by 1 from there
		indexToProcess = thresholdWindowSize / 2;

		curSpectrum = new float[AudioAnalyzer.spectrumSampleSize];
		prevSpectrum = new float[AudioAnalyzer.spectrumSampleSize];

		hertzPerBand = AudioSettings.outputSampleRate * .5f / AudioAnalyzer.spectrumSampleSize;
		Debug.Log(hertzPerBand + "hz/band");
	}


	public void analyzeSpectrum(float[] spectrum, float time)
	{
		// Set spectrum
		curSpectrum.CopyTo(prevSpectrum, 0);
		spectrum.CopyTo(curSpectrum, 0);

		// Get current spectral flux from spectrum
		SpectralFluxInfo curInfo = new SpectralFluxInfo();
		curInfo.spectrum = spectrum;
		curInfo.time = time;
		curInfo.spectralFlux = calculateRectifiedSpectralFlux();

		curInfo.fluxes = new SpectralFluxInfo.FrequencyFlux[analysisFreqs.Length];
		for (int i = 0; i < analysisFreqs.Length; ++i)
		{
			var flux = new SpectralFluxInfo.FrequencyFlux();
			flux.frequency = analysisFreqs[i];
			flux.spectralFlux = CalculateRectifiedSpectralFluxAt(analysisFreqs[i]);
			curInfo.fluxes[i] = flux;
		}


		spectralFluxSamples.Add(curInfo);

		// We have enough samples to detect a peak
		if (spectralFluxSamples.Count >= thresholdWindowSize)
		{
			var fluxSample = spectralFluxSamples[indexToProcess];
			// Get Flux threshold of time window surrounding index to process
			fluxSample.threshold = getFluxThreshold(indexToProcess);

			// Only keep amp amount above threshold to allow peak filtering
			fluxSample.prunedSpectralFlux = getPrunedSpectralFlux(indexToProcess);

			// Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
			int indexToDetectPeak = indexToProcess - 1;

			for (int i = 0; i < fluxSample.fluxes.Length; ++i)
			{
				fluxSample.fluxes[i].threshold = GetFluxThresholdAt(indexToProcess, i);
				fluxSample.fluxes[i].Prune();
				fluxSample.fluxes[i].isPeak = IsPeak(indexToDetectPeak, i);
			}

			bool curPeak = isPeak(indexToDetectPeak);

			if (curPeak)
			{
				fluxSample.isPeak = true;
			}

			indexToProcess++;
		}
		else
		{
			//Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", spectralFluxSamples.Count, thresholdWindowSize));
		}
	}

	private float CalculateRectifiedSpectralFluxAt(int targetFrequency)
	{
		int targetIndex = (int)(targetFrequency / hertzPerBand);

		float sum = 0f;

		// this is f**k'd, will not work
		for (int i = Mathf.Max(0, targetIndex - 3); i < targetIndex + 3; ++i)
			sum += Mathf.Max(0f, curSpectrum[i] - prevSpectrum[i]);

		return sum;
	}

	float calculateRectifiedSpectralFlux()
	{
		float sum = 0f;

		// Aggregate positive changes in spectrum data
		for (int i = 0; i < AudioAnalyzer.spectrumSampleSize; i++)
		{
			sum += Mathf.Max(0f, curSpectrum[i] - prevSpectrum[i]);
		}
		return sum;
	}

	float GetFluxThresholdAt(int spectralFluxIndex, int fluxFreqIndex)
	{
		// How many samples in the past and future we include in our average
		int windowStartIndex = Mathf.Max(0, spectralFluxIndex - thresholdWindowSize / 2);
		int windowEndIndex = Mathf.Min(spectralFluxSamples.Count - 1, spectralFluxIndex + thresholdWindowSize / 2);

		// Add up our spectral flux over the window
		float sum = 0f;
		for (int i = windowStartIndex; i < windowEndIndex; i++)
		{
			sum += spectralFluxSamples[i].fluxes[fluxFreqIndex].spectralFlux;
		}

		// Return the average multiplied by our sensitivity multiplier
		float avg = sum / (windowEndIndex - windowStartIndex);
		return avg * thresholdMultiplier;
	}

	float getFluxThreshold(int spectralFluxIndex)
	{
		// How many samples in the past and future we include in our average
		int windowStartIndex = Mathf.Max(0, spectralFluxIndex - thresholdWindowSize / 2);
		int windowEndIndex = Mathf.Min(spectralFluxSamples.Count - 1, spectralFluxIndex + thresholdWindowSize / 2);

		// Add up our spectral flux over the window
		float sum = 0f;
		for (int i = windowStartIndex; i < windowEndIndex; i++)
		{
			sum += spectralFluxSamples[i].spectralFlux;
		}

		// Return the average multiplied by our sensitivity multiplier
		float avg = sum / (windowEndIndex - windowStartIndex);
		return avg * thresholdMultiplier;
	}

	float getPrunedSpectralFlux(int spectralFluxIndex)
	{
		return Mathf.Max(0f, spectralFluxSamples[spectralFluxIndex].spectralFlux - spectralFluxSamples[spectralFluxIndex].threshold);
	}


	bool IsPeak(int spectralFluxIndex, int fluxFreqIndex)
	{
		return (spectralFluxSamples[spectralFluxIndex].fluxes[fluxFreqIndex].prunedSpectralFlux
				> spectralFluxSamples[spectralFluxIndex + 1].fluxes[fluxFreqIndex].prunedSpectralFlux)
			&& (spectralFluxSamples[spectralFluxIndex].fluxes[fluxFreqIndex].prunedSpectralFlux
				> spectralFluxSamples[spectralFluxIndex - 1].fluxes[fluxFreqIndex].prunedSpectralFlux);
	}

	bool isPeak(int spectralFluxIndex)
	{
		if (spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex + 1].prunedSpectralFlux &&
			spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex - 1].prunedSpectralFlux)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	void logSample(int indexToLog)
	{
		int windowStart = Mathf.Max(0, indexToLog - thresholdWindowSize / 2);
		int windowEnd = Mathf.Min(spectralFluxSamples.Count - 1, indexToLog + thresholdWindowSize / 2);
		Debug.Log(string.Format(
			"Peak detected at song time {0} with pruned flux of {1} ({2} over thresh of {3}).\n" +
			"Thresh calculated on time window of {4}-{5} ({6} seconds) containing {7} samples.",
			spectralFluxSamples[indexToLog].time,
			spectralFluxSamples[indexToLog].prunedSpectralFlux,
			spectralFluxSamples[indexToLog].spectralFlux,
			spectralFluxSamples[indexToLog].threshold,
			spectralFluxSamples[windowStart].time,
			spectralFluxSamples[windowEnd].time,
			spectralFluxSamples[windowEnd].time - spectralFluxSamples[windowStart].time,
			windowEnd - windowStart
		));
	}
}