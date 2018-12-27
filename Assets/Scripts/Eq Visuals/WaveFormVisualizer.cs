using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveFormVisualizer : MonoBehaviour {

	static int width = 1024; // texture width 
    static int height = 200; // texture height 
    public Color backgroundColor = Color.black; 
    public Color waveformColorL = Color.green;
	public Color waveformColorR = Color.blue;
	public bool useDBInColorAlpha = false;
	public bool useSolidLines = false;
	private Texture2D texture;
	[SerializeField] RawImage image = null;
	[SerializeField] AudioAnalyzer analyzer = null;
	private float dbMin = -24;

	private Color[] blank; // blank image array 
	float uvShift;
	int waveFormToTexture;
	int halfHeight;
	float prevPointL;
	float prevPointR;

	void OnEnable()
	{
		if (useSolidLines == true) {
			AudioAnalyzer.waveFormUpdated += UpdateVisualsSolid;	
		} else {
			AudioAnalyzer.waveFormUpdated += UpdateVisualsLines;
		}
	}
    void Start()
    {
        texture = new Texture2D (width, height);
		image.texture = texture;
		image.uvRect = new Rect(0,0,1,1);

		blank = new Color[width * height]; 
		for (int i = 0; i < blank.Length; i++) { 
			blank [i] = backgroundColor; 
		} 
		texture.SetPixels (blank, 0);
		texture.Apply (); 

		uvShift = 0;
		waveFormToTexture = analyzer.waveFormL.Length / width;
		halfHeight = height/2;
		prevPointL = halfHeight;
		prevPointR = halfHeight;
    }
	private float pixelFractions = 0;
	
	int ShiftTextureUV() {
		float numberOfTexturePixelsNeededUpdateFloat = width * (analyzer.GetWaveFormDeltaTimeProcessed() / analyzer.waveFormLengthInSeconds) + pixelFractions;
		int numberOfTexturePixelsNeededUpdate = (int)numberOfTexturePixelsNeededUpdateFloat;
		pixelFractions = numberOfTexturePixelsNeededUpdateFloat - numberOfTexturePixelsNeededUpdate;
		// Scroll the texture UV based on the time passed in the last update
		float uvShiftDelta = (float)numberOfTexturePixelsNeededUpdate / (float)width;
		uvShift += uvShiftDelta;
		image.uvRect = new Rect( uvShift ,0,1,1);

		return numberOfTexturePixelsNeededUpdate;
	}

	int TextureBeginIndex(int numberOfTexturePixelsNeededUpdate) {
		return ((width-numberOfTexturePixelsNeededUpdate) + (int)(uvShift*width)) % width;
	}
	enum Channels
	{
		left, right, both
	}
	float GetSumWaveForm(int count, bool abs, ref int waveFormIndexer, Channels channels = Channels.both) {
		float sum = 0;
		for (int j = 0; j < count; j++) {
			if (waveFormIndexer<analyzer.waveFormResolution) {
				float value = 0;
				float leftValue = analyzer.waveFormL[waveFormIndexer];
				float rightValue = analyzer.waveFormR[waveFormIndexer];
				switch (channels)
				{
				case Channels.left:
					value = leftValue;
					break;
				case Channels.right:
					value = rightValue;
					break;
				case Channels.both:
					value = leftValue + rightValue;
					break;
				default:
					break;
				}
				sum += abs==true ? Mathf.Abs(value) : value;
				waveFormIndexer += 1;
			}
		}
		sum = sum / waveFormToTexture;
		return sum;
	}

	Color GetCurrentColor(bool left = true) {
		float value = Mathf.Max(0.6f, Mathf.InverseLerp(dbMin,0, Mathf.Max(analyzer.GetDBDecayed() , dbMin)));
		Color chosenColor = left ? waveformColorL : waveformColorR;
		Color currentColor = new Color(chosenColor.r, chosenColor.g, chosenColor.b, 
			useDBInColorAlpha==true ? value : chosenColor.a);
		return currentColor;
	}
	
	void UpdateVisualsLines () {
		int numberOfTexturePixelsNeededUpdate = ShiftTextureUV();

		int waveFormIndexer =  analyzer.waveFormResolution - analyzer.GetWaveFormBinsInLastUpdate();
		
		// Calculate the indexes in the texture that needs to be updated:
		int textureBeginIndex = TextureBeginIndex(numberOfTexturePixelsNeededUpdate);
		
		// draw the waveform
		float newPointL = 0;
		float newPointR = 0;
		int numberOfTextureLinesDrawn = 0;
		int i = textureBeginIndex;
		while (numberOfTextureLinesDrawn<numberOfTexturePixelsNeededUpdate) {
			int savedIndexer = waveFormIndexer;
			float sumL = GetSumWaveForm(waveFormToTexture, false, ref waveFormIndexer);
			waveFormIndexer = savedIndexer;
			float sumR = GetSumWaveForm(waveFormToTexture, false, ref waveFormIndexer, Channels.right);
			int numPixelsL = (int)(halfHeight * sumL);
			int numPixelsR = (int)(halfHeight * sumR);
			// Reset the color to background color
			DrawLine(i, 0, height, true, backgroundColor);
			newPointL = halfHeight + numPixelsL;
			newPointR = halfHeight + numPixelsR;
			DrawLine(i-1, prevPointL, newPointL, false, waveformColorL);
			DrawLine(i-1, prevPointR, newPointR, false, waveformColorR);
			
			numberOfTextureLinesDrawn += 1;
			i = (i+1) % width;
			prevPointL = newPointL;
			prevPointR = newPointR;
		}

		texture.Apply (); 
	}
	
	void UpdateVisualsSolid () {
		int numberOfTexturePixelsNeededUpdate = ShiftTextureUV();
		
		// Find the place in the waveForm that is equivalent to the last delta time
		int waveFormIndexer =  analyzer.waveFormResolution - analyzer.GetWaveFormBinsInLastUpdate();
		
		Color currentColor = GetCurrentColor();		
		
		// Calculate the indexes in the texture that needs to be updated:
		int textureBeginIndex = TextureBeginIndex(numberOfTexturePixelsNeededUpdate);
		
		// draw the waveform 
		int numberOfTextureLinesDrawn = 0;
		int i = textureBeginIndex;
		while (numberOfTextureLinesDrawn<numberOfTexturePixelsNeededUpdate) {
			float sum = GetSumWaveForm(waveFormToTexture, true, ref waveFormIndexer);
			int numPixels = (int)(halfHeight * sum);
			// Reset the color to background color
			DrawLine(i, 0, halfHeight-numPixels, true, backgroundColor);
			DrawLine(i, halfHeight+numPixels, height, true, backgroundColor);
			
			// Set the new waveform color
			DrawLine(i, halfHeight-numPixels, halfHeight+numPixels, true, currentColor);
			numberOfTextureLinesDrawn += 1;
			i = (i+1) % width;
		}

		texture.Apply (); 
	}
	
	public void DrawLine(float fromX, float fromY, float toY, bool sameX, Color col)
	{
		Vector2 t = new Vector2(fromX, fromY);
		Vector2 p1 = t;
		Vector2 p2 = new Vector2(sameX ? fromX : fromX+1, toY);
		float frac = 1/Mathf.Sqrt ((sameX==true ? 0 : 1) + Mathf.Pow (toY - fromY, 2));
		float ctr = 0;
		
		while ((int)t.y != (int)toY) {
			t = Vector2.Lerp(p1, p2, ctr);
			ctr += frac;
			texture.SetPixel((int)t.x, (int)t.y, col);
		}
	}
}
