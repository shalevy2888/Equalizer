using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AudioFilterLowPassFilter : AudioFilterParent
{
    public float frequency = 500f;
    public float resonance = 1f;
    private float sampleRate;

    override protected void InitFilters()
    {
        sampleRate = AudioSettings.outputSampleRate;

        myFilters = new BiQuadFilter[1];
        myFilters[0] = BiQuadFilter.LowPassFilter(sampleRate, frequency, resonance);
    }

}
