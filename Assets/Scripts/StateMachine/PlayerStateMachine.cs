using Cinemachine;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.Windows;


[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerStateMachine : MonoBehaviour
{
    StateFactory stateFactory;
    BaseState currentState;
    public Text speed;
    public float walkNormalizingTime;
    [Header("General Settings")]
    [SerializeField] float gravity;
    [SerializeField] float walkingSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float slideSpeed;
    [SerializeField] float forceAppliedInAir;

    [Header("Camera Settings")]
    [SerializeField] float lookSpeed;
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] GameObject playerCameraFollowPoint;
    [SerializeField] float lookXLimit;

    Rigidbody rb;
    CapsuleCollider col;


    float tgtSpeed;
    bool isGrounded;
    private float rotationX = 0;
    private float rotationY = 0;
    private float distanceToGround;
    Vector3 newGravityDirection;
    private Vector3 moveDirection = Vector3.zero;
   
    private PlayerInputAction playerInput;
    private InputAction move;
    private InputAction camDirection;
    private InputAction jump;
    private InputAction gravityDirection;



    #region input
    private void Awake()
    {
        playerInput = new();
        stateFactory = new(this);
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();


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
        Debug.Log(isGrounded);
        //characterController.Move(moveDirection * Time.deltaTime);

        // Move the controller
        currentState.UpdateState();

        // Player and Camera rotation
        cameraMovement();

        //displays the current speed
        //speed.text = Math.Round(PCC._currentVelocityMagnitude).ToString();

        CheckGrounded();

        switchGravity();
    }

    private void FixedUpdate()
    {
        currentState.FixedState();

        ArtificialGravity();
    }

    public void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + col.center, -transform.up, out RaycastHit hit, distanceToGround + .8f);
    }

    //private void LateUpdate()
    //{
    //    currentState.LateUpdateState();
    //}

    public void cameraMovement()
    {
        rotationX += -LookDir().y * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        
        rotationY += LookDir().x * lookSpeed;

        playerCameraFollowPoint.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);


    }

    public Vector2 MoveDir()
    {
        return move.ReadValue<Vector2>();
    }

    public Vector2 LookDir()
    {
        return camDirection.ReadValue<Vector2>();
    }

    public Vector3 MovementVector()
    {
        // We are grounded, so recalculate move direction based on axes

        Vector3 forward = Vector3.ProjectOnPlane(Camera.main.transform.forward, transform.up);
        Vector3 right = Vector3.ProjectOnPlane(Camera.main.transform.right, transform.up); 
        Vector3 outp = forward * MoveDir().y + right * MoveDir().x;
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
        if (gravityDirection.inProgress)
        {
            Vector2 input = gravityDirection.ReadValue<Vector2>(); // Read input
            Vector3 moveDir = (transform.forward * input.y) + (transform.right * input.x); // Get movement direction

            // Determine the dominant axis efficiently
            int dominantAxis = (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y) && Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.z)) ? 0 :
                               (Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.z)) ? 1 : 2;

            // Create the snapped direction using the dominant axis
            newGravityDirection = Vector3.zero;
            newGravityDirection[dominantAxis] = Mathf.Sign(moveDir[dominantAxis]); 


            Debug.Log("Gravity pressed" + newGravityDirection);
        }
        if (gravityDirection.WasReleasedThisFrame())
        {
            //ignore for now, create a coroutine later
            //// Compute new rotation with snappedDir as the new up direction
            //Quaternion targetRotation = Quaternion.FromToRotation(transform.up, newGravityDirection) * transform.rotation;

            //// Apply the new rotation smoothly (optional)
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            transform.rotation = Quaternion.FromToRotation(transform.up, newGravityDirection) * transform.rotation;



            Debug.Log("Gravity released" + newGravityDirection);
        }
    }



    #region getters and setters (DO NOT OPEN IF NOT NECESSARY, BRAINROT GURANTEED)
    public float _gravity { get => gravity; set => gravity = value; }
    public float _walkingSpeed { get => walkingSpeed; set => walkingSpeed = value; }
    public float _jumpSpeed { get => jumpSpeed; set => jumpSpeed = value; }
    public float _slideSpeed { get => slideSpeed; set => slideSpeed = value; }
    public float _forceAppliedInAir { get => forceAppliedInAir; set => forceAppliedInAir = value; }

    public float _moveDirectionX { get { return moveDirection.x; } set { moveDirection.x = value; } }
    public float _moveDirectionY { get { return moveDirection.y; } set { moveDirection.y = value; } }
    public float _moveDirectionZ { get { return moveDirection.z; } set { moveDirection.z = value; } }
    public Vector3 _moveDirection { get { return moveDirection; } set { moveDirection = value; } }

    public InputAction _move { get { return move; } set { move = value; } }
    public InputAction _jump { get { return jump; } set { jump = value; } }
    public BaseState _currentState { get { return currentState; } set { currentState = value; } }
    public CinemachineVirtualCamera _playerCamera { get { return playerCamera; } set { playerCamera = value; } }
    public float _TGTSpeed { get { return tgtSpeed; } set { tgtSpeed = value; } }

    public bool _isGrounded { get => isGrounded; }

    public Vector3 _player_Up => transform.up;
    public float _player_Up_velocity => Vector3.Dot(rb.velocity, transform.up);

    public Vector3 _player_Right=> transform.right;
    public Vector3 _velocity { get { return rb.velocity; } set { rb.velocity = value; } }

    public void _force(Vector3 force, ForceMode mode = ForceMode.Force) => rb.AddForce(force, mode);


    #endregion
}

