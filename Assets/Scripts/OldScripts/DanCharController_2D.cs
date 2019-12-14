using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*	IMPORTANT PROJECT SETTINGS
* Physics > Disable "Queries Hit Triggers"
* Set up collision mask to not collide with self, and to collide with terrain, enemies, etc.
* 
* 
* 
*/
 
public abstract class DanCharController_2D : MonoBehaviour {

    // #region used to be able to collapse large chunks of code
    #region GlobalVariables
    public LayerMask collisionMask;

    [Header("Objects")]
    public Text UI_hpText;      // TODO: move UI elements into UI class on the UI Object, and have this controller communicate with that
    public Text UI_coinsText;
    public Slider UI_fuelSlider;
    public Slider UI_hpSlider;

    public GameObject jetpackEffects;
    public GameObject jetpackEmptyEffects;
    //public JetpackController jetpackController;

    public SpriteRenderer playerSprite;
    public SpriteRenderer attackSprite;

	public List<AudioSource> audioSources;
    public AudioClip groundJumpSound;
    public AudioClip airJumpSound;
    public AudioClip jumpLandSound;
    public List<AudioClip> footstepSounds;
    public List<AudioClip> groundAttackSounds;
    public List<AudioClip> airAttackSounds;
	public float jumpAudioVolume;
	public float landAudioVolume;
	public float footstepAudioVolume;
	public float AttackAudioVolume;
    private BoxCollider2D boxCollider;

    public bool footStepFlag = false;
    public bool footStepFlagHandled = false;

	public bool useUnscaledTime = false;
	private float deltaTime = 0f;

    [System.Serializable]
    public class InputState  // subclasses used to categorise and order variables in unity inspector
    {
        public bool moveLeft_bHeld = false;
        public bool moveRight_bHeld = false;
        public bool moveUp_bHeld = false;
        public bool moveDown_bHeld = false;

        public bool moveLR_bUp = false;
        public bool moveLR_bDown = false;
        public bool moveUD_bUp = false;
        public bool moveUD_bDown = false;


        public bool jump_bUp = false;
        public bool jump_bHeld = false;
        public bool jump_bDown = false;

        public bool jetpack_bUp = false;
        public bool jetpack_bHeld = false;
        public bool jetpack_bDown = false;

        public bool dash_bUp = false;
        public bool dash_bHeld = false;
        public bool dash_bDown = false;

        public bool attack_bUp = false;
        public bool attack_bHeld = false;
        public bool attack_bDown = false;

        public bool sprint_bDown = false;
        public bool sprint_bHeld = false;
        public bool sprint_bUp = false;

        public bool crouch_bUp = false;
        public bool crouch_bHeld = false;
        public bool crouch_bDown = false;

		public bool restart_bDown = false;
	}
    public InputState input;

    [System.Serializable]
    public class Abilities
    {
		public bool sprintEnabled = false;
		public bool crouchEnabled = false;

		public bool attackEnabled = false;
        public bool jetpackEnabled = false;
        public int maxAirJumps = 0;
        public bool wallJumpEnabled = false;
        public float maxDashes = 1;
    }
    public Abilities abilities;

    //Rigidbody rb;
    //Renderer r;

    Vector2 respawnPos;
    RaycastHit2D rayHit;
    protected Animator anim;

    [System.Serializable]
    public class Stats
    {
        public int playerHealth = 100;
        public int HEALTH_MAX = 100;
        public int dosh = 0;
        public float jetpackFuel = 1.0f;
        public float JETPACK_FUEL_MAX = 1.0f;

    }
    public Stats stats;

    // [SerializeField] : make variable visible in inspector, without making it public.

    [System.Serializable]
    public class General
    {
        public float ON_GROUND_GRACE_MAX = 0.1f;        // how long the player is treated as 'on the ground for' after leaving it. makes jumping off ledges friendlier when the player pushes the button a bit too late.
        public float DAMAGE_INTERVAL = 1.0f;            // how long the player is invulnerable for after taking damage   
        public int MAX_SPRITE_DAMAGE_BLINK_TIME = 3;    // the rate at which the sprite flickers when invulnerable
        public float GRAVITY_FORCE = 50f;               // how strong gravity is when applied to the player
		public bool isRightFacing = true;   // move into struct
	}
	public General general;

    Vector2 velocity = new Vector3(0, 0, 0);
    //Vector2 accel = new Vector3(0, 0, 0);

    float recentDamageTimer = 0;
    bool recentDamage = false;
    public bool onGround = false; // TODO: Make all state variables (onGround, collision, jumping, attacking, etc) public, so other entities can act based on them. Organise in a struct
    int onGroundGraceTimerState = 0;
    float onGroundGraceTimer = 0f;
    bool rightCollision = false;
    bool leftCollision = false;
    bool upCollision = false;
    bool isSpriteActive = true;
    int spriteDamageBlinkTimer = 0;
    bool isKnockbackActive = false;
    float knockbackTimer = 0f;
    float knockbackTimerMax = 0f; // set by AddKnockback() on each knockback instance
    bool isKnockbackControllable = false;

    // used by SetPlayerAbility to disable abilities temporarily
    bool disableMovement = false;
    bool disableJump = false;
    bool disableDash = false;
    bool disableWallJump = false;
    int movementDisableCounter = 0;
    int jumpDisableCounter = 0;
    int wallJumpDisableCounter = 0;
    int dashDisableCounter = 0;

    [System.Serializable]
    public class Jetpack
    {
        public float FORCE = 21f;                       // the speed of jetpack movement
        public float FUEL_DRAIN_MULTIPLIER = 1.0f;      // how fast the jetpack fuel drains
    }
    public Jetpack jetpack;

    bool isJetpacking = false;
    bool isJetpackHeld = false;
    bool isJetpackExpended = false;
    Vector2 jetpackDir = Vector2.zero;

    [System.Serializable]
    public class Walk
    {
        public float MAX_SPEED = 8f;			 // how fast the player can walk
        public float DECAY_SPEED = 50f;          // how fast the player reaches said max walking speed
        public float ACCEL_SPEED = 50f;          // same as above, but for slowing down again
        public float STEP_INTERVAL = 2f;
    }
    public Walk walk;

	[System.Serializable]
	public class Sprint
	{
		public float MAX_SPEED = 12f;            // how fast the player can walk
		public float DECAY_SPEED = 50f;          // how fast the player reaches said max walking speed
		public float ACCEL_SPEED = 50f;          // same as above, but for slowing down again
		public float STEP_INTERVAL = 2f;
	}
	public Sprint sprint;

	[System.Serializable]
	public class Crouch
	{
		public float MAX_SPEED = 4f;			 // how fast the player can walk
		public float DECAY_SPEED = 50f;          // how fast the player reaches said max walking speed
		public float ACCEL_SPEED = 50f;          // same as above, but for slowing down again
		public float STEP_INTERVAL = 2f;
		public float scale = 0.5f;				// height of crouched player, relative to full height
		public float offset = 0.5f;				// amount player's centre moves down when crouching
	}
	public Crouch crouch;

