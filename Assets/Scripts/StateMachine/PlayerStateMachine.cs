using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main player controller implementing a state machine pattern to handle player movement,
/// gravity manipulation, and interactions.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerStateMachine : MonoBehaviour
{
    #region Seriliazed Fields
    [Header("General Settings")]
    [SerializeField] float gravity;
    [SerializeField] float walkingSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float forceAppliedInAir;
    [SerializeField] float timeInAirBeforeGameOver;
    [SerializeField] GameObject PlayerModel;

    [Header("Gravity Settings")]
    [SerializeField] GameObject gravityPreview;
    [SerializeField] float velocityClampOnGravitySwitching;
    [SerializeField] float gravityChangeTime;
    #endregion

    private bool isGrounded;   
    private bool canSwitchGravity = true;
    private Vector3 newGravityDirection;

    private Camera mainCamera;
    private StateFactory stateFactory;
    private BaseState currentState;
    private Rigidbody rb;
    private PlayerInputAction playerInput;
    private GameManager gameManager;
    private Coroutine gravityChangeCoroutine;
    private Coroutine visualizeGravityCoroutine;
    private Animator anim;

    private InputAction move;
    private InputAction jump;



    #region input
    private void Awake()
    {
        PlayerModel.TryGetComponent<Animator>( out anim);
        gameManager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;


        playerInput = new();
        stateFactory = new(this);

        move = playerInput.Player.Movement;
        jump = playerInput.Player.Jump;
    }

    private void OnEnable()
    {
        move.Enable();
        jump.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
    }
    #endregion

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);
        currentState = stateFactory.Idle();
        currentState.EnterState();
    }


    void Update()
    {
        // Move the controller
        currentState.UpdateState();

        CheckGrounded();
    }

    private void FixedUpdate()
    {
        currentState.FixedState();
        ArtificialGravity();
    }


    /// <summary>
    /// Determines if the player is currently grounded using a raycast.
    /// </summary>
    public void CheckGrounded()
    {
        Vector3 position = PlayerModel.transform.position;
        Vector3 direction= PlayerModel.transform.up;

        isGrounded = Physics.Raycast(position + direction * 0.2f, -direction, 0.3f);
    }

    /// <summary>
    /// Gets the current movement input direction.
    /// </summary>
    public Vector2 MoveDir => move.ReadValue<Vector2>();


    /// <summary>Adds a force to the player.</summary>
    /// <param name="force">The force to apply.</param>
    /// <param name="mode">The force mode to use.</param>
    public void Force(Vector3 force, ForceMode mode = ForceMode.Force) => rb.AddForce(force, mode);

    /// <summary>
    /// Gets the camera's forward direction projected onto the player's plane.
    /// </summary>
    public Vector3 CameraForwardInPlayerPlane => Vector3.ProjectOnPlane(mainCamera.transform.forward, transform.up);
    
    /// <summary>
    /// Gets the camera's right direction projected onto the player's plane.
    /// </summary>
    public Vector3 CameraRightInPlayerPlane => Vector3.ProjectOnPlane(mainCamera.transform.right, transform.up);
    
    
    
    /// <summary>
    /// Calculates the movement vector based on input and camera orientation.
    /// </summary>
    /// <returns>The movement direction vector relative to camera orientation.</returns>
    public Vector3 MovementVector()
    {
        Vector3 forward = CameraForwardInPlayerPlane;
        Vector3 right = CameraRightInPlayerPlane; 
        Vector3 outp = forward * MoveDir.y + right * MoveDir.x;
        return outp;
    }

    /// <summary>
    /// Rotates the player model to face the movement direction.
    /// Only rotates when there is movement input.
    /// </summary>
    public void FaceCamera()
    {
        // Rotate the player model to face the camera
        if (MovementVector() != Vector3.zero)
            PlayerModel.transform.rotation = Quaternion.Slerp(PlayerModel.transform.rotation, Quaternion.LookRotation(MovementVector(), transform.up), Time.deltaTime * 10f);
    }


    /// <summary>
    /// Applies gravity to the player, with different force when grounded vs in air.
    /// </summary>
    public void ArtificialGravity()
    {
        float gravityForce = isGrounded ? 1f : gravity;
        rb.AddForce(-transform.up * gravityForce, ForceMode.Acceleration);
    }




    /// <summary>
    /// Coroutine that rotates the player over time when changing gravity direction.
    /// </summary>
    /// <param name="newGravityDirection">Target gravity direction.</param>
    /// <param name="duration">Duration of the rotation in seconds.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    IEnumerator RotateOverTime(Vector3 newGravityDirection, float duration)
    {
        canSwitchGravity = false;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -newGravityDirection) * transform.rotation;
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rb.velocity = rb.velocity.normalized *Mathf.Clamp(rb.velocity.magnitude, 0, velocityClampOnGravitySwitching);
            
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = targetRotation;  //ensure the rotation is exactly the target rotation 
        canSwitchGravity = true;
    }




    /// <summary>
    /// Coroutine that animates the gravity preview rotation.
    /// </summary>
    /// <param name="newGravityDirection">Target gravity direction.</param>
    /// <param name="duration">Duration of the preview animation in seconds.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    IEnumerator VisualizeGravityOverTime(Vector3 newGravityDirection, float duration)
    {

        Quaternion startRotation = gravityPreview.transform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -newGravityDirection) * transform.rotation * Quaternion.Euler(0, 180, 0);
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            
            elapsed += Time.deltaTime;
            gravityPreview.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }
        gravityPreview.transform.rotation = targetRotation;  //ensure the rotation is exactly the target rotation 
    }



    /// <summary>
    /// Handles the gravity direction change visualization based on input.
    /// </summary>
    /// <param name="gravitySwitchDirection">Input action context for gravity direction.</param>
    public void VisualizeGravityChange(InputAction.CallbackContext gravitySwitchDirection)
    {
        if (canSwitchGravity)
        {
            if (gravityChangeCoroutine != null)
                StopCoroutine(gravityChangeCoroutine);

            if (!gravityPreview.activeSelf && gravitySwitchDirection.started)
            {
                gravityPreview.SetActive(true);
                gravityPreview.transform.position = transform.position; // Match player position
                gravityPreview.transform.rotation= transform.rotation * Quaternion.Euler(0, 180, 0); // Match player rotation
            }
            else if (gravityPreview.activeSelf && gravitySwitchDirection.performed)
            {

                UpdateNewGravityDirection(gravitySwitchDirection.ReadValue<Vector2>());

                // Update the preview position and rotation
                if (visualizeGravityCoroutine != null) { StopCoroutine(visualizeGravityCoroutine); }
                
                visualizeGravityCoroutine = StartCoroutine(VisualizeGravityOverTime(newGravityDirection, gravityChangeTime));
            }
        }
    }

    /// <summary>
    /// Updates the new gravity direction based on input values.
    /// Determines the dominant axis for gravity change.
    /// </summary>
    /// <param name="input">Input direction for gravity change.</param>
    public void UpdateNewGravityDirection(Vector2 input)
    {
        Vector3 RotateTowards = ( CameraForwardInPlayerPlane * input.y) + (CameraRightInPlayerPlane * input.x); // Get rotation direction

        // Determine the dominant axis efficiently
        int dominantAxis = (Mathf.Abs(RotateTowards.x) > Mathf.Abs(RotateTowards.y) && Mathf.Abs(RotateTowards.x) > Mathf.Abs(RotateTowards.z)) ? 0 :
                            (Mathf.Abs(RotateTowards.y) > Mathf.Abs(RotateTowards.z)) ? 1 : 2;

        // Create the snapped direction using the dominant axis
        newGravityDirection = Vector3.zero;
        newGravityDirection[dominantAxis] = Mathf.Sign(RotateTowards[dominantAxis]);
    }


    /// <summary>
    /// Confirms and applies the new gravity direction when input is received.
    /// </summary>
    /// <param name="confirmGravityChange">Input action context for confirming gravity change.</param>
    public void SetNewGravityDirection(InputAction.CallbackContext confirmGravityChange)
    {
        if( gravityPreview.activeSelf && confirmGravityChange.started)
        {
            gravityPreview.SetActive(false);

            gravityChangeCoroutine = StartCoroutine(RotateOverTime(newGravityDirection, gravityChangeTime));
            if (visualizeGravityCoroutine != null)
                StopCoroutine(visualizeGravityCoroutine);
        }
    }



    #region getters and setters
    public float _gravity { get => gravity; set => gravity = value; }
    public float _walkingSpeed { get => walkingSpeed; set => walkingSpeed = value; }
    public float _jumpSpeed { get => jumpSpeed; set => jumpSpeed = value; }
    public float _forceAppliedInAir { get => forceAppliedInAir; set => forceAppliedInAir = value; }

    public float _timeInAirBeforeGameOver => timeInAirBeforeGameOver;
    public InputAction _move { get => move; set => move = value; }
    public InputAction _jump { get => jump; set => jump = value; }
    public BaseState _currentState { get => currentState; set => currentState = value; }

    public GameManager _gameManager { get => gameManager; set => gameManager = value; }

    public Animator _anim { get => anim; set => anim = value; }
    public bool _isGrounded { get => isGrounded; }

    public Vector2 _playerVelocityInPlane => new Vector2(
            Vector3.Dot(_velocity, transform.right),   // X-axis (Right)
            Vector3.Dot(_velocity, transform.forward));

    public Vector3 _player_Up => transform.up;
    public float _player_Up_velocity => Vector3.Dot(rb.velocity, transform.up);

    public Vector3 _velocity { get => rb.velocity; set => rb.velocity = value; }


    #endregion
}

