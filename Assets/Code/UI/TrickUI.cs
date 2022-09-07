using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrickUI : MonoBehaviour
{
    public Canvas canvas;
    public Color generalColor;

    public RectTransform underline;
    public Text distanceTraveled; // * Has seperate color from the rest. *
    public Text totalScore;
    public Text chainScore; // Score of current chain with multiplier.
    private float chainScoreF; // Float keeping track of chain score.
    private int chainTrickCount; // How many tricks in this chain.
    public Font textFont;
    public Vector2 startingPosition = new Vector2(-200f, 0f);
    public Vector2 startingSize = new Vector2(300f, 50f);
    public float verticalSpacing = -50f; // Space between each trick text on Y axis.
    public int fontSize = 24;
    public int maxTrickCount = 20; // The maximum amount of trick text we fit at a time.

    public float killDelay = 0.1f; // Delay in between each kill. (Creates a smooth gradual fade of each text item)

    public AnimationCurve motionCurve;
    private int trickCount;
    private float longestTrickText; // The pixel length of the longest trick text. Used for the underline at the bottom.

    private Color underlineColorShown;
    private Color underlineColorHidden;
    private bool underlineIsShown; // Is the underline shown?
    private float underlineFadeDuration = 0.2f;
    private float underlineStartTime;  // Time when underline started changing color. (Fade)
    private float underlineTop;

    RectTransform[] trickRects;

    void Start () {
        trickRects = new RectTransform[maxTrickCount];
        underlineColorShown = generalColor;
        underlineColorHidden = new Color(underlineColorShown.r, underlineColorShown.g, underlineColorShown.b, 0f);
        underlineTop = totalScore.rectTransform.anchoredPosition.y;

        totalScore.color = generalColor;
    }

    void Update()
    {
        if(underlineIsShown) {
            underline.GetComponent<Image>().color = Color.Lerp(underlineColorHidden, underlineColorShown, (Time.time - underlineStartTime) / underlineFadeDuration);
            chainScore.color = Color.Lerp(underlineColorHidden, underlineColorShown, (Time.time - underlineStartTime) / (underlineFadeDuration * 2f));
        }
        else {
            underline.GetComponent<Image>().color = Color.Lerp(underlineColorShown, underlineColorHidden, (Time.time - underlineStartTime) / underlineFadeDuration);
            AnimateChainScoreText();
        }
    }

    public void UpdateScore (float score) {
        totalScore.text = ((int)score).ToString();
    } 
    public void UpdateChain (float score, int mult) {
        chainScore.text = ((int)score).ToString() + " x " + mult.ToString();
        chainScoreF = score;
        chainTrickCount = mult;
    }   

    public void ClearTricks () {
        for(int i = 0; i < trickCount; i++) {
            if(trickRects[i])
                trickRects[i].GetComponent<TrickTextAnimator>().Kill(i * killDelay);
        }
        trickCount = 0;
        longestTrickText = 0;
        HideUnderline();
    }

    public void AddTrick (string name) {
        trickCount++;
        // FIFO - if we reached max trick count, then remove first text obj, move all others back one index and make space for a new one at the end of the array.
        if(trickCount > maxTrickCount) {
            for(int i = 0; i < maxTrickCount; i++) {
                if(i == 0) {
                    GameObject.Destroy(trickRects[0].gameObject);
                }
                else {
                    TrickTextAnimator animator = trickRects[i].GetComponent<TrickTextAnimator>();
                    trickRects[i].anchoredPosition = new Vector2(animator.originalPos.x, animator.originalPos.y - verticalSpacing);
                    animator.originalPos = trickRects[i].anchoredPosition;
                    trickRects[i-1] = trickRects[i];
                }
            }
            trickCount = maxTrickCount;
        }
        // Create new object and add components.
        GameObject newTrick = new GameObject("Trick " + (trickCount));
        newTrick.transform.SetParent(canvas.transform);
            
        CanvasRenderer cRenderer = newTrick.AddComponent<CanvasRenderer>();
        RectTransform rectTrans = newTrick.AddComponent<RectTransform>();
        Text newText = newTrick.AddComponent<Text>();

        // Set up Rect Transform
        rectTrans.pivot = new Vector2(0.5f, 0.5f);
        rectTrans.anchorMax = new Vector2(1f, 1f);
        rectTrans.anchorMin = new Vector2(1f, 1f);
        rectTrans.anchoredPosition = new Vector2(0f, verticalSpacing * trickCount) + startingPosition;
        rectTrans.sizeDelta = startingSize;
        trickRects[trickCount-1] = rectTrans;

        // Set up text
        newText.font = textFont;
        newText.fontSize = fontSize;
        newText.color = Color.clear;
        newText.transform.localScale = Vector3.one;

        if(trickCount > 1)
            newText.text = ("+ " + name);
        else
            newText.text = (name);  
        if(newText.preferredWidth > longestTrickText)
            longestTrickText = newText.preferredWidth;
        newText.alignment = TextAnchor.MiddleRight; 

        // Initiate Animator
        TrickTextAnimator newTrickAnimation = newTrick.AddComponent<TrickTextAnimator>();
        newTrickAnimation.verticalSpacing = verticalSpacing; 
        newTrickAnimation.motionCurve = motionCurve;
        newTrickAnimation.myColor = generalColor;
        newTrickAnimation.killTime = 0.3f;
        newTrickAnimation.Initialize();

        // Underline
        AdjustUnderline(longestTrickText, rectTrans.anchoredPosition.y + verticalSpacing / 1.5f);

    }

    void AnimateChainScoreText () { // Also moves the underline (Chain score's parent) up.
        float newY = Mathf.Lerp(underline.anchoredPosition.y, underlineTop, 3f * Time.deltaTime);
        underline.anchoredPosition = new Vector2(underline.anchoredPosition.x, newY);
        chainScore.text = ((int)chainScoreF * chainTrickCount).ToString();
        chainScore.color = Color.Lerp(underlineColorShown, underlineColorHidden, (Time.time - underlineStartTime) / (underlineFadeDuration * 5f));
    }

    void AdjustUnderline (float newSize, float yPos) {
        ShowUnderline();
        underline.sizeDelta = new Vector2(newSize, underline.sizeDelta.y);
        underline.anchoredPosition = new Vector2(startingPosition.x + ((200f - newSize) /2f) + (startingSize.x - 200f) / 2f, yPos);
    }
    void HideUnderline () {
        if(!underlineIsShown)
            return;
        underlineIsShown = false;
        underlineStartTime = Time.time;
    }
    void ShowUnderline () {
        if(underlineIsShown)
            return;
        underlineIsShown = true;
        underlineStartTime = Time.time;
    }
}
