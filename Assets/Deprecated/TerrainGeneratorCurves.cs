using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TerrainGeneratorCurves : MonoBehaviour
{

    public SpriteShapeController shape;
    public int scale = 100;
    public int amountOfPoints = 100;
    public int chasmPoints = 20; // The amount of points creating the chasm jump, out of "amountOfPoints" - Ex: 20 means the last 20 points of "amountOfPoints" will have different rules to make a jump
    public float pointDistance = 1;

    private TerrainCurvesLibrary curvesLibrary;

    private float seed;

    void Awake () {
        curvesLibrary = GameObject.Find("TerrainCurvesLibrary").GetComponent<TerrainCurvesLibrary>();

        // Create the size of the chunk and set points 1 - 4 to the outer borders
        shape.spline.SetPosition(2, shape.spline.GetPosition(2) + Vector3.right * scale);
        shape.spline.SetPosition(3, shape.spline.GetPosition(3) + Vector3.right * scale);
        shape.spline.SetPosition(3, shape.spline.GetPosition(3) + Vector3.down * scale * 1.9f);
        shape.spline.SetPosition(0, shape.spline.GetPosition(0) + Vector3.down * scale * 1.9f);

        int currentCurveLength = Random.Range(17, 23); // Length of our current curve in amount of points dedicated to it
        float currentCurveMagnitude = Random.Range(2f, 9f);
        AnimationCurve currentCurve = curvesLibrary.curves[0];
        int currentCurveIndex = 0; // Where we are on the current curve
        float yPos = shape.spline.GetPosition(1).y;
        for(int i = 0; i < amountOfPoints; i++) {
            float xPos = shape.spline.GetPosition(i + 1).x + pointDistance;
            float currentY = yPos;
            
            if(currentCurveIndex < currentCurveLength) {
                currentY += currentCurve.Evaluate((float)currentCurveIndex / currentCurveLength) * currentCurveMagnitude;
                currentCurveIndex++;
            }
            else {
                currentCurveLength = Random.Range(17, 23);
                currentCurve = curvesLibrary.curves[Random.Range(0, 4)];
                currentCurveIndex = 0;
                yPos = shape.spline.GetPosition(i + 1).y;
                currentY = yPos;
                currentCurveMagnitude = Random.Range(2f, 8f);
            }

            shape.spline.InsertPointAt(i + 2, new Vector3(xPos, currentY));
            shape.spline.SetTangentMode(i + 2, ShapeTangentMode.Continuous);
            Vector3 leftTangent = shape.spline.GetPosition(i + 1) - shape.spline.GetPosition(i + 2);
            shape.spline.SetLeftTangent(i + 2, leftTangent.normalized * 0.5f);   
            shape.spline.SetRightTangent(i + 2, -leftTangent.normalized * 0.5f);

            /* if(i < amountOfPoints - chasmPoints) {
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
            }*/
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
