using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(RawImage))]
public class AudioFilterAmpGain : MonoBehaviour
{
    public double bpm = 140.0F;
    public float gain = 0.5F;
	public float amplification = 1;
    public int signatureHi = 4;
    public int signatureLo = 4;
	public float waveLength = 1;

    //private double nextTick = 0.0F;
    //private float amp = 1.0F;
    private double phase = 0.0F;
    private double sampleRate = 0.0F;
    //private int accent;
    private bool running = false;

	// Audio Wave:
	static int width = 500; // texture width 
    static int height = 100; // texture height 
    public Color backgroundColor = Color.black; 
    public Color waveformColor = Color.green;
	public float displayedTime = 1; // second
     
    private Color[] blank; // blank image array 
    private Texture2D texture;
    void Start()
    {
        //accent = signatureHi;
        double startTick = AudioSettings.dspTime;
        sampleRate = AudioSettings.outputSampleRate;
		//nextTick = startTick * sampleRate;
		Debug.Log("Output sample rate: " + sampleRate);
		//Debug.Log("Start Tick: " + (startTick * sampleRate) + " next tick: " + nextTick);
        // create the texture and assign to the guiTexture: 
		texture = new Texture2D (width, height);
		
		GetComponent<RawImage>().texture = texture; 
		
		// create a 'blank screen' image 
		blank = new Color[width * height]; 
		
		for (int i = 0; i < blank.Length; i++) { 
			blank [i] = backgroundColor; 
		} 
		running = true;
		waveFormUnitSize = (int)( (1/(float)width) / (1/(float)sampleRate * 2) );
    }

	int currentTexBufferLoc = 0;
	float[] copiedData = new float[width];
	int waveFormUnitSize;
	int currentWaveFormUnitCount = 0;
	void Update()
	{
		if (copiedData==null) return;
		// clear the texture 
		texture.SetPixels (blank, 0); 
		// draw the waveform 
		for (int i = 0; i < copiedData.Length; i++) { 
			texture.SetPixel ((int)(width * i / copiedData.Length), (int)(height * (copiedData [i] + 1f) / 2f), waveformColor);
		} // upload to the graphics card 
		
		texture.Apply (); 
	}

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!running)
            return;

		//double samplesPerTick = sampleRate * 60.0F / bpm * 4.0F / signatureLo;
        //double sample = AudioSettings.dspTime * sampleRate;
		//Debug.Log("sample: " + sample);
        int dataLen = data.Length / channels;

        int n = 0;
        while (n < dataLen)
        {
            //float x = gain * amp * Mathf.Sin((float)phase);
            int i = 0;
            while (i < channels)
            {
                data[n * channels + i] *= amplification;
				data[n * channels + i] += gain;

				copiedData[currentTexBufferLoc] += data[n * channels + i];
				currentWaveFormUnitCount += 1;
				if (currentWaveFormUnitCount >= waveFormUnitSize) {
					copiedData[currentTexBufferLoc] /= currentWaveFormUnitCount;
					currentWaveFormUnitCount = 0;
					currentTexBufferLoc += 1;
					if (currentTexBufferLoc >= width) currentTexBufferLoc = 0;
				}

                i++;
            }
            /*while (sample + n >= nextTick)
            {
				nextTick += samplesPerTick;
				amp = 1.0F;
                if (++accent > signatureHi)
                {
                    accent = 1;
                    amp *= 2.0F;
                }
                Debug.Log("Tick: " + accent + "/" + signatureHi);
            }*/
			double waveLengthProtected = waveLength <= 0.0001 ? 0.0001 : waveLength;
            phase += (double)(2*Mathf.PI) / sampleRate / waveLengthProtected;
            //amp *= 0.993F;
            n++;
        }

		/* int destOffset = currentTexBufferLoc*sizeof(float)*singleFrameSize;
		//Debug.Log("Dest offset: " + destOffset + " date length (bytes): " + data.Length*sizeof(float) 
		//	+ " copied data length (bytes): " + copiedData.Length * sizeof(float));
		Buffer.BlockCopy(data, 0, copiedData, destOffset, data.Length*sizeof(float));
		currentTexBufferLoc += 1;
		if (currentTexBufferLoc >= texBufferSize) currentTexBufferLoc = 0;*/
    }
}