	bool isWalking = false;
	bool isSprinting = false;
	bool isCrouching = false;
	float stepCounter = 0;

	[System.Serializable]
    public class Jump
    {
        public float MIN_DURATION = 0.075f;     // minimum duration the jump button will be held for, regardless of how long the player actually holds it for. Makes small jumps feel more consistent
        public float START_DECAY = 0.24f;       // point at which the player begins decellerating
        public float INIT_FORCE = 12f;          // force applied at start of jump
        public float ACCEL_FORCE = 0f;          // force applied every frame.
        public float DECAY_RATE = 50f;          // rate at which the jump decays towards its peak
        public float MAX_RISE_SPEED = 12f;      // maximum upwards speed during jump
        public float MAX_FALL_SPEED = 18f;      // maximum fall speed
        public float FALL_ACCEL_FORCE = 0f;     // downwards pull applied only while falling [y > 0]
        public float FLOATINESS = 0.5f;         // Range: 0 to 1. kills a portion of the y velocity when the player lets go of the jump button. Greater is floatier.
    }
    public Jump jump;

    int airJumpCounter = 0;
    bool isJumping = false;
    bool isJumpHeld = false;
    float jumpDuration = 0f;
    bool jumpLanded = true;

    [System.Serializable]
    public class WallJump
    {
        public bool WALL_CLIMB = false;         // allows player to jump of the same wall over and over. This lets the player scale an singular wall indefinitely.
        public bool SLIDE_AFTER_JUMP = false;   // disables the slide slowdown if the player cannot currently jump off the wall.
        public float FALL_SPEED_MAX = 5f;       // how fast the player falls when sliding on a wall
    }
    public WallJump wallJump;

    //bool isWallJumping = false;
    bool hasLeftWallJumped = false;
    bool hasRightWallJumped = false;

    [System.Serializable]
    public class Dash
    {
        public float DURATION = 0.2f;           // the length of the dash in seconds
        public float SPEED = 30f;               // how fast the player is moving for the duration
        public float RECHARGE_DELAY = 1.5f;     // how long it takes for a single dash usage to regenerate, does nothing if RESET_ON_GROUND is true
        public float RECHARGE_RATE = 1f;        // how much of a delay there is before dash charges begin to regenerate
        public bool RESET_ON_GROUND = true;     // if false, it uses the above delayed recharge system. If true, all charges are immediately restored on landing.
        public float counter = 1;               // current dash count               
    }
    public Dash dash;

    float dashActiveTimer = 0f;
    bool isDashing = false;
    float dashRechargeTimer = 0f;
    Vector2 dashDir;

    [System.Serializable]
    public class Collision
    {
        public float OFFSET = .2f;                              // no longer necessary, delete later
        public float RAY_DISTANCE_LR = 0.3f;                    // the distance the left and right ray casts are sent out to, defining how close the player can get to a wall.
        public float RAY_DISTANCE_DOWN = 0.6f;                  // as above, for down
        public float RAY_DISTANCE_UP = 0.3f;                    // as above, for up
        public float UP_COLLISION_STRICTNESS_FACTOR = 0.5f;     // how lenient the up collision is with corners. High leniency will have the player round the corner instead of colliding.
    }
    public Collision collision;

    #endregion

    protected virtual void Start () // virtual allows it to be overridden by child AI classes
    {
        //Application.targetFrameRate = 60;   // Place in global script, or remove.
        //r = GetComponent<Renderer>();
        //rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        respawnPos = this.transform.position; // initialise respawn position to starting position.
		UI_fuelSlider.gameObject.SetActive(abilities.jetpackEnabled); // enable jetpack UI if jetpack is enabled
        UpdateUI();
    }
	
	protected virtual void Update () 
    {
		if (useUnscaledTime)
		{
			deltaTime = Time.unscaledDeltaTime;
		}
		else
		{
			deltaTime = Time.deltaTime;
		}

		if (input.restart_bDown)
		{
			// improve this functionality in the future
			ReloadCurrentScene();
		}

        HandleMovement();
        HandleJump();
        //HandleJetpack();
        HandleDash();
        HandleAttack();

        HandleKnockback();
        EnforceMaxSpeeds();
        HandleRecentDamage();
        DetectCollision();

        // apply final velocity to character
        this.transform.Translate(velocity * deltaTime);
        anim.SetFloat("velocity.x", velocity.x);
        anim.SetFloat("velocity.y", velocity.y);
        //print(velocity.ToString("F4"));

        // flip the player sprite if necessary
        if (velocity.x < 0 && general.isRightFacing)
        {
            FlipSprite();

        }
        else if (velocity.x > 0 && !general.isRightFacing)
        {
            FlipSprite();
        }
    }

    void UpdateUI()
    {
        UI_hpText.text = "Health: " + stats.playerHealth;
        UI_hpText.text = "Health: " + stats.playerHealth;

        if (abilities.jetpackEnabled)
        {
            UI_fuelSlider.value = stats.jetpackFuel;
        }

        float hp = (float)stats.playerHealth / 100f;
        UI_hpSlider.value = hp - (1f - hp) * 0.03f; // slight offset to fix visuals
        UI_coinsText.text = "" + stats.dosh;
    }

