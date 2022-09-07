using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public Transform playerTransform;
    public TerrainChunkBuilder currentChunk; // The current chunk's code
    public TerrainCurvesLibrary TCL;
    public TerrainCurvesLibrary tutorialTCL;
    private TerrainChunkBuilder lastChunk;

    public GameObject chunkPrefab;
    public GameObject obstaclePrefab; // The prefab used for obstacles spawning.
    public bool spawnObstacles = true; // Only relates to first chunk.
    public bool spawnRopes = true;     // Only relates to first chunk.


    public GameObject[] visualObjects; // These are objects we spawn for visual purposes, they serve no gameplay purpose. Trees, ruins, houses etc.
    public GameObject[] trees;
    public GameObject[] interactiveObjects; // Objects that can be interacted with by the player.
    private Vector2 currentChunkStartEnd; // The X values of the current terrain chunk's beginning and end
    private Vector2 lastChunkStartEnd;

    private List<GameObject> currentChunkObjects;
    private List<GameObject> lastChunkObjects;

    [Header("UI Control")]
    public Canvas generalUI;
    public Canvas mainMenuUI;
    public PauseButtonScript pauseButtonScript;

    private float sceneLoadTime;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ttpAudioClip; // Played when we tap to start game.
    public AudioSource OSTAudio;
    public AudioSource introAudio;
    public AudioMixer mainMixer;

    [Header("Tutorial")]
    public bool isTutorial;
    public GameObject tutorialAssets;

    [Header("Hierarchy Transforms")]
    public Transform obstaclesT;
    public Transform visualsT;
    public Transform interactivesT;
    public Transform terrainsT;

    public enum GameState {
        Loading,
        Intro,
        Menu,
        SettingsMenu,
        Started
    }

    private GameState currentGameState;


    void WebGLIsMobile () {
        
    }

    void SetRenderScale () {
        float pixelCount = Screen.width * Screen.height;
        float scale = 3502080 / pixelCount;

        if(scale > 1)
            scale = 1;
        else if(scale < 0.1f)
            scale = 0.1f;
        
        UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.renderScale = scale;
    }

    void SetTargetFPS () {
        if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WebGLPlayer)
            Application.targetFrameRate = 60;
        else if(Application.platform == RuntimePlatform.WindowsPlayer)
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
        else
            Application.targetFrameRate = 60;
    }

    void Start()
    {
        if(Application.platform == RuntimePlatform.Android)
            SetRenderScale();

        SetTargetFPS();
        Time.timeScale = 1f;

        if(CheckTutorialCompletion())
            isTutorial = false;
        else 
            isTutorial = true;
        

        if(isTutorial) {
            spawnObstacles = false;
            spawnRopes = false;
        }

        
        if(isTutorial) {
            tutorialAssets.SetActive(true);
            currentChunk.Initialize(tutorialTCL, this);
        }
        else {
            tutorialAssets.SetActive(false);
            currentChunk.Initialize(TCL, this);
        }

        currentGameState = GameState.Loading;
        Shader.SetGlobalFloat("_YStatic", 0f);


        currentChunkStartEnd = currentChunk.GetStartEnd();

        currentChunkObjects = new List<GameObject>();
        lastChunkObjects = new List<GameObject>();

        sceneLoadTime = Time.time;
        if(spawnObstacles)
            SpawnObstacles();
        SpawnTrees();
        SpawnHouses();
        if(spawnRopes)
            SpawnRopeGrinds();
        currentGameState = GameState.Intro;
        
    }

    void Update()
    {
        CreateNextEnvironment(); 
        CleanUp(); 
        if(Time.time - sceneLoadTime >= 4f && currentGameState == GameState.Intro)
            FinishIntro();
    }

    public void FinishTutorial () {
        isTutorial = false;
        tutorialAssets.SetActive(false);
        OSTAudio.Play();
        FadeOSTIn();
        OSTAudio.loop = true;
        playerTransform.GetComponent<TutorialHelper>().enabled = false;
        PlayerPrefs.SetInt("Tutorial", 1);
    }

    public void FinishIntro () {
        currentGameState = GameState.Menu;
        mainMenuUI.gameObject.SetActive(true);
    }

    public void StartGame () { // Start the game. Called by PlayerController.
        currentGameState = GameState.Started;
        generalUI.gameObject.SetActive(true);
        mainMenuUI.gameObject.SetActive(false);
        Shader.SetGlobalFloat("_YStatic", 1f);
        audioSource.PlayOneShot(ttpAudioClip);

        pauseButtonScript.Appear();

        introAudio.GetComponent<AudioFade>().FadeOut();
        if(!isTutorial) {
            Invoke("FadeOSTIn", 1f);
            OSTAudio.loop = true;
        }
    }

    public GameState GetGameState () {
        return currentGameState;
    }
    public void EnterSettingsMenu () {
        currentGameState = GameState.SettingsMenu;
        mainMixer.SetFloat("ambientCutoff", 1000f);
        mainMixer.SetFloat("musicCutoff", 1000f);
    }
    public void ExitSettingsMenu () {
        currentGameState = GameState.Menu;
        mainMixer.SetFloat("ambientCutoff", 22000f);
        mainMixer.SetFloat("musicCutoff", 22000f);
    }

    
    void CreateNextEnvironment () { // Creates the next terrain chunk and populates it with objects.
        float chunkLength = currentChunkStartEnd.y - currentChunkStartEnd.x;
        if(playerTransform.position.x - currentChunkStartEnd.x >= chunkLength / 2f) { // Gets called when player passes the middle of the current terrain chunk.
            SpawnNewChunk();
            SpawnObstacles();
            SpawnTrees();
            SpawnHouses();
            SpawnRopeGrinds();
        }
    }

    void CleanUp () { // Cleans up assets that are behind the player. (Deletes passed terrain chunks and gameobjects).
        if(lastChunk) {
            if(playerTransform.position.x - lastChunkStartEnd.y >= 100f) {
                lastChunk.Despawn();
                foreach(GameObject gObj in lastChunkObjects) {
                    GameObject.Destroy(gObj);
                }
                lastChunkObjects.Clear();
            }  
        }
    }

    void SpawnHouses () {
        foreach(Vector3 pos in RandomPointsOnSpline(8f, 25, 15f)) {
            if(pos != Vector3.zero) {
                int randInd = Random.Range(0, 3);
                GameObject newSpawn = Instantiate(visualObjects[3 + randInd], pos, Quaternion.Euler(-90f, Random.Range(0f, 360f), 0f));
                newSpawn.transform.parent = visualsT;
                Transform trans = newSpawn.transform;

                currentChunkObjects.Add(newSpawn);

                int rand = Random.Range(0, 2);
                trans.position += Vector3.forward * 5f - Vector3.forward * 16f * rand;
                float randY = Random.Range(0f, 1.2f);
                trans.position += -Vector3.up * 3.5f * rand + Vector3.up * randY;
            }
        }
    }

    void SpawnNewChunk () { // Spawns a new terrain chunk. Stores the old one in lastChunk.
        float y = currentChunk.GetHeightOfLastPoint();
        lastChunk = currentChunk;
        lastChunkStartEnd = currentChunkStartEnd;
        GameObject newSpawn = Instantiate(chunkPrefab, new Vector3(currentChunkStartEnd.y + currentChunk.chasmSize, y, 0f), Quaternion.identity);
        newSpawn.transform.parent = terrainsT;

        currentChunk = newSpawn.GetComponent<TerrainChunkBuilder>();
        currentChunk.chunkSize = Random.Range(300, 600);
        currentChunk.Initialize(TCL, this);
        currentChunkStartEnd = currentChunk.GetStartEnd();

        foreach(GameObject gObj in currentChunkObjects) {
            lastChunkObjects.Add(gObj);
        }
        currentChunkObjects.Clear();
    }

    void SpawnRopeGrinds () { // Rolls a die to see how many to spawn on this chunk. Then for each rolls more RNG to see how many segments they will have, and spawns them with the info.
        int amount = Random.Range(1, 5);
        float minDistance = 100f;
        if(amount == 0)
            return;
        foreach(Vector3 pos in RandomPointsOnSpline(minDistance, amount, 80f)) {
            if(pos == Vector3.zero)
                continue;
            int segCount = Random.Range(2, 5); // Will have between 2 and 4 segments.
            Vector2 lastSegment = new Vector2(pos.x, pos.y);
            Vector2[] segments = new Vector2[segCount];

            Transform visualTower = null; // Will be used later for moving back one index in the rope.

            for(int j = 0; j < segCount; j++) {
                int xOffset = 0;
                int yOffset = Random.Range(-1, 3);
                float height = 3f;
                if(j > 0) {
                    xOffset = Random.Range(15, 35);
                    lastSegment.y = currentChunk.GetHeightOnSpline(lastSegment.x + xOffset);
                }
                
                segments[j] = new Vector2(lastSegment.x + xOffset, lastSegment.y + yOffset + height);

                Vector3 towerPos;
                if(j == segCount - 1) {
                    float xPos = segments[j].x - 1f;
                    towerPos = new Vector3(xPos, segments[j].y + 1.1f, 0.65f);
                }
                else {
                    towerPos = new Vector3(segments[j].x, segments[j].y + 1.1f, 0.65f);
                }

                visualTower = Instantiate(visualObjects[2], towerPos, Quaternion.identity).transform;
                visualTower.parent = visualsT;
                currentChunkObjects.Add(visualTower.gameObject);
                lastSegment = segments[j];
            }
            
            GameObject newSpawn = Instantiate(interactiveObjects[1], new Vector3(0f, 0f, 0f), Quaternion.identity);
            newSpawn.transform.parent = interactivesT;
            RopeGrind ropeGrind = newSpawn.GetComponent<RopeGrind>();

            currentChunkObjects.Add(newSpawn);

            ropeGrind.segments = segments;
            ropeGrind.Initialize();

            if(visualTower)
                visualTower.position = new Vector3(visualTower.position.x, ropeGrind.GetHeight(visualTower.position.x), visualTower.position.z);

        }
        
    }

    void SpawnTrees () { // Spawns trees on terrain.
        // Background trees
        int treeCount = Random.Range(185, 195); 
        float minDistance = 3f;
        Transform currentTree;
        foreach(Vector3 pos in RandomPointsOnSpline(minDistance, treeCount, 35f)) {
            float index = Mathf.Pow(Random.Range(0f, 1f), 2f) * 3f;
            int i = (int)Mathf.Floor(index);
            if(pos != Vector3.zero) {
                GameObject newSpawn = Instantiate(trees[i], pos, Quaternion.identity);
                newSpawn.transform.parent = visualsT;

                currentTree = newSpawn.transform;
                float scaleMult = Random.Range(1.2f, 2.5f);
                currentTree.position -= Vector3.up * Random.Range(0f, 0.3f);
                currentTree.position += Vector3.forward * 0.8f + Vector3.forward * Random.Range(0f, 1f);
                currentTree.localScale *= scaleMult;

                currentChunkObjects.Add(newSpawn);
            }
        }

        // Foreground trees
        treeCount = Random.Range(20, 28);
        foreach(Vector3 pos in RandomPointsOnSpline(minDistance, treeCount, 35f)) {
            if(pos != Vector3.zero) {
                GameObject newSpawn = Instantiate(visualObjects[1], pos, Quaternion.Euler(90f, 180f, 0f));
                newSpawn.transform.parent = visualsT;
                currentTree = newSpawn.transform;

                currentChunkObjects.Add(newSpawn);

                float scaleMult = Random.Range(0.85f, 2.5f);
                currentTree.position += new Vector3(0f, -2.8f, -14f);
                currentTree.localScale *= scaleMult;
            }
        }
    }

    void SpawnObstacles () { // Spawns rock obstacles on terrain.
        int obstaclesCount = Random.Range(4, 7); // How many obstacles on our chunk.
        Vector2[] spawnAreas = new Vector2[obstaclesCount+1];
        //float[] obstaclesX = new float[obstaclesCount];
        float minX = 15f;
        int spawnAreaCount = 1;
        Vector3 currentChunkPos = currentChunk.transform.position;
        spawnAreas[0] = new Vector2(minX + currentChunkPos.x + currentChunk.spawnCutoff, currentChunk.chunkSize - minX - currentChunk.spawnCutoff + currentChunkPos.x);
        for(int i = 0; i < obstaclesCount; i++) {
            if(spawnAreaCount < 1) {
                print("Couldn't place all obstacles - not enough room");return; 
            }
            int area = Random.Range(0, spawnAreaCount);      
            float xPos = Random.Range(spawnAreas[area].x, spawnAreas[area].y);
            float yPos = currentChunk.GetHeightOnSpline(xPos);
            Vector2 terrainNormal = currentChunk.GetNormal2D(xPos);
            GameObject newSpawn = Instantiate(obstaclePrefab, new Vector3(xPos, yPos, 0f), Quaternion.LookRotation(Vector3.forward, terrainNormal));
            newSpawn.transform.parent = obstaclesT;

            // Adjust obstacle position to closest point on terrain using raycast in -terrainNormal direction.
            Vector2 rayDirection = -terrainNormal;
            LayerMask layerMask = 1 << 3;
            RaycastHit2D hit2D = Physics2D.Raycast(newSpawn.transform.position, rayDirection, 5f, layerMask);
            if(hit2D.collider) {
                newSpawn.transform.position = (Vector3)hit2D.point;
            }
            

            currentChunkObjects.Add(newSpawn);
            
            float cachedEnd = spawnAreas[area].y;
            spawnAreas[area] = new Vector2(spawnAreas[area].x, xPos - minX);
            spawnAreas[spawnAreaCount] = new Vector2(xPos + minX, cachedEnd);

            spawnAreaCount++;

            for(int j = 0; j < spawnAreaCount; j++) {
                float length = spawnAreas[j].y - spawnAreas[j].x; 
                if(length <= 0f) {
                    if(j < spawnAreaCount - 1) {
                        spawnAreas[j] = spawnAreas[j+1];
                        spawnAreas[j+1] = Vector2.zero;
                        j--;
                    }
                    else {
                        spawnAreas[j] = Vector2.zero;
                    }
                    spawnAreaCount--;
                }
            }  
        }
    }

    public void SpawnArchGrind(Vector2 parameters) { // Receives spawn command from TerrainChunkBuilder on specific curves. X - Start X position; Y - End X position.
        GameObject newSpawn = Instantiate(interactiveObjects[0], Vector3.zero, Quaternion.identity);
        ArchGrind archGrind = newSpawn.GetComponent<ArchGrind>();

        currentChunkObjects.Add(newSpawn);

        archGrind.xBounds = new Vector2(Mathf.Floor(parameters.x), Mathf.Floor(parameters.y));
        archGrind.gameManager = this;
    }

     Vector3[] RandomPointsOnSpline (float minDistance, int count, float maxAngle) { // Returns an array of random points on the spline, that each keep a minimum of "minDistance" distance from the others. Count is how many to spawn. maxAngle - Maximum angle of the slope at which we can spawn.
        Vector3[] points = new Vector3[count];

        Vector2[] spawnAreas = new Vector2[count+1];
        //float[] obstaclesX = new float[count];
        float minX = minDistance;
        int spawnAreaCount = 1;
        Vector3 currentChunkPos = currentChunk.transform.position;
        spawnAreas[0] = new Vector2(minX + currentChunk.spawnCutoff + currentChunkPos.x, currentChunk.chunkSize - minX - currentChunk.spawnCutoff + currentChunkPos.x);
        for(int i = 0; i < count; i++) {
            if(spawnAreaCount < 1) {
                print("Couldn't place all obstacles - not enough room");return points; 
            }
            int area = Random.Range(0, spawnAreaCount);      
            float xPos = Random.Range(spawnAreas[area].x, spawnAreas[area].y);
            // Check slope angle to see if we can spawn (Made for environmental objects that shouldnt exist on sloped terrain - houses, trees, ruins, etc.)
            float heightLeft = currentChunk.GetHeightOnSpline(xPos - 1);
            float heightRight = currentChunk.GetHeightOnSpline(xPos + 1);
            float angle = Mathf.Atan(((heightLeft - heightRight) / 2f)) * 180f / Mathf.PI; // Calculate the angle of the slope using point before and point after desired spawn point.
            if(angle <= maxAngle) {
                float yPos = currentChunk.GetHeightOnSpline(xPos);
                points[i] = new Vector3(xPos, yPos, 0f);
            }
            else {
                points[i] = Vector3.zero;
            }
            
            float cachedEnd = spawnAreas[area].y;
            spawnAreas[area] = new Vector2(spawnAreas[area].x, xPos - minX);
            spawnAreas[spawnAreaCount] = new Vector2(xPos + minX, cachedEnd);

            spawnAreaCount++;

            for(int j = 0; j < spawnAreaCount; j++) {
                float length = spawnAreas[j].y - spawnAreas[j].x; 
                if(length <= 0f) {
                    if(j < spawnAreaCount - 1) {
                        spawnAreas[j] = spawnAreas[j+1];
                        spawnAreas[j+1] = Vector2.zero;
                        j--;
                    }
                    else {
                        spawnAreas[j] = Vector2.zero;
                    }
                    spawnAreaCount--;
                }
            } 
        }
        return points;
    }

    public TerrainChunkBuilder GetChunkOnX (float xPos) { // Returns the chunk that corresponds to a given X position. This basically chooses which chunk to return, last chunk or current chunk.
        if(lastChunk) { 
            if(xPos >= lastChunkStartEnd.x && xPos < lastChunkStartEnd.y)
                return lastChunk;
            else if(xPos >= currentChunkStartEnd.x && xPos < currentChunkStartEnd.y)
                return currentChunk;
            else
                return null;
        }
        else {
            if(xPos >= currentChunkStartEnd.x && xPos < currentChunkStartEnd.y)
                return currentChunk;
            else
                return null;
        }
    }

    bool CheckTutorialCompletion () {
        if(PlayerPrefs.HasKey("Tutorial")) {
            if(PlayerPrefs.GetInt("Tutorial") > 0)
                return true;
        }
        
        return false;
    }

    void FadeOSTIn() {
        OSTAudio.GetComponent<AudioFade>().FadeIn();
        OSTAudio.Play();
    }

    public void ReloadScene () {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
