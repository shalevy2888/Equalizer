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