    #region HandleEverything
    void HandleMovement()
    {
        float max_speed;
        float accel_speed;
        float decay_speed;
		float step_interval;
        // if sprinting, use sprint values
        if (abilities.sprintEnabled && input.sprint_bHeld && !(isCrouching && upCollision == true) )
        {
            //print("SPRINTING");
			max_speed = sprint.MAX_SPEED;
			accel_speed = sprint.ACCEL_SPEED;
			decay_speed = sprint.DECAY_SPEED;
			step_interval = sprint.STEP_INTERVAL;

			if (isCrouching)  // return character to full size if currently crouched
			{
				SpriteSquish(false);
			}

			isSprinting = true;
			isCrouching = false;
		}
		// if crouching, use crouch values
		else if (abilities.crouchEnabled && (input.crouch_bHeld || (!input.crouch_bHeld && upCollision == true) ) )
        {
            //print("CROUCHING");
			max_speed = crouch.MAX_SPEED;
			accel_speed = crouch.ACCEL_SPEED;
			decay_speed = crouch.DECAY_SPEED;
			step_interval = crouch.STEP_INTERVAL;

			if (!isCrouching)  // squish character if not currently crouched
			{
				SpriteSquish(true);
			}
			isSprinting = false;
			isCrouching = true;

		}
		// otherwise, use normal walk speed
		else
        {
            //print("WALKING");
			max_speed = walk.MAX_SPEED;
			accel_speed = walk.ACCEL_SPEED;
			decay_speed = walk.DECAY_SPEED;
			step_interval = walk.STEP_INTERVAL;

			if (isCrouching)  // return character to full size if currently crouched
			{
				SpriteSquish(false);
			}

			isSprinting = false;
			isCrouching = false;
		}

		// handle sprite squishing for crouching
		//if (input.crouch_bDown)
		//	SpriteSquish(true);
		//if (input.crouch_bUp)
		//	SpriteSquish(false);

		//move right
		//getAxis determines which direction player is moving in.
		//jetpacking and uncontrollable knockback overrides normal movement
		if (input.moveRight_bHeld && !isJetpacking && !disableMovement )
        {
			print("iswalking right");
			if (!rightCollision) // prevent movement into collision objects
            {
                velocity += Vector2.right * accel_speed * deltaTime; // apply movement velocity
                stepCounter += deltaTime * max_speed;
                if (velocity.x > max_speed) // cap walking speed
                {
                    velocity.x = max_speed;
                }
                isWalking = true;
                anim.SetBool("isWalking", true);
            }
        }

        //move left
        if (input.moveLeft_bHeld && !isJetpacking && !disableMovement)
        {
            if (!leftCollision)
            {
                velocity += Vector2.left * accel_speed * deltaTime;
                stepCounter += deltaTime * max_speed;
                if (velocity.x < -max_speed)
                {
                    velocity.x = -max_speed;
                }
                isWalking = true;
                anim.SetBool("isWalking", true);
            }
        }

        // apply movement decay
        if (!isWalking && !isJetpacking /*&& !disableMovement*/)    // dont perform under specific circumstances
        {
            if (velocity.x > 0)
            {
                velocity.x -= decay_speed * deltaTime;
                if (velocity.x < 0)
                    velocity.x = 0;
            }

            if (velocity.x < 0)
            {
                velocity.x += decay_speed * deltaTime;
                if (velocity.x > 0)
                    velocity.x = 0;
            }
        }

        // apply gravity
        if (!isJumping && !isJetpacking && (!onGround || onGroundGraceTimerState == 1)) // grace 1: active
        {
            velocity.y -= general.GRAVITY_FORCE * deltaTime;
        }

        // apply slight downward force when falling
        if (velocity.y < 0)
        {
            velocity.y -= jump.FALL_ACCEL_FORCE * deltaTime;
        }

        // handle footstep sound
        if (stepCounter >= step_interval)
        {
			// play sound
			//PlayAudioClip(groundJumpSound, AttackAudioVolume);
			//stepCounter -= step_interval;
			stepCounter = 0f;
        }

        if (footStepFlag && !footStepFlagHandled) // set by animator
        {
			//int i = Mathf.FloorToInt(Random.Range(0.0f, 3.99f)); // 0 - 4
			int i = Random.Range(0, footstepSounds.Count);
            PlayAudioClip(footstepSounds[i], footstepAudioVolume);
            footStepFlagHandled = true;
        }
        if (!footStepFlag && footStepFlagHandled)
        {
            footStepFlagHandled = false;
        }

        // reset walk flag the moment the key is released
        if (input.moveLR_bUp)
        {
            EndMovement();
        }
    }

    // seperated to enable calling in multiple locations
    void EndMovement()
    {
        isWalking = false;
        anim.SetBool("isWalking", false);
        stepCounter = 0f;
    }

    void HandleJump()
    {
        // reset a few things when player lands
        if (onGround)
        {
            airJumpCounter = 0; // air jumps
            hasLeftWallJumped = false; // wall jump logic
            hasRightWallJumped = false;
            if (!jumpLanded)
            {
                PlayAudioClip(jumpLandSound, landAudioVolume);
                jumpLanded = true;
            }
        }

        //unity>edit>project settings >input - define input values
        if (input.jump_bDown)
        {
            isJumpHeld = true;
            if (!disableJump)
            {
                // check for wall jumping
                // if wall jump enabled AND player on ground, AND EITHER (conditional wall logic) OR (wall climb enabled AND any wall collision)
                // conditional wall logic = (right collision AND hasnt just jumped off a right wall OR left collision AND hasnt just jumped off a left wall)
                if (abilities.wallJumpEnabled && !onGround && ((rightCollision && !hasRightWallJumped) || (leftCollision && !hasLeftWallJumped) || (wallJump.WALL_CLIMB && (leftCollision || rightCollision))))
                {
                    PlayAudioClip(groundJumpSound, jumpAudioVolume); //CHANGE TO UNIQUE SOUND
                    isJumping = true;
                    velocity.y = 0f; // negate preexisting y velocity
                    velocity += Vector2.up * jump.INIT_FORCE; // apply jump force upwards

                    // also apply sideways force, depending on collision direction
                    if (leftCollision)
                    {
                        velocity += Vector2.right * jump.INIT_FORCE;

                        if (!wallJump.WALL_CLIMB)   //skip this logic if WALL_CLIMB is on, it isnt needed
                        {
                            hasLeftWallJumped = true;   // prevents repeated wall jumping off the same wall direction
                            hasRightWallJumped = false; // enables jumping off the opposite wall direction
                        }
                    }
                    else // right collision
                    {
                        velocity += Vector2.left * jump.INIT_FORCE;
                        if (!wallJump.WALL_CLIMB)
                        {
                            hasRightWallJumped = true;
                            hasLeftWallJumped = false;
                        }
                    }
                }
                // check for air jumping
                else if (!onGround && airJumpCounter < abilities.maxAirJumps)
                {
                    airJumpCounter++;
                    PlayAudioClip(airJumpSound, jumpAudioVolume);
                    isJumping = true;
                    velocity.y = 0f;
                    velocity += Vector2.up * jump.INIT_FORCE;
                }
                // check for ground jumping
                else if (onGround)
                {
                    PlayAudioClip(groundJumpSound, jumpAudioVolume);
                    isJumping = true;
                    velocity.y = 0f;
                    velocity += Vector2.up * jump.INIT_FORCE;
                } // else, dont jump
            }
        }

        if (input.jump_bUp)
        {
            isJumpHeld = false;
        }

        // check for the end of the jump
        if (isJumping)
        {
            // end jump if button no longer held, and min jump duration elapsed
            if (!isJumpHeld && jumpDuration >= jump.MIN_DURATION)
            {
                EndJump();
                velocity.y *= jump.FLOATINESS; // apply jump floatiness
            }
            else
            {
                //main jump logic, applied every frame of the jump
                // end the jump when player is no longer moving up.
                if (velocity.y > 0f)
                {
                    //slow down jump after START_DECAY time
                    if (jumpDuration >= jump.START_DECAY)
                    {
                        velocity.y += jump.ACCEL_FORCE - (jump.DECAY_RATE * deltaTime); // apply decay
                        if (velocity.y < 0f)
                        {
                            velocity.y = 0f;
                        }
                    }
                    else // full speed head otherwise
                    {
                        velocity.y += (jump.ACCEL_FORCE * deltaTime);
                        jumpDuration += deltaTime;
                    }

                    //enforce max jump speed
                    if (velocity.y > jump.MAX_RISE_SPEED)
                    {
                        velocity.y = jump.MAX_RISE_SPEED;
                    }
                }
                else // end jump
                {
                    velocity.y = 0f;
                    EndJump();
                }
            }
        }
    }

