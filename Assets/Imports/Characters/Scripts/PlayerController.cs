using UnityEngine;
using UnityEngine.InputSystem;

namespace Abdulrahman.PlayerSystem
{
    /// <summary>
    /// Advanced First Person Controller optimized for Single Player and Local Co-Op.
    /// Handles movement, crouching, jumping, and camera effects like HeadBob and Tilt.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Elman Game Dev Tools/Player System/Player Controller")]
    public class PlayerController : MonoBehaviour
    {
        [Header("CO-OP CONFIGURATION")]
        [Tooltip("Check this box on the Player 2 prefab instance in your scene.")]
        public bool isPlayer2 = false;

        [Tooltip("Animator on the remote/third-person body of this player.")]
        public Animator bodyAnimator;

        [Header("PLAYER 2 GAMEPAD (New Input System)")]
        [Tooltip("Right-stick look speed in degrees per second for Player 2's gamepad.")]
        public float gamepadLookSpeed = 200f;
        [Tooltip("Invert the vertical look axis for Player 2's gamepad.")]
        public bool gamepadInvertLookY = false;
        [Range(0f, 0.6f)]
        [Tooltip("Right-stick magnitude below this is treated as zero. Prevents the camera drifting (e.g. looking down-left forever) when the stick is centered/idle. Raise it if your controller drifts a lot.")]
        public float gamepadLookDeadzone = 0.2f;
        [Header("REFERENCES")]
        [Tooltip("The CharacterController component used for physics-based movement.")]
        public CharacterController controller;
        [Tooltip("The Transform of the camera, usually a child of the player object.")]
        public Transform playerCamera;

        [Header("MOVEMENT SETTINGS")]
        public float speed = 6f;
        public float runSpeed = 9f;
        public float jumpHeight = 1.2f;
        public float gravity = -25f;
        public float sensitivity = 2f;
        public KeyCode runKey = KeyCode.LeftShift;
        public KeyCode crouchKey = KeyCode.LeftControl;

        [Header("CAMERA SETTINGS")]
        public float cameraForwardOffset = 0.4f;
        public float maxLookUpAngle = 90f;
        public float maxLookDownAngle = -90f;
        public bool enableHeadBob = true;
        [Range(0.01f, 0.15f)] public float bobAmountX = 0.04f;
        [Range(0.01f, 0.15f)] public float bobAmountY = 0.05f;
        public float walkBobFrequency = 12f;
        public float runBobFrequency = 16f;
        public float crouchBobFrequency = 8f;
        public float bobSmoothness = 10f;

        [Header("CAMERA INERTIA & WEIGHT")]
        [Range(1f, 30f)] public float cameraWeight = 12f;
        private float _targetYaw;
        private float _targetPitch;
        private float _currentYaw;
        private float _currentPitch;
        private float _smoothInputX;

        [Header("CAMERA EFFECTS")]
        public bool enableCameraTilt = true;
        public float tiltAmount = 2f;
        public float tiltSmoothness = 8f;
        public float runTiltMultiplier = 1.2f;
        public float crouchTiltMultiplier = 0.5f;
        [Space]
        public float turnTiltAmount = 1.5f;
        public float maxTotalTilt = 5f;

        [Header("CROUCH SETTINGS")]
        public float crouchHeight = 1.2f;
        public float crouchSmoothTime = 0.1f;

        [Header("FOV SETTINGS")]
        public bool enableRunFov = true;
        public float normalFov = 60f;
        public float runFov = 70f;
        public float fovChangeSpeed = 8f;

        [Header("STAMINA SETTINGS")]
        public float maxStamina = 100f;
        public float staminaDepletionRate = 20f;
        public float staminaRegenRate = 15f;
        private float _currentStamina;
        private bool _canSprint = true;

        [Header("STANDING DETECTION & GROUND CHECK")]
        public GameObject standingHeightMarker;
        public float standingCheckRadius = 0.2f;
        public LayerMask obstacleLayerMask = ~0;
        public float minStandingClearance = 0.01f;
        public LayerMask groundLayer = 1;
        public float groundCheckDistance = 0.5f;

        private Vector3 _velocity;
        private float _currentTilt;
        private float _timer;
        private float _originalHeight;
        private float _targetHeight;
        private float _currentMovementSpeed;
        private float _cameraBaseHeight;
        private float _markerHeightOffset;

        private bool _isGrounded;
        private bool _isCrouching;
        private bool _hasJumped;
        private MovementState _currentMovementState = MovementState.Walking;

