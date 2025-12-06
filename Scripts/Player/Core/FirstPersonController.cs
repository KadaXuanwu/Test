using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {
    [SerializeField] private FirstPersonControllerConfig config;

    [Header("Input")]
    [Tooltip("Input configuration. If null, uses InputSystemCharacterInputConfig by default.")]
    [SerializeField] private ScriptableObject inputConfig;

    [Header("Modifiers")]
    [Tooltip("Modifier configs in execution order. First = runs first.")]
    [SerializeField] private List<ScriptableObject> modifierConfigs = new List<ScriptableObject>();

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
    private ModifierStateContainer _stateContainer = new ModifierStateContainer();

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
            InitializeInput();
        }

        InitializeModifiers();
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
    /// Manually inject a custom input handler. Call before Start() or use inputConfig field instead.
    /// </summary>
    public void Initialize(ICharacterInput input) {
        Input = input;
    }

    private void InitializeInput() {
        if (inputConfig != null) {
            if (inputConfig is ICharacterInputConfig characterInputConfig) {
                Input = characterInputConfig.CreateInput();
            }
            else {
                Debug.LogError($"[{nameof(FirstPersonController)}] {inputConfig.name} does not implement ICharacterInputConfig", this);
            }
        }

        // Fallback to default if no config assigned or creation failed
        if (Input == null) {
            InputSystemCharacterInput defaultInput = new InputSystemCharacterInput();
            defaultInput.Initialize(InputManager.Instance.Actions.Player);
            Input = defaultInput;
            Debug.LogWarning($"[{nameof(FirstPersonController)}] No input config assigned, using default InputSystemCharacterInput", this);
        }
    }

    private void InitializeModifiers() {
        foreach (ScriptableObject configObj in modifierConfigs) {
            if (configObj == null) {
                continue;
            }

            if (configObj is IModifierConfig modifierConfig) {
                IMovementModifier modifier = modifierConfig.CreateModifier();
                modifier.OnInitialize(this);
                _modifiers.Add(modifier);
            }
            else {
                Debug.LogWarning($"[{nameof(FirstPersonController)}] {configObj.name} does not implement IModifierConfig", this);
            }
        }
    }

    #endregion

    #region Modifier Management

    public T AddModifier<T>(T modifier) where T : class, IMovementModifier {
        if (_modifiers.Contains(modifier)) {
            Debug.LogWarning($"[{nameof(FirstPersonController)}] Modifier {modifier.GetType().Name} already added.");
            return modifier;
        }

        modifier.OnInitialize(this);
        _modifiers.Add(modifier);
        return modifier;
    }

    public void InsertModifier(IMovementModifier modifier, int index) {
        if (_modifiers.Contains(modifier)) {
            Debug.LogWarning($"[{nameof(FirstPersonController)}] Modifier {modifier.GetType().Name} already added.");
            return;
        }

        modifier.OnInitialize(this);
        _modifiers.Insert(Mathf.Clamp(index, 0, _modifiers.Count), modifier);
    }

    public bool RemoveModifier(IMovementModifier modifier) {
        if (_modifiers.Remove(modifier)) {
            modifier.OnRemove();
            return true;
        }
        return false;
    }

    public bool RemoveModifier<T>() where T : class, IMovementModifier {
        T modifier = GetModifier<T>();
        if (modifier != null) {
            return RemoveModifier(modifier);
        }
        return false;
    }

    public T GetModifier<T>() where T : class, IMovementModifier {
        for (int i = 0; i < _modifiers.Count; i++) {
            if (_modifiers[i] is T typed) {
                return typed;
            }
        }
        return null;
    }

    public bool TryGetModifier<T>(out T modifier) where T : class, IMovementModifier {
        modifier = GetModifier<T>();
        return modifier != null;
    }

    public bool HasModifier<T>() where T : class, IMovementModifier {
        for (int i = 0; i < _modifiers.Count; i++) {
            if (_modifiers[i] is T) {
                return true;
            }
        }
        return false;
    }

    public void GetModifiers<T>(List<T> results) where T : class, IMovementModifier {
        results.Clear();
        for (int i = 0; i < _modifiers.Count; i++) {
            if (_modifiers[i] is T typed) {
                results.Add(typed);
            }
        }
    }

    public IReadOnlyList<IMovementModifier> GetAllModifiers() => _modifiers;

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

        _stateContainer.ResetAll();

        _currentContext = new MovementContext {
            MoveInput = new Vector3(rawInput.x, 0f, rawInput.y),
            WorldMoveDirection = worldDirection,
            Velocity = _velocity,
            Position = transform.position,
            IsGrounded = IsGrounded,
            WasGroundedLastFrame = _groundedLastFrame,
            GroundInfo = CurrentGroundInfo,
            DeltaTime = Time.deltaTime,
            PreviousYVelocity = _yVelocityLastFrame,
            PreventMovement = false,
            PreventGravity = false,
            SpeedMultiplier = 1f,
            State = _stateContainer
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

        if (Physics.Raycast(centerPos, Vector3.down, out RaycastHit centerHit, config.GroundRaycastDistance, config.GroundLayers)) {
            info.OnGround = true;
            float angle = Vector3.Angle(centerHit.normal, Vector3.up);
            if (angle < flattestAngle) {
                flattestAngle = angle;
                flattestNormal = centerHit.normal;
                groundCollider = centerHit.collider;
            }
        }

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

    public float GetLastGroundedTimestamp() => _lastGroundedTimestamp;

    public float GetTimeSinceGrounded() => Time.time - _lastGroundedTimestamp;

    #endregion

    #region Public API

    public void SetVelocity(Vector3 velocity) {
        _velocity = velocity;
    }

    public void AddVelocity(Vector3 velocity) {
        _velocity += velocity;
    }

    public void SetHorizontalVelocity(Vector3 velocity) {
        _velocity.x = velocity.x;
        _velocity.z = velocity.z;
    }

    public void SetVerticalVelocity(float yVelocity) {
        _velocity.y = yVelocity;
    }

    public void SetRotation(Vector3 rotation) {
        _currentRotation = rotation;
        _currentRotation.x = Mathf.Clamp(_currentRotation.x, -config.ClampAngle, config.ClampAngle);
    }

    public void AddRotation(Vector3 delta) {
        _currentRotation += delta;
        _currentRotation.x = Mathf.Clamp(_currentRotation.x, -config.ClampAngle, config.ClampAngle);
    }

    public void ResetVelocity() {
        _velocity = Vector3.zero;
        _yVelocityLastFrame = 0f;
    }

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

    public void SetControllerEnabled(bool enabled) {
        _controller.enabled = enabled;
    }

    public void SetCursorLocked(bool locked) {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    #endregion

    #region Queries

    public bool IsCrouching => _stateContainer.TryGet<CrouchState>(out CrouchState cs) && cs.IsCrouching;
    public bool IsRunning => _stateContainer.TryGet<RunState>(out RunState rs) && rs.IsRunning;
    public bool IsOnSlope => CurrentGroundInfo.OnSlope;
    public float HorizontalSpeed => new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
    public float VerticalSpeed => _velocity.y;

    /// <summary>
    /// Access the state container to query custom modifier states.
    /// </summary>
    public ModifierStateContainer ModifierState => _stateContainer;

    #endregion

    #region Editor

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if (config == null) {
            return;
        }

        Vector3 centerPos = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position;

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(centerPos, centerPos + Vector3.down * config.GroundRaycastDistance);

        for (int i = 0; i < config.GroundCheckCount; i++) {
            float angle = i * Mathf.PI * 2f / config.GroundCheckCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * config.GroundCheckRadius;
            Vector3 rayOrigin = centerPos + offset;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * config.GroundRaycastDistance);
        }

        Gizmos.color = Color.yellow;
        DrawGizmoCircle(centerPos, config.GroundCheckRadius, 16);

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