    void EndJump()
    {
        jumpDuration = 0;
        isJumping = false;
        jumpLanded = false;
    }

	/*
    void HandleJetpack()
    {
        if (!abilities.jetpackEnabled) // do nothing if jetpack hasnt been acquired yet
        {
            return;
        }

        // refuel if on the ground and not actively jetpacking
        if (onGround && !isJetpacking)
    	{
    		if (stats.jetpackFuel != stats.JETPACK_FUEL_MAX) // prevent continually refuelling jetpack and updating UI
    		{
    			stats.jetpackFuel = stats.JETPACK_FUEL_MAX;
    			UpdateUI();    			
    		}

    	}

        if (input.jetpack_bDown) // add && !isJetpackExpended to allow pressing JP in advance of next boost
        {
			isJetpackHeld = true;
        }

        // main logic
        // the jetpack will not function when expended
        if (input.jetpack_bHeld && !isJetpackExpended)
        {
	        bool dirSet = false;

	        // set direction based on held direction key at moment of activating jetpack
        	if (input.moveUp_bHeld || input.moveDown_bHeld || input.moveLeft_bHeld || input.moveRight_bHeld)
        	{
        		isJetpacking = true;
                jetpackDir = GetDir();
                dirSet = true;
            }

	        // perform movement if a direction is held
            if (dirSet)
            {
                //HandleJetpackEffects(true, jetpackDir);
                jetpackController.SendMessage("SetJetpackState", true);
                jetpackController.SendMessage("SetJetpackDir", jetpackDir);

                // move nowhere if a collision is occuring (by setting the direction to 0)
                if (jetpackDir == Vector2.left && leftCollision)
                {
                	jetpackDir = Vector2.zero;
                }
                else if (jetpackDir == Vector2.right && rightCollision)
                {
                	jetpackDir = Vector2.zero;
                }
                else if (jetpackDir == Vector2.up && upCollision)
                {
                	jetpackDir = Vector2.zero;
                }
                else if (jetpackDir == Vector2.down && onGround)
                {
                	jetpackDir = Vector2.zero;
                }

                velocity = jetpackDir * jetpack.FORCE * 1f; // move
                stats.jetpackFuel -= jetpack.FUEL_DRAIN_MULTIPLIER * deltaTime; // drain fuel
                if (stats.jetpackFuel < 0)
                {
                    stats.jetpackFuel = 0;
                }
				UpdateUI();
            }
            else // no direction held, do nothing
            {
            	isJetpacking = false;
                //HandleJetpackEffects(false, jetpackDir);
                jetpackController.SendMessage("SetJetpackState", false);
                jetpackController.SendMessage("SetJetpackDir", jetpackDir);

            }
        }

        // respond to letting go of the button
        if(input.jetpack_bUp)
        {
        	isJetpacking = false;
			isJetpackHeld = false;
            //HandleJetpackEffects(false, jetpackDir);
            jetpackController.SendMessage("SetJetpackState", false);
            jetpackController.SendMessage("SetJetpackDir", jetpackDir);
        }

        // respond to running out of fuel
        if (stats.jetpackFuel <= 0)
		{
        	isJetpacking = false;
        	isJetpackExpended = true;
            //HandleJetpackEffects(false, jetpackDir);			
            HandleJetpackEmptyEffects();
            jetpackController.SendMessage("SetJetpackState", false);
            jetpackController.SendMessage("SetJetpackDir", jetpackDir);
        }

        // the jetpack is unexpended (and thus usable again) only after regaining fuel AND letting go of the button
        if (isJetpackExpended)
		{
			if (stats.jetpackFuel > 0 && !isJetpackHeld)
			{
				isJetpackExpended = false;
			}
		}
    }
	*/
    void HandleDash()
    {
        if (input.dash_bDown && dash.counter >= 1f && !disableDash) // dash only if enabled, and player has dashes remaining
        {
            dash.counter -= 1f;
            isDashing = true;
            dashDir = GetDir();
            dashRechargeTimer = 0f;

            // dash forward if no direction is held
            if (dashDir == Vector2.zero || dashDir == Vector2.up || dashDir == Vector2.down)  // extra checks disable up and down dashing for the time being
            {
                if (general.isRightFacing)
                {
                    dashDir = Vector2.right;
                }
                else
                {
                    dashDir = Vector2.left;
                }
            }
        }
        // apply dash velocity every frame for the duration
        if (isDashing)
        {
            velocity = dashDir * dash.SPEED * 1f;
            dashActiveTimer += deltaTime;
            if (dashActiveTimer >= dash.DURATION) // end when duration has elapsed
            {
                EndDash();
            }
        }
        if (dash.RESET_ON_GROUND)   // reset dash charges on landing (if enabled)
        {
            if(onGround && !isDashing)
            {
                dash.counter = abilities.maxDashes;
            }
        }
        else    // reset dash over time when not in use (if above isnt enabled)
        {
            if (!isDashing && (dash.counter < abilities.maxDashes)) // commence recharge if not currently dashing, and not at full dash charges.
            {
                if (dashRechargeTimer < dash.RECHARGE_DELAY) // delay start of recharge
                {
                    dashRechargeTimer += deltaTime;
                }
                else if (dash.counter < abilities.maxDashes) // recharge until full
                {
                    dash.counter += deltaTime * dash.RECHARGE_RATE;
                    if (dash.counter > abilities.maxDashes)
                    {
                        dash.counter = abilities.maxDashes;
                        //dashRechargeTimer = 0f;
                    }
                }
            }
        }

        //print("DASHOMETER: " + dash.counter);
    }

    void EndDash()
    {
        isDashing = false;
        dashActiveTimer = 0f;
    }

    bool isAttacking = false;
    bool isAttackOnCooldown = false;
    float attackCooldownTimer = 0f;
    float ATTACK_COOLDOWN = 1f;
    float ATTACK_ANIM_SPEED = 1f;
    float tempAnimTimer = 0f;
    bool isAirAttack = false;
    bool attackSoundPlayed = false;

