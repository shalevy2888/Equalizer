using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFilterFactory : MonoBehaviour  {
    /* In Unity document:
        Also note that OnAudioFilterRead is called on a different thread from the main thread (namely the audio thread) so calling into many Unity functions from this function is not allowed (if you try, a warning shows up at run time).
        Which means that the Thread that is adding and removing filters is different than the Audio thread 
        accessing this filter list in the OnAudioFilterRead() function. Hence the locking.
     */
    private readonly object filterLock = new object();
    private List<BiQuadFilter> filters = new List<BiQuadFilter>();
    public void AddFilter(BiQuadFilter filter) {
        lock (filterLock) {
            filters.Add(filter);
        }
        //Debug.Log("add #filters: " + filters.Count);
    }
    public void RemoveFilter(BiQuadFilter filter) {
        lock (filterLock) {
            filters.Remove(filter);
        }
        //Debug.Log("remove #filters: " + filters.Count);
    }
    void OnAudioFilterRead(float[] data, int channels) {
        lock (filterLock) {
            //Debug.Log("audio filter read, #filter: " + filters.Count);
            foreach (var filter in filters.ToArray()) {
                for (int i = 0; i < data.Length; i++) {
                    if (filter!=null) data[i] = filter.Transform(data[i]);   
                }
            }
        }
    }
}

