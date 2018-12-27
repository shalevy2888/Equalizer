using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSynth : MonoBehaviour {
	public double frequency = 440;
	public float gain;
	public float attackInMilliSeconds;
	public float sustainInMilliSeconds;
	public float decayInMilliSeconds;
	private float currentGain = 0;
	private double increment;
	private double attackIncrement = 0;
	private double sustainIncrement = 0;
	private double decayIncrement = 0;
	private double phase;
	private double attackPhase;
	private double sustainPhase;
	private double decayPhase;
	private double samlingRate = 48000;
	private const double attackExponentFactor = 4;
	private const double decayExponentFactor = 4;

	enum State
	{
		attack, sustain, decay, end
	}
	private State state = State.end;
	void Start()
	{
		samlingRate = AudioSettings.outputSampleRate;
		//Debug.Log(samlingRate);
	}
	void OnAudioFilterRead(float[] data, int channels)
	{
		//increment = frequency * 2 * Mathf.PI / samlingRate;
		for (int i = 0; i < data.Length; i += channels) {
			// Gain phase
			switch (state)
			{
			case State.attack:
				currentGain = gain * Mathf.Exp( (float)(-attackExponentFactor + attackPhase) );
				attackPhase += attackIncrement;
				if (attackPhase>=attackExponentFactor) {
					state = State.sustain;
				}
				//Debug.Log("CG: " + currentGain + " AP: " + attackPhase);
				break;
			case State.sustain:
				sustainPhase += sustainIncrement;
				if (sustainPhase >= 1) {
					state = State.decay;
				}
				break;
			case State.decay:
				decayPhase += decayIncrement;
				currentGain = gain * (1 - Mathf.Pow((float)decayPhase, (float)(1/decayExponentFactor)));
				if (decayPhase >= 1) {
					state = State.end;
				}
				break;
			case State.end:
				currentGain = 0;
				break;
			default:
				break;
			}
			// Frequency Phase
			phase += increment;
			float result = (float)( currentGain * Mathf.Sin((float)phase));
			for (int j = 0; j < channels; j++) {
				data[i+j] = result;
			}
			if (phase > (Mathf.PI*2)) {
				phase = 0;
			}
		}
	}
	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			// Start a new note
			phase = 0;
			increment = frequency * 2 * Mathf.PI / samlingRate;
			currentGain = gain;
			attackPhase = 0;
			sustainPhase = 0;
			decayPhase = 0;
			attackIncrement = attackExponentFactor / ((attackInMilliSeconds/1000) * samlingRate);
			sustainIncrement = 1 / ((sustainInMilliSeconds/1000) * samlingRate);
			decayIncrement = 1 / ((decayInMilliSeconds/1000) * samlingRate);
			state = State.attack;
		}
	}
}