    void HandleAttack_OLD()
    {
        if (input.attack_bDown && !isAttacking && !isAttackOnCooldown) // add more clauses as needed
        {
            isAttacking = true;
            attackCooldownTimer = 0f;
            print("Commence Attack");
            //attackSprite.gameObject.SetActive(true);
            Instantiate(attackSprite, this.transform.position, this.transform.rotation); // SETACTIVE INSTEAD, put swipe as child of char
        }

        if (isAttacking)
        {
            print("attack progressing:");
            // progress animation
            tempAnimTimer += deltaTime;
            if (tempAnimTimer >= 1f)
            {
                // if anim finished
                // commence CD
                tempAnimTimer = 0f;
                isAttacking = false;
                isAttackOnCooldown = true;
            }
        }

        if (isAttackOnCooldown)
        {
            print("Attack on CD: ");
            attackCooldownTimer += deltaTime;
            if (attackCooldownTimer >= ATTACK_COOLDOWN)
            {
                attackCooldownTimer = 0f;
                isAttackOnCooldown = false;
                print("Attack Complete.");
            }
        }
    }

    void HandleAttack()
    {
		// do nothing if attack is disabled
		if (abilities.attackEnabled == false)
			return;

        // check for start of attack chain
        if (input.attack_bDown && !isAttacking && !isAttackOnCooldown) // add more clauses as needed
        {
            isAttacking = true;
            attackCooldownTimer = 0f;
            anim.SetInteger("attackState", 1);
            attackSoundPlayed = false;
            
            // different flags for air and ground attacks
            if (onGround && onGroundGraceTimerState == 0)
            {
                SetPlayerAbility("movement", false);
                isAirAttack = false;
                print("Commence Attack");
            }
            else
            {
                isAirAttack = true;
                print("Commence Air Attack");
            }
        }

        // main logic for GROUND attack
        if (isAttacking && !isAirAttack)
        {
            // attack interruptable by movement after 0.6s
            if (tempAnimTimer >= 0.6f && (input.moveLeft_bHeld || input.moveRight_bHeld || input.jump_bDown) )
            {
                EndAttack();
                print("Attack Interrupted");
                return;
            }
            // attack interruptable by jumping
            if (input.jump_bDown)
            {
                EndAttack();
                print("Attack Interrupted");
                return;
            }

            // ATTACK 1
            // Move player during attack
            if (tempAnimTimer >= 0.14f && tempAnimTimer <= 0.24f && anim.GetInteger("attackState") == 1)
            {
                if (general.isRightFacing)
                {
                velocity.x += 100f * deltaTime; 
                }
                else
                {
                velocity.x -= 100f * deltaTime; 
                }
            }

            // play sound at right moment
            if (tempAnimTimer >= 0.1f && !attackSoundPlayed && anim.GetInteger("attackState") == 1)
            {
                PlayAudioClip(groundAttackSounds[0], AttackAudioVolume);
                attackSoundPlayed = true;
            }


            // ATTACK 2
            // Move player during attack
            if (tempAnimTimer >= 0.14f && tempAnimTimer <= 0.24f && anim.GetInteger("attackState") == 2)
            {
                if (general.isRightFacing)
                {
                velocity.x += 100f * deltaTime; 
                }
                else
                {
                velocity.x -= 100f * deltaTime; 
                }
            }

            // play sound at right moment
            if (tempAnimTimer >= 0.1f && !attackSoundPlayed && anim.GetInteger("attackState") == 2)
            {
                PlayAudioClip(groundAttackSounds[1], AttackAudioVolume);
                attackSoundPlayed = true;
            }

            // ATTACK 3
            // Move player during attack
            if (tempAnimTimer >= 0.14f && tempAnimTimer <= 0.3f && anim.GetInteger("attackState") == 3)
            {
                if (general.isRightFacing)
                {
                velocity.x += 1000f * deltaTime; 
                }
                else
                {
                velocity.x -= 1000f * deltaTime; 
                }
            }

            // play sound at right moment
            if (tempAnimTimer >= 0.1f && !attackSoundPlayed && anim.GetInteger("attackState") == 3)
            {
                PlayAudioClip(groundAttackSounds[2], AttackAudioVolume);
                attackSoundPlayed = true;
            }

            // Proceed to attack 2 if conditions are met 
            // attack can proceed after 0.3s, on attack input
            if (tempAnimTimer >= 0.24f && anim.GetInteger("attackState") == 1 && input.attack_bDown)
            {
                anim.SetInteger("attackState", 2);
                tempAnimTimer = 0f;
                attackSoundPlayed = false;
                

                // on attack, flip direction if player is holding the other direction
                if ( (general.isRightFacing && input.moveLeft_bHeld) || (!general.isRightFacing && input.moveRight_bHeld) )
                {
                    FlipSprite();
                }
            }

            // Proceed to attack 3 if conditions are met 
            // each attack is seperate to allow for different conditions
            if (tempAnimTimer >= 0.24f && anim.GetInteger("attackState") == 2 && input.attack_bDown)
            {
                anim.SetInteger("attackState", 3);
                tempAnimTimer = 0f;
                attackSoundPlayed = false;

                // on attack, flip direction if player is holding the other direction
                if ( (general.isRightFacing && input.moveLeft_bHeld) || (!general.isRightFacing && input.moveRight_bHeld) )
                {
                    FlipSprite();
                }
            }

            // attack ends after 1s
            if ( (tempAnimTimer >= 1.0f && anim.GetInteger("attackState") != 3) || 
                 (tempAnimTimer >= 0.6f && anim.GetInteger("attackState") == 3) ) // shorter end on final attack
            {
                EndAttack();
            }
            tempAnimTimer += deltaTime;
        }

        // main logic for AIR attack
        if (isAttacking && isAirAttack)
        {
            
            // ATTACK 1
            // Move player during attack
            if (tempAnimTimer >= 0.0f && tempAnimTimer <= 0.15f && anim.GetInteger("attackState") == 1)
            {
                if (general.isRightFacing)
                {
                velocity.x += 100f * deltaTime; 
                }
                else
                {
                velocity.x -= 100f * deltaTime; 
                }
            }

            // play sound at right moment
            if (tempAnimTimer >= 0.1f && !attackSoundPlayed)
            {
                PlayAudioClip(airAttackSounds[0], AttackAudioVolume);
                attackSoundPlayed = true;
            }

            // end attack after enough time, or if the player hits the ground
            if (tempAnimTimer >= 0.3f || onGround)
            {
                EndAttack();
            }
            tempAnimTimer += deltaTime;
        }
        
    }

    void HitBoxTriggered(Collider2D other)
    {
        print ("Hit something!");
    }

    // end any existing attack animation
    void EndAttack()
    {
            anim.SetInteger("attackState", 0);
            tempAnimTimer = 0f;
            isAttacking = false;
            SetPlayerAbility("movement", true);
            attackSoundPlayed = false;
    //      isAttackOnCooldown = false;
    //      attackCooldownTimer = 0f;
    }