        // Centralized input tracking variables
        private float _horizontalInput;
        private float _verticalInput;
        private float _horizontalRawInput;
        private float _verticalRawInput;
        private float _lookInputX;
        private float _lookInputY;
        private bool _runInput;
        private bool _crouchInput;
        private bool _jumpInput;

        public enum MovementState { Walking, Running, Crouching, Jumping }

        public bool IsGrounded => _isGrounded;
        public bool IsCrouching => _isCrouching;
        public MovementState CurrentState => _currentMovementState;
        public float CurrentStamina => _currentStamina;
        public float MaxStamina => maxStamina;

        private void Start()
        {
            _currentStamina = maxStamina;
            if (controller == null) controller = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            _originalHeight = controller.height;
            _targetHeight = _originalHeight;
            _cameraBaseHeight = playerCamera.localPosition.y;

            _targetYaw = transform.eulerAngles.y;
            _targetPitch = playerCamera.localEulerAngles.x;
            _currentYaw = _targetYaw;
            _currentPitch = _targetPitch;

            if (standingHeightMarker != null)
                _markerHeightOffset = standingHeightMarker.transform.position.y - transform.position.y;
        }

        private void Update()
        {
            GatherInputs(); // Step 1: Collect inputs based on player identity

            CheckGroundStatus();
            HandleCrouchLogic();
            UpdateMovementState();
            HandleMovement();
            HandleHeightAndCamera();
            HandleCameraControl();
            HandleCameraTilt();
            HandleFovChange();

            if (enableHeadBob) HandleHeadBob();
            if (bodyAnimator != null) UpdateAnimator();
            HandleStamina();
        }

        /// <summary>
        /// Reads input data into safe framework variables depending on whether this component manages Player 1 or Player 2.
        /// </summary>
        private void GatherInputs()
        {
            if (!isPlayer2)
            {
                // PLAYER 1: Reads standard Desktop Keyboard and Mouse entries
                _horizontalInput = Input.GetAxis("Horizontal");
                _verticalInput = Input.GetAxis("Vertical");
                _horizontalRawInput = Input.GetAxisRaw("Horizontal");
                _verticalRawInput = Input.GetAxisRaw("Vertical");
                _lookInputX = Input.GetAxis("Mouse X") * sensitivity;
                _lookInputY = Input.GetAxis("Mouse Y") * sensitivity;
                _runInput = Input.GetKey(runKey);
                _crouchInput = Input.GetKey(crouchKey);
                _jumpInput = Input.GetButtonDown("Jump");
            }
            else
            {
                // PLAYER 2: New Input System gamepad (e.g. DualShock 4 / PS4 controller).
                // Reading via the Gamepad abstraction avoids platform-specific legacy
                // axis-number issues (DS4 right-stick Y is not the same axis on every OS).
                Gamepad gp = Gamepad.current;
                if (gp != null)
                {
                    Vector2 move = gp.leftStick.ReadValue();
                    // Read ONLY the right stick for look (never the triggers), then
                    // apply a radial deadzone so idle stick drift produces zero rotation.
                    Vector2 look = ApplyRadialDeadzone(gp.rightStick.ReadValue(), gamepadLookDeadzone);

                    _horizontalInput = move.x;
                    _verticalInput = move.y;
                    _horizontalRawInput = Mathf.Abs(move.x) > 0.1f ? Mathf.Sign(move.x) : 0f;
                    _verticalRawInput = Mathf.Abs(move.y) > 0.1f ? Mathf.Sign(move.y) : 0f;

                    // Sticks return a sustained value, so scale by deltaTime for
                    // frame-rate-independent rotation (degrees per second).
                    float lookYSign = gamepadInvertLookY ? -1f : 1f;
                    _lookInputX = look.x * gamepadLookSpeed * Time.deltaTime;
                    _lookInputY = look.y * gamepadLookSpeed * Time.deltaTime * lookYSign;

                    _runInput = gp.leftStickButton.isPressed;        // L3 = sprint (hold)
                    _crouchInput = gp.buttonEast.isPressed;          // Circle = crouch (hold)
                    _jumpInput = gp.buttonSouth.wasPressedThisFrame; // Cross = jump
                }
                else
                {
                    // No gamepad connected: keep Player 2 idle rather than reading stale input.
                    _horizontalInput = _verticalInput = 0f;
                    _horizontalRawInput = _verticalRawInput = 0f;
                    _lookInputX = _lookInputY = 0f;
                    _runInput = _crouchInput = _jumpInput = false;
                }
            }
        }

