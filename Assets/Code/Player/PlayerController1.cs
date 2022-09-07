using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController1 : MonoBehaviour
{
    // RIGIDBODY AND MOTION
    [Header("Rigidbody and Motion")]
    public Rigidbody2D playerRigidbody;
    public float minSpeed = 11f; // Minimum speed the player will travel in on ground. This can increase with tricks and pickups. This value never goes below originalMinSpeed.
    public float maxSpeed = 24f; // Max speed we can reach with tricks.
    public float grindSpeedMult = 0.7f; // Multiplies our current min speed by this. This is the min speed for a grind.
    public float maxYVelocity; // Max velocity on Y axis while in the air.
    public float jumpHeight = 3f; // Desired height when jumping - we calculate jump force using this
    public float flipSpeed = 170f; // Speed of flip in degrees per second
    public float autoRotateSpeed = 90f; // Speed of automatic rotation to face ground. (deg/sec)

    private bool hasJumped = false;
    private float originalMinSpeed = 11f; // This is the base min speed of our character.
    private float speedLoss = 0.2f; // Speed loss per second. If minSpeed is above originalMinspeed we subtract this value from it every second until we reach originalMinSpeed.
    private float gravityScaleOriginal; // Initial gravity scale on rigidbody.
    private Vector2 lastVelocity; // Velocity last frame.
    private float currentAngle; // Used for lerping in 'MomentumChange()'

    // GROUND INTERACTION
    [Header("Ground")]
    public float groundedDistance = 0.15f;
    public bool isGrounded = false;

    private Vector2 terrainNormal; // The normal of the terrain below the player. This is used to adjust player's rotation to match the terrain - On ground and in the air
    private float heightAboveTerrain;

    // ROPE GRIND
    private bool isOnRope; // This is switched true after having rope grind collider enter our trigger, and is set false when it leaves it. Used to check if we move above the rope *while* still triggered by it.
    private RopeGrind ropeGrind;
    private Collider2D lastRopeCol;

    // TRICKS SECTION
    [Header("Trick Section")]
    public TrickSystem trickSystem;
    public float speedLossDelay = 2f; // Time until we start losing speed after reward.
    private float anglesRotated = 0f; // Angles rotated during a flip.
    private bool isFlipping = false; // Is used to tell if the player started a new flip.
    public bool isGrinding = false; // Are we currently on a grind?
    private float distanceGrinded = 0f; // Distance we grinded on arch/rope.
    private Vector3 lastPos; // Last position used to calculate grind distance.
    private int grindID; // Instance ID of current grind object. To make sure we don't award duplicate trick points for same grind.
    private float lastSpeedRewardTime; // Used to delay speed loss after reward.

    public UnityEngine.UI.Text distanceTraveledText;
    private float distanceTraveled; // Total distance traveled on X axis.
    private float lastXPos; // Last x position for distance traveled.


    // AUDIO SECTION
    [Header("Audio")]
    public AudioSource snowglideAudioSource; // Sound of moving on snow
    public AudioSource jumpLandAudioSource; // Sound of jumping up or landing down
    public AudioSource ropeGrindAudioSource;
    public AudioSource fallWindAudioSource; // Sound of wind while falling.
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip recoverClip; // Recover from crash clip.
    public AudioClip crashGameOverClip;
    public AudioFade OSTFade; // Main soundtrack audio source.

    // ANIMATION SECTION
    [Header("Animation")]
    public Animator playerAnimator;
    public GameObject myGraphic;
    public ScarfScript scarfScript;
    public GameObject crashGraphic;
    public GameObject ragdollGraphic;
    public GameObject snowboardRagdoll;
    public ParticleSystem snowglideParticles; // Particle system creating an effect behind snowboard when on ground.
    public SeatedCharScript seatedChar;
    public InvinsibilityFieldAnimation invAnimation;
    private GameObject spawnedSnowboard;

    // CONTROL AND DELEGATION
    [Header("Control & Death")]
    public GameManager gameManager;
    public bool canJump = true;
    public bool canFlip = true;
    private bool inControl = false;
    private bool touchToPlay = false;
    private bool hasInput = false;
    private bool tapSwitch = false;
    private bool getTapUp = false;
    public bool hasCrashed = false;
    private bool isRagdollFlying = false;
    private bool isInvincible = false;
    private bool diedInChasm = false;

    private bool splashScreenFinished = false;

    private TutorialHelper tutorialHelper; // Used for events like crashing during the tutorial.
    private MessageController messageController; // Used for events like crashing during gameplay. (Outside of the tutorial)


    private CameraFollow cameraFollow;

    void Start()
    {
        tutorialHelper = GetComponent<TutorialHelper>();
        messageController = GetComponent<MessageController>();

        if(!gameManager.isTutorial) {
            tutorialHelper.enabled = false;
        }
        else {
            canJump = false;
            canFlip = false;
        }

        gravityScaleOriginal = playerRigidbody.gravityScale;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        Freeze();
        HideGraphic();
        touchToPlay = true;
        snowglideAudioSource.volume = 0f;
        scarfScript.maxSpeed = maxSpeed;
        scarfScript.speed = minSpeed;
        scarfScript.minSpeed = originalMinSpeed;
        scarfScript.Initialize();
        
        lastXPos = transform.position.x;
    }
    private bool touchSwitch = false;
    void Update() {   
        if(!splashScreenFinished) {
            if(UnityEngine.Rendering.SplashScreen.isFinished) 
                splashScreenFinished = true;
            return;
        }
        ParseInput();
        AllowTTP();
        AnimateSnowglide();
        
        if(gameManager.GetGameState() == GameManager.GameState.Started) {
            SnowglideAudioControl();
            WindAudioControl();
        }

        distanceTraveled += transform.position.x - lastXPos;
        lastXPos = transform.position.x;
        distanceTraveledText.text = ((int)distanceTraveled).ToString() + "m";
    }

    void FixedUpdate()
    {  
        //playerRigidbody.angularVelocity = 0f;
        if(inControl) {
            RaysToGround();

            AdjustMinSpeed();
            MaintainMinSpeed(); 
            Gravity();

            AirDrag();

            Grind();
            RopeGrind();
            
            FaceGround(); 
            StickToGround();

            Flip();

            AdjustInvincibility();

            playerAnimator.SetFloat("yShift", MomentumChange(1f));
            
        }
        else {
             // Allow touch to play when player rigidbody isn't moving and no input is detected.
            RaysToGround();
            if(hasCrashed) {
                SlowDownCrash();
            }
            if(!hasJumped) {
                if(isGrounded) {
                    FaceGround();
                    StickToGround();
                }
            }
            else {
                if(hasCrashed)
                    RagdollFly();
            }
        }
        
    }

    public void ContinueGame () {
        inControl = true;
        Unfreeze();
        ShowGraphic();
        playerRigidbody.AddForce(new Vector2(0.5f, 1f) * 5f * playerRigidbody.mass, ForceMode2D.Impulse);
        hasJumped = true;
    }

    public void StartGame () {
        inControl = true;
        Unfreeze();
        seatedChar.StartGame();
        ShowGraphic();
        //playerRigidbody.AddForce(new Vector2(0.5f, 1f) * 1f * playerRigidbody.mass, ForceMode2D.Impulse);
        hasJumped = true;
        gameManager.StartGame();
    }
    

    public void AdjustMinSpeed (float add) {
        if(add > 0f) {
            MakeInvincible();
            add = add + Mathf.Clamp(((maxSpeed - minSpeed) / (maxSpeed - originalMinSpeed)), 0f, 1f) * add; // Extra speed addition the slower we are.
        }
        
        minSpeed += add;
        if(minSpeed > maxSpeed)
            minSpeed = maxSpeed;
        if(add > 0f)
            lastSpeedRewardTime = Time.time;
        
        scarfScript.speed = minSpeed;
    }
    
    public bool CheckInvincibility () {
        return isInvincible;
    }

    public void Crash (bool fly) { // Called upon crashing from rocks/landing incorrectly on ground. fly = whether we ragdoll or not.
        //Freeze();
        HideGraphic();
        playerRigidbody.drag = 0.9f;
        inControl = false;
        snowglideAudioSource.volume = 0f;
        if(!hasCrashed)
            jumpLandAudioSource.PlayOneShot(crashGameOverClip);
        if(fly) {
            playerRigidbody.freezeRotation = true;
            hasJumped = true;
            isRagdollFlying = true;
            ragdollGraphic.SetActive(true);
            if(spawnedSnowboard)
                GameObject.Destroy(spawnedSnowboard);
            spawnedSnowboard = Instantiate(snowboardRagdoll, transform.position, transform.rotation);
            Rigidbody2D spawnedSnowboardRB = spawnedSnowboard.GetComponent<Rigidbody2D>();
            spawnedSnowboardRB.AddForce(playerRigidbody.velocity * 1.2f * spawnedSnowboardRB.mass, ForceMode2D.Impulse);
            spawnedSnowboardRB.AddTorque(Random.Range(-15f, 15f), ForceMode2D.Impulse);
            RagdollFly();
        }
        else {
            playerRigidbody.freezeRotation = false;
            ragdollGraphic.SetActive(false);
            crashGraphic.SetActive(true);
            isRagdollFlying = false;
            if(!hasCrashed) {
                if(spawnedSnowboard)
                    GameObject.Destroy(spawnedSnowboard);
                spawnedSnowboard = Instantiate(snowboardRagdoll, transform.position, transform.rotation);
                Rigidbody2D spawnedSnowboardRB = spawnedSnowboard.GetComponent<Rigidbody2D>();
                spawnedSnowboardRB.AddForce(playerRigidbody.velocity.normalized * 4f * spawnedSnowboardRB.mass, ForceMode2D.Impulse);
            }
            //crashGraphic.transform.position = transform.position - Vector3.up * heightAboveTerrain;
            //crashGraphic.transform.rotation = Quaternion.Euler(0f, 0f, -Vector2.Angle(terrainNormal, Vector2.up));
        }

        hasCrashed = true;
        touchToPlay = false;

        OSTFade.FadeOut();

        if(tutorialHelper.enabled) {
            tutorialHelper.Crashed();
        }
        else {
            messageController.Crashed(diedInChasm);
        }
    }   

    public void DiedInChasm () {
        diedInChasm = true;
        Crash(true);
        cameraFollow.DiedInChasm();
    }

    public bool getTTP () {
        return touchToPlay;
    }



    // END PUBLIC METHODS
    // ================================================================================================================================================================================================
    // END PUBLIC METHODS





    // END PUBLIC METHODS
    // ================================================================================================================================================================================================
    // END PUBLIC METHODS

    void SlowDownCrash () {
        if(diedInChasm) {
            if(transform.position.y < cameraFollow.transform.position.y - 15f) {
                Freeze();
            }
        }

        if(isRagdollFlying || !isGrounded || playerRigidbody.velocity.magnitude == 0f)
            return;

        Vector2 force = -playerRigidbody.velocity; // Slowdown force is equal and opposite to velocity vector intially.
        // Add extra force based on how much gravity is affecting our motion.
        force += force.normalized * Vector2.Dot(-transform.right.normalized, Vector2.up) * playerRigidbody.gravityScale * Physics.gravity.magnitude;
        
        if(force.magnitude > 0.1f && playerRigidbody.velocity.magnitude > 0.2f)
            playerRigidbody.AddForce(force * playerRigidbody.mass);
        else
            Freeze();
    }

    void MakeInvincible () {
        isInvincible = true;
        invAnimation.Initiate(speedLossDelay);
    }

    void RecoverCrash () {
        if(spawnedSnowboard)
            GameObject.Destroy(spawnedSnowboard);
        ShowGraphic();
        Unfreeze();
        crashGraphic.SetActive(false);
        ragdollGraphic.SetActive(false);
        inControl = true;

        if(diedInChasm) {
            float xPos = gameManager.currentChunk.GetStartEnd().x + 3.5f;
            float yPos = gameManager.currentChunk.GetHeightOnSpline(xPos);

            transform.position = new Vector3(xPos, yPos, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, -20f);
            cameraFollow.RecoverChasm();
            diedInChasm = false;
        }
        else {
            playerRigidbody.AddForce(new Vector2(0.5f, 1f) * 2f * playerRigidbody.mass, ForceMode2D.Impulse);
        }
        hasJumped = true;
        hasCrashed = false;

        jumpLandAudioSource.PlayOneShot(recoverClip);
        OSTFade.FadeIn();

        AdjustMinSpeed(originalMinSpeed - minSpeed);
        MakeInvincible();
    }

    void AllowTTP () { // Allow touch to play when player rigidbody isn't moving and no input is detected.
        if(playerRigidbody.velocity.magnitude <= 0.5f && !hasInput && Input.touchCount == 0)
            touchToPlay = true;
    }

    void RagdollFly () {
        playerRigidbody.freezeRotation = true;
        ragdollGraphic.transform.rotation = Quaternion.Euler(0f, 0f, -Vector2.SignedAngle(playerRigidbody.velocity.normalized, transform.right.normalized));
        if(heightAboveTerrain > 1f)
            hasJumped = true;     
    }

    void AnimateSnowglide () {
        if(isGrounded) {
            if(!isGrinding && !isOnRope && !hasCrashed) {
                snowglideParticles.Play();
            }
            else {
                snowglideParticles.Stop();
            }
        }
        else {
            snowglideParticles.Stop();
        }
    }

    void ParseInput () {
        if(IsPointerOverUIObject())
            return;
        if(gameManager.GetGameState() == GameManager.GameState.SettingsMenu) {
            return;
        }

        hasInput = false;
        if(inControl) {
            if(Input.GetMouseButtonDown(0)) {
                Jump();
                hasInput = true;
            }
            else if(Input.GetMouseButton(0)) {
                hasInput = true;
            }  
            else {
                playerAnimator.SetBool("Flip", false); // No input so no flip anim.
            }

            if(isGrinding || isGrounded)
                playerAnimator.SetBool("Flip", false);
            
        }
        else {
            if(touchToPlay) {
                hasInput = true;
                if(gameManager.GetGameState() == GameManager.GameState.Intro) {
                    if(Input.GetMouseButtonDown(0)) {
                        gameManager.FinishIntro();  
                    }
                    else    
                        hasInput = false;
                }
                else if(gameManager.GetGameState() == GameManager.GameState.Menu) {
                    if(Input.GetMouseButtonDown(0)) {
                        StartGame();
                    }
                    else    
                        hasInput = false;
                }
                else {
                    if(Input.GetMouseButtonDown(0)) {
                        if(hasCrashed) {
                            RecoverCrash();
                        }
                        else {
                            ContinueGame(); 
                        }
                    }
                    else
                        hasInput = false;
                }
            }
            else {
                hasInput = true;
                if(Input.touchCount == 0 && !Input.GetKey("space"))
                    hasInput = false;
            }
        }

        if(hasInput && !tapSwitch) {
            tapSwitch = true;            
        }
        else if(!hasInput && tapSwitch) {
            tapSwitch = false;
        }
    }



    void Freeze () {
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }
    void Unfreeze () {
        playerRigidbody.constraints = RigidbodyConstraints2D.None;
    
    }

    void HideGraphic () {
        myGraphic.SetActive(false);
    }
    void ShowGraphic () {
        myGraphic.SetActive(true);
    }

    float MomentumChange (float mult) { // Calculates change in velocity for animation.
        float angle = Vector2.SignedAngle(lastVelocity, playerRigidbody.velocity);
        currentAngle = Mathf.Lerp(currentAngle, angle, 2f * Time.fixedDeltaTime);
        lastVelocity = playerRigidbody.velocity;
        return currentAngle;
    }

    void AdjustMinSpeed () { // Called in FixedUpdate to slowly decrease min speed over time.
        if(minSpeed > originalMinSpeed && Time.time > lastSpeedRewardTime + speedLossDelay)
            minSpeed -= speedLoss * Time.fixedDeltaTime * (minSpeed / originalMinSpeed);
            
    }

    void AdjustInvincibility () { // Disables invincibility after a certain amount of time.
        if(Time.time > lastSpeedRewardTime + speedLossDelay) {
            isInvincible = false;
        }
    }
    

    void AirDrag () {
        if(isGrounded || isGrinding) {
            playerRigidbody.drag = 0.08f;
        }
        else {
            playerRigidbody.drag = 0.15f;
        }
    }

    void Gravity () {
        if(isGrounded) {
            playerRigidbody.gravityScale = gravityScaleOriginal;
            return;
        }

        playerRigidbody.gravityScale = gravityScaleOriginal * (1f - Mathf.Pow(((Mathf.Clamp(playerRigidbody.velocity.y, -100f, 0f)) / maxYVelocity), 2f));
    }

    void Flip () {
        if(!canFlip)
            return;
        if(!Input.GetMouseButton(0)) // Return if no input
            return;
        if(isGrounded || isGrinding) // We only flip if we're airborne
            return;
        anglesRotated = Vector3.SignedAngle(Vector3.down, transform.right, Vector3.forward);    
        if(anglesRotated < 0f && anglesRotated >= -35f && isFlipping) {
            trickSystem.AddTrick(TrickSystem.TrickType.Backflip);
            isFlipping = false;
        }
        else if(anglesRotated > 10f) {
            isFlipping = true;
            
        }
        float deltaRot = flipSpeed;
        playerRigidbody.angularVelocity = deltaRot;
        playerAnimator.SetBool("Flip", true);
    }

    void Grind () { // Constantly stores player position as lastPos. When grinding uses lastPos to calculate distance grinded.
        if(isGrinding) {
            distanceGrinded += Vector3.Distance(lastPos, transform.position);
            float currentDistance = Vector3.Distance(lastPos, transform.position);
            if(isOnRope) {
                trickSystem.AddTrick(TrickSystem.TrickType.RopeGrind, grindID, currentDistance);
            }
            else {
                trickSystem.AddTrick(TrickSystem.TrickType.ArchGrind, grindID, currentDistance);
            }
            playerAnimator.SetBool("Grind", true);
        }
        else {
            playerAnimator.SetBool("Grind", false);
        }
        lastPos = transform.position;
    }

    void Jump() {
        if(!canJump)
            return;
        if(!isGrounded)
            if(!isGrinding)
                return;
        if(!hasJumped) { // Check that we haven't already jumped
            float jumpVelocity = Mathf.Sqrt((gravityScaleOriginal * Physics.gravity.magnitude) * 2f * jumpHeight); // Velocity calculated based on desired height (h) -> V = Sqrt(2a * h)
            playerRigidbody.velocity += Vector2.up * jumpVelocity; // Use added velocity to jump
            hasJumped = true; // When this is true we can't jump again until we've landed and bool has been reset in RaysToGround()
            if(isInvincible) {
                invAnimation.Initiate(0.3f);
            }
        }
        else if(hasJumped && isGrinding) { // If we already jumped, but haven't landed on terrain - rather we landed on a grind.
            float jumpVelocity = Mathf.Sqrt((gravityScaleOriginal * Physics.gravity.magnitude) * 2f * jumpHeight);
            playerRigidbody.velocity += Vector2.up * jumpVelocity;
        }
        if(!isGrinding){
            jumpLandAudioSource.PlayOneShot(jumpClip);
        }
    }   

    void CompleteJump () { // This is used to reset hasJumped bool to false, and to finish any trick chains we've made while airborne.
        trickSystem.FinishChain();
        isFlipping = false;
        hasJumped = false;
        isGrinding = false;

        if(hasCrashed) {
            Crash(false);
        }
        else {
            RaysToGround();
            float dot = Vector2.Dot(transform.up.normalized, terrainNormal);
            if(dot <= 0.65f) {
                Crash(false);
            }
        }


        jumpLandAudioSource.PlayOneShot(landClip);
    }

    void MaintainMinSpeed () {
        if(!isGrounded || playerRigidbody.velocity.magnitude >= minSpeed)
            if(!isGrinding)
                return;
        if(hasJumped)
            if(!isGrinding)
                return;
        
        if(!isGrinding) {
            float acceleration = minSpeed * 1f;
            acceleration += Vector2.Dot(transform.right.normalized, Vector2.up) * playerRigidbody.gravityScale * Physics.gravity.magnitude;
            if(playerRigidbody.velocity.magnitude + acceleration * Time.fixedDeltaTime > minSpeed) {
                acceleration = (minSpeed - playerRigidbody.velocity.magnitude) / Time.fixedDeltaTime;
            }
            playerRigidbody.AddForce(transform.right * acceleration * playerRigidbody.mass);
        }
        else {  
            float grindSpeed = minSpeed * grindSpeedMult;
            if(playerRigidbody.velocity.magnitude < grindSpeed) {
                float acceleration = grindSpeed * 1f;
                acceleration += Vector2.Dot(transform.right.normalized, Vector2.up) * playerRigidbody.gravityScale * Physics.gravity.magnitude;
                if(playerRigidbody.velocity.magnitude + acceleration * Time.fixedDeltaTime > grindSpeed) {
                    acceleration = (grindSpeed - playerRigidbody.velocity.magnitude) / Time.fixedDeltaTime;
                }
                playerRigidbody.AddForce(transform.right * acceleration * playerRigidbody.mass);
            }
        }
    }

    void StickToGround () {
        if(!isGrounded)
            return;
        if(hasJumped)
            return;    
        if(isGrinding)
            return;
        Vector3 direction = -terrainNormal;
        playerRigidbody.AddForce(playerRigidbody.mass * 45f * direction);
    }

    void FaceGround () { // While grounded we want player to align with terrain's normal at all times
        if(!isGrounded) {// If we're airborne we slowly tilt to face the ground beneath us
            if(isOnRope) {
                if(terrainNormal != Vector2.zero) { // Just to make sure we see that there is a normal to align to
                    float angle = Vector2.SignedAngle(transform.up, terrainNormal);
                    float deltaRot = angle;
                    playerRigidbody.angularVelocity = deltaRot / Time.fixedDeltaTime;
                }
            }
            else {
                // Align to face terrain below us (Currently unused)
               /* if(terrainNormal != Vector2.zero) { // Just to make sure we see that there is a normal to align to
                    float angle = Vector2.SignedAngle(transform.up, terrainNormal);
                    if(!hasInput) {
                        float rotateSpeed = autoRotateSpeed * Mathf.Pow((1f - heightAboveTerrain / 25f), 2f);
                        float deltaRot = Mathf.Sign(angle) * rotateSpeed;
                        playerRigidbody.angularVelocity = deltaRot;
                    }
                }*/
                // Align to face ~45 degrees left. (Rotate clockwise)
                if(!hasInput || !canFlip) {
                    float angle = Vector2.SignedAngle(transform.up, new Vector2(1f, 1.2f));
                    float deltaRot = Mathf.Clamp(angle * 0.6f, -autoRotateSpeed, autoRotateSpeed);
                    
                    playerRigidbody.angularVelocity = deltaRot;
                }
                
            }
        }
        else { // If we're grounded we quickly align with the ground
            if(terrainNormal != Vector2.zero) { // Just to make sure we see that there is a normal to align to
                float angle = Vector2.SignedAngle(transform.up, terrainNormal);
                float deltaRot = angle;
                playerRigidbody.angularVelocity = deltaRot * 0.8f / Time.fixedDeltaTime;
            }
        }
    }

    void RopeGrind () {    
        if(isOnRope) {  
            if(isGrinding) {
                terrainNormal = ropeGrind.GetNormal2D(transform.position.x);
                ropeGrindAudioSource.volume =  Mathf.Lerp(ropeGrindAudioSource.volume, 1f * Time.timeScale, 15f * Time.unscaledDeltaTime);
                if(!ropeGrindAudioSource.isPlaying)
                    ropeGrindAudioSource.Play();   
            }
            else {
                if(ropeGrindAudioSource.isPlaying) 
                    ropeGrindAudioSource.Stop();
            }
        }
        else {
            if(ropeGrindAudioSource.isPlaying) 
                ropeGrindAudioSource.Stop();
        }
    }

    bool AttemptRopeGrind (Collider2D col) {
        if(hasCrashed)
            return false;

        ropeGrind = col.GetComponent<RopeGrind>();
        Vector2 normal = ropeGrind.GetNormal2D(transform.position.x);
        float dot = Vector2.Dot(transform.up.normalized, normal);
        print(dot);

        if(dot >= 0.5f)
            return true;
        else
            return false;
    }

    /* OPTIMIZATION NOTES < RaysToGround() >
    
    Change the frequency of raycasts based on our velocity and distance from ground: Less frequent rays when slow and high up in the air, more frequent
    when on the ground or close to it.
    
    */
    void RaysToGround () { // RAYCAST TO TERRAIN - Check for terrain's normal and see if player is grounded (Touching the terrain, or close to)
        int layerMaskA = 1 << 3;
        int layerMaskB = 1 << 14;
        int layerMask = layerMaskA | layerMaskB;
        Vector2 ray = new Vector3(0f, -1f);
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, ray, 25f, layerMask);

        
        if(hit.collider != null) { // If we hit terrain (within distance of 25)
            terrainNormal = hit.normal; // Store terrain's normal at hit point
            heightAboveTerrain = transform.position.y - hit.point.y;
            /*if(Vector2.Distance(transform.position, hit.point) < groundedDistance) { // If we are very close (within groundedDistance) we declare ourselves grounded
                isGrounded = true;
            }
            else {
                isGrounded = false;
                if(hasJumped) // If we were airborne after jumping, we've now landed back and can reset the jump boolean
                    hasJumped = false;
            }*/
        }
        else {// If we didn't hit terrain we store terrain's normal as zero
            terrainNormal = Vector2.zero;
            heightAboveTerrain = 25f;
        }

        
    }

    public float GetHeightAboveTerrain () {
        return heightAboveTerrain;
    }

    void OnTriggerEnter2D(Collider2D col) {
        /*if(col.gameObject.layer == 3) { // Terrain layer
            isGrounded = true;
            if(hasJumped) // If we were airborne after jumping, or after grinding: we've now landed back and can reset the jump boolean and finish score streak.
                CompleteJump();
        }*/
        if(col.gameObject.layer == 10) {
            isGrinding = true;   
            grindID = col.gameObject.GetInstanceID();
        }
        if(col.gameObject.layer == 11 && !col.isTrigger) {
            lastRopeCol = col;
            if(isGrounded) {
                if(Vector2.Dot(playerRigidbody.velocity.normalized, col.GetComponent<RopeGrind>().GetNormal2D(transform.position.x)) < 0f) {
                    col.enabled = false;
                }
            }  
            else {
                if(AttemptRopeGrind(col)) {   
                    hasJumped = true;
                }
                else {
                    print("col disabled");
                    col.enabled = false;
                }
            }
        }
    }
    void OnTriggerExit2D(Collider2D col) {
        if(col.gameObject.layer == 3) {
            isGrounded = false;
            hasJumped = true;
        }
        if(col.gameObject.layer == 10) {
            isGrounded = false;
            isGrinding = false;
        }
        if(col.gameObject.layer == 11) {
            //gameObject.layer = 12;
        }
    }

    void OnCollisionEnter2D (Collision2D col) {
        if(col.gameObject.layer == 3) {
            isGrounded = true;
            if(hasJumped || isGrinding) {
                CompleteJump();
            }
            isGrinding = false;
        }
        else if(col.collider.gameObject.layer == 11 && !col.collider.isTrigger) {
            if(isGrounded) {
                if(Vector2.Dot(playerRigidbody.velocity.normalized, col.collider.GetComponent<RopeGrind>().GetNormal2D(transform.position.x)) < 0f)
                    col.collider.enabled = false;
            }
            else {
                if(AttemptRopeGrind(col.collider)) {      
                    isOnRope = true;
                    hasJumped = true;
                    isGrinding = true;
                    grindID = col.collider.gameObject.GetInstanceID();
                    ropeGrind = col.collider.GetComponent<RopeGrind>();
                }
                else {
                    col.collider.enabled = false;
                }
            }
            
        }
    }
    void OnCollisionExit2D (Collision2D col) {
        if(col.collider.gameObject.layer == 11) {
            isOnRope = false;
            isGrinding = false;
            //gameObject.layer = 12;
        }
    }


    void SnowglideAudioControl() {
        if(inControl) {
            if(isGrounded && !isGrinding) {
                snowglideAudioSource.volume = Mathf.Lerp(snowglideAudioSource.volume, 1f * Time.timeScale, 15f * Time.unscaledDeltaTime);
            } 
            else {
                snowglideAudioSource.volume = Mathf.Lerp(snowglideAudioSource.volume, 0f, 35f * Time.unscaledDeltaTime);
            }
        }
    }

    private float velForWind;
    private float yVelForWind;

    void WindAudioControl() {
            yVelForWind = Mathf.Lerp(yVelForWind, Mathf.Abs(playerRigidbody.velocity.y), 0.5f);
            float yMod = Mathf.Clamp01((yVelForWind - 2f) / 20f);

            velForWind = Mathf.Lerp(velForWind, playerRigidbody.velocity.magnitude, 0.5f);
            float vMod = Mathf.Clamp01((velForWind - 5f) / 15f);
            vMod = Mathf.Sqrt(vMod);

            fallWindAudioSource.volume = Mathf.Lerp(0f, 1f, vMod);
            fallWindAudioSource.pitch = Mathf.Lerp(0.8f, 1.5f, yMod);
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
