using System.Runtime.CompilerServices;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    // Public Variables
    public CharacterController _controller;
    public float _speed;
    public float _gravity;
    public float _jumpheight;
    public Transform _groundCheck;
    public float _groundDistance;
    public LayerMask _groundMask;
    public float _slideSpeed;
    public Camera _viewCamera;

    // Private Variables
    Vector3 _velocity;
    bool _isGrounded;
    int _jumpCount;
    float _slideTimer = 100;
    bool _isSliding = false;
    float _ogSpeed;
    bool _momentum = false;
    bool _momentumApplied = false;
    bool _isWallrunning = false;
    bool _isWallrunningLeft = false;
    Transform _wallrunningReference;


    // Constants
    const float SLIDE_DURATION = 0.4f;
    const int SLIDE_COOLDOWN_MIN_TIME = 5;
    const float SLIDE_HEIGHT_MODIFIER = 1.3f;
    const float WALLRUN_CHECK_DISTANCE = 2f;

    private void Start()
    {
        // Set the default speed of the player
        _ogSpeed = _speed;
    }

    private void Update()
    {
        // WallRunning
        if (Input.GetButtonDown("Wall Running") && !_isWallrunning)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, -transform.right, out hit, WALLRUN_CHECK_DISTANCE))
            {
                if (hit.transform.tag == "Wall Running")
                {
                    _wallrunningReference = hit.transform.gameObject.GetComponent<Enviroment_WallRunning_Orientation>()._leftOrientation.transform;
                    transform.localEulerAngles = new Vector3(
                        transform.localEulerAngles.x,
                        _wallrunningReference.localEulerAngles.y,
                        transform.localEulerAngles.z);
                    _isWallrunning = true;
                    _isWallrunningLeft = true;
                    ResetJumpCounter();
                }
            } 
            else if (Physics.Raycast(transform.position, transform.right, out hit, WALLRUN_CHECK_DISTANCE))
            {
                if (hit.transform.tag == "Wall Running")
                {
                    _wallrunningReference = hit.transform.gameObject.GetComponent<Enviroment_WallRunning_Orientation>()._rightOrientation.transform;
                    transform.localEulerAngles = new Vector3(
                        transform.localEulerAngles.x,
                        _wallrunningReference.localEulerAngles.y,
                        transform.localEulerAngles.z);
                    _isWallrunning = true;
                    _isWallrunningLeft = false;
                    ResetJumpCounter();
                }
            }


            Debug.DrawRay(transform.position, -transform.right * hit.distance, Color.yellow, WALLRUN_CHECK_DISTANCE);
        }
        
        if (_isWallrunning)
        {
            RaycastHit hit;

            _wallrunningReference.position = transform.position;


            if (_isWallrunningLeft && !Physics.Raycast(transform.position, -_wallrunningReference.right, out hit, WALLRUN_CHECK_DISTANCE))
            {
                _isWallrunning = false;
            } else if (!_isWallrunningLeft && !Physics.Raycast(transform.position, _wallrunningReference.right, out hit, WALLRUN_CHECK_DISTANCE))
            {
                _isWallrunning = false;
            }
        }

        // ------------------------------------------------

        // Check if player in on the ground
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            ResetJumpCounter();
        }

        // Apply Slide
        if (Input.GetButtonDown("Slide") && _slideTimer > SLIDE_COOLDOWN_MIN_TIME && !_isSliding && _isGrounded)
        {
            _momentum = true; // Amplify speed
            _slideTimer = 0f; // Set timer to 0
            _isSliding = true; // Activate cooldown

            // Move down the camera
            _viewCamera.transform.position = new Vector3(
                _viewCamera.transform.position.x,
                _viewCamera.transform.position.y - SLIDE_HEIGHT_MODIFIER,
                _viewCamera.transform.position.z);

            // Change the height of the character controller
            _controller.height -= SLIDE_HEIGHT_MODIFIER;

            // Make the center of the character controllen below it's original
            // point to avoid the character controller get down with the gravity
            _controller.center = new Vector3(
                _controller.center.x,
                _controller.center.y - (SLIDE_HEIGHT_MODIFIER / 2),
                _controller.center.z);
        }

        // If the player is sliding, make the following checks
        if (_isSliding)
        {
            // Check if the player, during the sliding, jumps or 
            // falls down, let it keep the
            if (!_isGrounded)
                _momentum = true;

            // When X seconds passes, the player stop the slide
            if (_slideTimer > SLIDE_DURATION)
            {
                if (_isGrounded) _momentum = false; // Reset speed only if the player touches the ground
                _isSliding = false; // Reset cooldown

                // Reset camera height
                _viewCamera.transform.position = new Vector3(
                    _viewCamera.transform.position.x,
                    _viewCamera.transform.position.y + SLIDE_HEIGHT_MODIFIER,
                    _viewCamera.transform.position.z);

                // Reset controller height
                _controller.height += SLIDE_HEIGHT_MODIFIER;

                // Reset controller center
                _controller.center = new Vector3(
                    _controller.center.x,
                    _controller.center.y + (SLIDE_HEIGHT_MODIFIER / 2),
                    _controller.center.z);
            }
        }
        else
        {
            // If the player is touching the floor, and not sliding, stop the momentum
            if (_isGrounded) _momentum = false;
        }

        // Increase slide timer
        _slideTimer += Time.deltaTime;

        // Get Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Apply momentum
        if (_momentum && !_momentumApplied)
        {
            _speed *= _slideSpeed;
            _momentumApplied = true;
        } 
        else
        {
            _speed = _ogSpeed;
            _momentumApplied = false;
        }

        // Apply movement
        // -- if the player is wallrunning, he can only move forward --
        Vector3 move;
        if (!_isWallrunning)
            move = transform.right * x + transform.forward * z;
        else
            move = _wallrunningReference.forward;

        _controller.Move(move * _speed * Time.deltaTime);

        // Apply jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
            Jump(1);

        // Check if double jump is available
        if (!_isGrounded && _jumpCount <= 1 && Input.GetButtonDown("Jump"))
            Jump(2);

        // Apply gravity

        if (!_isWallrunning)
            _velocity.y += _gravity * Time.deltaTime;
        else
            _velocity.y = 0;

        // Move Character Controller
        _controller.Move(_velocity * Time.deltaTime);
    }

    // Jumping
    private void Jump(int increaser)
    {
        _jumpCount += increaser;
        _velocity.y = Mathf.Sqrt(_jumpheight * -2f * _gravity);
    }

    private void ResetJumpCounter()
    {
        _jumpCount = 0;
    }
}