using Unity.VisualScripting;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Custom Cinemachine")]
		public GameObject MainCamera;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		//Flags for basic skills
		//Flags are used instead of setting speeds to zero, because it's easier
		//to give the player hints when specific conditions are false
		[Header("Skills")] 
		private bool _prevCanSee = false;
		public bool canSee = false;
		public bool canLookAround = false;
		public bool canMove = false;
		public bool canJump = false;
		public bool canSprint = false;

		// cinemachine
		private float yLookAngle;

		// player
		private float _speed;
		private float xRotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
		
		
		
		// minigame
		private PlayerInput _playerInput;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;
		
		
		//Sounds

		[SerializeField] private AudioSource audioSource;
		[SerializeField] private AudioClip jumpingSoundStart;
		[SerializeField] private AudioClip jumpingSoundEnd;
		[SerializeField] private AudioClip walkingSound;
		[SerializeField] private AudioClip runningSound;


		public bool groundedBefore = false;

		private void Awake()
		{
			audioSource = GetComponent<AudioSource>();
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			_playerInput = GetComponent<PlayerInput>();

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			
			if (PlayerPrefs.HasKey("Mouse/Sensitivity"))
			{
				RotationSpeed = PlayerPrefs.GetFloat("Mouse/Sensitivity");
			}
			
			EventManager.StartListening("Mouse/SensitivityChanged",handleSensitivityChanged);
		}

		void handleSensitivityChanged(object data)
		{
			var newSensitivity = data is float f ? f : 0;
			RotationSpeed = newSensitivity;
		}
		
		private void Update()
		{
			if (!canMove)
			{
				_input.move = Vector2.zero;
			}

			if (!canJump)
			{
				_input.jump = false;
			}

			if (!canSprint)
			{
				_input.sprint = false;
			}
			
			if (_prevCanSee != canSee)
			{
				var visualBlock = FindObjectOfType<CannotSee>();
				if (visualBlock != null)
				{
					visualBlock.SetVisibility(canSee);
				}
			}

			if (!IsPlayerActionmapActive()) { return;}
			
			if (_input.weaponSwitch.y != 0)
			{
				//_input.weaponSwitch.y = 0;
				Debug.Log("Switch");
			}

			Move();
			JumpAndGravity();
			GroundedCheck();
			_prevCanSee = canSee;
			handleMoveSounds();
		}

		private void handleMoveSounds()
		{

			if (_input.sprint && _controller.isGrounded && _controller.velocity.magnitude > 2.0f &!audioSource.isPlaying)
			{
				audioSource.clip = runningSound;
				audioSource.volume = Random.Range(0.4f, 0.6f);
				audioSource.pitch = Random.Range(0.9f, 1.1f);
				audioSource.Play();
				//Debug.Log("Running");
			}
			if (_controller.isGrounded && _controller.velocity.magnitude > 2.0f &!audioSource.isPlaying)
			{
				audioSource.clip = walkingSound;
				audioSource.volume = Random.Range(0.4f, 0.6f);
				audioSource.pitch = Random.Range(0.9f, 1.1f);
				audioSource.Play();
				//Debug.Log("Walking");
			}

			if (!canMove && audioSource.isPlaying)
			{
				audioSource.Stop();
			}
		}

		private void LateUpdate()
		{
			if (!IsPlayerActionmapActive()) return;
			CameraRotation();


			if(!canJump){
				_input.jump = false;
			}
			if(!canSprint){
				_input.sprint = false;
			}
		}

		private bool IsPlayerActionmapActive()
		{
			if (_playerInput.currentActionMap.name == "Minigame") return false;
			if (_playerInput.currentActionMap.name == "Menue") return false;
			return true;
		}

		private void GroundedCheck()
		{
			groundedBefore = Grounded;
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

			if(!groundedBefore && Grounded)
			{
				audioSource.volume = 0.5f;
				audioSource.pitch = 1f;
				audioSource.PlayOneShot(jumpingSoundEnd);
				//Debug.Log("Landed");
			}
		}

		private void CameraRotation()
		{
			if (!canLookAround) { return; }
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				yLookAngle += _input.look.y * RotationSpeed * Time.deltaTime;
				xRotationVelocity = _input.look.x * RotationSpeed * Time.deltaTime;

				// clamp our pitch rotation
				yLookAngle = ClampAngle(yLookAngle, BottomClamp, TopClamp);


				// rotate the player left and right
				transform.Rotate(Vector3.up * xRotationVelocity);
				MainCamera.transform.localRotation = Quaternion.Euler(yLookAngle, 0.0f, 0.0f);
			}
		}
		
		
		public void ForceLookAt(Vector3 LookAt)
		{
			var directionToLookAt = LookAt - transform.position;
			var transRot = Quaternion.FromToRotation(transform.forward, directionToLookAt).eulerAngles;
			
			transform.Rotate(Vector3.up * transRot.y);
			MainCamera.transform.localRotation= Quaternion.Euler(0, 0, 0);
			yLookAngle = 0;
		}

		private void Move()
		{
			if (!canMove) { return; }
			// set target speed based on move speed, sprint speed and if sprint is pressed

			float targetSpeed = _input.sprint && canSprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;

			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (canJump && _input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					audioSource.volume = 0.5f;
					audioSource.pitch = 1f;
					audioSource.PlayOneShot(jumpingSoundStart);
					//Debug.Log("Jumping");
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
					_input.jump = false;
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}