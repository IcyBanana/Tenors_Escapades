using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunScript : MonoBehaviour
{
    private Vector2 XZ;

    public Vector2 sunrisePos;
    public Vector2 nighttimePos;
    public Vector2 sunsetPos;

    public Color sunriseColor;
    public Color sunsetColor;

    public float power = 2f; // This will be used to make the sun's travel elliptical.

    private Color currentColor;

    void Start () {
        
    }

    void Update()
    {
        if(XZ.y <= 0f) {
            float xPos = sunrisePos.x * XZ.x;
            float yPos = (-Mathf.Pow((xPos/4), 2) + 12);
            transform.localPosition = new Vector3(xPos, yPos - 1f, transform.localPosition.z);

            float p = 0f;
            if(xPos >= sunsetPos.x - 1.3f)
                p = (xPos + 1.3f - sunsetPos.x);
            currentColor = Color.Lerp(sunriseColor, sunsetColor, Mathf.Pow(p, power));
            GetComponent<SpriteRenderer>().color = Color.Lerp(sunriseColor, sunsetColor, Mathf.Pow(p, power));
        }
        else {
            transform.localPosition = new Vector3(nighttimePos.x, nighttimePos.y, transform.localPosition.z);
        }
    }
    public void SetXZ (Vector2 XZ) {
        this.XZ = XZ;
    }

    public Color GetColor() {
        return currentColor;
    }
}
