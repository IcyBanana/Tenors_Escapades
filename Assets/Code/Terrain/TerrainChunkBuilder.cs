using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkBuilder : MonoBehaviour
{
    /* << Class Description >>   **Check Known bugs at bottom
    This builds a chunk of randomly generated terrain for the player to snowboard on.
    STEPS:
    1. Generate a 'spline' - a lengthy, curvy one dimensional line consisting of vertices. This spline will act as a guide to build smooth hills around, and it will define the peaks and valleys of terrain
        Generation is based on curves in an animation curve library. (See more at: << TerrainCurvesLibrary.cs >>)
    2. Duplicate multiple times in each Z direction (back and forward), each time slighindexy lowering its Y coordinate. This should create a nice hilly 3D effect.
    3. Connect the multiple splines together and form triangles and faces from their vertices to complete our 3D mesh.
    */

    public bool isFirstChunk = false; // Is this the first chunk in the game?
    public bool isPremade = false; // If true, this chunk is already premade and saved as is. No spline/mesh/col generation will take place.

    public float chunkSize = 400; // The length of our chunk from 1st vertex to last vertex on the X axis.
    public int vertexCountPerSpline = 400; // How many vertices in this chunk per spline. This affects detail, the higher the count the better the resolution of terrain.
    public float spawnCutoff = 40f; // Spawn cutoff on either side. Regular objects (trees, houses, rocks etc) won't spawn within the cutoff on either edge of the terrain.


    public float maxSizeBeforeChasm = 50f;

    public GameObject foregroundTerrain; // This will be a 2D plane mesh that copies our terrain's spline for visuals. See more at function: < GenerateForegroundTerrain() >
    public float foregroundCutoff = 50f; // Units in X axis. The foreground terrain lerps downwards towards the edges of the main terrain. This is the distance through which it does this.
    private GameObject spawnedForegroundTerrain; // This is filled when we generate the foreground terrain.

    public float chasmSize = 5f; // Size of chasm after this chunk ends.
    public GameObject chasmCameraTrigger;
    public GameObject chasmPrefabStart;
    public GameObject chasmPrefabEnd;

    private GameManager gameManager;
    private TerrainCurvesLibrary curvesLibrary; // The curves library from which we build our terrain.

    private Vector3[] mainSpline; // The main spline, off of which we make the mesh, and the collider. This is what the player travels on.

    private Mesh myMesh; // The final terrain mesh.
    private Vector3[] myVertices;
    private int[] myTriangles;

    private List<GameObject> spawnedObjects;

    void Awake() {
        spawnedObjects = new List<GameObject>();
        
    }


    public void Initialize(TerrainCurvesLibrary tcl, GameManager gameManager) {
        if(isPremade)
            return;
        curvesLibrary = tcl;
        this.gameManager = gameManager;

        vertexCountPerSpline = (int)chunkSize * 6;

        GenerateSpline();
        GenerateForegroundTerrain();
        GenerateMeshOneSided_MaskReady(48.25f, 2f, 15f);
        GetComponent<MeshFilter>().mesh = myMesh;
        CreateEdgeCollider();

        GetComponent<MeshRenderer>().material.SetVector("_maskXPos", GetStartEnd()); // Set mask position for the chasm.
        //ExtendMeshForMasking(15f);
        float lastPointY = GetHeightOfLastPoint();
        spawnedObjects.Add(Instantiate(chasmPrefabStart, new Vector3(transform.position.x + chunkSize, lastPointY, 0f), Quaternion.identity));
        GameObject camTrigger = Instantiate(chasmCameraTrigger, new Vector3(transform.position.x + chunkSize + chasmSize / 2f, lastPointY, 0f), Quaternion.identity);
        camTrigger.GetComponent<ChasmTrigger>().Initialize(chasmSize, this);
        spawnedObjects.Add(camTrigger);
        if(!isFirstChunk)
            spawnedObjects.Add(Instantiate(chasmPrefabEnd, transform.position, Quaternion.identity));
    }

    public Vector2 GetNormal2D (float xPos) { // Returns terrain's normal at world X position.
        Vector2 normal = Vector2.zero;
        int ratio = vertexCountPerSpline / (int)chunkSize; // How many vertices per unit of length on the x axis.
        
        xPos -= transform.position.x; // Localize world x position.

        int vCenter = (int)xPos * ratio;
        Vector2 leftToRight = (myVertices[vCenter + 1] - myVertices[vCenter - 1]).normalized;
        
        normal = new Vector2(-leftToRight.y, leftToRight.x);


        return normal;
    }

    public void Despawn () {
        GameObject.Destroy(spawnedForegroundTerrain);
        foreach(GameObject gObj in spawnedObjects) {
            GameObject.Destroy(gObj);
        }
        GameObject.Destroy(gameObject);
    }

    void GenerateSpline () {
        AnimationCurve currentCurve;
        Vector4 currentCurveData;

        if(isFirstChunk) {
            currentCurve = curvesLibrary.curves[0]; // The current curve we'll be working on, henceforth known as 'cc' for the rest of the variables.
            currentCurveData = curvesLibrary.curveData[0];
        }
        else {
            currentCurve = curvesLibrary.GetChasmCurveAfter(out currentCurveData);
        }
        float ccLength = Random.Range(currentCurveData.x, currentCurveData.y); // Length of our current curve on X axis.
        float ccMagnitude = Random.Range(currentCurveData.z, currentCurveData.w); // How high/low the current curve can stretch.   
        float ccIndex = 0; // Which vertex we are at on the current curve.

        float xPos = 0f; // The X position used for each vertex placement. Starting from transform.position. This will linearly increase as we move through the spline.
        float baseY = 0f; // The base Y position. This is the '0' of each curve. From this position we add or subtract on the Y axis to lay out our curve. On every new curve the baseY changes.
        float yPos = baseY;
        float xRemainder = chunkSize;

        mainSpline = new Vector3[vertexCountPerSpline];
        float xAdd = chunkSize / vertexCountPerSpline; // This multiplier shows us the relation between vertices and world X axis. If the space between vertices is 1, it's 1. If the space is 0.25 it's 0.25.

        for(int i = 0; i < vertexCountPerSpline; i++) {
            yPos = baseY;

            // DRAW CURRENT CURVE
            if(ccIndex < ccLength) {
                yPos += currentCurve.Evaluate(ccIndex / ccLength) * ccMagnitude;
                ccIndex += 1f * xAdd;
            }
            // SELECT NEW CURVE
            else {
                baseY += currentCurve.Evaluate(1f) * ccMagnitude; // Offset the base Y to the end of our current curve
                yPos = baseY; // The Y pos of the first vertex
                if(xRemainder > maxSizeBeforeChasm) {            
                    currentCurve = curvesLibrary.GetNewCurve();
                    currentCurveData = curvesLibrary.GetCurveData();
                    ccLength = Random.Range(currentCurveData.x, currentCurveData.y);
                    ccMagnitude = Random.Range(currentCurveData.z, currentCurveData.w);

                    Vector3 archGrindData = curvesLibrary.GetArchGrindData();
                    if(archGrindData != Vector3.zero) {
                        Vector2 archBounds = new Vector2(xPos + xAdd + ccLength * archGrindData.y + transform.position.x, xPos + xAdd + ccLength * archGrindData.z + transform.position.x);
                        if(archBounds.y <= transform.position.x + chunkSize) // Check if the bounds of the arch fit within our chunk
                            gameManager.SpawnArchGrind(archBounds);
                    }  
                }
                else {
                    currentCurve = curvesLibrary.GetChasmCurveBefore(out currentCurveData, xRemainder);
                    ccLength = xRemainder;
                    
                    ccMagnitude = currentCurveData.z + currentCurveData.z * ((ccLength - currentCurveData.x) / currentCurveData.x);
                    chasmSize = currentCurveData.w;
                }

                ccIndex = 0;
            }

            mainSpline[i] = new Vector3(xPos, yPos, 0f);

            xPos += xAdd; // Advance in X position
            xRemainder -= xAdd;
        }
    }

    void GenerateMeshOneSided_MaskReady (float zDelta, float power, float elongation) { // zDelta - Z axis displacement per extra spline; power - formula for y displacement; Elongation - extra length at mesh end for masking.(Chasms)
        // VERTEX GENERATION
        myMesh = new Mesh();
        myVertices = new Vector3[vertexCountPerSpline * 1 + vertexCountPerSpline + 4];
        int index = 0;
        for(int i = 0; i < 1; i++) {
            for(int j = 0; j < vertexCountPerSpline; j++) {
                float zMove = zDelta * (1 - i);
                myVertices[index] = mainSpline[j] + new Vector3(0f, -Mathf.Pow(zMove * 0.12f, power), -zMove);
                index++;
            }
        }

        for(int i = 0; i < vertexCountPerSpline; i++) {
            myVertices[index] = mainSpline[i];
            index++;
        }

        // TRIANGLE GENERATION
        int arraySize = 1 * (vertexCountPerSpline - 1) * 6 + 12;
        myTriangles = new int[arraySize];
        int t = 0;
        for(int i = 0; i < 1; i++) {
            for(int j = 0; j < vertexCountPerSpline - 1; j++) {
                myTriangles[t] = j + i * vertexCountPerSpline;
                myTriangles[t+1] = j + vertexCountPerSpline + i * vertexCountPerSpline;
                myTriangles[t+2] = j + 1 + i * vertexCountPerSpline;

                myTriangles[t+3] = j + 1 + i * vertexCountPerSpline;
                myTriangles[t+4] = j + vertexCountPerSpline + i * vertexCountPerSpline;
                myTriangles[t+5] = j + vertexCountPerSpline + 1 + i * vertexCountPerSpline;
                t += 6;
            }
        }

        myVertices[index] = myVertices[0] + new Vector3(-elongation, 0f, 0f);
        myVertices[index+1] = myVertices[vertexCountPerSpline] + new Vector3(-elongation, 0f, 0f);
        myVertices[index+2] = myVertices[vertexCountPerSpline - 1] + new Vector3(elongation, 0f, 0f);
        myVertices[index+3] = myVertices[index - 1] + new Vector3(elongation, 0f, 0f);

        myTriangles[t] = index;
        myTriangles[t+1] = index + 1;
        myTriangles[t+2] = 0;
        myTriangles[t+3] = 0;
        myTriangles[t+4] = index + 1;
        myTriangles[t+5] = vertexCountPerSpline;

        myTriangles[t+6] = vertexCountPerSpline - 1;
        myTriangles[t+7] = index - 1;
        myTriangles[t+8] = index + 2;
        myTriangles[t+9] = index + 2;
        myTriangles[t+10] = index - 1;
        myTriangles[t+11] = index + 3;



        // MESH GENERATION
        myMesh.vertices = myVertices;
        myMesh.triangles = myTriangles;
        Vector3[] normals = new Vector3[myVertices.Length];
        myMesh.RecalculateNormals();
        for(int i = 0; i < normals.Length; i++) {
            normals[i] = Vector3.up;
        }
        myMesh.normals = normals;

    }

    void GenerateMeshOneSided (int sCount, float zDelta, float power) { // sCount - amount of extra splines (Per side); zDelta - Z axis displacement per extra spline; power - formula for y displacement
        // VERTEX GENERATION
        myMesh = new Mesh();
        myVertices = new Vector3[vertexCountPerSpline * sCount + vertexCountPerSpline];
        int index = 0;
        for(int i = 0; i < sCount; i++) {
            for(int j = 0; j < vertexCountPerSpline; j++) {
                float zMove = zDelta * (sCount - i);
                if(i > sCount / 2)
                    myVertices[index] = mainSpline[j] + new Vector3(0f, -Mathf.Pow(zMove * 0.85f, power), -zMove);
                else
                    myVertices[index] = mainSpline[j] + new Vector3(0f, -Mathf.Pow(zMove * 1.25f, power), -zDelta * sCount / 2);
                index++;
            }
        }

        for(int i = 0; i < vertexCountPerSpline; i++) {
            myVertices[index] = mainSpline[i];
            index++;
        }


        // TRIANGLE GENERATION
        int arraySize = sCount * (vertexCountPerSpline - 1) * 6;
        myTriangles = new int[arraySize];
        int t = 0;
        for(int i = 0; i < sCount; i++) {
            for(int j = 0; j < vertexCountPerSpline - 1; j++) {
                myTriangles[t] = j + i * vertexCountPerSpline;
                myTriangles[t+1] = j + vertexCountPerSpline + i * vertexCountPerSpline;
                myTriangles[t+2] = j + 1 + i * vertexCountPerSpline;

                myTriangles[t+3] = j + 1 + i * vertexCountPerSpline;
                myTriangles[t+4] = j + vertexCountPerSpline + i * vertexCountPerSpline;
                myTriangles[t+5] = j + vertexCountPerSpline + 1 + i * vertexCountPerSpline;
                t += 6;
            }
        }

        // MESH GENERATION
        myMesh.vertices = myVertices;
        myMesh.triangles = myTriangles;
       /* Vector3[] normals = new Vector3[myVertices.Length];
        for(int i = 0; i < normals.Length; i++) {
            normals[i] = (Vector3.up - Vector3.forward).normalized;
        }
        myMesh.normals = normals;*/
        myMesh.RecalculateNormals();
    }

    void GenerateMesh (int sCount, float zDelta, float power) { // sCount - amount of extra splines (Per side); zDelta - Z axis displacement per extra spline; power - formula for y displacement
        // VERTEX GENERATION
        myMesh = new Mesh();
        myVertices = new Vector3[vertexCountPerSpline * sCount * 2 + vertexCountPerSpline];
        int index = 0;
        for(int i = 0; i < sCount; i++) {
            for(int j = 0; j < vertexCountPerSpline; j++) {
                float zMove = zDelta * (sCount - i);
                myVertices[index] = mainSpline[j] + new Vector3(0f, -Mathf.Pow(zMove * 0.1f, power), -zMove);
                index++;
            }
        }

        for(int i = 0; i < vertexCountPerSpline; i++) {
            myVertices[index] = mainSpline[i];
            index++;
        }

        for(int i = 0; i < sCount; i++) {
            for(int j = 0; j < vertexCountPerSpline; j++) {
                float zMove = zDelta * (i + 1);
                myVertices[index] = mainSpline[j] + new Vector3(0f, -Mathf.Pow(zMove * 0.1f, power), zMove);
                index++;
            }
        }


        // TRIANGLE GENERATION
        int arraySize = sCount * 2 * (vertexCountPerSpline - 1) * 6;
        myTriangles = new int[arraySize];
        int t = 0;
        for(int i = 0; i < sCount * 2; i++) {
            for(int j = 0; j < vertexCountPerSpline - 1; j++) {
                myTriangles[t] = j + i * vertexCountPerSpline;
                myTriangles[t+1] = j + vertexCountPerSpline + i * vertexCountPerSpline;
                myTriangles[t+2] = j + 1 + i * vertexCountPerSpline;

                myTriangles[t+3] = j + 1 + i * vertexCountPerSpline;
                myTriangles[t+4] = j + vertexCountPerSpline + i * vertexCountPerSpline;
                myTriangles[t+5] = j + vertexCountPerSpline + 1 + i * vertexCountPerSpline;
                t += 6;
            }
        }

        // MESH GENERATION
        myMesh.vertices = myVertices;
        myMesh.triangles = myTriangles;
        myMesh.RecalculateNormals();
    }

    void CreateEdgeCollider () {
        EdgeCollider2D ourCollider = GetComponent<EdgeCollider2D>();
        Vector2[] newPoints = new Vector2[mainSpline.Length];
        int i = 0;
        foreach(Vector3 pos in mainSpline) {
            newPoints[i] = pos;
            i++;
        }
        ourCollider.points = newPoints;
    }

    public Vector2 GetStartEnd () { // Returns the X coordinates of the first and last vertex on the main spline. Vector2(first, last).
        return new Vector2(transform.position.x, transform.position.x + chunkSize);
    }

    public float GetHeightOfLastPoint() { // Returns the Y coordinate of the last vertex in the main spline.
        return mainSpline[mainSpline.Length - 1].y + transform.position.y;
    }

    public float GetHeightOnSpline (float xPos) { // Recieves an X Position in world coordinates. Returns the height of a vertex in a given index on the main spline.
    int vertex = (int)((xPos - transform.position.x) * vertexCountPerSpline / chunkSize);
    if(vertex < 0)
        vertex = 0;
    if(vertex >= 0 && vertex < mainSpline.Length)
        return mainSpline[vertex].y + transform.position.y;
    else
        return 10000f;
    }

    public float[] GetHeightOnSpline (float xStart, float xEnd) { // Returns the height of the spline for every vertex between xStart and xEnd (Inclusive) (World coordinates).
        xStart -= transform.position.x;
        xEnd -= transform.position.x;
        int vertexStart = (int)(xStart * vertexCountPerSpline / chunkSize);
        int vertexEnd = (int)(xEnd * vertexCountPerSpline / chunkSize);
        float[] returnedHeight = new float[vertexEnd - vertexStart + 1];
        for(int i = 0; i < returnedHeight.Length; i++) {
            if(i + vertexStart >= mainSpline.Length) {
                vertexStart--;
            }
            else if(i + vertexStart < 0) {
                vertexStart++;
            }
            returnedHeight[i] = mainSpline[i + vertexStart].y + transform.position.y;
        }
        return returnedHeight;
    }

    void GenerateForegroundTerrain () {
        int sCount = 2;
        Mesh newMesh = new Mesh();
        Vector3[] vertices = new Vector3[vertexCountPerSpline * sCount];
        Vector3[] normals = new Vector3[vertices.Length];
        spawnedForegroundTerrain = Instantiate(foregroundTerrain, transform.position, Quaternion.identity);
        spawnedForegroundTerrain.transform.position -= Vector3.forward * 15f;
        spawnedForegroundTerrain.transform.position -= Vector3.up * 2f;
        spawnedForegroundTerrain.transform.position += Vector3.right * 1.6f;
        int index = 0;
        float fadeMult = 0f;
        for(int i = 0; i < sCount; i++) {
            for(int j = 0; j < vertexCountPerSpline; j++) {
                float xPos = mainSpline[j].x;
                if(xPos <= foregroundCutoff) {
                    fadeMult = Mathf.Pow(1f - (xPos / foregroundCutoff), 2f);
                }
                else if(xPos >= chunkSize - foregroundCutoff) {
                    fadeMult = Mathf.Pow(((xPos - chunkSize + foregroundCutoff) / foregroundCutoff), 2f);
                }
                else {
                    fadeMult = 0f;
                }
                vertices[index] = mainSpline[j] + new Vector3(0f, -200f * i - 50f * fadeMult, 0f);
                index++;
            }
        }


        // TRIANGLE GENERATION
        int arraySize = (sCount - 1) * (vertexCountPerSpline - 1) * 6;
        int[] triangles = new int[arraySize];
        int t = 0;
        
        for(int j = 0; j < vertexCountPerSpline - 1; j++) {
            triangles[t] = j;
            triangles[t+2] = j + vertexCountPerSpline;
            triangles[t+1] = j + 1;

            triangles[t+3] = j + 1;
            triangles[t+5] = j + vertexCountPerSpline;
            triangles[t+4] = j + vertexCountPerSpline + 1;
            t += 6;
        }
        
        // SET ALL NORMALS TO FACE UP (Specifically for this mesh)
        for(int i = 0; i < vertices.Length; i++) {
            normals[i] = Vector3.up;
        }

        // MESH GENERATION
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.normals = normals;
        spawnedForegroundTerrain.GetComponent<MeshFilter>().mesh = newMesh;
    }


    /* ------------------- KNOWN BUGS ------------------
        1. cs:240 GetHeightOnSpline (float xStart, float xEnd): Out of index error at local line 7: "returnedHeight[i] = mainSpline[i + vertexStart].y + transform.position.y;"
            
           << Fixed_Final >>

    */

}
