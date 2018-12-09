using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AudioAnalyzer : MonoBehaviour {
	
	const int SAMPLE_SIZE = 512;
	const int SPECTRUM_SIZE = 2048;
	[Header("db and pitch properties")]
	public float dbReference = 0.1f;
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
	public bool createVisuals = false;
	//public AudioListener source;
	private float[] samples;
	private float[] spectrum;
	private float sampleRate;

	private float rmsValue;
	private float dbValue;
	private float dbValueDecayed;
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
		visualScale = new float[numOfBands];
		visualScaleDecayed = new float[numOfBands];

		int midNum = numOfBands % 2 != 0 ? (numOfBands/2)+1 : (numOfBands/2);
		bands = GetHertzBands(startingFrequency, midFrequency, midNum);
		List<float> highBands  = GetHertzBands(midFrequency, endingFrequency, numOfBands/2);
		highBands.RemoveAt(0);
		bands.AddRange(highBands);
		//bands = GetHertzBands(startingFrequency, endingFrequency, numOfVisualCubes);
	}
	// Use this for initialization
	void Awake () {
		//source = GetComponent<AudioSource>();
		samples = new float[SAMPLE_SIZE];
		spectrum = new float[SPECTRUM_SIZE];
		sampleRate = AudioSettings.outputSampleRate;
		InitArrays();
		if (createVisuals) SpawnVisuals();
	}
	
	// Update is called once per frame
	void Update () {
		AnalyzeSound();
		if (createVisuals) UpdateVisuals();
		if (displayText){ 
			displayText.text = "RMS: "+rmsValue.ToString("F2")+
				" ("+dbValue.ToString("F1")+" dB)\n"+
				"Pitch: "+pitchValue.ToString("F0")+" Hz";
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
	float CalculateAverage(float[] data, int from, int to) {
		float sum = 0;
		for (int i = from; i <= to; i++) {
			sum += (data[i]);
		}
		return sum / (to-from+1);
	}
	float CalculateDB(float RMS) {
		float dbValue = 20*Mathf.Log10(RMS/dbReference); // calculate dB
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
        AudioListener.GetOutputData(samples, 0); // fill array with samples
		AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        // Overall channels db
		AnalyzeDB();

        // Calculate Pitch
        AnalyzePitch();

        // Update frequency bands:
        AnalyzeFrequencyBands();
    }

    private void AnalyzeFrequencyBands()
    {
        float sum;
        int visualIndex = 0;
        int visualIndexEnd = 0;
        int spectrumIndex = 0;
        float hertzToSampleBin = (sampleRate / 2) / SPECTRUM_SIZE;

        while (visualIndex < numOfBands)
        {
            spectrumIndex = (int)(bands[visualIndex] / hertzToSampleBin);
            visualIndexEnd = (int)(bands[visualIndex + 1] / hertzToSampleBin);
            float db = CalculateDB(CalculateRMS(spectrum, spectrumIndex, visualIndexEnd) * Mathf.Max(Mathf.Log(visualIndexEnd - spectrumIndex), .5f));
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
    }

    private void AnalyzeDB()
    {
        float sum = 0;
        for (int i = 0; i < SAMPLE_SIZE; i++)
        {
            sum += samples[i] * samples[i]; // sum squared samples
        }
        rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE); // rms = square root of average
        dbValue = 20 * Mathf.Log10(rmsValue / dbReference); // calculate dB
        if (dbValue < -160) dbValue = -160; // clamp it to -160dB min

		dbValueDecayed -= Time.deltaTime * decayedMovementSmoothSpeed * 7.5f;
		if (dbValueDecayed < dbValue) dbValueDecayed = dbValue;
    }

    private void AnalyzePitch()
    {
        float maxV = 0;
        int maxN = 0;
        for (int i = 0; i < SPECTRUM_SIZE; i++)
        { // find max 
            if (spectrum[i] > maxV && spectrum[i] > thresholdAmplitude)
            {
                maxV = spectrum[i];
                maxN = i; // maxN is the index of max
            }
        }
        float freqN = maxN; // pass the index to a float variable
        if (maxN > 0 && maxN < SPECTRUM_SIZE - 1)
        { // interpolate index using neighbours
            var dL = spectrum[maxN - 1] / spectrum[maxN];
            var dR = spectrum[maxN + 1] / spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        pitchValue = freqN * (sampleRate / 2) / SPECTRUM_SIZE; // convert index to frequency
    }
}
