using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrickTextAnimator : MonoBehaviour
{
    public Color myColor;
    public float verticalSpacing;
    public float appearTime = 0.3f;
    public float killTime = 0.2f;
    public Vector2 originalPos;
    public AnimationCurve motionCurve;
    private RectTransform rectTrans;
    private Text text;

    private float animationStartTime;

    public enum CurrentState {
        Appear,
        Hide,
        Kill
    }
    public CurrentState currentState;
    
    // Start is called before the first frame update
    public void Initialize()
    {
        text = GetComponent<Text>();
        rectTrans = GetComponent<RectTransform>();
        originalPos = rectTrans.anchoredPosition;
        Appear();
    }

    // Update is called once per frame
    void Update()
    {
        float t;
        switch (currentState) {   
            case CurrentState.Appear:
                t = Mathf.Clamp(((Time.time - animationStartTime) / appearTime), 0f, 1f);
                t = motionCurve.Evaluate(t);
                rectTrans.anchoredPosition = LerpWithoutClamp(originalPos + new Vector2(200f, 0f), originalPos, t);
                text.color = Color.Lerp(Color.clear, myColor, t);
                break;
            case CurrentState.Hide:
                break;
            case CurrentState.Kill:
                t = Mathf.Clamp(((Time.time - animationStartTime) / killTime), 0f, 1f);
                //t = motionCurve.Evaluate(t);
                rectTrans.anchoredPosition = Vector2.Lerp(originalPos, originalPos + new Vector2(200f, 0f) * Mathf.Sign(t), Mathf.Abs(t));
                text.color = Color.Lerp(myColor, Color.clear, t);
                if(t == 1)
                    GameObject.Destroy(gameObject);
                break;
        }
    }

    public void Appear () {
        animationStartTime = Time.time;
        currentState = CurrentState.Appear;
    }
    public void Hide () {
        currentState = CurrentState.Hide;
    }
    public void Kill () {
        animationStartTime = Time.time;
        currentState = CurrentState.Kill;
    }
    public void Kill (float delay) { // Overload includes delay before kill starts.
        animationStartTime = Time.time + delay;
        currentState = CurrentState.Kill;
    }


    private Vector2 LerpWithoutClamp(Vector2 A, Vector2 B, float t)
    {
        return A + (B-A)*t;
    }
}

