using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCurvesLibrary : MonoBehaviour
{
    /* << Class Description >>
    Holds arrays of Animation Curves and Vector4s that are used to draw out the main terrain's surface on which the player moves. Every curve in the array is paired to a Vector4 that determines its
    dimensions. 
    The standard curves are selected based on probability read from a float array of probabilities, that is also paired to the curve array (Same indices).
    Holds arrays of chasm-specific curves that are selected at random, without probabilities.
    Archway grinds are built using this library's "archGrind" Vector3 array.
    */

    /* Standard
    The standard curves used for most of the terrain.
    */
    public AnimationCurve[] curves; // Main curve array for most of the terrain.
    public Vector4[] curveData; // Curve data in order - X: Min length, Y: Max length, Z: Min magnitude, W: Max magnitude
    public float[] curveProbabilities; // The probability of this curve being selected.
    public float probabilitySum; // The sum of all curve probabilities.
    
    /* Arch
    Archway grinds - A Vector3 array directly tied (Same indexing) with the standard curve array. We use this to determine if we spawn an archway grind on that curve.
    */
    public Vector3[] archGrind; // X - 0 or 1: Does the curve have an arch grind on it?. Y - 0 to 1: Start position of grind on the curve. Z - 0 to 1: End position on the curve.

    /* Chasm
    Two extra animation curve arrays for building the terrain that leads into, and out of a chasm. These are selected by terrain chunks at their birth. One is drawn from here to start the chunk, and one is selected to be
    used at the end.
    These hold their own seprate vector4 data arrays.
    */
    public AnimationCurve[] curvesChasmBefore; // Curve array specifically for the part that comes before a chasm. Last curve on a chunk.
    public AnimationCurve[] curvesChasmAfter; // After chasm. First curve on a new chunk.
    public Vector4[] curveDataChasmBefore;
    public Vector4[] curveDataChasmAfter;

    // Indices
    private int newCurveIndex; // This stores the index of the last curve that was asked of the library. This is to determine which curve to give out data for after giving the curve in GetNewCurve().
    private int chasmIndex; // This index refers to a given chasm curve.

    // Tutorial
    [Header("Tutorial Parameters")]
    public bool isTutorial = false; // If in tutorial we're using a tailored library. Select new curves by going through the list one by one, without randomness.
    private int tutorialIndex = 1;  // Index of tutorial curves. We start at 1, because 0 is reserved for the starting area curve.


    // REGULAR CURVE
    public AnimationCurve GetNewCurve () {
        if(isTutorial)
            return GetNewCurveTutorial();
        
        float probValue = Random.Range(0f, probabilitySum);
        float probSum = 0f; // We keep summing up probabilities we went over.
        newCurveIndex = 0;
        foreach(float p in curveProbabilities) {
            probSum += p;
            if(probValue <= probSum) 
                break;
            else 
                newCurveIndex++;
                
        }
        return curves[newCurveIndex];
    }
    
    // TUTORIAL CURVE
    public AnimationCurve GetNewCurveTutorial () {
        int i = tutorialIndex;
        if(i >= curves.Length) {
            i = 1;
            tutorialIndex = 1;
        }
        else {
            tutorialIndex++;
        }
        newCurveIndex = i; // Index to be used in other methods within this class. 
        return curves[i];
    }

    // CHASM METHODS
    public AnimationCurve GetChasmCurveBefore () { // Randomizes index for chasm and returns the before.
        chasmIndex = Random.Range(0, curvesChasmBefore.Length);
        return curvesChasmBefore[chasmIndex];
    }
    public AnimationCurve GetChasmCurveBefore (out Vector4 curveData) { // Overflow includes an output of curve data.
        chasmIndex = Random.Range(0, curvesChasmBefore.Length);
        curveData = curveDataChasmBefore[chasmIndex];
        return curvesChasmBefore[chasmIndex];
    }
    public AnimationCurve GetChasmCurveBefore (out Vector4 curveData, float length) { // Overflow includes an output of curve data and picks curve by length.
        int i = 0;
        float x;
        foreach(Vector4 curveVector in curveDataChasmBefore) {
            x = curveVector.y; // Max length
            if(x >= length) {
                chasmIndex = i;
                break;
            }
            i++;
        }
        if(chasmIndex != i) {
            Debug.LogError("Error in TerrainCurvesLibrary.cs -> GetChasmCurveBefore: Couldn't find curve large enough for remainder of terrain chunk.");
            chasmIndex = i;
        }   
        curveData = curveDataChasmBefore[chasmIndex];
        return curvesChasmBefore[chasmIndex];
    }

    public AnimationCurve GetChasmCurveAfter () { // Uses the stored index for chasm to return the after.
        return curvesChasmAfter[chasmIndex];
    }
    public AnimationCurve GetChasmCurveAfter (out Vector4 curveData) { // Overflow includes an output of curve data.
        curveData = curveDataChasmAfter[chasmIndex];
        return curvesChasmAfter[chasmIndex];
    }

    // DATA METHODS
    public Vector4 GetCurveData () {
        return curveData[newCurveIndex];
    }
    public Vector3 GetArchGrindData () {
        if(archGrind[newCurveIndex].x == 1f)
            return archGrind[newCurveIndex];
        else
            return Vector3.zero;
    }

    // VALIDATION
    void OnValidate() {

        // VALIDATE PROBABILITY SUM
        float sum = 0f;
        foreach(float p in curveProbabilities) {
            sum += p;
        }
        probabilitySum = sum;

        // VALIDATE ARCH GRIND VECTOR3
        for(int x = 0; x < archGrind.Length; x++) {
            // Validate: x component is either a 0 or a 1 (Acts as a boolean)
            if(archGrind[x].x < 0f) {
                archGrind[x].x = 0f;
            }
            else if(archGrind[x].x >= 1f) {
                archGrind[x].x = 1f;
            }
            else {
                archGrind[x].x = 0f;
            }
            // Validate: locations are between 0 and 1 on curve.
            if(archGrind[x].y > 1f)
                archGrind[x].y = 1f;
            else if(archGrind[x].y < 0f)
                archGrind[x].y = 0f;

            if(archGrind[x].z > 1f)
                archGrind[x].z = 1f;
            else if(archGrind[x].z < 0f)
                archGrind[x].z = 0f;

            // Validate: start location isn't ahead of end location
            if(archGrind[x].y > archGrind[x].z)
                archGrind[x].y = archGrind[x].z;
        }
    }
}
