using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFilterFactory : MonoBehaviour  {
    private List<BiQuadFilter> filters = new List<BiQuadFilter>();
    public void AddFilter(BiQuadFilter filter) {
        filters.Add(filter);
        //Debug.Log("add #filters: " + filters.Count);
    }
    public void RemoveFilter(BiQuadFilter filter) {
        filters.Remove(filter);
        //Debug.Log("remove #filters: " + filters.Count);
    }
    void OnAudioFilterRead(float[] data, int channels) {
        //Debug.Log("audio filter read, #filter: " + filters.Count);
        foreach (var filter in filters.ToArray()) {
            for (int i = 0; i < data.Length; i++) {
                if (filter!=null) data[i] = filter.Transform(data[i]);   
            }
        }
    }
}
/* 
    public static LowPass CreateLowPassFilter(float sampleRate, float frequency, float q) {
        return new LowPass(sampleRate, frequency, q);
    }
    public static HighPass CreateHiPassFilter(float sampleRate, float frequency, float q) {
        return new HighPass(sampleRate, frequency, q);
    }
    public static PeakingEq CreatePeakingFilter(float sampleRate, float[] frequency, float q, float[] dbGain) {
        return new PeakingEq(sampleRate, frequency, q, dbGain);
    }
    public abstract class GenericFilter {
        static protected Dictionary<int, BiQuadFilter> filters = new Dictionary<int, BiQuadFilter>();
        static protected void AddFilter(BiQuadFilter filter) {

        }
        virtual public void Process(ref float[] data) {
            for (int i = 0; i < data.Length; i++) {
                for (int j = 0; j < filter.Length; j++) {
                    data[i] = filter[j].Transform(data[i]);   
                }
            }
        }
    }
    public class LowPass : GenericFilter {
        public LowPass(float sampleRate, float frequency, float q) {
            filter = new BiQuadFilter[1];
            filter[0] = BiQuadFilter.LowPassFilter(sampleRate, frequency, q);
        }
    }
    public class HighPass : GenericFilter {
        public HighPass(float sampleRate, float frequency, float q) {
            filter = new BiQuadFilter[1];
            filter[0] = BiQuadFilter.HighPassFilter(sampleRate, frequency, q);
        }
    }
    public class PeakingEq : GenericFilter {

        public PeakingEq(float sampleRate, float[] frequency, float q, float[] dbGain) {
            filter = new BiQuadFilter[frequency.Length];
            for (int i = 0; i < frequency.Length; i++) {
                filter[i] = BiQuadFilter.PeakingEQ(sampleRate, frequency[i], q, dbGain[i]);
            }
        }
    }

	
}*/
