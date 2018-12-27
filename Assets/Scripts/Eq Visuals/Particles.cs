using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{
    [Header("Properties")]
    public float speedFactor = 10;
    [Header("References")]
    public AudioAnalyzer analyzer = null;
    public ParticleSystem particleSystem = null;
    public Transform emitterTransform;
    public float emitterYMin = 0;
    public float emitterYMax = 9;
    public int emitterYPosBand = 1;
    // Update is called once per frame
    void Update()
    {
        var main = particleSystem.main;
        var db = Mathf.InverseLerp(-64,0, analyzer.GetDB());
        var band = analyzer.GetVisualScale(emitterYPosBand);
        main.startSpeed = db * speedFactor * band;
        float y = Mathf.Lerp(emitterYMin, emitterYMax, band);
        emitterTransform.position = new Vector3(emitterTransform.position.x, y, emitterTransform.position.z );
        particleSystem.Emit((int)(band*500));
        var shape = particleSystem.shape;
        shape.arc = Mathf.Lerp(60,360, db);
    }
}
