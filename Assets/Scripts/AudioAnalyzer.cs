using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AudioAnalyzer : MonoBehaviour {
	
	const int SAMPLE_SIZE = 2048;
	const int SPECTRUM_SIZE = 2048;
	[Header("db and pitch properties")]
	public float dbReference = 1f;
	public float thresholdAmplitude = 0.02f;
	public Text displayText;
	[Header("Frequency Band")]
	public float startingFrequency;
	public float midFrequency;
	public float endingFrequency;
	
	[Header("Spectrum Properties")]
	public int numOfBands = 25;
	public float visualYModifier = 10;
	public float decayedMovementSmoothSpeed = 2f;
	public float fastMovementSmoothSpeed = 5f;
	public float maxYScale = 8;
	public float dbScale = 0.2f;
	public float dbCutOff = -60f;
	[Header("Enable/Disable")]
	public bool createVisuals = false;
	public bool analyzePitch = false;
	[HideInInspector] public CyclicArray<float> waveFormL;
	[HideInInspector] public CyclicArray<float> waveFormR;
	[Header("Wave Form Properties")]
	public float waveFormLengthInSeconds = 1;
	public int waveFormResolution = 500;
	public delegate void WaveFormUpdated();
    public static event WaveFormUpdated waveFormUpdated;
	public delegate void SpectrumBarsUpdated();
    public static event SpectrumBarsUpdated spectrumBarsUpdated;
	private float waveFormSingleBinInSeconds;
	//public AudioListener source;
	private float[] samplesL;
	private float[] samplesR;
	private float[] spectrumL;
	private float[] spectrumR;
	private float sampleRate;
	private float sampledTime;
	private int sampledBeginningIndex = 0;

	private float rmsValue;
	private float dbValue = -160;
	private float dbValueDecayed = -160;
	private float pitchValue;

	private Transform[] visualCubes;
	private float[] visualScale;
	private float[] visualScaleDecayed;
	private List<float> bands = new List<float>();
	
	public float GetVisualScale(int i) {
		return visualScale[i] / visualYModifier;
	}
	public float GetVisualScaleDecayed(int i) {
		return visualScaleDecayed[i] / visualYModifier;
	}

	public float GetDB() {
		return dbValue;
	}

	public float GetDBDecayed() {
		return dbValueDecayed;
	}

	void SpawnVisuals() {
		visualCubes = new Transform[numOfBands];
		for (int i = 0; i < numOfBands; i++) {
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
			visualCubes[i] = go.transform;
			visualCubes[i].position = Vector3.right * (i-(numOfBands/2));
		}
		
	}
	void InitArrays() {
		samplesL = new float[SAMPLE_SIZE];
		spectrumL = new float[SPECTRUM_SIZE];

		samplesR = new float[SAMPLE_SIZE];
		spectrumR = new float[SPECTRUM_SIZE];

		visualScale = new float[numOfBands];
		visualScaleDecayed = new float[numOfBands];

		int midNum = numOfBands % 2 != 0 ? (numOfBands/2)+1 : (numOfBands/2);
		bands = GetHertzBands(startingFrequency, midFrequency, midNum);
		List<float> highBands  = GetHertzBands(midFrequency, endingFrequency, numOfBands/2);
		highBands.RemoveAt(0);
		bands.AddRange(highBands);
		//bands = GetHertzBands(startingFrequency, endingFrequency, numOfVisualCubes);

		waveFormL = new CyclicArray<float>(waveFormResolution); 
		waveFormR = new CyclicArray<float>(waveFormResolution); 
		waveFormSingleBinInSeconds = waveFormLengthInSeconds / waveFormResolution;
	}
	// Use this for initialization
	void Awake () {
		//source = GetComponent<AudioSource>();
		sampleRate = AudioSettings.outputSampleRate;
		sampledTime = (float)SAMPLE_SIZE/sampleRate;
		InitArrays();
		if (createVisuals) SpawnVisuals();
	}
	
	// Update is called once per frame
	void Update () {
		AnalyzeSound();
		if (createVisuals) UpdateVisuals();
		if (displayText){ 
			string pitchStr = "Pitch: "+pitchValue.ToString("F0")+" Hz";
			displayText.text = "RMS: "+rmsValue.ToString("F2")+
				" ("+dbValue.ToString("F1")+" dB)\n"+ (analyzePitch==true ? 
				pitchStr : "");
		}
	}

	List<float> GetHertzBandsLogBased(float startingPointHertz, float endPointHertz, int numberOfBands) {
		float[] bands = new float[numberOfBands];
		float ln = (float)Math.Log(endPointHertz-startingPointHertz);
		for (int i = 1; i < (numberOfBands+1); i++) {
			bands[i-1] = startingPointHertz + Mathf.Exp( i * (ln/numberOfBands) );
		}
		return new List<float>(bands);
	}

	List<float> GetHertzBands(float startingPointHertz, float endPointHertz, int numberOfBands) {
		float[] bands = new float[numberOfBands+1];
		float c = (endPointHertz-startingPointHertz) / (2<<numberOfBands);
		bands[0] = startingPointHertz;
		for (int i = 1; i < (numberOfBands+1); i++) {
			bands[i] = startingPointHertz + c*(2<<i);
		}
		return new List<float>(bands);
	}

	float CalculateRMS(float[] data, int from, int to) {
		float sum = 0;
		for (int i = from; i <= to; i++) {
			sum += (data[i] * data[i]);
		}
		return Mathf.Sqrt(sum / (to-from+1));
	}

	float CalculateSum(float[] data, int from, int to) {
		float sum = 0;
		for (int i = from; i <= to; i++) {
			sum += (data[i]);
		}
		return sum;
	}
	float CalculateSumSquare(float[] data, int from, int to) {
		float sum = 0;
		for (int i = from; i <= to; i++) {
			sum += (data[i] * data[i]);
		}
		return sum;
	}
	float CalculateAverage(float[] data, int from, int to) {
		float sum = CalculateSum(data, from, to);
		return sum / (to-from+1);
	}
	float CalculateDB(float RMS, float dbRef) {
		float dbValue = 20*Mathf.Log10(RMS/dbRef); // calculate dB
		if (dbValue < -160) dbValue = -160; // clamp it to -160dB min
		return dbValue;
	}

	public float GetBandMidFreq(int i) {
		float startBand = bands[i];
		float endBand = bands[i+1];
		return startBand + (endBand - startBand)/2;
	}

	void UpdateVisuals() {
		for (int i = 0; i < numOfBands; i++) {
			visualCubes[i].localScale = Vector3.one + Vector3.up * visualScaleDecayed[i];
		}
	}

	void AnalyzeSound()
    {
		// TODO: Analyze Stereo as well
        AudioListener.GetOutputData(samplesL, 0); // fill array with samples
		AudioListener.GetOutputData(samplesR, 1); // fill array with samples
		AudioListener.GetSpectrumData(spectrumL, 0, FFTWindow.BlackmanHarris);
		AudioListener.GetSpectrumData(spectrumR, 1, FFTWindow.BlackmanHarris);

		if (sampledTime > Time.deltaTime) {
			sampledBeginningIndex = SAMPLE_SIZE - (int)(Time.deltaTime * sampleRate );
		}

        // Overall channels db
		AnalyzeDB();

        // Calculate Pitch
        AnalyzePitch();

        // Update frequency bands:
        AnalyzeFrequencyBands();

		// Wave form:
		AnalyzeWaveForm();
    }
	private float deltaTimeFraction = 0;
	public float GetWaveFormDeltaTimeProcessed() {
		return Time.deltaTime+deltaTimeFraction;
	}
	public int GetWaveFormBinsInLastUpdate() {
		float binsFloat = (Time.deltaTime+deltaTimeFraction)/waveFormSingleBinInSeconds;
		int bins = (int)binsFloat;
		deltaTimeFraction = (binsFloat - bins) * waveFormSingleBinInSeconds;
		return bins;
	}
	void AnalyzeWaveForm() {
		int timePassedInWaveformBins = GetWaveFormBinsInLastUpdate();
		
		// First shift the current array by waveFormLastSampleSize to the left:
		waveFormL.Shift(timePassedInWaveformBins);
		waveFormR.Shift(timePassedInWaveformBins);
		
		// Condense the samples into the waveForm bins
		// 1. Check the starting point in the samples and in the waveform based on the amount
		// of time sampled and the amount of time passed
		int waveFormBeginIndex = waveFormResolution-timePassedInWaveformBins;
		
		if (sampledTime < Time.deltaTime) {
			Debug.LogWarning("Not enough samples to produce full wave form, could be due to low frame rate");
			int tempIndex = waveFormResolution - (int)(sampledTime / waveFormSingleBinInSeconds);
			// Reset old entries:
			for (int i = waveFormBeginIndex; i < tempIndex; i++) {
				waveFormL[ i ] = 0;
				waveFormR[ i ] = 0;
			}
			waveFormBeginIndex = tempIndex;
		}
		int sampleIndex = sampledBeginningIndex;
		
		int dataToWaveRatio = (int)Mathf.Floor((float)(SAMPLE_SIZE-sampleIndex) / (float)timePassedInWaveformBins);
		for (int i = waveFormBeginIndex; i < waveFormResolution; i++) {
			float sumL = 0;
			float sumR = 0;
			for (int j = 0; j < dataToWaveRatio; j++) {
				sumL += /* Mathf.Abs*/(samplesL[sampleIndex]);
				sumR += /* Mathf.Abs*/(samplesR[sampleIndex]);
				sampleIndex += 1;
			}
			waveFormL[ i ] = sumL / dataToWaveRatio;
			waveFormR[ i ] = sumR / dataToWaveRatio;
			//Debug.Log(waveForm[ i ]);
		}
		if (waveFormUpdated!=null) {
			waveFormUpdated();
		}
	}

    private void AnalyzeFrequencyBands()
    {
        float sum;
        int visualIndex = 0;
        int spectrumIndexEnd = 0;
        int spectrumIndexBegin = 0;
        float hertzToSampleBin = (sampleRate / 2) / SPECTRUM_SIZE;

        while (visualIndex < numOfBands)
        {
            spectrumIndexBegin = (int)(bands[visualIndex] / hertzToSampleBin);
            spectrumIndexEnd = (int)(bands[visualIndex + 1] / hertzToSampleBin);
			float maxRMS = Mathf.Max(CalculateRMS(spectrumL, spectrumIndexBegin, spectrumIndexEnd),CalculateRMS(spectrumR, spectrumIndexBegin, spectrumIndexEnd));
            float db = CalculateDB(maxRMS * Mathf.Max(Mathf.Log(spectrumIndexEnd - spectrumIndexBegin), 0.5f), dbReference/2);
            if (db < dbCutOff) db = dbCutOff;
            sum = Mathf.Pow(Mathf.InverseLerp(dbCutOff, 0, db), dbScale);

            float scaleY = sum * visualYModifier;
            // faster decayed
            visualScale[visualIndex] -= Time.deltaTime * fastMovementSmoothSpeed;
            if (visualScale[visualIndex] < scaleY) visualScale[visualIndex] = scaleY;

            // Decayed visual
            visualScaleDecayed[visualIndex] -= Time.deltaTime * decayedMovementSmoothSpeed;
            if (visualScaleDecayed[visualIndex] < scaleY) visualScaleDecayed[visualIndex] = scaleY;

            visualIndex++;
        }
		if (spectrumBarsUpdated!=null) {
			spectrumBarsUpdated();
		}
    }

    private void AnalyzeDB()
    {
        rmsValue = CalculateSumSquare(samplesL, sampledBeginningIndex, SAMPLE_SIZE-1) + CalculateSumSquare(samplesR, sampledBeginningIndex, SAMPLE_SIZE-1);   //CalculateRMS(samplesL, sampledBeginningIndex, SAMPLE_SIZE-1);
		rmsValue = Mathf.Sqrt( rmsValue / (2*(SAMPLE_SIZE-sampledBeginningIndex))  );
		dbValue = CalculateDB(rmsValue, dbReference);

		dbValueDecayed -= Time.deltaTime * decayedMovementSmoothSpeed * 3.5f;
		if (dbValueDecayed < dbValue) dbValueDecayed = dbValue;
    }

    private void AnalyzePitch()
    {
        if (analyzePitch==false) return;
		float maxV = 0;
        int maxN = 0;
        for (int i = 0; i < SPECTRUM_SIZE; i++)
        { // find max 
			float combinedSpectrum = Mathf.Max(spectrumL[i],spectrumR[i]);
            if (combinedSpectrum > maxV && combinedSpectrum > thresholdAmplitude)
            {
                maxV = combinedSpectrum;
                maxN = i; // maxN is the index of max
            }
        }
        float freqN = maxN; // pass the index to a float variable
        if (maxN > 0 && maxN < SPECTRUM_SIZE - 1)
        { // interpolate index using neighbours
			float combinedSpectrumMaxNMinus = Mathf.Max(spectrumL[maxN - 1],spectrumR[maxN - 1]);
			float combinedSpectrumMaxN = Mathf.Max(spectrumL[maxN],spectrumR[maxN]);
			float combinedSpectrumMaxNPlus = Mathf.Max(spectrumL[maxN + 1],spectrumR[maxN + 1]);
            var dL = combinedSpectrumMaxNMinus / combinedSpectrumMaxN;
            var dR = combinedSpectrumMaxNPlus / combinedSpectrumMaxN;
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        pitchValue = freqN * (sampleRate / 2) / SPECTRUM_SIZE; // convert index to frequency
    }
}
