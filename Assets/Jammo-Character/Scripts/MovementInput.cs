using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Cinemachine;
using DG.Tweening;

[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour
{
	[HideInInspector] public UnityEvent OnMovementBoost;

	private BoostSystem boostSystem;
	private ArrowSystem arrowSystem;
	private TargetSystem targetSystem;

	private Animator anim;
	private Camera cam;
	private CharacterController controller;
	private Vector3 desiredMoveDirection;
	private Vector2 moveAxis;
	private float verticalVel;
    private Coroutine boostCoroutine;

    [Header("Movement Settings")]
	[SerializeField] float movementSpeed;
	[SerializeField] float rotationSpeed = 0.1f;

    [Header("Acceleration Settings")]
	private float currentAcceleration = 1;
	[SerializeField] float runAcceleration = 2;
	private float accelerationMultiplier = 1f;
	[SerializeField] float boostMultiplier = 1.5f;
	[SerializeField] float boostDuration = 1;
	[SerializeField] float accelerateLerp = .006f;
	[SerializeField] float decelerateLerp = .05f;
	[SerializeField] float runRotationSpeed = 100;
	[SerializeField] float chargeStrafeSpeed = 15;

	[Header("Jump Settings")]
    [SerializeField] float jumpSpeed = 8.0f;
    [SerializeField] float jumpHoldTime = 0.2f;
    [SerializeField] float jumpTimer;
	[SerializeField] private float verticalVelocity;
	[SerializeField] float gravity = 9.8f;
	
	[Header("Collision Settings")]
    [SerializeField] LayerMask groundLayerMask;

	[Header("Booleans")]
	public bool isGrounded;
	[SerializeField] bool isJumping;
	[SerializeField] bool blockRotationPlayer;
	public bool isRunning;
	public bool isBoosting;
	public bool finishedBoost;

	[Header("Input")]
    [SerializeField] InputActionReference runAction;
	[HideInInspector] public bool holdRunInput;
    [SerializeField] InputActionReference jumpAction;

	void Start()
	{
		anim = this.GetComponent<Animator>();
		cam = Camera.main;
		controller = GetComponent<CharacterController>();

		boostSystem = GetComponent<BoostSystem>();
		arrowSystem = GetComponent<ArrowSystem>();
		targetSystem = GetComponent<TargetSystem>();

		arrowSystem.OnTargetHit.AddListener(Boost);

        //Input
        runAction.action.performed += RunAction_performed;
        runAction.action.canceled += RunAction_canceled;
		jumpAction.action.started += JumpAction_started;
		jumpAction.action.canceled += JumpAction_cancelled;
	}


    void Update()
	{
		InputMagnitude();

        CheckGrounded();

		float lerp = finishedBoost ? accelerateLerp : decelerateLerp;
		currentAcceleration = Mathf.Lerp(currentAcceleration, isRunning ? (runAcceleration * accelerationMultiplier) : 1, lerp * Time.deltaTime);

		CheckJump();

		if(holdRunInput && canRun() && moveAxis.magnitude > 0)
			isRunning = true;
    }

	void CheckJump()
	{
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            float jumpHeight = Mathf.Clamp01(jumpTimer / jumpHoldTime);
            verticalVelocity = jumpSpeed * jumpHeight;
            if (verticalVelocity >= jumpSpeed)
                isJumping = false;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }


	void CheckGrounded()
	{
        isGrounded = Physics.Raycast(transform.position + (transform.up * .05f), Vector3.down, .2f, groundLayerMask);
		anim.SetBool("isGrounded", isGrounded);
		anim.SetFloat("GroundedValue", isGrounded ? 0 : 1, .1f, Time.deltaTime);
    }

    void PlayerMoveAndRotation()
	{
		var camera = Camera.main;
		var forward = cam.transform.forward;
		var right = cam.transform.right;

		forward.y = 0f;
		right.y = 0f;

		forward.Normalize();
		right.Normalize();

        if (isRunning)
        {
			//Vector3 sideMovement = arrowSystem.isCharging ? transform.right * moveAxis.x : Vector3.zero;

			transform.eulerAngles += new Vector3(0, moveAxis.x * Time.deltaTime * runRotationSpeed, 0);

			controller.Move( transform.forward * movementSpeed * currentAcceleration * Time.deltaTime);

			return;
        }

		desiredMoveDirection = forward * moveAxis.y + right * moveAxis.x;

		if (blockRotationPlayer == false)
		{
			//Camera
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), rotationSpeed * currentAcceleration);
			controller.Move(desiredMoveDirection * Time.deltaTime * (movementSpeed * currentAcceleration));
		}
	}

	public void LookAt(Vector3 pos)
	{
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos), rotationSpeed);
	}


	//Is listening to the ArrowSystem TargetHit function
	public void Boost()
    {
		if (!holdRunInput)
			return;

		if (!isRunning && moveAxis.magnitude <= 0)
			return;

		OnMovementBoost.Invoke();

		if (!isGrounded)
			anim.SetTrigger("Flip");

		finishedBoost = false;

		if(boostCoroutine != null)
			StopCoroutine(boostCoroutine);

		boostCoroutine = StartCoroutine(BoostCoroutine());

		IEnumerator BoostCoroutine()
        {
			if(!isGrounded)
				isJumping = true;

			isBoosting = true;
			accelerationMultiplier = boostMultiplier;

            yield return new WaitForSeconds(boostDuration);

			isBoosting = false;
			accelerationMultiplier = 1;
			finishedBoost = true;

			yield return new WaitForSeconds(1);

			finishedBoost = false;
        }
    }


	public void RotateToCamera(Transform t)
	{
		var forward = cam.transform.forward;

		desiredMoveDirection = forward;
		Quaternion lookAtRotation = Quaternion.LookRotation(desiredMoveDirection);
		Quaternion lookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, lookAtRotation.eulerAngles.y, transform.rotation.eulerAngles.z);

		t.rotation = Quaternion.Slerp(transform.rotation, lookAtRotationOnly_Y, rotationSpeed);
	}

	void InputMagnitude()
	{
		//Calculate the Input Magnitude
		float inputMagnitude = new Vector2(moveAxis.x, moveAxis.y).sqrMagnitude;

		//Physically move player
		if (inputMagnitude > 0.1f || isRunning)
		{
			anim.SetFloat("InputMagnitude", (isRunning ? 1 : inputMagnitude) * currentAcceleration, .1f, Time.deltaTime);
			PlayerMoveAndRotation();
		}
		else
		{
			anim.SetFloat("InputMagnitude", inputMagnitude * currentAcceleration, .1f, Time.deltaTime);
		}
	}

	#region Input

	public void OnMove(InputValue value)
	{
		moveAxis.x = value.Get<Vector2>().x;
		moveAxis.y = value.Get<Vector2>().y;
	}

    private void JumpAction_started(InputAction.CallbackContext context)
    {

        if (controller.isGrounded)
        {
			anim.SetTrigger("Jump");
            isJumping = true;
            jumpTimer = 0.0f;
        }

    }

	private void JumpAction_cancelled(InputAction.CallbackContext context)
	{
        isJumping = false;
		jumpTimer = 0;
    }

    private void RunAction_canceled(InputAction.CallbackContext obj)
	{
		isRunning = false;
		holdRunInput = false;
    }

    private void RunAction_performed(InputAction.CallbackContext obj)
	{
		holdRunInput = true;

        if (canRun() && moveAxis.magnitude > 0)
			isRunning = true;
	}

	#endregion

	bool canRun()
	{
		bool state;
		state = boostSystem != null ? (boostSystem.boostAmount > 0 ? true : false) : false;
		return state;
	}

	private void OnDisable()
	{
		anim.SetFloat("InputMagnitude", 0);
	}

    private void OnDrawGizmos()
    {
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position + (transform.up * .05f), transform.position + (Vector3.down * .2f));
    }
}