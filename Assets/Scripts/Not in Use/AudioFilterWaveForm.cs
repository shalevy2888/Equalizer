using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(RawImage))]
public class AudioFilterWaveForm : MonoBehaviour
{
    private double sampleRate;
    private bool running = false;

	// Audio Wave:
	static int width = 500; // texture width 
    static int height = 100; // texture height 
    public Color backgroundColor = Color.black; 
    public Color waveformColor = Color.green;
	public float displayedTime = 1; // second
    
    private int currentTexBufferLoc = 0;
	private float[] copiedData = new float[width];
	private int waveFormUnitSize;
	private int currentWaveFormUnitCount = 0;
    private Color[] blank; // blank image array 
    private Texture2D texture;
    void Start()
    {
        double startTick = AudioSettings.dspTime;
        sampleRate = AudioSettings.outputSampleRate;
		
        texture = new Texture2D (width, height);
		GetComponent<RawImage>().texture = texture; 
		// create a 'blank screen' image 
		blank = new Color[width * height]; 
		for (int i = 0; i < blank.Length; i++) { 
			blank [i] = backgroundColor; 
		} 
		running = true;
    }

	
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

        float[] spectrum = new float[256];

        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        for (int i = 1; i < spectrum.Length - 1; i++)
        {
            Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
        }
	}

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!running)
            return;
        waveFormUnitSize = (int)( (displayedTime/(float)width) / (1/(float)sampleRate * 2) );
		int dataLen = data.Length / channels;

        int n = 0;
        while (n < dataLen)
        {
            int i = 0;
            while (i < channels)
            {
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
            n++;
        }
    }
}
