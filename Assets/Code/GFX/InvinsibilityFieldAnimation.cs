using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvinsibilityFieldAnimation : MonoBehaviour
{
    public bool isPrimary = false;
    public Transform target;
    public float duration = 2f;
    public AnimationCurve intenistyCurve;
    public float intensity = 1f;
    private float startTime;
    private SpriteRenderer mySpriteRenderer;

    public InvinsibilityFieldAnimation[] children;

    // Start is called before the first frame update
    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        startTime = -duration;
    }

    // Update is called once per frame
    void Update()
    {
        if(isPrimary)
            transform.position = target.position;

        if(Time.time < startTime + duration)
            Animate();
        else
            mySpriteRenderer.color = new Color(mySpriteRenderer.color.r, mySpriteRenderer.color.g, mySpriteRenderer.color.b, 0f);
    }
    void Animate () {
        intensity = intenistyCurve.Evaluate((Time.time - startTime) / duration);
        mySpriteRenderer.color = new Color(mySpriteRenderer.color.r, mySpriteRenderer.color.g, mySpriteRenderer.color.b, intensity);
    }

    public void Initiate () {
        startTime = Time.time;
        foreach(InvinsibilityFieldAnimation child in children) {
            child.Initiate();
        }
    }

    public void Initiate (float dur) {
        startTime = Time.time;
        duration = dur;
        foreach(InvinsibilityFieldAnimation child in children) {
            child.Initiate(dur);
        }
    }
}
