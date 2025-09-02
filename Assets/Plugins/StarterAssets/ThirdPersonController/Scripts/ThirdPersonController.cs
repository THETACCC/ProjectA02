 using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Default Position")]
        [Tooltip("Default position for the player.")]
        public Vector3 defaultPosition = new Vector3(4, -8, 101);

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private StarterAssetsInputs _starterAssetsInputs;

        private bool isTeleported = false;


        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string keyX = sceneName + "_LastTriggerX";
            string keyY = sceneName + "_LastTriggerY";
            string keyZ = sceneName + "_LastTriggerZ";

            _starterAssetsInputs = GetComponent<StarterAssetsInputs>();

            if (_starterAssetsInputs != null)

            {
                // Call the TeleportToLastPosition method
                _starterAssetsInputs.TeleportToLastPosition();
                isTeleported = true;
            }
            else
            {
                Debug.LogWarning("StarterAssetsInputs component not found!");
            }

            // Check if the saved position exists for this scene
            if (PlayerPrefs.HasKey(keyX) && PlayerPrefs.HasKey(keyY) && PlayerPrefs.HasKey(keyZ))
            {
                float x = PlayerPrefs.GetFloat(keyX);
                float y = PlayerPrefs.GetFloat(keyY);
                float z = PlayerPrefs.GetFloat(keyZ);
                transform.position = new Vector3(x, y, z);
                Debug.Log($"Loaded saved position for scene {sceneName}: {transform.position}");
            }
            else
            {
                // Only set the default position if no saved position is found for this scene
                transform.position = defaultPosition;
                Debug.Log($"No saved position found for scene {sceneName}. Using default position: {defaultPosition}");
            }

            // Set the initial camera rotation
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            // Initialize other components
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            AssignAnimationIDs();

            // Reset timeouts
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }


        private void Update()
        {
            //print("Player's position:" + transform.position);


            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();

            if (isTeleported && _input.move != Vector2.zero)
            {
                isTeleported = false; // Allow movement after input is detected
            }


        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            if (isTeleported)
                return;

            // --- Speed calculation (unchanged) ---
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // --- Rotation from input (unchanged) ---
            Vector3 inputDirection = new Vector3(_input.move.x, 0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }

            // --- Desired horizontal delta in camera-facing forward ---
            Vector3 desiredForwardDelta = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward * (_speed * Time.deltaTime);

            // --- "Air-wall" edge clamp logic ---
            // If stepping forward would leave ground, try sliding along the platform edge (left/right)
            Vector3 safeHorizontalDelta = ComputeEdgeClampedDelta(transform.position, desiredForwardDelta);

            // --- Real wall slide (optional but useful): project against hit normal if we collide with walls ---
            // CharacterController already slides along colliders, but we can pre-project to reduce jitter.
            if (safeHorizontalDelta.sqrMagnitude > 0f)
            {
                // Capsule cast forward to see if a wall blocks part of this motion.
                float castDist = Mathf.Max(0.2f, safeHorizontalDelta.magnitude + 0.1f);
                Vector3 castDir = safeHorizontalDelta.normalized;

                // Approximate the controller as a capsule using its height/radius
                float radius = _controller.radius * 0.95f;
                float halfH = Mathf.Max(0.01f, (_controller.height * 0.5f) - radius);
                Vector3 p1 = transform.position + Vector3.up * (radius);
                Vector3 p2 = p1 + Vector3.up * (halfH * 2f);

                if (Physics.CapsuleCast(p1, p2, radius, castDir, out var wallHit, castDist, ~0, QueryTriggerInteraction.Ignore))
                {
                    // Project the desired motion onto the plane orthogonal to wall normal (tangent slide)
                    Vector3 tangent = ProjectOnPlane(safeHorizontalDelta, wallHit.normal);
                    // Keep it inside ground as well (re-check)
                    safeHorizontalDelta = ComputeEdgeClampedDelta(transform.position, tangent);
                }
            }

            // --- Vertical component (unchanged) ---
            Vector3 finalMove = Vector3.up * _verticalVelocity * Time.deltaTime;

            // Only add horizontal if the clamped solution is valid (non-zero or still on ground)
            if (safeHorizontalDelta.sqrMagnitude > 0f)
                finalMove += safeHorizontalDelta;

            // --- Apply movement ---
            _controller.Move(finalMove);

            // --- Animator params (unchanged) ---
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
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
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
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
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // if we hit the death plane, reset position
            if (other.CompareTag("DeathPlane"))
            {
                // temporarily disable the CharacterController so moving the transform doesn't conflict with internal state
                _controller.enabled = false;

                // teleport
                transform.position = defaultPosition;

                // zero out any vertical velocity so you don't immediately fall again
                _verticalVelocity = 0f;

                // re‐enable the controller
                _controller.enabled = true;
            }

        }

        // --- Helpers: ground probing & projection ---

        // Returns true if there is ground beneath the given world position within rayDistance.
        private bool GroundBelow(Vector3 worldPos, out RaycastHit hitInfo, float rayDistance = 1.2f)
        {
            // Cast slightly above to avoid starting inside colliders
            Vector3 origin = worldPos + Vector3.up * 0.5f;
            return Physics.Raycast(origin, Vector3.down, out hitInfo, rayDistance, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        // Projects a direction onto a plane defined by a normal
        private static Vector3 ProjectOnPlane(Vector3 dir, Vector3 planeNormal)
        {
            return dir - Vector3.Project(dir, planeNormal);
        }

        // Try to "air-wall" clamp: if forward step leaves ground, try sliding along the platform edge.
        private Vector3 ComputeEdgeClampedDelta(Vector3 currentPos, Vector3 desiredForwardDelta, float maxProbe = 0.6f)
        {
            // 1) If stepping forward still has ground, just return it.
            if (GroundBelow(currentPos + desiredForwardDelta, out _))
                return desiredForwardDelta;

            // 2) Sample left/right near the "forward" tip to estimate which side still has ground.
            Vector3 fwd = desiredForwardDelta.normalized;
            float mag = desiredForwardDelta.magnitude;

            // Local lateral axes (relative to current facing)
            Vector3 right = Quaternion.Euler(0f, 90f, 0f) * fwd;
            Vector3 left = -right;

            // Probe positions close to the forward edge
            float sideProbe = Mathf.Clamp(maxProbe, 0.1f, 1.0f);
            bool leftOK = GroundBelow(currentPos + fwd * (mag * 0.6f) + left * sideProbe, out var leftHit);
            bool rightOK = GroundBelow(currentPos + fwd * (mag * 0.6f) + right * sideProbe, out var rightHit);

            // Prefer the side that still has ground. If both, choose the one that better matches player's input x.
            Vector3 slideDir = Vector3.zero;
            if (leftOK ^ rightOK)
            {
                slideDir = leftOK ? left : right;
            }
            else if (leftOK && rightOK)
            {
                // Choose side by which is "higher" (safer) or by player intent (input x)
                slideDir = (leftHit.point.y >= rightHit.point.y) ? left : right;
            }
            else
            {
                // Neither side near the tip has ground: do a conservative clamp by shortening the forward step
                // until we find ground, like a binary-ish shrink.
                Vector3 tryDelta = desiredForwardDelta;
                for (int i = 0; i < 5; i++)
                {
                    tryDelta *= 0.5f; // shrink
                    if (tryDelta.magnitude <= 0.01f || GroundBelow(currentPos + tryDelta, out _))
                        return tryDelta;
                }
                return Vector3.zero;
            }

            // 3) We have a lateral slide direction along the edge. Try to move along it while staying on ground.
            Vector3 lateralTry = slideDir * mag; // same magnitude as intended forward delta
                                                 // If full magnitude fails, gradually shrink until safe.
            Vector3 candidate = lateralTry;
            for (int i = 0; i < 5; i++)
            {
                if (GroundBelow(currentPos + candidate, out _))
                    return candidate;
                candidate *= 0.5f;
            }

            return Vector3.zero;
        }

    }
}