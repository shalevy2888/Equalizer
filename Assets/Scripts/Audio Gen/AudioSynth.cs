using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSynth : MonoBehaviour {
	public double frequency = 440;
	public float gain;
	private double increment;
	private double phase;
	private double samlingRate = 48000;
	void Start()
	{
		samlingRate = AudioSettings.outputSampleRate;
		//Debug.Log(samlingRate);
	}
	void OnAudioFilterRead(float[] data, int channels)
	{
		increment = frequency * 2 * Mathf.PI / samlingRate;
		for (int i = 0; i < data.Length; i += channels) {
			phase += increment;
			data[i] = (float)( gain * Mathf.Sin((float)phase));
			if (channels == 2) {
				data[i+1] = (float)( gain * Mathf.Sin((float)phase));
			}
			if (phase > (Mathf.PI*2)) {
				phase = 0;
			}
		}
	}
}