    // handle any knockback effect currently applying to the player
    void HandleKnockback()
    {
        if (!isKnockbackActive) // do nothing if not active
        {
            return;
        }

        knockbackTimer += deltaTime;
        if (knockbackTimer >= knockbackTimerMax || (onGround && velocity.y < 0) ) // end effect when time expires, or the player hits the ground.
        {
            isKnockbackActive = false;
            knockbackTimer = 0f;
            knockbackTimerMax = 0f;

            if (!isKnockbackControllable)   // return player control if it was taken from them
            {
                SetPlayerAbility("all", true);
                isKnockbackControllable = true;
            }
        }
    }
    void HandleJetpackEmptyEffects()
    {

    }

    // contains invincibility timer that applies after taking damage
    void HandleRecentDamage()
    {
        // count down damage timer
        if (recentDamage)
        {
            // blink player every few frames
            if (spriteDamageBlinkTimer >= general.MAX_SPRITE_DAMAGE_BLINK_TIME)
            {
                ToggleSprite();
                spriteDamageBlinkTimer = 0;
            }
            spriteDamageBlinkTimer++;

            recentDamageTimer -= deltaTime;

            if (recentDamageTimer <= 0) // end effect when time is up
            {
                recentDamage = false;
                if (!isSpriteActive) // ensure sprite is active once flickering stops.
                {
                    ToggleSprite();
                }
            }
        }
    }
    #endregion


    // enforce maximum movement speeds
    void EnforceMaxSpeeds()
    {
        if (!isJetpacking && !isDashing && !isKnockbackActive) // ignore maximums when jetpacking dashing, or being knocked back
        {
            // enforce jump maximum
            if (velocity.y > jump.MAX_RISE_SPEED)
            {
                velocity = new Vector2(velocity.x, jump.MAX_RISE_SPEED);
            }
            else // check for falling maximums
            {
                bool isSliding = false;
                if (wallJump.SLIDE_AFTER_JUMP)
                {
                    // slide if colliding with something, and wall jumping is enabled
                    if ((rightCollision || leftCollision) && (abilities.wallJumpEnabled))
                    {
                        isSliding = true;
                    }
                }
                else
                {
                    // slide if colliding with something AND player didnt just jump off a wall facing the same direction, and wall jumping is enabled
                    if (((rightCollision && !hasRightWallJumped) || (leftCollision && !hasLeftWallJumped)) && (abilities.wallJumpEnabled))
                    {
                        isSliding = true;
                    }

                }

                if (isSliding)
                {
                    if (velocity.y < -wallJump.FALL_SPEED_MAX) // apply sliding max fall speed
                    {
                        velocity = new Vector3(velocity.x, -wallJump.FALL_SPEED_MAX);
                    }
                }
                else
                {
                    if (velocity.y < -jump.MAX_FALL_SPEED) // apply regular max fall speed
                    {
                        velocity = new Vector3(velocity.x, -jump.MAX_FALL_SPEED);
                    }
                }
            }

			// choose max speed to check, based on current state
			float max_speed;
			if (abilities.sprintEnabled && input.sprint_bHeld)
				max_speed = sprint.MAX_SPEED;
			else if (abilities.crouchEnabled && input.crouch_bHeld)
				max_speed = crouch.MAX_SPEED;
			else
				max_speed = walk.MAX_SPEED;

			// enforce max horizontal walk speeds
			if (velocity.x > max_speed)
            {
                velocity = new Vector3(max_speed, velocity.y);
            }
            else if (velocity.x < -max_speed)
            {
                velocity = new Vector3(-max_speed, velocity.y);
            }
        }
    }

