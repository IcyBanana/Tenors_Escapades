using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class CameraFollow : MonoBehaviour
{
    [Header("General Parameters")]
    public Transform target;
    public PlayerController playerController;
    public Vector2 offset = new Vector2(12f, 1.5f);
    public float desiredPlayXPos = 0.5f; // Desired normalized screen position for the player on screen. -1 is at the right edge of screen. 0 is center, 1 is at the left edge.
    public float desiredPlayYPos = 0.9f;
    private Vector2 xOffsetMinMax;
    private Vector2 yOffsetMinMax;
    private float extraYOffset;
    private Rigidbody2D playerRigidbody; 
    private GameManager gameManager;
    private Camera myCamera;

    private float spawnTime; // Time.time on Start()
    private Vector3 startPos; // Position on Start()

    public float xSpeed = 5f; // Move speed on X axis.
    public float ySpeed = 2f; 
    public float accelTime = 1f; // Time to max speed.
    private float currentSpeed; 

    [Header("Zoom Parameters")]
    public bool useZoom = true;

    public float originalZ = -50f; // Original camera position on Z axis. This is Min zoom.
    public float maxZoomZ = -70f; // Max zoom camera position on Z axis.
    public float zoomLerpSpeed = 5f;
    public Vector2 zoomSpeedBounds = new Vector2(8f, 22f); // This vector stores the player speeds within which we define our zoom. X = min zoom, Y = max zoom.
    public Vector2 zoomFOVBounds = new Vector2(20f, 23f);
    public Camera foregroundCamera; // Sky cam for FOV changes on sky background.

    private float currentZoomPercent; // The current percentage of zoom-out. 0 = original zoom, 1 = fully zoomed out.
    private float terrainHeight; // The height of the terrain where the player/camera are.
    private float fallDuration = 0f; // How long the camera has been falling below the player.
    private float maxFallDuration = 2f; // Time it takes the camera to reach max distance below player when falling.

    private bool followChasm = false;
    private Transform chasmTarget; // Chasm center's transform.
    private float chasmMidPoint; // Mid point for chasm. See more at << ChasmCameraTrigger.cs >>
    private bool deadInChasm = false;

    private float oldHeightAboveTerrain = 0; 

    private float followP = 0f;

    private Vector2 mainMenuPos = new Vector2(4f, 3f);
    private Vector2 settingsMenuPos = new Vector2(-18f, 3f);

    [Header("TimeManagement")]
    private bool changeTimescale = false;
    private bool timeSlowed = false;
    private bool tutorialExplainFinished = false;
    private float timescaleLerp = 0f;
    private float desiredTimescale = 1f;
 

    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = target.GetComponent<Rigidbody2D>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        myCamera = GetComponent<Camera>();

        spawnTime = Time.time;
        startPos = transform.position;

        SetXOffsets();
        SetYOffsets();
    }

    void Update () {
        if(timeSlowed) {
            if(tutorialExplainFinished) {
                StartTimescaleChange(1f, 5f);
                tutorialExplainFinished = false;
            }
        }

        GameManager.GameState currentGameState = gameManager.GetGameState();
        switch (currentGameState) {
            case GameManager.GameState.Started:
                LerpXYOffset();
                if(deadInChasm)
                    LerpFollowP(0f, 8f);
                else
                    LerpFollowP(1f, 1f);
 
                FollowPlayer(followP);
                Zoom();
                break;
                
            case GameManager.GameState.Menu:
                LerpXYOffset(mainMenuPos);
                FollowPlayer(1f);
                Zoom();
            break;
            case GameManager.GameState.SettingsMenu:
                LerpXYOffset(settingsMenuPos);
                FollowPlayer(1f);
            break;
            case GameManager.GameState.Intro:
                FollowPlayerSlow();
            break;
        }
    }

    void LateUpdate() {
        ChangeTime();  
    }

    public void DiedInChasm () {
        deadInChasm = true;
    }

    public void RecoverChasm () {
        deadInChasm = false;
    }   


    public void StartTimescaleChangeFast (float timescale) {
        changeTimescale = true;
        desiredTimescale = timescale;
        timescaleLerp = 15f;
    }

    public void StartTimescaleChange (float timescale, float lerp) {
        changeTimescale = true;
        desiredTimescale = timescale;
        timescaleLerp = lerp;
    }

    public void TutorialExplainFinished() {
        tutorialExplainFinished = true;
    }

    void LerpXYOffset () {
        if(offset.y - 0.5f < 0.05f)
            offset.y = 0.5f;
        else 
            offset.y = Mathf.Lerp(offset.y, 0.5f, 2f * Time.deltaTime);

        if(xOffsetMinMax.x - offset.x < 0.05f)
            offset.x = xOffsetMinMax.x;
        else 
            offset.x = Mathf.Lerp(offset.x, xOffsetMinMax.x, 2f * Time.deltaTime);
    }
    void LerpXYOffset (Vector2 xy) {
        if(Mathf.Abs(offset.y - xy.y) < 0.05f)
            offset.y = xy.y;
        else 
            offset.y = Mathf.Lerp(offset.y, xy.y, 5f * Time.deltaTime);

        if(Mathf.Abs(xy.x - offset.x) < 0.05f)
            offset.x = xy.x;
        else 
            offset.x = Mathf.Lerp(offset.x, xy.x, 5f * Time.deltaTime);
    }

    void LerpFollowP (float desiredP, float lerpSpeed) {
        if(Mathf.Abs(desiredP - followP) < 0.1f) 
            followP = desiredP;
        else
            followP = Mathf.Lerp(followP, desiredP, lerpSpeed * Time.deltaTime);
    }

    public void SwitchToChasm (Transform newTarget, float midPoint) {
        chasmTarget = newTarget;
        chasmMidPoint = midPoint;
        followChasm = true;
    }
    public void SwitchToPlayer () {
        followChasm = false;
    }

    void FollowChasm () {
        Vector3 targetPos = (chasmTarget.position - target.position) * chasmMidPoint + target.position;
        targetPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, 7f * Time.deltaTime);

        if(target.position.x > chasmTarget.position.x) {
            chasmMidPoint = Mathf.Lerp(chasmMidPoint, 0f, 2f * Time.deltaTime);
        }
    }
    
    void FollowPlayerSlow () {
        float xOffset = offset.x + (xOffsetMinMax.y - xOffsetMinMax.x) * currentZoomPercent;
        float yOffset = offset.y;

        Vector3 desiredPos = new Vector3(target.position.x + xOffset, target.position.y + yOffset, transform.position.z);
        float x = Mathf.Clamp((Time.time - spawnTime) / 4f, 0f, 1f);
        transform.position = Vector3.Lerp(startPos, desiredPos, (x * x * (3 - 2 * x)));
    }

    
    void FollowPlayer (float followPercentage) {
        float xOffset = offset.x + (xOffsetMinMax.y - xOffsetMinMax.x) * currentZoomPercent;
        float yOffset = offset.y + extraYOffset;

        bool isGrounded = playerController.isGrounded;
        float maxDistance = Mathf.Lerp(yOffsetMinMax.x, yOffsetMinMax.y, currentZoomPercent); // Max distance on Y axis the camera can reach below the player when falling.
        float fixedMaxDistance = maxDistance;
        if(!isGrounded) {
            float playerVerticalVelocity = playerRigidbody.velocity.y;
            float heightAboveTerrain = Mathf.Lerp(oldHeightAboveTerrain, playerController.GetHeightAboveTerrain(), 0.5f);
            oldHeightAboveTerrain = heightAboveTerrain;
            if(heightAboveTerrain < maxDistance * 4f) {
                maxDistance -= (1f - (heightAboveTerrain / (maxDistance * 4f))) * maxDistance;
            }
            if(playerVerticalVelocity < 0f) {
                float mult = Mathf.Clamp01(fallDuration / maxFallDuration);
                mult = mult * mult * (3f - 2f * mult); // Smoothstep interpolation.
                extraYOffset = -maxDistance * mult;
                fallDuration += Time.deltaTime;
            }      
        }
        else {
            fallDuration = 0f;
            if(extraYOffset < 0f)
                extraYOffset += maxDistance * Time.deltaTime * 2f;
            if(extraYOffset > 0f)
                extraYOffset = 0f;
        }
        Vector3 desiredPos = new Vector3(target.position.x + xOffset, target.position.y + yOffset, transform.position.z);

        transform.position += (desiredPos - transform.position) * followPercentage;
    }

    float CalculateScreenY () { // Calculates the screen height on Z = 0, where the player is. This will help us to determine if both player and terrain can be put into view.
        float fovAngle = GetComponent<Camera>().fieldOfView * Mathf.PI / 180f; // Angle of camera's field of view in radians.
        float distance = -transform.position.z; // Get our camera's distance from Z = 0.
        float screenY = distance * Mathf.Sin(fovAngle); // Calculate screen Y.

        return screenY;
    }

    void CustomZoom (float fov, float zOffset, float lerpSpeed) { // Zooms the camera out by moving it back on the Z axis and changing FOV. Based on given parameters.
        float zoom = Mathf.Lerp(transform.position.z, zOffset, lerpSpeed * Time.deltaTime);
        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, fov, lerpSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, zoom);
    }

    private float lastPlayerSpeed;
    void Zoom () { // Zooms the camera out by moving it back on the Z axis based on player speed.
        if(!useZoom)
            return;

        float playerSpeed = playerRigidbody.velocity.magnitude; // Grab player speed from rigidbody.
        playerSpeed = Mathf.Lerp(lastPlayerSpeed, playerSpeed, 0.5f);
        lastPlayerSpeed = playerSpeed;
        float desiredZoomPercent = Mathf.Clamp01((playerSpeed - zoomSpeedBounds.x) / (zoomSpeedBounds.y - zoomSpeedBounds.x)); // Calculate zoom-out percentage based on where the player speed is between min and max bounds.
        currentZoomPercent = Mathf.Lerp(currentZoomPercent, desiredZoomPercent, zoomLerpSpeed * Time.deltaTime);
        myCamera.fieldOfView = zoomFOVBounds.x + (zoomFOVBounds.y - zoomFOVBounds.x) * currentZoomPercent;
        if(foregroundCamera)
            foregroundCamera.fieldOfView = myCamera.fieldOfView;
    }

    void ChangeTime () {
        if(Time.timeScale < 1f)
            timeSlowed = true;
        else
            timeSlowed = false;
        if(!changeTimescale)
            return;
       if(Mathf.Abs(desiredTimescale - Time.timeScale) < 0.001f) {
            Time.timeScale = desiredTimescale;
            changeTimescale = false;
            return;
        }
        Time.timeScale = Mathf.Lerp(Time.timeScale, desiredTimescale, timescaleLerp * Time.unscaledDeltaTime);    
    }

    void SetXOffsets () {
        CameraShaderXWidth camXWidth = GetComponent<CameraShaderXWidth>(); // Camera component used to determine X Width at Z = 0 within camera screen bounds. (Left and right bounds)
        xOffsetMinMax.x = camXWidth.CalculateXWidth(zoomFOVBounds.x) * desiredPlayXPos;
        xOffsetMinMax.y = camXWidth.CalculateXWidth(zoomFOVBounds.y) * desiredPlayXPos;
    }

    void SetYOffsets () {
        CameraShaderXWidth camXWidth = GetComponent<CameraShaderXWidth>(); // Camera component used to determine X Width at Z = 0 within camera screen bounds. (Left and right bounds)
        yOffsetMinMax.x = camXWidth.CalculateYWidth(zoomFOVBounds.x) * desiredPlayYPos;
        yOffsetMinMax.y = camXWidth.CalculateYWidth(zoomFOVBounds.y) * desiredPlayYPos;
    }

    // Check if touch is over UI.
    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

}

