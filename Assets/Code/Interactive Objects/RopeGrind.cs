using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RopeGrind : MonoBehaviour
{
    // ROPE GRIND Creation and handling.
    // 1. Create continuous mesh that passes through all anchor positions for the rope grind.
    // 2. Create edge collider using the top vertices.
    // 3. Add mesh filter, renderer, material.

    public Vector2[] segments; // The segment XY positions of the rope grind. Z = 0 always. Minimum 2 segments to create one rope.
    public float lowestY = -1f; // How low the middle of the rope will be set.
    public float power = 1.2f;
    public float width = 0.5f; // Width of the rope (on Y axis).
    public int vertexPerUnit = 1; // How many vertices per unit of distance.
    public Material myMat;

    public Gradient flagColors;

    public GameObject flagPrefab;

    private Vector3[] vertices;
    private PlayerController playerController;
    private EdgeCollider2D myEdgeCollider;

    public bool selfInitialize = false;

    void Start () {
        if(selfInitialize)
            Initialize();
    }

    public void Initialize () {
        GenerateMesh(GetTotalLength());
        CreateEdgeCollider();
        CreateTriggerCollider();
        CreateRenderer();
        AddFlags();
    }

    void FixedUpdate () {
        
    }

    public float GetHeight (float xPos) {
        xPos -= (transform.position.x + segments[0].x); // Localize.
        int x = (int)Mathf.Floor(xPos) + 1; // Floor to control resulting int.
        int x2 = (int)Mathf.Floor(xPos);
        x *= vertexPerUnit;             // Convert using vertex resolution
        x2 *= vertexPerUnit;             // Convert using vertex resolution
        if(vertices[x2].y < vertices[x].y) // We want to return the lowest Y of left and right X positions.
            x = x2;
        if(x < 0f)        // x is behind the rope.
            return 100f;
        if(x >= vertices.Length / 2)
            return 100f;  // x is ahead of the rope.
        
        return vertices[x].y + width + transform.position.y; // Get height of bottom and add width and transform Y pos.
    }

    public Vector2 GetNormal2D (float xPos) {
        Vector2 normal = Vector2.up;

        xPos -= (transform.position.x + segments[0].x); // Localize.
        int x = (int)(xPos); // Floor to make sure we use lowest integer.
        x *= vertexPerUnit;             // Convert using vertex resolution

        if(x < 0)        // x is behind the rope.
            return normal;
        if(x >= vertices.Length / 2) {
            return normal;  ;// x is ahead of the rope.
        }

        if(x == 0)
            normal = Quaternion.AngleAxis(90f, Vector3.forward) * (vertices[vertexPerUnit] - vertices[0]).normalized;
        else    
            normal = Quaternion.AngleAxis(90f, Vector3.forward) * (vertices[x] - vertices[x-vertexPerUnit]).normalized;

        Debug.DrawRay(vertices[x] + Vector3.up * width + transform.position, normal, Color.red); 
        return normal;
    }

    int GetTotalLength() {
        int length = 0;
        for(int i = 0; i < segments.Length - 1; i++) {
            length += (int)segments[i + 1].x - (int)segments[i].x;
        }
        return length;
    }

    void GenerateMesh (int totalLength) {
        MeshFilter myMeshFilter = gameObject.AddComponent<MeshFilter>();
        vertices = new Vector3[vertexPerUnit * totalLength * 2]; // Calculate how many vertices in total for the mesh.
        Mesh newMesh = new Mesh();
        int halfLength = vertices.Length / 2; // Half of the length of vertex array. It's the length of each 'spline' on the mesh.
        int[] triangles = new int[(halfLength - 1) * 6];
        int vIndex = 0;

        // VERTEX GENERATION
        // Create BOTTOM HALF of mesh.
        for(int i = 0; i < segments.Length - 1; i++) {  
            int length = (int)segments[i + 1].x - (int)segments[i].x;
            int vertexCount = vertexPerUnit * length;
            for(int j = 0; j < vertexCount; j++) {
                float yPos = segments[i].y + (segments[i + 1].y - segments[i].y) * j / (vertexCount); // First we set the Y position to linearly interpolate between the current and next segments' Y positions.
                float yMod = Mathf.Abs(((float)j - (float)(vertexCount/2)) / (float)(vertexCount)) * 2f; // Goes from 1 at start of rope to 0 in the middle and back to 1 at the end. Used to create curved interpolation.
                yPos += lowestY * (1f - Mathf.Pow(yMod, power)); // We use a formula to create a curve on the Y offsets using power.
                float xPos = j / vertexPerUnit + segments[i].x; // X position is number of vertices divided by vertex count per unit. Added to the segment's world X pos
                vertices[vIndex] = new Vector3(xPos, yPos, 0.2f); // Set the vertex position.
                vIndex++;
            }
        }
        // Create TOP HALF of mesh.
        for(int i = halfLength; i < vertices.Length; i++) { 
            vertices[i] = vertices[i - halfLength] + Vector3.up * width;
        }

        // TRIANGLE GENERATION
        int t = 0;
        for(int j = 0; j < halfLength - 1; j++) {
            triangles[t] = j;
            triangles[t+1] = j + halfLength;
            triangles[t+2] = j + 1;

            triangles[t+3] = j + 1;
            triangles[t+4] = j + halfLength;
            triangles[t+5] = j + halfLength + 1;
            t += 6;
        }

        // MESH GENERATION
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        Vector2[] uv = new Vector2[vertices.Length];
        for(int i = 0; i < vertices.Length; i++) {
            if(i < halfLength)
                uv[i] = new Vector2(vertices[i].x, 0f);
            else
                uv[i] = new Vector2(vertices[i].x, width);
        }
        newMesh.uv = uv;
        newMesh.RecalculateNormals();

        myMeshFilter.mesh = newMesh;
    }

    void CreateEdgeCollider () {
        EdgeCollider2D col = gameObject.AddComponent<EdgeCollider2D>();
        Vector2[] colPoints = new Vector2[vertices.Length / 2];
        for(int i = 0; i < vertices.Length / 2; i++) {
            colPoints[i] = new Vector2(vertices[i].x, vertices[i].y + width);
        }
        col.points = colPoints;

        myEdgeCollider = col;
    }

    void CreateTriggerCollider () { // Trigger collider used to create one-way platform collider interaction.
        PolygonCollider2D col = gameObject.AddComponent<PolygonCollider2D>();
        Vector2[] colPoints = new Vector2[vertices.Length];
        float heightOffset = -5f;

        for(int i = 0; i < vertices.Length / 2; i++) {
            float xAdd = 0f;
            if(i == 0)
                xAdd = -1.5f;
            else if(i == vertices.Length / 2 - 1)
                xAdd = 1.5f;
            colPoints[i] = new Vector2(vertices[i].x + xAdd, vertices[i].y + width - 0.01f);
        }
        for(int i = 0; i < vertices.Length / 2; i++) {
            colPoints[i + vertices.Length / 2] = new Vector2(vertices[vertices.Length / 2 - i - 1].x, vertices[vertices.Length / 2 - i - 1].y + width + heightOffset);
        }
        col.points = colPoints;
        col.isTrigger = true;
        col.offset += new Vector2(0f, -0.1f);
    }

    void CreateRenderer () {
        MeshRenderer myRenderer = gameObject.AddComponent<MeshRenderer>();
        myRenderer.material = myMat;
        myRenderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    void AddFlags () {
        int i = 0;
        foreach(Vector2 pos in myEdgeCollider.points) {
            if(i == 0) {

            }
            else {
                Vector2 normal = GetNormal2D(pos.x);
                Vector3 spawnPos = new Vector3(pos.x, pos.y, 0.2f) - new Vector3(normal.x, normal.y, 0f) * 0.23f;
                GameObject newFlag = Instantiate(flagPrefab, spawnPos, Quaternion.LookRotation(Vector3.forward, normal));
                newFlag.transform.parent = transform;
                Material mat = newFlag.GetComponentInChildren<MeshRenderer>().material;
                
                mat.color = flagColors.Evaluate((pos.x % 8) / 7f);
            }
            i++;
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(col.isTrigger)
            return;

        if(col.gameObject.layer == 12) {
            col.GetComponent<PlayerController>().SetInsideRopeTrigger(myEdgeCollider, true);
        }
    }
    void OnTriggerExit2D(Collider2D col) {
        if(col.isTrigger)
            return;

        if(col.gameObject.layer == 12) {
            col.GetComponent<PlayerController>().SetInsideRopeTrigger(myEdgeCollider, false);
        }
    }

}