    #region Constraints
    // detects collision objects around the player and prevents movement into them
    // redefined by me to provide smoother functionality and greater control over the default rigidbody collision mechanics
    void DetectCollision()
    {
        Vector2 v;
        Vector2 pScaleX; // player scale
        Vector2 pScaleY; // player scale
        float xDist = velocity.x * deltaTime;
        // offset detection origin. If offset is too small, player can occasionally clip through walls.
        // player is 0.025 units inside a wall at closest., need to offset boxcast by at least that much for it to always work
		v = new Vector2(transform.position.x - collision.OFFSET, transform.position.y - 0f);

		pScaleX = new Vector2(0.01f, boxCollider.size.y * transform.localScale.y * 0.9f);  // localscale may not be needed
        pScaleY = new Vector2(boxCollider.size.x * transform.localScale.x, 0.01f);

		//pScaleX = new Vector2(1f, 5f);

		// smaller check if crouching
		float local_ray_distance_up = collision.RAY_DISTANCE_UP;
		float local_ray_distance_down = collision.RAY_DISTANCE_DOWN;
		if (isCrouching)
		{
			local_ray_distance_up *= crouch.scale * 1.2f; // x1.2 to make sure it hits the roof above the player when crouching in a small space. Essential to not uncrouch into a wall.
			// > better fix: have normal sized raycasy up, and disable jumping when its hitting something.
			local_ray_distance_down *= crouch.scale;
		}

        // check right collision (only if moving right)
        rayHit = Physics2D.BoxCast(v, pScaleX, 0f, Vector2.right, Mathf.Max(collision.RAY_DISTANCE_LR, xDist) , collisionMask);
        Debug.DrawRay(v, Vector2.right * Mathf.Max(collision.RAY_DISTANCE_LR, xDist), Color.yellow);
        if (rayHit && velocity.x > 0)
        {
            rightCollision = true;
            if (anim != null)
            {
                anim.SetBool("rightCollision", true);
            }
            velocity.x = 0; // stop player movement
            this.transform.Translate(Vector3.left * (collision.RAY_DISTANCE_LR - rayHit.distance)); // set their position to be at the edge of the object they collided with
        }

        // if currently colliding, check for the absense of collision
        if (rightCollision)
        {
            rayHit = Physics2D.BoxCast(v, pScaleX, 0f, Vector2.right, collision.RAY_DISTANCE_LR, collisionMask);
            if (!rayHit)
            {
                rightCollision = false;
                if (anim != null)
                {
                    anim.SetBool("rightCollision", false);
                }
            }
        }

        // change offset for new direction
        v += new Vector2(2 * collision.OFFSET, 0);

        // check left collision (only if moving left), same logic as right collision
        // raycast distance = set distance, OR distance the player will travel in this frame, whichever is higher.
        rayHit = Physics2D.BoxCast(v, pScaleX, 0f, Vector2.left,  Mathf.Max(collision.RAY_DISTANCE_LR, -xDist), collisionMask);
        Debug.DrawRay(v, Vector2.left * Mathf.Max(collision.RAY_DISTANCE_LR, xDist), Color.yellow);
        if (rayHit && velocity.x < 0)
        {
            leftCollision = true;
            if (anim != null)
            {
                anim.SetBool("leftCollision", true);
            }

            velocity.x = 0;
            this.transform.Translate(Vector2.right * (collision.RAY_DISTANCE_LR - rayHit.distance));
        }

        if (leftCollision)
        {
            rayHit = Physics2D.BoxCast(v, pScaleX, 0f, Vector2.left, collision.RAY_DISTANCE_LR, collisionMask);
            if (!rayHit)
            {
                leftCollision = false;
                if (anim != null)
                {
                    anim.SetBool("leftCollision", false);
                }
            }
        }

        // check up collision
        float upStrictness = collision.UP_COLLISION_STRICTNESS_FACTOR + 1f;
        rayHit = Physics2D.BoxCast(transform.position, pScaleY, 0f, Vector2.up, local_ray_distance_up, collisionMask);
        Debug.DrawRay(v, Vector2.up * Mathf.Max(local_ray_distance_up, xDist), Color.yellow);
        if (rayHit && (velocity.y > 0 || onGround) ) // used to be just velocity.y > 0, changed for crouching to work
		{
            upCollision = true;
            velocity.y = 0;
            this.transform.Translate(Vector2.down * (local_ray_distance_up - rayHit.distance));
            print("collision UP: " + (collision.RAY_DISTANCE_UP - rayHit.distance) + " = " + collision.RAY_DISTANCE_UP + " + " + rayHit.distance);

        }
        else
        {
            upCollision = false;
        }

        // check down collision
        rayHit = Physics2D.BoxCast(transform.position, pScaleY, 0f, Vector2.down, local_ray_distance_down, collisionMask);
        Debug.DrawRay(v, Vector2.down * Mathf.Max(local_ray_distance_down, xDist), Color.yellow);
        if (rayHit /* && velocity.y < 0 */)   // y < 0 may sometimes eat jump input.
        {
            onGround = true; // player considered on the ground if colliding with something downwards
            //onGroundGraceTimerState = 0;
            if (anim != null)
            {
                anim.SetBool("onGround", true);
            }
            if (!isJumping && !isJetpacking && !isDashing && !isKnockbackActive) // this clause allows the player to leave the ground again
            {
                velocity.y = 0;
                this.transform.Translate(Vector2.up * (local_ray_distance_down - rayHit.distance));
            }
        }
        else // player not on the ground
        {
            rayHit = Physics2D.BoxCast(transform.position, pScaleY, 0f, Vector2.down, local_ray_distance_down + 0.3f, collisionMask); // slightly longer raycast
            if (rayHit && !isJumping && onGround && !isKnockbackActive) // if ground is only slightly beneath player, and they didnt jump, were on the ground last frame, and werent knocked back, its a slope
            {
                //print("SLOPE " + rayHit.distance);
                this.transform.Translate(Vector2.down * (rayHit.distance - local_ray_distance_down));
            }
            else
            {
                onGround = false;
                if (anim != null)
                {
                    anim.SetBool("onGround", false);
                }
            }
        }

        // Grace timer
        // gives player a short amount of time before conisdering them off the ground.
        switch (onGroundGraceTimerState)
        {
            case 0: // primed
                if (!onGround) // activate when player leaves the ground
                {
                    if (!isJumping && !isDashing)
                    {
                        onGroundGraceTimerState = 1; // 1: active
                        onGroundGraceTimer = 0f;
                    }
                    else
                    {
                        onGroundGraceTimerState = 2; // expend grace timer if player left the ground via jumping or dashing.
                    }
                }

                break;
            case 1: // active
                onGround = true; //override collision
                onGroundGraceTimer += deltaTime;

                if (onGroundGraceTimer >= general.ON_GROUND_GRACE_MAX || isJumping || isDashing)
                {
                    onGround = false;
                    if (anim != null)
                    {
                        anim.SetBool("onGround", false);
                    }
                    onGroundGraceTimerState = 2; // 2: expended
                }
                break;
            case 2: // expended
                if (onGround)
                {
                    onGroundGraceTimerState = 0;    // NEVER CALLED RIGHT NOW, FIX
                }
                break;
        }
    }
    #endregion

    // register the collecting of an item
    void CollectibleGet(int type)
    {
        // TYPES
        // 1 - gem (10)
        // 2 - gem (100)
        // 3 - gem (1000)
        // 4 - small health pickup
        // 5 - big health pickup
        // 6 - fuel pickup
        // 7 - jetpack
        // 8 - double jump pickup
        // 9 - wall jump pickup

        switch (type)
        {
            case 1: // small gem
                stats.dosh += 10;
                break;
            case 2: // bigger gem
                stats.dosh += 100;
                break;
            case 3: // biggest gem
                stats.dosh += 1000;
                break;
            case 4: // small heal
                TakeDamage(-20); // heal 20
                break;
            case 5: // full heal
                TakeDamage(-999); // heal to full
                break;
            case 6: // jetpack fuel
                stats.jetpackFuel = stats.JETPACK_FUEL_MAX; 
                break;
            case 7: // jetpack enabler
                abilities.jetpackEnabled = true;
                UI_fuelSlider.gameObject.SetActive(true); // enable ui element
                break;
            case 8: // double jump enabler
                abilities.maxAirJumps = 1;
                break;
            case 9: // wall jump enabler
                abilities.wallJumpEnabled = true;
                break;
            case 10: // dash enabler
                abilities.maxDashes++;
                dash.counter = abilities.maxDashes;
                break;
            default:
                print("INVALID ITEM ID");
                break;
        }
        UpdateUI(); // taking damage updates twice., fix if needed
    }
    // Apply damage to the player. Negative damage is treated as healing.
    public void TakeDamage(int d)
    {
        if (d > 0) // damage
        {
            if (!recentDamage) // dont apply damage if the recent damage invincibility timer has not elapsed.
            {
                stats.playerHealth -= d;
                recentDamage = true;
                recentDamageTimer = general.DAMAGE_INTERVAL;
                if (stats.playerHealth <= 0)
                {
                    KillPlayer();
                }
            }

        }
        else // health
        {
            stats.playerHealth -= d;
            if (stats.playerHealth > stats.HEALTH_MAX)
            {
                stats.playerHealth = stats.HEALTH_MAX;
            }
        }

        UpdateUI();

        //change player color based on hp
        // white = full hp, red = no hp
        //r.material.color = Color.Lerp(Color.red, Color.white, (float)stats.playerHealth / 100f);
    }

