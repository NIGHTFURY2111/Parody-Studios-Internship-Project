using Cinemachine;
using Cinemachine.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


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

    [Header("Camera Settings")]
    [SerializeField] float lookXLimit;
    [SerializeField] float lookSpeed;

    [Header("Gravity Settings")]
    [SerializeField] GameObject gravityPreview;
    [SerializeField] float maxVelocityClampWhenGravitySwitching;
    [SerializeField] float gravityChangeTime;
    #endregion

    bool isGrounded;   
    private bool canSwitchGravity = true;
    private bool canVisualizeGravity = true;
    private Vector3 newGravityDirection;

    private Camera mainCamera;

    private StateFactory stateFactory;
    private BaseState currentState;
    private Rigidbody rb;
    private CapsuleCollider col;
    private PlayerInputAction playerInput;
    private GameManager gameManager;
    private Coroutine gravityChangeCoroutine;
    private Coroutine visualizeGravityCoroutine;
    private Animator anim;

    private InputAction move;
    private InputAction camDirection;
    private InputAction jump;
    private InputAction gravityDirection;
    private InputAction setNewGravity;



    #region input
    private void Awake()
    {
        PlayerModel.TryGetComponent<Animator>( out anim);
        gameManager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        mainCamera = Camera.main;


        playerInput = new();
        stateFactory = new(this);

        move = playerInput.Player.Movement;
        camDirection = playerInput.Player.Camera;
        jump = playerInput.Player.Jump;
        gravityDirection = playerInput.Player.SwitchGravity;
        setNewGravity = playerInput.Player.SetNewGravity;
    }

    private void OnEnable()
    {
        move.Enable();
        jump.Enable();
        camDirection.Enable();
        gravityDirection.Enable();
        setNewGravity.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        camDirection.Disable();
        gravityDirection.Disable();
        setNewGravity.Disable();
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

    public void CheckGrounded()
    {
        Vector3 position = PlayerModel.transform.position;
        Vector3 direction= PlayerModel.transform.up;

        //isGrounded = Physics.SphereCast(position, col.radius, -direction, out _, 0.3f);

        isGrounded = Physics.Raycast(position + direction * 0.2f, -direction, 0.3f);
    }

    //public void cameraMovement()
    //{ Not needed as we using Cinemachine now to handle camera movement

    //    //rotationX += -LookDir.y * lookSpeed;
    //    //rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        
    //    //rotationY += LookDir.x * lookSpeed;

    //    //playerCameraFollowPoint.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
    //}

    public Vector2 MoveDir => move.ReadValue<Vector2>();

    public Vector2 LookDir => camDirection.ReadValue<Vector2>();

    public Vector3 CameraForwardInPlayerPlane => Vector3.ProjectOnPlane(mainCamera.transform.forward, transform.up);
    public Vector3 CameraRightInPlayerPlane => Vector3.ProjectOnPlane(mainCamera.transform.right, transform.up);

    public Vector3 MovementVector()
    {
        // We are grounded, so recalculate move direction based on axes

        Vector3 forward = CameraForwardInPlayerPlane;
        Vector3 right = CameraRightInPlayerPlane; 
        Vector3 outp = forward * MoveDir.y + right * MoveDir.x;
        return outp;
    }

    public void FaceCamera()
    {
        // Rotate the player model to face the camera
        if (MovementVector() != Vector3.zero)
            PlayerModel.transform.rotation = Quaternion.Slerp(PlayerModel.transform.rotation, Quaternion.LookRotation(MovementVector(), transform.up), Time.deltaTime * 10f);
    }


    public void ArtificialGravity()
    {
        float gravityForce = isGrounded ? 1f : gravity;
        rb.AddForce(-transform.up * gravityForce, ForceMode.Acceleration);
    }

    IEnumerator RotateOverTime(Vector3 newGravityDirection, float duration)
    {
        canSwitchGravity = false;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -newGravityDirection) * transform.rotation;
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rb.velocity = rb.velocity.normalized *Mathf.Clamp(rb.velocity.magnitude, 0, maxVelocityClampWhenGravitySwitching);
            
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = targetRotation;  //ensure the rotation is exactly the target rotation 
        canSwitchGravity = true;
    }
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



    #region getters and setters (DO NOT OPEN IF NOT NECESSARY, BRAINROT GURANTEED)
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

    public void _force(Vector3 force, ForceMode mode = ForceMode.Force) => rb.AddForce(force, mode);

    #endregion
}

