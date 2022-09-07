using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TerrainGenerator : MonoBehaviour
{
    public float xPosAngle = 0.45f;
    public float length = 0.5f;
    public float inSine = 1f;

    public float firstPerlinResolution = 1f;
    public float firstPerlinMagnitude = 1f;
    public float secondPerlinResolution = 1f;
    public float secondPerlinMagnitude = 5f;
    public float thirdPerlinResolution = 0.02f;
    public float thirdPerlinMagnitude = 2f;



    public SpriteShapeController shape;
    public int scale = 100;
    public int amountOfPoints = 100;
    public int chasmPoints = 20; // The amount of points creating the chasm jump, out of "amountOfPoints" - Ex: 20 means the last 20 points of "amountOfPoints" will have different rules to make a jump
    public float pointDistance = 1;

    private float seed;

    void Awake () {
        shape.spline.SetPosition(2, shape.spline.GetPosition(2) + Vector3.right * scale);
        shape.spline.SetPosition(3, shape.spline.GetPosition(3) + Vector3.right * scale);
        shape.spline.SetPosition(3, shape.spline.GetPosition(3) + Vector3.down * scale * 1.9f);
        shape.spline.SetPosition(0, shape.spline.GetPosition(0) + Vector3.down * scale * 1.9f);

        seed = Random.Range(0f, 1000000f);

        /*for(int i = 0; i < amountOfPoints; i++) {
            float xPos = shape.spline.GetPosition(i + 1).x + pointDistance;
            float yPos = shape.spline.GetPosition(i + 1).y;
            float newInSine = inSine * (Mathf.PerlinNoise(xPos * inSineResolution, 0f));
            float newLength = length * (Mathf.PerlinNoise(xPos * lengthResolution, 0f));
            float newXPosAngle = 5f * (Mathf.Pow(Mathf.PerlinNoise(xPos * xPosResolution, 0f), 2f));
            shape.spline.InsertPointAt(i + 2, new Vector3(xPos, newLength * Mathf.Sin(xPos * newInSine) - newXPosAngle));
            shape.spline.SetTangentMode(i + 2, ShapeTangentMode.Continuous);
            Vector3 leftTangent = shape.spline.GetPosition(i + 1) - shape.spline.GetPosition(i + 2);
            shape.spline.SetLeftTangent(i + 2, leftTangent.normalized * 0.05f);
           // Vector3 rightTangent = shape.spline.GetPosition(i + 3) - shape.spline.GetPosition(i + 2);
            shape.spline.SetRightTangent(i + 2, -leftTangent.normalized * 0.05f);
        }*/
        for(int i = 0; i < amountOfPoints; i++) {
            float xPos = shape.spline.GetPosition(i + 1).x + pointDistance;
            float yPos = shape.spline.GetPosition(i + 1).y;
            
            if(i < amountOfPoints - chasmPoints) {
                yPos -= firstPerlinMagnitude * Mathf.Pow(Mathf.PerlinNoise((xPos + transform.position.x + seed) * firstPerlinResolution, 0f), 2f);
                yPos -= secondPerlinMagnitude * Mathf.Pow(Mathf.PerlinNoise((xPos + transform.position.x + seed) * secondPerlinResolution, 0f), 7f);
                yPos -= thirdPerlinMagnitude * Mathf.Pow(Mathf.PerlinNoise((xPos + transform.position.x + seed) * thirdPerlinResolution, 0f), 3f);
                yPos += 0.3f;

                shape.spline.InsertPointAt(i + 2, new Vector3(xPos, yPos));
                shape.spline.SetTangentMode(i + 2, ShapeTangentMode.Continuous);
                Vector3 leftTangent = shape.spline.GetPosition(i + 1) - shape.spline.GetPosition(i + 2);
                shape.spline.SetLeftTangent(i + 2, leftTangent.normalized * 0.5f);
                // Vector3 rightTangent = shape.spline.GetPosition(i + 3) - shape.spline.GetPosition(i + 2);
                shape.spline.SetRightTangent(i + 2, -leftTangent.normalized * 0.5f);
            }
            else { 
                yPos += 1.2f * (chasmPoints - (amountOfPoints - i)) / chasmPoints;
                shape.spline.InsertPointAt(i + 2, new Vector3(xPos, yPos));
                shape.spline.SetTangentMode(i + 2, ShapeTangentMode.Continuous);
                Vector3 leftTangent = shape.spline.GetPosition(i + 1) - shape.spline.GetPosition(i + 2);
                shape.spline.SetLeftTangent(i + 2, leftTangent.normalized * 0.5f);
                // Vector3 rightTangent = shape.spline.GetPosition(i + 3) - shape.spline.GetPosition(i + 2);
                shape.spline.SetRightTangent(i + 2, -leftTangent.normalized * 0.5f);
            }
        }
        shape.spline.SetPosition(amountOfPoints + 2, shape.spline.GetPosition(amountOfPoints + 1) - Vector3.up);

    }

    public Vector2 GetCurrentTerrainNormal (float x) {
        int i = (int)x;
        Vector3 leftTangent = shape.spline.GetLeftTangent(i).normalized;
        Vector3 normal = Quaternion.AngleAxis(-90f, Vector3.forward) * leftTangent;
        return new Vector2(normal.x, normal.y);
    }

    void Update () {
        /*if(Input.GetKeyDown("p"))
            UpdatePoints();*/
    }

    public Vector2 GetStartEnd () { // Returns X values of first and last point on the terrain's surface
        Vector2 startEnd = new Vector2(transform.position.x, transform.position.x + scale);
        return startEnd;
    }

    public float GetHeightOfLastPoint () { // Returns the Y of the last point on the terrain's surface
        return transform.position.y + shape.spline.GetPosition(amountOfPoints + 1).y;
    }

    /*void UpdatePoints () {
        for(int i = 0; i < amountOfPoints; i++) {
            float xPos = shape.spline.GetPosition(i + 2).x;
            float yPos = shape.spline.GetPosition(i + 2).y;
            shape.spline.SetPosition(i + 2, new Vector3(xPos, PerlinNoise(xPos)));
            
        }
    }*/

   /* float PerlinNoise (float xPos) {
        float y = 0f;
        int i = 0;
        foreach(float scale in noiseScales) {
            float displacement = yNoiseScales[i] * Mathf.PerlinNoise(xPos / 9f, 0f);
            y += Mathf.Sin(xPos * scale) * displacement - xPos * xPosAngle;
            i++;
        }

        return y;
    }*/


}