    // Misc, small, self explanatory functions
    #region MiscFunctions
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Coin")
        {
            //print("COIN GET PLAYER");
        }
    }
    void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //generic scene loader
        //public string sceneToLoad;
        //SceneManager.LoadScene(sceneToLoad);
    }
    void KillPlayer()
    {
		ReloadCurrentScene();
		return;
	}
    void UpdateRespawn(Vector2 p)
    {
        respawnPos = p;
    }

    void PlayAudioClip(AudioClip audioClip, float audioVolume)
    {
		// if theres no audio sources, do nothing.
		if (audioSources.Count == 0)
		{
			print("WARNING: No Audio Source found, but attempted to play sound");
			return;
		}

		for (int i = 0; i < audioSources.Count; i++)
		{
			if (audioSources[i].isPlaying == false)
			{
				audioSources[i].clip = audioClip;
				audioSources[i].volume = audioVolume;
				// any extra effects here
				audioSources[i].Play();
				return; // job done
			}
		}
		// if here, all sounds full
		// current decision: override oldest one
		float maxTime = 0f;
		int victimID = 0;

		// find oldest sound
		for (int i = 0; i < audioSources.Count; i++)
		{
			if (audioSources[i].time > maxTime)
			{
				victimID = i;
				maxTime = audioSources[i].time;
			}
		}

		// override it
		audioSources[victimID].clip = audioClip;
		audioSources[victimID].volume = audioVolume;
		// any extra effects here
		audioSources[victimID].Play();
	}

	// basic play version
	void PlayAudioClip(AudioClip audioClip)
	{
		for (int i = 0; i < audioSources.Count; i++)
		{
			if (audioSources[i].isPlaying == false)
			{
				audioSources[i].clip = audioClip;
				audioSources[i].Play();
				return; // job done
			}
		}

		// if here, all sounds full
		// current decision: override oldest one
		float maxTime = 0f;
		int victimID = 0;

		// find oldest sound
		for (int i = 0; i < audioSources.Count; i++)
		{
			if (audioSources[i].time > maxTime)
			{
				victimID = i;
				maxTime = audioSources[i].time;
			}
		}

		// override it
		audioSources[0].clip = audioClip;
		audioSources[0].Play();
	}

	// flips player sprite direction on the x axis
	public void FlipSprite()
    {
        Vector3 scale = playerSprite.transform.localScale;
        scale.x *= -1;
        playerSprite.transform.localScale = scale;
		general.isRightFacing = !general.isRightFacing;
    }
    void ToggleSprite()
    {
        isSpriteActive = !isSpriteActive;
        playerSprite.gameObject.SetActive(isSpriteActive);
    }

	bool isSpriteSquished = false;
	void SpriteSquish(bool state)
	{
		if (state && !isSpriteSquished)
		{
			//Vector3 scale = playerSprite.transform.localScale;
			//Vector3 pos = playerSprite.transform.position;
			Vector3 scale = transform.localScale;
			Vector3 pos = transform.position;

			scale.y *= 0.5f;
			//playerSprite.transform.localScale = scale;
			transform.localScale = scale;
			pos.y -= 0.5f;
			//playerSprite.transform.position = pos;
			transform.position = pos;
			isSpriteSquished = true;
		}
		else if (!state && isSpriteSquished)
		{
			//Vector3 scale = playerSprite.transform.localScale;
			//Vector3 pos = playerSprite.transform.position;
			Vector3 scale = transform.localScale;
			Vector3 pos = transform.position;

			scale.y *= 2f;
			//playerSprite.transform.localScale = scale;
			transform.localScale = scale;
			pos.y += 0.5f;
			//playerSprite.transform.position = pos;
			transform.position = pos;
			isSpriteSquished = false;
		}
	}

    // get direction based on player's held direction key
    Vector2 GetDir()
    {
        Vector2 dir;
        if (input.moveRight_bHeld)
        {
            dir = Vector2.right;
        }
        else if (input.moveLeft_bHeld)
        {
            dir = Vector2.left;
        }
        else if (input.moveUp_bHeld)
        {
            dir = Vector2.up;
        }
        else if (input.moveDown_bHeld)
        {
            dir = Vector2.down;
        }
        else // return zero if no direction currently held
        {
            dir = Vector2.zero;
        }
        return dir;
    }
    // used externally to read what the player is doing
    public bool GetState(string command)
    {
        switch (command)
        {
            case "onGround":
                return onGround;
            case "rightCollision":
                return rightCollision;
            case "leftCollision":
                return leftCollision;
            case "upCollision":
                return upCollision;
            case "isJumping":
                return isJumping;
            default:
                print("UNDEFINED GET REQUEST");
                return false;
        }
    }
    #endregion


    // activates and deactivates specified funtionality of the player
    void SetPlayerAbility(string type, bool state)
    {
        switch (type)
        {
            case "movement":
                if (state)
                {
                    movementDisableCounter--;
                    if (movementDisableCounter <= 0)
                    {
                        disableMovement = false;
                    }
                }
                else
                {
                    disableMovement = true;
                    movementDisableCounter++;
                    EndMovement();
                }
                break;
            case "jump":    // includes wall jump. add additional case if necessary.
                if (state)
                {
                    jumpDisableCounter--;
                    if (jumpDisableCounter <= 0)
                    {
                        disableJump = false;
                    }
                }
                else
                {
                    disableJump = true;
                    jumpDisableCounter++;
                    // END JUMP
                }
                break;
            case "dash":
                if (state)
                {
                    dashDisableCounter--;
                    if (dashDisableCounter <= 0)
                    {
                        disableDash = false;
                    }
                }
                else
                {
                    disableDash = true;
                    dashDisableCounter++;
                    EndDash();
                }
                break;
            case "all": // calls this function again for all other states
                {
                    SetPlayerAbility("movement", state);
                    SetPlayerAbility("jump", state);
                    SetPlayerAbility("dash", state);
                }
                break;
            default:
                print("Invalid SetPlayerAbility type requested");
                break;
        }
    }

    // applies a force to the player based on the input. updated in HandleKnockback() after initial setup
    public void ApplyKnockback(Vector2 dir, float vel, float duration, int type, bool relativeDir) 
    {
        switch (type)
        {
            case 0:                     // no player control, does not apply if invincible (e.g. damage)
                if (recentDamage)
                {
                    return;
                }
                EndAttack();
                velocity = Vector2.zero;
                SetPlayerAbility("all", false);
                isKnockbackControllable = false;
                break;
            case 1:                     // no player control, always applies
                velocity = Vector2.zero;
                SetPlayerAbility("all", false);
                isKnockbackControllable = false;
                break;
            case 2:                     // player control, does not reset player momentum, always applies (e.g.  winds)
                isKnockbackControllable = true;
                break;
            case 3:                     // player control. resets player momentum, always applies
                velocity = Vector2.zero;
                isKnockbackControllable = true;
                break;
            case 4:                     // player control, reset player y momentum only, always applies, treated as a JUMP
                if (isKnockbackActive  && !isKnockbackControllable) // do not perform bounce if performing uncontrollable knockback
                {
                    return;
                }
                isJumping = true;
                airJumpCounter = 0; // reset double jump on bounce
                dash.counter = abilities.maxDashes;
                velocity.y = 0f;
                velocity += Vector2.up * jump.INIT_FORCE;
                isKnockbackControllable = true;
                break;
            default:
                print ("Invalid knockback type received.");
                break;
        }

        if (relativeDir && !general.isRightFacing)  // align knockback dir with player's facing direction if relative direction enabled
        {
            dir.x *= -1f;
        }

        velocity += dir * vel;
        isKnockbackActive = true;
        knockbackTimer = 0f;
        knockbackTimerMax = duration;
    }
}