        /// <summary>
        /// Radial deadzone with rescaling. Returns Vector2.zero while the stick is
        /// inside the deadzone (eliminating idle drift), then ramps the magnitude
        /// smoothly from 0 once the stick moves beyond the threshold.
        /// </summary>
        private static Vector2 ApplyRadialDeadzone(Vector2 stick, float deadzone)
        {
            float magnitude = stick.magnitude;
            if (magnitude <= deadzone || magnitude <= 0.0001f) return Vector2.zero;
            float scaled = Mathf.Clamp01((magnitude - deadzone) / (1f - deadzone));
            return (stick / magnitude) * scaled;
        }

        private void HandleStamina()
        {
            if (_currentMovementState == MovementState.Running && (_horizontalInput != 0 || _verticalInput != 0))
            {
                _currentStamina -= staminaDepletionRate * Time.deltaTime;
                if (_currentStamina <= 0)
                {
                    _currentStamina = 0;
                    _canSprint = false;
                }
            }
            else
            {
                _currentStamina += staminaRegenRate * Time.deltaTime;
                if (_currentStamina >= maxStamina)
                {
                    _currentStamina = maxStamina;
                    _canSprint = true;
                }
            }
        }

        private void UpdateAnimator()
        {
            float speedMag = new Vector2(_horizontalRawInput, _verticalRawInput).magnitude * _currentMovementSpeed;

            bodyAnimator.SetFloat("Speed", speedMag, 0.1f, Time.deltaTime);
            bodyAnimator.SetBool("IsCrouching", _isCrouching);
            bodyAnimator.SetFloat("DirectionX", _horizontalRawInput, 0.1f, Time.deltaTime);
            bodyAnimator.SetFloat("DirectionY", _verticalRawInput, 0.1f, Time.deltaTime);
            bodyAnimator.SetBool("IsJumping", !_isGrounded);
        }

        private void CheckGroundStatus()
        {
            Vector3 origin = transform.position + Vector3.up * controller.radius;
            bool groundHit = Physics.SphereCast(origin, controller.radius * 0.8f, Vector3.down, out _, groundCheckDistance, groundLayer);
            _isGrounded = groundHit || controller.isGrounded;

            if (_isGrounded && _velocity.y < 0)
            {
                _hasJumped = false;
                _velocity.y = -5f;
            }
        }

        private void UpdateMovementState()
        {
            bool wantsToRun = _runInput && _verticalInput > 0.1f && _currentStamina > 0 && _canSprint;

            if (!_isGrounded)
            {
                _currentMovementState = MovementState.Jumping;
                _currentMovementSpeed = wantsToRun ? runSpeed : speed;
                return;
            }

            if (_isCrouching)
            {
                _currentMovementState = MovementState.Crouching;
                _currentMovementSpeed = speed * 0.5f;
            }
            else
            {
                _currentMovementState = wantsToRun ? MovementState.Running : MovementState.Walking;
                _currentMovementSpeed = wantsToRun ? runSpeed : speed;
            }
        }

        private void HandleMovement()
        {
            Vector3 moveInput = transform.right * _horizontalInput + transform.forward * _verticalInput;
            if (moveInput.magnitude > 1f) moveInput.Normalize();

            if (_jumpInput && _isGrounded && !_isCrouching)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _hasJumped = true;
                _isGrounded = false;
            }

            if (standingHeightMarker != null)
                standingHeightMarker.transform.position = new Vector3(transform.position.x, transform.position.y + _markerHeightOffset, transform.position.z);

            controller.Move(moveInput * _currentMovementSpeed * Time.deltaTime);
            _velocity.y += gravity * Time.deltaTime;
            _velocity.x = Mathf.Lerp(_velocity.x, 0f, Time.deltaTime * 10f);
            _velocity.z = Mathf.Lerp(_velocity.z, 0f, Time.deltaTime * 10f);
            controller.Move(_velocity * Time.deltaTime);
        }

        private void HandleCrouchLogic()
        {
            _isCrouching = _crouchInput || !CanStandUp();
            _targetHeight = _isCrouching ? crouchHeight : _originalHeight;
        }

