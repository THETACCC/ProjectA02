using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Default Spawn (No manual numbers)")]
        [Tooltip("If a GameObject with this name exists in the active scene, we use it as the scene default spawn when no saved position exists.")]
        public string defaultSpawnObjectName = "Player_DefaultSpawn";

        [Header("World Spawn Rule")]
        [Tooltip("If true: scenes ending with 'World' will NEVER load saved PlayerPrefs position (always use default spawn).")]
        public bool neverUseSavedPosInWorldScenes = true;

        [Header("Player")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
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

        private bool isTeleported = false;

        // “Scene default” resolved per scene (from Player_DefaultSpawn if present)
        private Vector3 _resolvedSceneDefaultPos;
        private Quaternion _resolvedSceneDefaultRot;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput != null && _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _hasAnimator = TryGetComponent(out _animator);

            AssignAnimationIDs();
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResolveSceneDefaultSpawn();
        }
#endif

        private void Start()
        {
            ResolveSceneDefaultSpawn();

#if ENABLE_INPUT_SYSTEM
            string sceneName = SceneManager.GetActiveScene().name;
#else
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
#endif
            string keyX = sceneName + "_LastTriggerX";
            string keyY = sceneName + "_LastTriggerY";
            string keyZ = sceneName + "_LastTriggerZ";

            bool hasSaved = PlayerPrefs.HasKey(keyX) &&
                PlayerPrefs.HasKey(keyY) &&
                PlayerPrefs.HasKey(keyZ);

            Debug.Log($"[TPC] DefaultSpawn resolved pos={_resolvedSceneDefaultPos} (scene='{sceneName}'");
            Debug.Log($"[TPC] Keys exist? {keyX}={PlayerPrefs.HasKey(keyX)} {keyY}={PlayerPrefs.HasKey(keyY)} {keyZ}={PlayerPrefs.HasKey(keyZ)} (willUseSaved={hasSaved})");
            Debug.Log($"[TPC] beforePos={transform.position}");

            if (hasSaved)
            {
                float x = PlayerPrefs.GetFloat(keyX);
                float y = PlayerPrefs.GetFloat(keyY);
                float z = PlayerPrefs.GetFloat(keyZ);

                TeleportTo(new Vector3(x, y, z), _resolvedSceneDefaultRot);
                Debug.Log($"[TPC] Loaded saved pos for {sceneName}: {transform.position}");
            }
            else
            {
                TeleportTo(_resolvedSceneDefaultPos, _resolvedSceneDefaultRot);
                isTeleported = false; // default spawn 不锁移动
                Debug.Log($"[TPC] Using DEFAULT spawn for {sceneName}: {_resolvedSceneDefaultPos}");
            }

            if (CinemachineCameraTarget != null)
                _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        }

        private void ResolveSceneDefaultSpawn()
        {
#if ENABLE_INPUT_SYSTEM
            var active = SceneManager.GetActiveScene();
            var roots = active.GetRootGameObjects();

            GameObject found = null;
            for (int i = 0; i < roots.Length; i++)
            {
                var t = roots[i].transform;
                var child = FindChildByName(t, defaultSpawnObjectName);
                if (child != null) { found = child.gameObject; break; }
            }

            if (found != null)
            {
                _resolvedSceneDefaultPos = found.transform.position;
                _resolvedSceneDefaultRot = found.transform.rotation;
                Debug.Log($"[TPC] DefaultSpawn FOUND in ActiveScene='{active.name}': '{found.name}' pos={_resolvedSceneDefaultPos}");
                return;
            }

            Debug.LogWarning($"[TPC] DefaultSpawn NOT found in ActiveScene='{active.name}'. Using current transform fallback. currentPos={transform.position}");
#endif
            _resolvedSceneDefaultPos = transform.position;
            _resolvedSceneDefaultRot = transform.rotation;
        }

        private Transform FindChildByName(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var r = FindChildByName(root.GetChild(i), name);
                if (r != null) return r;
            }
            return null;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();

            if (isTeleported && _input != null && _input.move != Vector2.zero)
                isTeleported = false;
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
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator) _animator.SetBool(_animIDGrounded, Grounded);
        }

        private void CameraRotation()
        {
            if (CinemachineCameraTarget == null) return;
            if (_input == null) return;

            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }

        private void Move()
        {
            if (isTeleported) return;
            if (_input == null || _controller == null) return;

            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else _speed = targetSpeed;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero && _mainCamera != null)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(
                targetDirection.normalized * (_speed * Time.deltaTime) +
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (_input == null) return;

            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator) _animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
                else if (_hasAnimator) _animator.SetBool(_animIDFreeFall, true);

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void TeleportTo(Vector3 pos, Quaternion rot)
        {
            if (_controller == null) _controller = GetComponent<CharacterController>();

            bool wasEnabled = false;
            if (_controller != null)
            {
                wasEnabled = _controller.enabled;
                _controller.enabled = false;
            }

            transform.SetPositionAndRotation(pos, rot);

            if (_controller != null) _controller.enabled = wasEnabled;

            isTeleported = true;
        }
    }
}