using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Equalizer : MonoBehaviour {
	public AudioAnalyzer visualizer;
	public GameObject equalizerBarPrefab;
	public GameObject barPrefab;
	public GameObject knobPrefab;
	public float barYScale = 0.43f;
	private float barXScale = 1f;
	private float columnScale = 1.1f;
	public float maxXCoordinate = 17;
	public Gradient barColorsGradient;
	public EqualizerBar[] bars;
	public AudioFilterPeakingFilter filter;
	// Use this for initialization
	void Start () {
		int num = visualizer.numOfBands;
		bars = new EqualizerBar[num];

		columnScale = maxXCoordinate / num;
		barXScale = columnScale * 0.92f;

		LineRenderer lr = GetComponent<LineRenderer>();
		lr.positionCount = num;
		lr.colorGradient = barColorsGradient;
		lr.material = new Material (Shader.Find("Particles/Additive"));

		for (int i = 0; i < num; i++) {
			GameObject go = GameObject.Instantiate(equalizerBarPrefab) as GameObject;
			go.transform.SetParent(this.transform);
			go.transform.localPosition = Vector3.right * (i-num/2) * columnScale;	
			EqualizerBar bar = go.GetComponent<EqualizerBar>();
			bars[i] = bar;
 			//bar.visualizer = visualizer;
			bar.barPrefab = barPrefab;
			//bar.visualizerBand = i;
			bar.barYScale = barYScale;
			bar.barXScale = barXScale;
			bar.baseColor = barColorsGradient.Evaluate(Mathf.InverseLerp(0,num,i));
			float hertz = visualizer.GetBandMidFreq(i);
			if (hertz >= 10000) {
				hertz = hertz / 1000;
				bar.text.text = hertz.ToString("F0") + "k";
			} else if (hertz >= 1000) {
				hertz = hertz / 1000;
				bar.text.text = hertz.ToString("F1") + "k";
			} else {
				bar.text.text = hertz.ToString("F0");
			}

			Vector3 knobPosition = new Vector3((i-num/2) * columnScale, 
				barYScale*7.5f, -7f);
			lr.SetPosition(i, knobPosition);

			GameObject knob = GameObject.Instantiate(knobPrefab) as GameObject;
			knob.transform.SetParent(this.transform);
			knobPosition.z = -0.8f;
			knob.transform.localPosition = knobPosition;
			knob.GetComponent<EqKnob>().id = i;
		}	
	}

	bool knobClicked = false;
	Transform knobClickedTransform = null;
	void Update()
	{
		for (int i = 0; i < visualizer.numOfBands; i++) {
			float value = visualizer.GetVisualScale(i);
			float decayedValue = visualizer.GetVisualScaleDecayed(i);
			bars[i].UpdateBar(value, decayedValue);
		}


		if (Input.GetMouseButtonDown(0)) {
        	//Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)){
				if (hit.collider != null && hit.collider.gameObject.tag == "knob") {
					knobClicked = true;
					knobClickedTransform = hit.collider.gameObject.transform;
					knobClickedTransform.gameObject.GetComponent<Renderer>().material.color = Color.red;
				}
			}
        }
		if (Input.GetMouseButtonUp(0)) {
			if (knobClickedTransform!=null) knobClickedTransform.gameObject.GetComponent<Renderer>().material.color = Color.grey;
			knobClicked = false;
			knobClickedTransform = null;
		}
		if (Input.GetMouseButton(0)) {
			if (knobClicked) {
				float newYPosition = 0;
				Vector3 v3 = Input.mousePosition;
				v3.z = (Camera.main.transform.position - knobClickedTransform.position).magnitude;
				v3 = Camera.main.ScreenToWorldPoint(v3);
				newYPosition = v3.y;
				float minYPos = barYScale/2;
				float maxYPos = barYScale*15 - barYScale/2;
				newYPosition = Mathf.Clamp(newYPosition, minYPos, maxYPos);
				knobClickedTransform.position = new Vector3(knobClickedTransform.position.x, newYPosition, 
					knobClickedTransform.position.z);
				int i = knobClickedTransform.GetComponent<EqKnob>().id;
				Vector3 lrPos = new Vector3((i-visualizer.numOfBands/2) * columnScale, newYPosition, -7f);
				LineRenderer lr = GetComponent<LineRenderer>();
				lr.SetPosition(i, lrPos);

				if (filter) {
					filter.dbGain[i] = Mathf.Lerp(-24f, 24f, (newYPosition-minYPos) / (maxYPos-minYPos));
					filter.Reprogram();
				}
			}
		}
	}
}
