using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[SerializeField] private float m_CrouchJumpForce = 100f;					// Additional force added when the player crouch jumps.
	[SerializeField] private int num_Jumps = 2;								// Number of jumps player can perform
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Transform m_WallCheck;								// A position marking where to check for walls
	[SerializeField] private Transform m_ledgeCheck;							// A position marking where to check for walls
	[SerializeField] private Collider2D m_CrouchDisableCollider;				// A collider that will be disabled when crouching

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;

	private float totalJumpForce;		//Aggregate force applied for jumps
	private RaycastHit2D wallCheckHit;	//Whether or not player is touching wall
	private RaycastHit2D ledgeCheckHit;	//Whether or not player is touching wall
	private bool isWallSliding = false;
	private bool isLedgeHanging = false;
	private float slideVelocityMultiplier = 1f;
	public float wallCheckDistance;
	public float ledgeCheckDistance;
	public float maxWallSlideVelocity = 2f;
	public float dashMaxCooldown = 3f;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate()
	{
		//Debug.Log(m_Rigidbody2D.velocity.x);
		if (dashMaxCooldown < 3f)
		{
			dashMaxCooldown+=Time.deltaTime;
		}
		bool wasGrounded = m_Grounded;

		if (slideVelocityMultiplier > 0)
		{
			slideVelocityMultiplier-=Time.deltaTime;
		}
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
				num_Jumps=2;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}

		//wallsliding logic
		wallCheckHit = Physics2D.Raycast(m_WallCheck.position, m_WallCheck.right, wallCheckDistance, m_WhatIsGround);
		ledgeCheckHit = Physics2D.Raycast(m_ledgeCheck.position, m_ledgeCheck.right, wallCheckDistance, m_WhatIsGround);

		if (wallCheckHit && m_Rigidbody2D.velocity.y <= 0 && !m_Grounded)
		{
			isWallSliding = true;
			m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
			num_Jumps=2;
		}
		else 
		{
			isWallSliding = false;
		}

		if (isWallSliding)
		{
			if(m_Rigidbody2D.velocity.x >= 0)
			{
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -maxWallSlideVelocity);
			}
		}

		//ledge-grabbing logic
		if (wallCheckHit && !ledgeCheckHit)
		{
			isLedgeHanging = true;			
		}
	}


	public void Move(float move, bool crouch, bool jump, bool dash)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{
			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}
				if (m_Grounded)
				{
					// Reduce the speed by the crouchSpeed multiplier
					move *= m_CrouchSpeed;

					// Disable one of the colliders when crouching
					if (m_CrouchDisableCollider != null)
						m_CrouchDisableCollider.enabled = false;
				}
				else 
				{
					m_Rigidbody2D.AddForce(new Vector2(0f, Mathf.Abs(m_Rigidbody2D.velocity.y)*-3));
				}
			} else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			
			//when wall sliding, reduce horizontal velocity to zero
			if (isWallSliding){
				targetVelocity.x = 0f;
			}

			//wall hanging logic NEEDS TO BE IMPLEMENTED ************************
/* 			if (isLedgeHanging)
			{
				m_Rigidbody2D.AddForce(new Vector2(0f, transform.down*9,8f));

				targetVelocity.x = 0f;
				targetVelocity.y = 0f;
			} */

			//ground sliding
/* 			if (m_Rigidbody2D.velocity.x >= 3f && crouch)
			{
				targetVelocity.x = targetVelocity.x * slideVelocityMultiplier;
			}
			else
			{
				slideVelocityMultiplier=1f;
			} */

			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}

		if (dash && dashMaxCooldown >= 3f)
		{
			Dash();
			dashMaxCooldown = 0f;
		}

		// If the player should jump...
		if (!m_Grounded && jump && num_Jumps > 1)
		{
			num_Jumps-=1;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
			
		}
		else if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			totalJumpForce=m_JumpForce;
			m_Grounded = false;
			if (crouch)
			{
				totalJumpForce = m_CrouchJumpForce + m_JumpForce;
			}
			m_Rigidbody2D.AddForce(new Vector2(0f, totalJumpForce));
		}
	}

	private void Dash()
	{
		m_Rigidbody2D.AddForce(new Vector2(m_Rigidbody2D.velocity.x*400f, 0f));
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		//Old method of flipping character. Deprecated after introducing FirePoint
/* 		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale; */
		transform.Rotate(0f,180f,0f);
	}
}
