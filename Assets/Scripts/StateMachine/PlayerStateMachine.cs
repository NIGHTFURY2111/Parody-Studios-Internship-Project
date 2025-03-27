using Cinemachine;
using Cinemachine.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerStateMachine : MonoBehaviour
{
    StateFactory stateFactory;
    BaseState currentState;
    public Text speed;

    [Header("General Settings")]
    [SerializeField] float gravity;
    [SerializeField] float walkingSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float forceAppliedInAir;
    [SerializeField] float TimerInAirBeforeGameOver;
    [SerializeField] GameObject PlayerModel;

    [Header("Camera Settings")]
    [SerializeField] CinemachineFreeLook playerCamera;
    [SerializeField] GameObject playerCameraFollowPoint;
    [SerializeField] float lookXLimit;
    [SerializeField] float lookSpeed;

    [Header("Gravity Settings")]
    [SerializeField] GameObject gravityPreview;
    [SerializeField] float maxGravitySwitchingVelocityClamp;
    [SerializeField] float gravityChangeTime;
    private bool canSwitchGravity = true;
    private Vector3 newGravityDirection;

    Rigidbody rb;
    CapsuleCollider col;


    bool isGrounded;
    private float rotationX = 0;
    private float rotationY = 0;
    private float distanceToGround;
   
    private PlayerInputAction playerInput;
    private GameManager gameManager;
    private Animator anim;
    private InputAction move;
    private InputAction camDirection;
    private InputAction jump;
    private InputAction gravityDirection;
    private Coroutine gravityChangeCoroutine;



    #region input
    private void Awake()
    {
        playerInput = new();
        stateFactory = new(this);
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        gameManager = FindObjectOfType<GameManager>();
        PlayerModel.TryGetComponent<Animator>( out anim);
        move = playerInput.Player.Movement;
        camDirection = playerInput.Player.Camera;
        jump = playerInput.Player.Jump;
        gravityDirection = playerInput.Player.SwitchGravity;
    }

    private void OnEnable()
    {
        move.Enable();
        jump.Enable();
        camDirection.Enable();
        gravityDirection.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        camDirection.Disable();
        gravityDirection.Disable();
    }
    #endregion

    void Start()
    {

        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);
        currentState = stateFactory.Idle();
        currentState.EnterState();
        lookSpeed *= 0.5f;
        distanceToGround = col.height/2 ;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        //Debug.Log(PCC._velocityMagnitude);
        Debug.Log(_playerVelocityInPlane);
        //characterController.Move(moveDirection * Time.deltaTime);

        // Move the controller
        currentState.UpdateState();

        // Player and Camera rotation
        cameraMovement();

        //displays the current speed
        //speed.text = Math.Round(PCC._currentVelocityMagnitude).ToString();

        //CheckGrounded();
        isGrounded = true;
        switchGravity();
    }

    private void FixedUpdate()
    {
        currentState.FixedState();
        ArtificialGravity();
    }

    public void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.TransformPoint(col.center), -transform.up, out RaycastHit hit, col.height/2  );
        Debug.DrawRay(transform.TransformPoint(col.center), -transform.up * (col.height / 2), Color.red);
    }

    //private void LateUpdate()
    //{
    //    currentState.LateUpdateState();
    //}

    public void cameraMovement()
    {
        rotationX += -LookDir.y * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        
        rotationY += LookDir.x * lookSpeed;

        playerCameraFollowPoint.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);


    }

    public Vector2 MoveDir => move.ReadValue<Vector2>();

    public Vector2 LookDir => camDirection.ReadValue<Vector2>();

    public Vector3 MovementVector()
    {
        // We are grounded, so recalculate move direction based on axes

        Vector3 forward = Vector3.ProjectOnPlane(Camera.main.transform.forward, transform.up);
        Vector3 right = Vector3.ProjectOnPlane(Camera.main.transform.right, transform.up); 
        Vector3 outp = forward * MoveDir.y + right * MoveDir.x;
        return outp;
    }

    public void FaceCamera()
    {
        //Quaternion newvect = playerCameraFollowPoint.transform.rotation;
        ////transform.rotation = Quaternion.Euler(0, playerCameraFollowPoint.transform.rotation.eulerAngles.y, 0);
        //////transform.rotation = Quaternion.Euler(0, Mathf.Atan2(MovementVector().x,MovementVector().y)*Mathf.Rad2Deg + playerCamera.transform.eulerAngles.y, 0);


        //transform.forward= Vector3.ProjectOnPlane(Camera.main.transform.forward, transform.up);
        //playerCameraFollowPoint.transform.rotation = newvect;

    }


    public void ArtificialGravity()
    {
        if (!isGrounded)
            rb.AddForce(-transform.transform.up * gravity, ForceMode.Acceleration);
        else
            rb.AddForce(-transform.transform.up, ForceMode.Acceleration);

    }


    public void switchGravity()
    {
        if (canSwitchGravity)
        {
            if (gravityDirection.WasPressedThisFrame())
            {
                StartVisualizeGravityChange();

                if(gravityChangeCoroutine != null)
                    StopCoroutine(gravityChangeCoroutine);
            }

            else if (gravityDirection.inProgress)
            {
                Vector2 input = gravityDirection.ReadValue<Vector2>(); // Read input
                Vector3 RotateTowards = (transform.forward * input.y) + (transform.right * input.x); // Get rotation direction

                VisualizeGravityChange(input,RotateTowards);
            }
            else if (gravityDirection.WasReleasedThisFrame())
            {
                StopVisualizeGravityChange();

                gravityChangeCoroutine = StartCoroutine(RotateOverTime(newGravityDirection, gravityChangeTime));
            }
        }
    }

    IEnumerator RotateOverTime(Vector3 newGravityDirection, float duration)
    {
        canSwitchGravity = false;

        rb.velocity = rb.velocity.normalized *Mathf.Clamp(rb.velocity.magnitude, 0, maxGravitySwitchingVelocityClamp);
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -newGravityDirection) * transform.rotation;
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = targetRotation;  //ensure the rotation is exactly the target rotation 
        canSwitchGravity = true;
    }


    public void StartVisualizeGravityChange()
    {
        
        gravityPreview.SetActive(true);
        gravityPreview.transform.position = transform.position; // Match player position
        gravityPreview.transform.rotation= transform.rotation; // Match player position
        gravityPreview.transform.rotation = Quaternion.FromToRotation(transform.up, - newGravityDirection) * transform.rotation;
    }
    public void VisualizeGravityChange(Vector2 input, Vector3 RotateTowards)
    {
        // Determine the dominant axis efficiently
        int dominantAxis = (Mathf.Abs(RotateTowards.x) > Mathf.Abs(RotateTowards.y) && Mathf.Abs(RotateTowards.x) > Mathf.Abs(RotateTowards.z)) ? 0 :
                            (Mathf.Abs(RotateTowards.y) > Mathf.Abs(RotateTowards.z)) ? 1 : 2;

        // Create the snapped direction using the dominant axis
        newGravityDirection = Vector3.zero;
        newGravityDirection[dominantAxis] = Mathf.Sign(RotateTowards[dominantAxis]);


        // Update the preview position and rotation
        gravityPreview.transform.position = transform.position;
        gravityPreview.transform.rotation = Quaternion.Slerp(
            gravityPreview.transform.rotation, 
            Quaternion.FromToRotation(transform.up, -newGravityDirection) * transform.rotation * Quaternion.Euler(0,180,0), // Rotate 180 degrees to match the preview to the Player's direction
            10f * Time.deltaTime);
    }

    public void StopVisualizeGravityChange()
    {
        gravityPreview.SetActive(false);
    }



    #region getters and setters (DO NOT OPEN IF NOT NECESSARY, BRAINROT GURANTEED)
    public float _gravity { get => gravity; set => gravity = value; }
    public float _walkingSpeed { get => walkingSpeed; set => walkingSpeed = value; }
    public float _jumpSpeed { get => jumpSpeed; set => jumpSpeed = value; }
    public float _forceAppliedInAir { get => forceAppliedInAir; set => forceAppliedInAir = value; }

    public float _TimerInAirBeforeGameOver => TimerInAirBeforeGameOver;
    public InputAction _move { get { return move; } set { move = value; } }
    public InputAction _jump { get { return jump; } set { jump = value; } }
    public BaseState _currentState { get { return currentState; } set { currentState = value; } }
    public CinemachineFreeLook _playerCamera { get { return playerCamera; } set { playerCamera = value; } }

    public GameManager _gameManager { get { return gameManager; } set { gameManager = value; } }

    public Animator _anim { get { return anim; } set { anim = value; } }
    public bool _isGrounded { get => isGrounded; }

    public Vector2 _playerVelocityInPlane => Vector3.ProjectOnPlane(_velocity, transform.up);

    public Vector3 _player_Up => transform.up;
    public float _player_Up_velocity => Vector3.Dot(rb.velocity, transform.up);

    public Vector3 _player_Right=> transform.right;
    public Vector3 _velocity { get { return rb.velocity; } set { rb.velocity = value; } }

    public void _force(Vector3 force, ForceMode mode = ForceMode.Force) => rb.AddForce(force, mode);

    #endregion
}

