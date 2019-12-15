using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerController : MonoBehaviour
{
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

	[System.Serializable]
	public class Walk
	{
		public float MAX_SPEED = 8f;             // how fast the player can walk
		public float DECAY_SPEED = 50f;          // how fast the player reaches said max walking speed
		public float ACCEL_SPEED = 50f;          // same as above, but for slowing down again
		public float STEP_INTERVAL = 2f;
	}
	public Walk walk;
	bool isWalking = false;

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
	public class Collision
	{
		public float OFFSET = .2f;                              // no longer necessary, delete later
		public float RAY_DISTANCE_LR = 0.3f;                    // the distance the left and right ray casts are sent out to, defining how close the player can get to a wall.
		public float RAY_DISTANCE_DOWN = 0.6f;                  // as above, for down
		public float RAY_DISTANCE_UP = 0.3f;                    // as above, for up
		public float UP_COLLISION_STRICTNESS_FACTOR = 0.5f;     // how lenient the up collision is with corners. High leniency will have the player round the corner instead of colliding.
		public float LR_COLLISION_HEIGHT_MULT = 1f;				// size of the left and right collision boxes vertically. Smaller means clipping onto ledges when your just a bit too short 
		public float UD_COLLISION_HEIGHT_MULT = 1f;				// size of the up / down collision boxes vertically. Smaller means sliding off edges.
	}
	public Collision collision;

	public LayerMask collisionMask;

	Vector2 velocity = new Vector3(0, 0, 0);
	Vector2 respawnPos;
	RaycastHit2D rayHit;
	private BoxCollider2D boxCollider;

	public bool onGround = false; // TODO: Make all state variables (onGround, collision, jumping, attacking, etc) public, so other entities can act based on them. Organise in a struct
	int onGroundGraceTimerState = 0;
	float onGroundGraceTimer = 0f;
	bool rightCollision = false;
	bool leftCollision = false;
	bool upCollision = false;

	protected virtual void Start() // virtual allows it to be overridden by child AI classes
	{
		boxCollider = GetComponent<BoxCollider2D>();
		respawnPos = this.transform.position; // initialise respawn position to starting position.
		//UpdateUI();
	}

	protected virtual void Update()
	{
		HandleMovement();
		HandleJump();

		//EnforceMaxSpeeds();
		DetectCollision();

		// apply final velocity to character
		this.transform.Translate(velocity * Time.deltaTime);
		//print(velocity.ToString("F4"));
	}

	void HandleMovement()
	{
		// TEMP MOVEMENT
		if (input.moveRight_bHeld)
		{
			velocity.x = walk.MAX_SPEED;
		}
		else if (input.moveLeft_bHeld)
		{
			velocity.x = -walk.MAX_SPEED;
		}
		else
		{
			velocity.x = 0f;
		}
		

		// apply gravity
		if (!isJumping && (!onGround || onGroundGraceTimerState == 1)) // grace 1: active
		{
			velocity.y -= general.GRAVITY_FORCE * Time.deltaTime;
		}
	}

	void HandleJump()
	{
		// reset a few things when player lands
		if (onGround)
		{
			airJumpCounter = 0; // air jumps
			if (!jumpLanded)
			{
				//PlayAudioClip(jumpLandSound, landAudioVolume);
				jumpLanded = true;
			}
		}

		//unity>edit>project settings >input - define input values
		if (input.jump_bDown)
		{
			isJumpHeld = true;
			// check for air jumping
			if (!onGround && airJumpCounter < abilities.maxAirJumps)
			{
				airJumpCounter++;
				//PlayAudioClip(airJumpSound, jumpAudioVolume);
				isJumping = true;
				velocity.y = 0f;
				velocity += Vector2.up * jump.INIT_FORCE;
			}
			// check for ground jumping
			else if (onGround)
			{
				//PlayAudioClip(groundJumpSound, jumpAudioVolume);
				isJumping = true;
				velocity.y = 0f;
				velocity += Vector2.up * jump.INIT_FORCE;
			} // else, dont jump
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
						velocity.y += jump.ACCEL_FORCE - (jump.DECAY_RATE * Time.deltaTime); // apply decay
						if (velocity.y < 0f)
						{
							velocity.y = 0f;
						}
					}
					else // full speed head otherwise
					{
						velocity.y += (jump.ACCEL_FORCE * Time.deltaTime);
						jumpDuration += Time.deltaTime;
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

	#region Constraints
	// detects collision objects around the player and prevents movement into them
	// redefined by me to provide smoother functionality and greater control over the default rigidbody collision mechanics
	void DetectCollision()
	{
		Vector2 v;
		Vector2 pScaleX; // player scale
		Vector2 pScaleY; // player scale
		float xDist = velocity.x * Time.deltaTime;
		// offset detection origin. If offset is too small, player can occasionally clip through walls.
		// player is 0.025 units inside a wall at closest., need to offset boxcast by at least that much for it to always work
		v = new Vector2(transform.position.x - collision.OFFSET, transform.position.y - 0f);

		pScaleX = new Vector2(0.01f, boxCollider.size.y * transform.localScale.y * collision.LR_COLLISION_HEIGHT_MULT);  // localscale may not be needed
		pScaleY = new Vector2(boxCollider.size.x * transform.localScale.x * collision.UD_COLLISION_HEIGHT_MULT, 0.01f);

		//pScaleX = new Vector2(1f, 5f);

		// smaller check if crouching
		float local_ray_distance_up = collision.RAY_DISTANCE_UP;
		float local_ray_distance_down = collision.RAY_DISTANCE_DOWN;

		// check right collision (only if moving right)
		rayHit = Physics2D.BoxCast(v, pScaleX, 0f, Vector2.right, Mathf.Max(collision.RAY_DISTANCE_LR, xDist), collisionMask);
		Debug.DrawRay(v, Vector2.right * Mathf.Max(collision.RAY_DISTANCE_LR, xDist), Color.yellow);
		if (rayHit && velocity.x > 0)
		{
			rightCollision = true;
			velocity.x = 0; // stop player movement
			this.transform.Translate(Vector3.left * (collision.RAY_DISTANCE_LR - rayHit.distance)); // set their position to be at the edge of the object they collided with
			//print("collision RIGHT: " + (collision.RAY_DISTANCE_LR - rayHit.distance) + " = " + collision.RAY_DISTANCE_LR + " + " + rayHit.distance);
		}

		// if currently colliding, check for the absense of collision
		if (rightCollision)
		{
			rayHit = Physics2D.BoxCast(v, pScaleX, 0f, Vector2.right, collision.RAY_DISTANCE_LR, collisionMask);
			if (!rayHit)
			{
				rightCollision = false;
			}
		}

		// change offset for new direction
		v += new Vector2(2 * collision.OFFSET, 0);

		// check left collision (only if moving left), same logic as right collision
		// raycast distance = set distance, OR distance the player will travel in this frame, whichever is higher.
		rayHit = Physics2D.BoxCast(v, pScaleX, 0f, Vector2.left, Mathf.Max(collision.RAY_DISTANCE_LR, -xDist), collisionMask);
		Debug.DrawRay(v, Vector2.left * Mathf.Max(collision.RAY_DISTANCE_LR, xDist), Color.yellow);
		if (rayHit && velocity.x < 0)
		{
			leftCollision = true;

			velocity.x = 0;
			this.transform.Translate(Vector2.right * (collision.RAY_DISTANCE_LR - rayHit.distance));
		}

		if (leftCollision)
		{
			rayHit = Physics2D.BoxCast(v, pScaleX, 0f, Vector2.left, collision.RAY_DISTANCE_LR, collisionMask);
			if (!rayHit)
			{
				leftCollision = false;
			}
		}

		// check up collision
		float upStrictness = collision.UP_COLLISION_STRICTNESS_FACTOR + 1f;
		rayHit = Physics2D.BoxCast(transform.position, pScaleY, 0f, Vector2.up, local_ray_distance_up, collisionMask);
		Debug.DrawRay(v, Vector2.up * Mathf.Max(local_ray_distance_up, xDist), Color.yellow);
		if (rayHit && (velocity.y > 0 || onGround)) // used to be just velocity.y > 0, changed for crouching to work
		{
			upCollision = true;
			velocity.y = 0;
			this.transform.Translate(Vector2.down * (local_ray_distance_up - rayHit.distance));
			//print("collision UP: " + (collision.RAY_DISTANCE_UP - rayHit.distance) + " = " + collision.RAY_DISTANCE_UP + " + " + rayHit.distance);

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
			if (!isJumping) // this clause allows the player to leave the ground again
			{
				velocity.y = 0;
				this.transform.Translate(Vector2.up * (local_ray_distance_down - rayHit.distance));
				//print("collision DOWN: " + (collision.RAY_DISTANCE_DOWN - rayHit.distance) + " = " + collision.RAY_DISTANCE_DOWN + " + " + rayHit.distance);
			}
		}
		else // player not on the ground
		{
			rayHit = Physics2D.BoxCast(transform.position, pScaleY, 0f, Vector2.down, local_ray_distance_down + 0.3f, collisionMask); // slightly longer raycast
			if (rayHit && !isJumping && onGround) // if ground is only slightly beneath player, and they didnt jump, were on the ground last frame, and werent knocked back, its a slope
			{
				//print("SLOPE " + rayHit.distance);
				this.transform.Translate(Vector2.down * (rayHit.distance - local_ray_distance_down));
			}
			else
			{
				onGround = false;
			}
		}

		// Grace timer
		// gives player a short amount of time before conisdering them off the ground.
		switch (onGroundGraceTimerState)
		{
			case 0: // primed
				if (!onGround) // activate when player leaves the ground
				{
					if (!isJumping)
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
				onGroundGraceTimer += Time.deltaTime;

				if (onGroundGraceTimer >= general.ON_GROUND_GRACE_MAX || isJumping)
				{
					onGround = false;
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

	// kills the player and ends the game
	public void KillPlayer()
	{
		GetComponentInChildren<Camera>().transform.SetParent(null); // detach camera
		Destroy(this.gameObject); // kill player
		// better solution forthcoming
	}
}
