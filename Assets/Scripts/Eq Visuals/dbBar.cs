using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dbBar : MonoBehaviour {
	public AudioAnalyzer analyzer;
	public GameObject equalizerBarPrefab;
	public GameObject barPrefab;
	public float barYScale = 0.43f;
	private float barXScale = 1f;
	EqualizerBar bar;
	const float dbMin = -24;
	// Use this for initialization
	void Start () {
		
		GameObject go = GameObject.Instantiate(equalizerBarPrefab) as GameObject;
		go.transform.SetParent(this.transform);
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
		bar = go.GetComponent<EqualizerBar>();
		bar.barPrefab = barPrefab;
		bar.barYScale = barYScale;
		bar.barXScale = barXScale;
		bar.baseColor = Color.green * 0.5f;
		bar.text.text = "-24db";	
	}

	void Update()
	{
		float value = Mathf.InverseLerp(dbMin,0, Mathf.Max(analyzer.GetDB() , dbMin));
		float decayedValue = Mathf.InverseLerp(dbMin,0, Mathf.Max(analyzer.GetDBDecayed() , dbMin));
		bar.UpdateBar(value, decayedValue);
		
	}
}
