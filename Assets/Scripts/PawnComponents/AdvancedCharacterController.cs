using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AdvancedCharacterController : NetworkBehaviour
{
    #region - Variables -
    public DefaultInput DefaultInput;
    public CharacterController Controller;
    public Vector2 Input_Movement;
    private Vector2 _input_View;
    private Vector3 _newCameraRotation;
    private Vector3 _newCharacterRotation;
    private Vector3 _newMovementSpeed;
    private Vector3 _newMovementSpeedVelocity;
    private float _weaponHeadtargetsYOffset;
    private float _weaponLeftArmtargetsYOffset;

    [Header("References")]
    [SerializeField]
    private Transform _cameraHolder;
    public Animator CharacterAnimator;
    [SerializeField]
    private Transform _headTarget;
    [SerializeField]
    private Transform _weaponTarget;
    [SerializeField]
    private Transform _leftArmTarget;
    [SerializeField]
    private List<MultiAimConstraint> _aimConstraints;

    [Header("View Settings")]
    [SerializeField]
    private float _viewSensitivityX;
    [SerializeField]
    private float _viewSensitivityY;
    [SerializeField]
    private bool _viewXInverted;
    [SerializeField]
    private bool _viewYInverted;
    [SerializeField]
    private float _viewClampYMin;
    [SerializeField]
    private float _viewClampYMax;
    [SerializeField]
    private float _weaponTargetYClampYMin;
    [SerializeField]
    private float _weaponTargetYClampYMax;
    [SerializeField]
    private float _shoulderMovementMultiplier;

    [Header("Movement - Walk")]
    [SerializeField]
    private float _walkingForwardSpeed;
    [SerializeField]
    private float _walkingBackwardSpeed;
    [SerializeField]
    private float _walkingStrafeSpeed;

    [Header("Movement - Run")]
    [SerializeField]
    private float _runningForwardSpeed;
    [SerializeField]
    private float _runningStrafeSpeed;

    [Header("Movement - Settings")]
    [SerializeField]
    private bool _holdToRun;
    [SerializeField]
    private float _moveSpeedSmoothing;

    [Header("Jumping")]
    [SerializeField]
    private float _jumpingHeight;
    [SerializeField]
    private float _jumpingFalloff;
    [SerializeField]
    private float _fallingSmoothing;
    [SerializeField]
    private float _fallingSpeedEffector;
    
    [Header("Gravity")]
    [SerializeField]
    private float _gravityAmount;
    [SerializeField]
    private float _gravityMin;
    [SerializeField]
    private float _playerGravity;
    [SerializeField]
    private Vector3 _jumpingForce;
    [SerializeField]
    private Vector3 _jumpingForceVelocity;

    [Header("Stance")]
    public bool IsFortified;
    public bool IsRunning;
    [SerializeField]
    private CapsuleCollider _standardCollider;
    [SerializeField]
    private CapsuleCollider _fortifiedCollider;

    [Header("Weapon")]
    [SerializeField]
    public WeaponController Weapon;

    #endregion

    #region - Awake / Start / Update

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (Weapon)
        {
            Weapon.Initialise(this);
        }

        _weaponHeadtargetsYOffset = _weaponTarget.localPosition.y - _headTarget.localPosition.y;
        _weaponLeftArmtargetsYOffset = _leftArmTarget.localPosition.y - _weaponTarget.localPosition.y;

        IsFortified = false;

        _newCameraRotation = _cameraHolder.localRotation.eulerAngles;
        _newCharacterRotation = transform.localRotation.eulerAngles;

        Controller = GetComponent<CharacterController>();

        DefaultInput = new DefaultInput();

        DefaultInput.Character.Movement.performed += e => Input_Movement = e.ReadValue<Vector2>();
        DefaultInput.Character.View.performed += e => _input_View = e.ReadValue<Vector2>();
        DefaultInput.Character.Jump.performed += e => Jump();
        DefaultInput.Character.Fortify.performed += e => Fortify();
        DefaultInput.Character.Run.performed += e => ToggleRun();
        DefaultInput.Character.ReleaseRun.performed += e => StopRun();

        DefaultInput.Enable();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //Camera.main.enabled = false;

        _cameraHolder.GetComponentInChildren<Camera>().enabled = IsOwner;
        _cameraHolder.GetComponentInChildren<AudioListener>().enabled = IsOwner;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //private void Awake()
    //{
    //    if (_weapon)
    //    {
    //        _weapon.Initialise(this);
    //    }

    //    _weaponHeadtargetsYOffset = _weaponTarget.localPosition.y - _headTarget.localPosition.y;
    //    _weaponLeftArmtargetsYOffset = _leftArmTarget.localPosition.y - _weaponTarget.localPosition.y;

    //    IsFortified = false;

    //    _newCameraRotation = _cameraHolder.localRotation.eulerAngles;
    //    _newCharacterRotation = transform.localRotation.eulerAngles;

    //    Controller = GetComponent<CharacterController>();

    //    _defaultInput = new DefaultInput();

    //    _defaultInput.Character.Movement.performed += e => Input_Movement = e.ReadValue<Vector2>();
    //    _defaultInput.Character.View.performed += e => _input_View = e.ReadValue<Vector2>();
    //    _defaultInput.Character.Jump.performed += e => Jump();
    //    _defaultInput.Character.Fortify.performed += e => Fortify();
    //    _defaultInput.Character.Run.performed += e => ToggleRun();
    //    _defaultInput.Character.ReleaseRun.performed += e => StopRun();

    //    _defaultInput.Enable();
    //}

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        CalculateView();
        CalculateMovement();
        CalculateJump();
        SetStanceCollider();
    }

    private void OnDestroy()
    {
        DefaultInput.Disable();

        DefaultInput.Character.Movement.performed -= e => Input_Movement = e.ReadValue<Vector2>();
        DefaultInput.Character.View.performed -= e => _input_View = e.ReadValue<Vector2>();
        DefaultInput.Character.Jump.performed -= e => Jump();
        DefaultInput.Character.Fortify.performed -= e => Fortify();
        DefaultInput.Character.Run.performed -= e => ToggleRun();
        DefaultInput.Character.ReleaseRun.performed -= e => StopRun();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion

    #region - Basic Movement -

    private void CalculateView()
    {
        _newCharacterRotation.y += _viewSensitivityX * (_viewXInverted ? -1 : 1) * _input_View.x * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(_newCharacterRotation);

        var newTargetOffset = _viewSensitivityY * (_viewYInverted ? 1 : -1) * _input_View.y * Time.deltaTime;
        //_newCameraRotation.x += _viewSensitivityY * (_viewYInverted ? 1 : -1) * _input_View.y * Time.deltaTime;
        //_newCameraRotation.x = Mathf.Clamp(_newCameraRotation.x, _viewClampYMin, _viewClampYMax);

        _cameraHolder.localRotation = Quaternion.Euler(_newCameraRotation);
        _headTarget.SetLocalPositionAndRotation(new Vector3(_headTarget.localPosition.x, Mathf.Clamp(_headTarget.localPosition.y + newTargetOffset, _weaponTargetYClampYMin - _weaponHeadtargetsYOffset, _weaponTargetYClampYMax - _weaponHeadtargetsYOffset), _headTarget.localPosition.z), _headTarget.localRotation);
        _leftArmTarget.SetLocalPositionAndRotation(new Vector3(_leftArmTarget.localPosition.x, Mathf.Clamp(_leftArmTarget.localPosition.y - newTargetOffset * _shoulderMovementMultiplier, _weaponTargetYClampYMin + _weaponLeftArmtargetsYOffset, _weaponTargetYClampYMax + _weaponLeftArmtargetsYOffset), _leftArmTarget.localPosition.z), _leftArmTarget.localRotation);
        _weaponTarget.SetLocalPositionAndRotation(new Vector3(_weaponTarget.localPosition.x, Mathf.Clamp(_weaponTarget.localPosition.y + newTargetOffset, _weaponTargetYClampYMin, _weaponTargetYClampYMax), _weaponTarget.localPosition.z), _weaponTarget.localRotation);
    }

    private void CalculateMovement()
    {
        if(Input_Movement.y <= 0.2f)
        {
            IsRunning = false;
            CharacterAnimator.SetBool(ConstantValuesHolder.isRunning, false);
        }

        var verticalSpeed = _walkingForwardSpeed;
        var horicontalSpeed = _walkingStrafeSpeed;

        if (IsRunning)
        {
            verticalSpeed = _runningForwardSpeed;
            horicontalSpeed = _runningStrafeSpeed;
        }

        if (!Controller.isGrounded)
        {
            verticalSpeed *= _fallingSpeedEffector;
            horicontalSpeed *= _fallingSpeedEffector;
        }

        if (IsFortified)
        {
            verticalSpeed = 0;
            horicontalSpeed = 0;
        }

        _newMovementSpeed = Vector3.SmoothDamp(_newMovementSpeed, new Vector3(horicontalSpeed * Input_Movement.x * Time.deltaTime, 0, verticalSpeed * Input_Movement.y * Time.deltaTime), ref _newMovementSpeedVelocity, Controller.isGrounded ? _moveSpeedSmoothing : _fallingSmoothing);
        var movementSpeed = transform.TransformDirection(_newMovementSpeed);

        CharacterAnimator.SetInteger(ConstantValuesHolder.verticalMovementInputValue, Mathf.RoundToInt(Input_Movement.y));
        CharacterAnimator.SetInteger(ConstantValuesHolder.horizontalMovementInputValue, Mathf.RoundToInt(Input_Movement.x));

        if (_playerGravity > _gravityMin)
        {
            _playerGravity -= _gravityAmount * Time.deltaTime;
        }

        if(_playerGravity < -0.1f && Controller.isGrounded)
        {
            _playerGravity = -0.1f;
        }

        movementSpeed.y += _playerGravity;
        movementSpeed += _jumpingForce * Time.deltaTime;

        Controller.Move(movementSpeed);
    }

    #endregion

    #region - Jump -

    private void CalculateJump()
    {
        _jumpingForce = Vector3.SmoothDamp(_jumpingForce, Vector3.zero, ref _jumpingForceVelocity, _jumpingFalloff);
    }

    private void Jump()
    {
        if (!IsOwner)
        {
            return;
        }

        if (!Controller.isGrounded)
        {
            return;
        }

        if (IsFortified)
        {
            foreach (var constraint in _aimConstraints)
            {
                constraint.weight = 1;
            }
            IsFortified = false;
            CharacterAnimator.SetBool(ConstantValuesHolder.isInFortifieddStance, IsFortified);
            return;
        }
        CharacterAnimator.SetTrigger(ConstantValuesHolder.isJumpingTrigger);
        _jumpingForce = Vector3.up * _jumpingHeight;
        _playerGravity = 0;
    }

    #endregion

    #region - Stance -

    private void SetStanceCollider()
    {
        if(!IsFortified && Controller.height != _standardCollider.height)
        {
            Controller.height = _standardCollider.height;
            Controller.radius = _standardCollider.radius;
            Controller.center = _standardCollider.center;
        }
        else if(IsFortified && Controller.height != _fortifiedCollider.height)
        {
            Controller.height = _fortifiedCollider.height;
            Controller.radius = _fortifiedCollider.radius;
            Controller.center = _fortifiedCollider.center;
        }
    }

    private void Fortify()
    {
        if (!IsOwner)
        {
            return;
        }

        if (IsFortified)
        {
            foreach (var constraint in _aimConstraints)
            {
                constraint.weight = 1;
            }
            IsFortified = false;
            CharacterAnimator.SetBool(ConstantValuesHolder.isInFortifieddStance, IsFortified);
            return;
        }
        foreach (var constraint in _aimConstraints)
        {
            constraint.weight = 0;
        }
        IsFortified = true;
        CharacterAnimator.SetBool(ConstantValuesHolder.isInFortifieddStance, IsFortified);
    }

    #endregion

    #region - Running -

    private void ToggleRun()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input_Movement.y <= 0.2f)
        {
            IsRunning = false;
            CharacterAnimator.SetBool(ConstantValuesHolder.isRunning, IsRunning);
            return;
        }

        IsRunning = !IsRunning;
        CharacterAnimator.SetBool(ConstantValuesHolder.isRunning, IsRunning);
    }

    private void StopRun()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_holdToRun)
        {
            IsRunning = false;
            CharacterAnimator.SetBool(ConstantValuesHolder.isRunning, IsRunning);
        }
    }

    #endregion

    //#region - Visuals - 

    //private void OnApplicationFocus(bool focus)
    //{
    //    if (focus)
    //    {
    //        Cursor.lockState = CursorLockMode.Locked;
    //        Cursor.visible = false;
    //    }
    //    else
    //    {
    //        Cursor.lockState = CursorLockMode.None;
    //        Cursor.visible = true;
    //    }
    //}

    //#endregion
}
