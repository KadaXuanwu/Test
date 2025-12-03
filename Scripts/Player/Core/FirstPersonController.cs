using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {
    [SerializeField] private FirstPersonControllerConfig config;

    [Header("References")]
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform playerVisuals;

    // Public state
    public bool IsGrounded { get; private set; }
    public Vector3 Velocity => _velocity;
    public float VelocityMagnitude => _velocity.magnitude;
    public GroundInfo CurrentGroundInfo { get; private set; }
    public MovementContext CurrentContext => _currentContext;

    // Dependencies
    public FirstPersonControllerConfig Config => config;
    public ICharacterInput Input { get; private set; }
    public CharacterEvents Events { get; } = new CharacterEvents();
    public CharacterController CharacterController => _controller;
    public Transform CameraHolder => cameraHolder;
    public Transform PlayerVisuals => playerVisuals;

    // Constants
    private const float BaseLookSensitivity = 0.008f;
    private const float GravityValue = -9.81f;
    private const float PerpendicularAngle = 90f;
    private const float VelocityImpactThreshold = 1f;

    // Internal state
    private CharacterController _controller;
    private List<IMovementModifier> _modifiers = new List<IMovementModifier>();
    private Vector3 _velocity;
    private Vector3 _currentRotation;
    private float _lastFrameDeltaTime;
    private MovementContext _currentContext;

    // Frame state tracking
    private bool _groundedLastFrame;
    private float _yVelocityLastFrame;
    private float _lastGroundedTimestamp;

    #region Lifecycle

    private void Awake() {
        _controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (config == null) {
            Debug.LogError($"[{nameof(FirstPersonController)}] Config is not assigned!", this);
            enabled = false;
        }
    }

    private void Start() {
        if (Input == null) {
            InitializeDefaultInput();
        }
    }

    private void Update() {
        _lastFrameDeltaTime = Time.deltaTime;
        UpdateGroundedState();
        HandleRotation();

        if (_lastFrameDeltaTime < Time.fixedDeltaTime) {
            ExecuteMovement();
        }
    }

    private void FixedUpdate() {
        if (_lastFrameDeltaTime >= Time.fixedDeltaTime) {
            ExecuteMovement();
        }
    }

    private void OnDestroy() {
        for (int i = _modifiers.Count - 1; i >= 0; i--) {
            _modifiers[i].OnRemove();
        }
        _modifiers.Clear();
        Events.ClearAllSubscribers();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize with custom input provider.
    /// Call before Start() or use default InputSystem input.
    /// </summary>
    public void Initialize(ICharacterInput input) {
        Input = input;
    }

    private void InitializeDefaultInput() {
        InputSystemCharacterInput defaultInput = new InputSystemCharacterInput();
        defaultInput.Initialize(InputManager.Instance.Actions.Player);
        Input = defaultInput;
    }

    #endregion

    #region Modifier Management

    /// <summary>
    /// Add a movement modifier. Automatically sorted by priority.
    /// </summary>
    public T AddModifier<T>(T modifier) where T : class, IMovementModifier {
        if (_modifiers.Contains(modifier)) {
            Debug.LogWarning($"[{nameof(FirstPersonController)}] Modifier {modifier.GetType().Name} already added.");
            return modifier;
        }

        modifier.OnInitialize(this);
        _modifiers.Add(modifier);
        SortModifiers();
        return modifier;
    }

    /// <summary>
    /// Remove a movement modifier.
    /// </summary>
    public bool RemoveModifier(IMovementModifier modifier) {
        if (_modifiers.Remove(modifier)) {
            modifier.OnRemove();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove a modifier by type.
    /// </summary>
    public bool RemoveModifier<T>() where T : class, IMovementModifier {
        T modifier = GetModifier<T>();
        if (modifier != null) {
            return RemoveModifier(modifier);
        }
        return false;
    }

    /// <summary>
    /// Get a modifier by type.
    /// </summary>
    public T GetModifier<T>() where T : class, IMovementModifier {
        for (int i = 0; i < _modifiers.Count; i++) {
            if (_modifiers[i] is T typed) {
                return typed;
            }
        }
        return null;
    }

    /// <summary>
    /// Try to get a modifier by type.
    /// </summary>
    public bool TryGetModifier<T>(out T modifier) where T : class, IMovementModifier {
        modifier = GetModifier<T>();
        return modifier != null;
    }

    /// <summary>
    /// Check if a modifier type exists.
    /// </summary>
    public bool HasModifier<T>() where T : class, IMovementModifier {
        for (int i = 0; i < _modifiers.Count; i++) {
            if (_modifiers[i] is T) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get all modifiers of a specific type.
    /// </summary>
    public void GetModifiers<T>(List<T> results) where T : class, IMovementModifier {
        results.Clear();
        for (int i = 0; i < _modifiers.Count; i++) {
            if (_modifiers[i] is T typed) {
                results.Add(typed);
            }
        }
    }

    /// <summary>
    /// Get read-only access to all modifiers.
    /// </summary>
    public IReadOnlyList<IMovementModifier> GetAllModifiers() => _modifiers;

    private void SortModifiers() {
        _modifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    #endregion

    #region Movement Execution

    private void ExecuteMovement() {
        CurrentGroundInfo = CalculateGroundInfo();
        BuildMovementContext();
        ProcessModifiers();
        ApplyGravity();
        ApplyMovement();
        UpdateFrameState();
    }

    private void BuildMovementContext() {
        Vector2 rawInput = Input?.MoveInput ?? Vector2.zero;
        Vector3 worldDirection = transform.forward * rawInput.y + transform.right * rawInput.x;
        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude > 0f) {
            worldDirection.Normalize();
        }

        _currentContext = new MovementContext {
            // Input
            MoveInput = new Vector3(rawInput.x, 0f, rawInput.y),
            WorldMoveDirection = worldDirection,

            // State
            Velocity = _velocity,
            Position = transform.position,
            IsGrounded = IsGrounded,
            WasGroundedLastFrame = _groundedLastFrame,
            GroundInfo = CurrentGroundInfo,
            DeltaTime = Time.deltaTime,
            PreviousYVelocity = _yVelocityLastFrame,

            // Modifier flags (set by modifiers)
            IsCrouching = false,
            IsRunning = false,

            // Control flags
            PreventMovement = false,
            PreventGravity = false,
            ConsumedJump = false
        };
    }

    private void ProcessModifiers() {
        for (int i = 0; i < _modifiers.Count; i++) {
            IMovementModifier modifier = _modifiers[i];
            if (modifier.IsActive) {
                modifier.ProcessMovement(ref _currentContext);
            }
        }

        _velocity = _currentContext.Velocity;
    }

    private void ApplyGravity() {
        if (_currentContext.PreventGravity) {
            return;
        }

        _velocity.y += GravityValue * config.GravityMultiplier * Time.deltaTime;

        if (IsGrounded && _velocity.y < 0f) {
            _velocity.y = -config.GroundedSnapVelocity;
        }
    }

    private void ApplyMovement() {
        if (!_controller.enabled || _currentContext.PreventMovement) {
            return;
        }

        Vector3 velocityBefore = _velocity;
        CollisionFlags flags = _controller.Move(_velocity * Time.deltaTime);
        bool hadImpact = false;

        if ((flags & CollisionFlags.Sides) != 0) {
            _velocity.x = _controller.velocity.x;
            _velocity.z = _controller.velocity.z;
            hadImpact = true;
        }

        if ((flags & CollisionFlags.Above) != 0) {
            _velocity.y = _controller.velocity.y;
            hadImpact = true;
        }

        if (hadImpact) {
            float impactMagnitude = (velocityBefore - _velocity).magnitude;
            if (impactMagnitude > VelocityImpactThreshold) {
                Events.InvokeVelocityImpact(velocityBefore, _velocity);
            }
        }
    }

    #endregion

    #region Rotation

    private void HandleRotation() {
        if (_lastFrameDeltaTime == 0f || Input == null) {
            return;
        }

        Vector2 lookInput = Input.LookInput;
        float sensitivityFactor = config.MouseSensitivity * BaseLookSensitivity * Time.deltaTime / _lastFrameDeltaTime;

        _currentRotation.x = Mathf.Clamp(
            _currentRotation.x + lookInput.y * sensitivityFactor,
            -config.ClampAngle,
            config.ClampAngle
        );
        _currentRotation.y += lookInput.x * sensitivityFactor;

        if (cameraHolder != null) {
            cameraHolder.localRotation = Quaternion.Euler(-_currentRotation.x, 0f, 0f);
        }

        transform.rotation = Quaternion.Euler(0f, _currentRotation.y, 0f);

        if (playerVisuals != null) {
            playerVisuals.rotation = Quaternion.Euler(0f, _currentRotation.y, 0f);
        }
    }

    /// <summary>
    /// Get the current rotation angles.
    /// X = vertical (pitch), Y = horizontal (yaw).
    /// </summary>
    public Vector3 GetRotation() => _currentRotation;

    #endregion

    #region Ground Detection

    private void UpdateGroundedState() {
        bool wasGrounded = IsGrounded;
        IsGrounded = _controller.isGrounded;

        if (wasGrounded != IsGrounded) {
            Events.InvokeGroundedChanged(IsGrounded);
        }
    }

    private GroundInfo CalculateGroundInfo() {
        Vector3 centerPos = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position;
        GroundInfo info = GroundInfo.None;

        float flattestAngle = PerpendicularAngle;
        Vector3 flattestNormal = Vector3.up;
        Collider groundCollider = null;

        // Center raycast
        if (Physics.Raycast(centerPos, Vector3.down, out RaycastHit centerHit, config.GroundRaycastDistance, config.GroundLayers)) {
            info.OnGround = true;
            float angle = Vector3.Angle(centerHit.normal, Vector3.up);
            if (angle < flattestAngle) {
                flattestAngle = angle;
                flattestNormal = centerHit.normal;
                groundCollider = centerHit.collider;
            }
        }

        // Radial raycasts
        for (int i = 0; i < config.GroundCheckCount; i++) {
            float angle = i * Mathf.PI * 2f / config.GroundCheckCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * config.GroundCheckRadius;
            Vector3 rayOrigin = centerPos + offset;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, config.GroundRaycastDistance, config.GroundLayers)) {
                info.OnGround = true;
                float hitAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (hitAngle < flattestAngle) {
                    flattestAngle = hitAngle;
                    flattestNormal = hit.normal;
                    groundCollider = hit.collider;
                }
            }
        }

        info.SlopeAngle = flattestAngle;
        info.SlopeNormal = flattestNormal;
        info.OnSlope = flattestAngle >= _controller.slopeLimit;
        info.GroundCollider = groundCollider;

        if (info.OnSlope) {
            Vector3 downwardForce = new Vector3(0f, config.SlopeSlideProjectionMagnitude, 0f);
            info.SlideDirection = Vector3.ProjectOnPlane(downwardForce, flattestNormal).normalized;
        }

        // Moving platform velocity
        if (groundCollider != null && groundCollider.attachedRigidbody != null) {
            info.GroundVelocity = groundCollider.attachedRigidbody.GetPointVelocity(centerPos);
        }

        return info;
    }

    #endregion

    #region Frame State

    private void UpdateFrameState() {
        if (IsGrounded) {
            _lastGroundedTimestamp = Time.time;
        }

        _groundedLastFrame = IsGrounded;
        _yVelocityLastFrame = _velocity.y;
    }

    /// <summary>
    /// Timestamp when the player was last grounded.
    /// Used for coyote time calculations.
    /// </summary>
    public float GetLastGroundedTimestamp() => _lastGroundedTimestamp;

    /// <summary>
    /// Time since the player was last grounded.
    /// </summary>
    public float GetTimeSinceGrounded() => Time.time - _lastGroundedTimestamp;

    #endregion

    #region Public API

    /// <summary>
    /// Directly set the velocity.
    /// </summary>
    public void SetVelocity(Vector3 velocity) {
        _velocity = velocity;
    }

    /// <summary>
    /// Add to the current velocity.
    /// </summary>
    public void AddVelocity(Vector3 velocity) {
        _velocity += velocity;
    }

    /// <summary>
    /// Set horizontal velocity only, preserving Y.
    /// </summary>
    public void SetHorizontalVelocity(Vector3 velocity) {
        _velocity.x = velocity.x;
        _velocity.z = velocity.z;
    }

    /// <summary>
    /// Set vertical velocity only, preserving XZ.
    /// </summary>
    public void SetVerticalVelocity(float yVelocity) {
        _velocity.y = yVelocity;
    }

    /// <summary>
    /// Set the look rotation.
    /// X = vertical (pitch), Y = horizontal (yaw).
    /// </summary>
    public void SetRotation(Vector3 rotation) {
        _currentRotation = rotation;
        _currentRotation.x = Mathf.Clamp(_currentRotation.x, -config.ClampAngle, config.ClampAngle);
    }

    /// <summary>
    /// Add to the current rotation.
    /// </summary>
    public void AddRotation(Vector3 delta) {
        _currentRotation += delta;
        _currentRotation.x = Mathf.Clamp(_currentRotation.x, -config.ClampAngle, config.ClampAngle);
    }

    /// <summary>
    /// Reset velocity to zero.
    /// </summary>
    public void ResetVelocity() {
        _velocity = Vector3.zero;
        _yVelocityLastFrame = 0f;
    }

    /// <summary>
    /// Teleport to a position with optional rotation and velocity reset.
    /// </summary>
    public void Teleport(Vector3 position, Vector3? rotation = null, bool resetVelocity = true) {
        bool wasEnabled = _controller.enabled;
        _controller.enabled = false;

        transform.position = position;

        if (rotation.HasValue) {
            SetRotation(rotation.Value);
        }

        if (resetVelocity) {
            ResetVelocity();
        }

        _controller.enabled = wasEnabled;
        UpdateGroundedState();

        Events.InvokeTeleported(position);
    }

    /// <summary>
    /// Enable or disable the character controller.
    /// </summary>
    public void SetControllerEnabled(bool enabled) {
        _controller.enabled = enabled;
    }

    /// <summary>
    /// Lock or unlock the cursor.
    /// </summary>
    public void SetCursorLocked(bool locked) {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    #endregion

    #region Queries

    /// <summary>
    /// Check if the player is currently crouching.
    /// Requires CrouchModifier to be active.
    /// </summary>
    public bool IsCrouching => _currentContext.IsCrouching;

    /// <summary>
    /// Check if the player is currently running.
    /// Requires RunModifier to be active.
    /// </summary>
    public bool IsRunning => _currentContext.IsRunning;

    /// <summary>
    /// Check if the player is on a slope that causes sliding.
    /// </summary>
    public bool IsOnSlope => CurrentGroundInfo.OnSlope;

    /// <summary>
    /// Get the current horizontal speed.
    /// </summary>
    public float HorizontalSpeed => new Vector3(_velocity.x, 0f, _velocity.z).magnitude;

    /// <summary>
    /// Get the current vertical speed (positive = up).
    /// </summary>
    public float VerticalSpeed => _velocity.y;

    #endregion

    #region Editor

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if (config == null) {
            return;
        }

        Vector3 centerPos = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position;

        // Ground check visualization
        Gizmos.color = IsGrounded ? Color.green : Color.red;

        // Center ray
        Gizmos.DrawLine(centerPos, centerPos + Vector3.down * config.GroundRaycastDistance);

        // Radial rays
        for (int i = 0; i < config.GroundCheckCount; i++) {
            float angle = i * Mathf.PI * 2f / config.GroundCheckCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * config.GroundCheckRadius;
            Vector3 rayOrigin = centerPos + offset;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * config.GroundRaycastDistance);
        }

        // Ground check radius circle
        Gizmos.color = Color.yellow;
        DrawGizmoCircle(centerPos, config.GroundCheckRadius, 16);

        // Velocity visualization
        if (Application.isPlaying) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _velocity * 0.5f);
        }
    }

    private void DrawGizmoCircle(Vector3 center, float radius, int segments) {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++) {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif

    #endregion
}