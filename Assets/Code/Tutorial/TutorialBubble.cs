using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialBubble : MonoBehaviour
{

    public Vector2 screenPos; // X,Y - coords of anchor point in normalized screen space (0 to 1).
    public string[] strings; // Text for each explain point.
    public Vector2[] sizes; // Sizes of the bubble per string.

    private Vector2 currentSize; // Determined by the string chosen in ChangeText().

    public Text myText;
    private Image myImage;
    public RectTransform myRect;

    void Start () {
        myImage = GetComponent<Image>();
        myRect = GetComponent<RectTransform>();
        Hide();
    }

    public void ChangeText (int index) {
        myText.text = strings[index];
        currentSize = sizes[index];
    }

    public void SetRectPos (Vector2 pos) {
        screenPos = pos;

        myRect.anchorMin = new Vector2(pos.x, pos.y);
        myRect.anchorMax = myRect.anchorMin;

        myRect.sizeDelta = new Vector2(currentSize.x, currentSize.y);
        myText.rectTransform.sizeDelta = myRect.sizeDelta - new Vector2(100f, 50f);
    }

    public void Appear () {
        myImage.enabled = true;
        myText.enabled = true;
    }
    public void Hide () {
        myImage.enabled = false;
        myText.enabled = false;
    }
}
