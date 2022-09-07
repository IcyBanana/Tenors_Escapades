using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchGrind_BACKUP : MonoBehaviour
{
    // Creates a grindable arch structure.
    // 1. Receives a start X and end X position from the spawner.
    // 2. Gets height of main terrain spline on every vertex between start X and end X.
    // 3. Reproduces spline and lifts it over terrain to a given height.
    // 4. Creates edge collider on the created spline so we can grind away!
    // 5. Creates 4 extra copies of the spline. One at the foreground bottom, one at the foreground top (below the central spline), one at the background top and last one at the background bottom.
    // 6. Creates a mesh with the given 5 sets of vertices.
    // 7. Adds a mesh filter, applies mesh, adds mesh renderer and applies proper material.

    public Vector2 xBounds; // Start and end X positions set here. Should be whole numbers, but we'll make a correction just in case.
    public float height; // Height of the main (collide-able) spline.
    public float percentageOfWall; // 0 to 1. This is how high the wall rises up to the triangle shape on the top. When 0 the wall is non existant, when 1 the top of the arch will be completely flat. 
    public float width; // Width of archway on the Z axis - *per side*.
    public TerrainChunkBuilder chunkBuilder; // The code for the builder of the terrain this archway is on. This is to retrieve the main terrain spline heights.
    public Material myMat; // The material we use for the archway's mesh renderer.

   // private Vector3[] mainSpline; // The main spline of the archway.
    private float[] heights;
    private Vector3[] vertices;
    private int startX;
    private int endX;
    void Start () {
        startX = (int)xBounds.x;
        endX = (int)xBounds.y;
        heights = chunkBuilder.GetHeightOnSpline(xBounds.x, xBounds.y);

        CreateEdgeCollider();
        CreateMesh();
        CreateRenderer();
        CreateArches();
    }

    void CreateEdgeCollider () {
        EdgeCollider2D col = gameObject.AddComponent<EdgeCollider2D>();
        Vector2[] newPoints = new Vector2[heights.Length];
        int i = 0;
        foreach(float y in heights) {
            newPoints[i] = new Vector2(startX + i * chunkBuilder.chunkSize / chunkBuilder.vertexCountPerSpline, y + height);
            i++;
        }
        col.points = newPoints;
    }

    void CreateEdgeCollider (int x) {
        EdgeCollider2D col = gameObject.AddComponent<EdgeCollider2D>();
        col.points = new Vector2[heights.Length];
        for(int i = 0; i < heights.Length; i++) {
            col.points[i] = new Vector2(startX + i * chunkBuilder.chunkSize / chunkBuilder.vertexCountPerSpline, heights[i] + height);
        }
    }

    void CreateMesh () {
        MeshFilter myMeshFilter = gameObject.AddComponent<MeshFilter>();
        Mesh newMesh = new Mesh();
        vertices = new Vector3[heights.Length * 5];
        int[] triangles = new int[4 * (heights.Length - 1) * 6]; // For a simple matrix style tri formation: ( <Length of vertex rows> - 1 ) * ( <Amount of rows> - 1 ) * 6.

        // VERTEX GENERATION
        int index = 0;
        for(int i = 0; i < 2; i++) {
            for(int j = 0; j < heights.Length; j++) {
                vertices[index] = new Vector3(startX + j * chunkBuilder.chunkSize / chunkBuilder.vertexCountPerSpline, heights[j] + height * percentageOfWall * i, -width);
                index++;
            }
        }
        for(int j = 0; j < heights.Length; j++) {
            vertices[index] = new Vector3(startX + j * chunkBuilder.chunkSize / chunkBuilder.vertexCountPerSpline, heights[j] + height, 0f);
            index++;
        }
        for(int i = 0; i < 2; i++) {
            for(int j = 0; j < heights.Length; j++) {
                vertices[index] = new Vector3(startX + j * chunkBuilder.chunkSize / chunkBuilder.vertexCountPerSpline, heights[j] + height * percentageOfWall * (1 - i), width);
                index++;
            }
        }

        // TRIANGLE GENERATION
        int t = 0;
        for(int i = 0; i < 4; i++) {
            for(int j = 0; j < heights.Length - 1; j++) {
                triangles[t] = j + i * heights.Length;
                triangles[t+1] = j + heights.Length + i * heights.Length;
                triangles[t+2] = j + 1 + i * heights.Length;

                triangles[t+3] = j + 1 + i * heights.Length;
                triangles[t+4] = j + heights.Length + i * heights.Length;
                triangles[t+5] = j + heights.Length + 1 + i * heights.Length;
                t += 6;
            }
        }

        // MESH GENERATION
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateNormals();
      
        myMeshFilter.mesh = newMesh;
    }

    void CreateRenderer () {
        MeshRenderer myRenderer = gameObject.AddComponent<MeshRenderer>();
        myRenderer.material = myMat;
    }

    void CreateArches () { // Displace the vertices on the first and last (Bottom) splines to create arches.
        float archWidth = 1.2f;
        float columnWidth = 0.7f;
        float firstX = vertices[0].x;
        float processedX = 0f;
        float vertexSpacing = vertices[1].x - vertices[0].x;
        int archIndex = 0;

        for(int i = 0; i < heights.Length; i++) {
            if(processedX > columnWidth / 2f && processedX < columnWidth / 2f + archWidth) {
                float sine = Mathf.Sin((processedX - columnWidth / 2f) * Mathf.PI / archWidth);
                vertices[i] += Vector3.up * height * 0.5f * sine;
                archIndex++;
            }
            else if(processedX > archWidth + columnWidth) {
                processedX = 0f;
                archIndex = 0;
            }
            processedX += vertexSpacing;
        }

        GetComponent<MeshFilter>().mesh.vertices = vertices;
    }


}
