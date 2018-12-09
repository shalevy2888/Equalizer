using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EqualizerBar : MonoBehaviour {
	public Text text;
	//public AudioVisualizer visualizer;
	public GameObject barPrefab;
	//public int visualizerBand = 4;
	public float barYScale = 0.43f;
	public float barXScale = 1f;
	public Color baseColor = new Color(0.203f, 0.505f, 1f, 1f);
	Transform[] visualBars;
	int numOfBars = 15;
	// Use this for initialization
	void Start () {
		visualBars = new Transform[numOfBars];
		for (int i = 0; i < numOfBars; i++) {
			GameObject go = GameObject.Instantiate(barPrefab) as GameObject;
			visualBars[i] = go.transform;
			visualBars[i].SetParent(this.transform);
			visualBars[i].localScale = new Vector3(barXScale, (barYScale-0.03f), 1);
			visualBars[i].localPosition = Vector3.up * i * barYScale + Vector3.up * 0.5f;

			Color newBaseColor = baseColor * (.85f * ((Mathf.InverseLerp(0,numOfBars, i) + 0.15f)));
			newBaseColor.a = 1;

			visualBars[i].gameObject.GetComponent<Renderer>().material.color = newBaseColor;
			visualBars[i].gameObject.GetComponent<Renderer>().material.SetColor ("_EmissionColor", newBaseColor);
		}
	}
	
	// Update is called once per frame
	public void UpdateBar (float value, float decayedValue) {
		int last = 0;
		//value = visualizer.GetVisualScale(visualizerBand);
		//decayedValue = visualizer.GetVisualScaleDecayed(visualizerBand);
		for (int i = 1; i < numOfBars; i++) {
			visualBars[i].gameObject.SetActive(value*numOfBars > i);
			if (decayedValue*numOfBars > i )
				last = i;
		}
		visualBars[last].gameObject.SetActive(true);
	}
}