        private void HandleHeightAndCamera()
        {
            float prevHeight = controller.height;
            controller.height = Mathf.Lerp(controller.height, _targetHeight, Time.deltaTime * (1f / crouchSmoothTime));

            if (_isGrounded)
            {
                float heightDiff = controller.height - prevHeight;
                if (heightDiff > 0) controller.Move(Vector3.up * heightDiff);
            }

            float currentRelativeHeight = _cameraBaseHeight * (controller.height / _originalHeight);
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, currentRelativeHeight, Time.deltaTime * (1f / crouchSmoothTime));
            playerCamera.localPosition = camPos;
            camPos.z = cameraForwardOffset;
        }

        private void HandleCameraControl()
        {
            _smoothInputX = Mathf.Lerp(_smoothInputX, _lookInputX, Time.deltaTime * cameraWeight);

            _targetYaw += _lookInputX;
            _targetPitch -= _lookInputY;
            _targetPitch = Mathf.Clamp(_targetPitch, maxLookDownAngle, maxLookUpAngle);

            float smoothFactor = Mathf.Clamp01(Time.deltaTime * cameraWeight);
            _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, smoothFactor);
            _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, smoothFactor);

            transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
            playerCamera.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentTilt);
        }

        private void HandleCameraTilt()
        {
            if (!enableCameraTilt) { _currentTilt = 0; return; }

            float keyboardTilt = -_horizontalInput * tiltAmount;
            float mouseTilt = -_smoothInputX * turnTiltAmount;
            float targetTiltTotal = keyboardTilt + mouseTilt;

            if (_currentMovementState == MovementState.Running) targetTiltTotal *= runTiltMultiplier;
            if (_isCrouching) targetTiltTotal *= crouchTiltMultiplier;

            targetTiltTotal = Mathf.Clamp(targetTiltTotal, -maxTotalTilt, maxTotalTilt);
            _currentTilt = Mathf.Lerp(_currentTilt, targetTiltTotal, Time.deltaTime * tiltSmoothness);
        }

        public void ApplyKnockback(Vector3 direction)
        {
            _velocity.x += direction.x * 40f;
            _velocity.z += direction.z * 40f;
            _velocity.y += direction.y * 40f;
        }

        /// <summary>
        /// Temporarily multiplies the player's jump height for a limited duration,
        /// then reverts it. Used by the shop's "Jump Boost" purchase.
        /// </summary>
        /// <param name="multiplier">Factor applied to jumpHeight (e.g. 1.5 = +50%).</param>
        /// <param name="duration">How long the boost lasts, in seconds.</param>
        public void BoostJump(float multiplier, float duration)
        {
            StartCoroutine(JumpBoostCoroutine(multiplier, duration));
        }

        private System.Collections.IEnumerator JumpBoostCoroutine(float multiplier, float duration)
        {
            jumpHeight *= multiplier;
            yield return new WaitForSeconds(duration);
            jumpHeight /= multiplier;
        }

        private void HandleFovChange()
        {
            if (!enableRunFov || playerCamera.GetComponent<Camera>() == null) return;
            bool isActuallyRunning = _runInput && _verticalInput > 0.1f;
            Camera cam = playerCamera.GetComponent<Camera>();
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, isActuallyRunning ? runFov : normalFov, Time.deltaTime * fovChangeSpeed);
        }

        private void HandleHeadBob()
        {
            float moveMag = new Vector2(_horizontalInput, _verticalInput).magnitude;
            float currentCamH = _cameraBaseHeight * (controller.height / _originalHeight);

            if (!_isGrounded || moveMag <= 0.1f)
            {
                _timer = 0;
                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, new Vector3(0, currentCamH, 0), Time.deltaTime * bobSmoothness);
                return;
            }

            float freq = (_currentMovementState == MovementState.Running) ? runBobFrequency : (_isCrouching ? crouchBobFrequency : walkBobFrequency);
            _timer += Time.deltaTime * freq;

            Vector3 newPos = new Vector3(
                Mathf.Cos(_timer * 0.5f) * bobAmountX,
                currentCamH + Mathf.Sin(_timer) * bobAmountY,
                0
            );
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, newPos, Time.deltaTime * bobSmoothness);
        }

        public bool CanStandUp()
        {
            if (standingHeightMarker == null) return true;
            Collider[] hits = Physics.OverlapSphere(standingHeightMarker.transform.position, standingCheckRadius, obstacleLayerMask);
            foreach (Collider col in hits)
            {
                if (col.transform.IsChildOf(transform) || col.transform == transform || col.isTrigger) continue;
                if (col.bounds.min.y < standingHeightMarker.transform.position.y + minStandingClearance) return false;
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (standingHeightMarker != null)
            {
                Gizmos.color = CanStandUp() ? Color.green : Color.red;
                Gizmos.DrawWireSphere(standingHeightMarker.transform.position, standingCheckRadius);
            }
        }
    }
}