using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AudioFilterPeakingFilter : AudioFilterParent
{
    public float[] frequencies = new float[]{500};
    public float[] dbGain = new float[]{0};
    public float resonance = 1f;
    private float sampleRate;
    
    override protected void InitFilters()
    {
        sampleRate = AudioSettings.outputSampleRate;

        myFilters = new BiQuadFilter[frequencies.Length];
        for (int i = 0; i < frequencies.Length; i++) {
            myFilters[i] = BiQuadFilter.PeakingEQ(sampleRate, frequencies[i], resonance, dbGain[i]);
        }
    }
}
