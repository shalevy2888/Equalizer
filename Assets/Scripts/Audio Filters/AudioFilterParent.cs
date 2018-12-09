using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioFilterFactory))]
[RequireComponent(typeof(AudioSource))]
public class AudioFilterParent : MonoBehaviour {
	AudioFilterFactory factory = null;
	protected BiQuadFilter[] myFilters = null;
	protected virtual void InitFilters() {}
	void Start() {
		//Debug.Log("Start");
		factory = GetComponent<AudioFilterFactory>();
		InitFiltersWrapper();
	}
	void OnValidate()
    {
        InitFiltersWrapper();
    }
	void InitFiltersWrapper() {
		UnRegisterFilters();
		InitFilters();
		RegisterFilters();
	}
	protected void UnRegisterFilters()
	{
		if (myFilters == null || factory==null) return;	
		for (int i = 0; i < myFilters.Length; i++) {
			factory.RemoveFilter(myFilters[i]);
		}
	}
	public void Reprogram() {
        InitFiltersWrapper();
    }
	void OnDisable()
	{
		//Debug.Log("On Disable");
		UnRegisterFilters();
	}

	protected void RegisterFilters()
	{
		if (myFilters == null || factory==null ) return;
		for (int i = 0; i < myFilters.Length; i++) {
			factory.AddFilter(myFilters[i]);
		}
		//Debug.Log("registered #: " + myFilters.Length);
	}

	void OnEnable() {
		//Debug.Log("On Enable");
		RegisterFilters();
	}
}
