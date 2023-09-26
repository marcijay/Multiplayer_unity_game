using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : NetworkBehaviour
{
    private AdvancedCharacterController _controller;
    public WeaponInput WeaponInput;
    private bool _isInitialized;
    private bool _reloading;
    private bool _ready;
    private bool _shooting;
    private bool _requestingReload;

    [Header("Settings")] public int Damage;
    public int AmmunitionReserve;
    public int MagazineSize;
    public int CurrentAmmuniotion;
    [SerializeField] private float _timeBetweenShots;
    [SerializeField] private float _range;
    [SerializeField] private float _reloadTime;
    [SerializeField] private float _movingSpreadModifier;
    [SerializeField] private float _spreadAmount;

    [Header("References")] [SerializeField]
    private Camera _camera;

    [SerializeField] private GameObject _impactEffect;
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Pawn _pawn;

    public void Initialise(AdvancedCharacterController controller)
    {
        _controller = controller;
        _isInitialized = true;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        CurrentAmmuniotion = MagazineSize;
        _ready = true;
        _reloading = false;

        WeaponInput = new WeaponInput();

        WeaponInput.Weapon.Shoot.performed += e => SetShot();
        WeaponInput.Weapon.Reload.performed += e => RequestReload();

        WeaponInput.Enable();
    }

    //private void Awake()
    //{
    //    _currentAmmuniotion = _magazineSize;
    //    _ready = true;
    //    _reloading = false;

    //    _weaponInput = new WeaponInput();

    //    _weaponInput.Weapon.Shoot.performed += e => SetShot();
    //    _weaponInput.Weapon.Reload.performed += e => RequestReload();

    //    _weaponInput.Enable();
    //}

    private void OnDestroy()
    {
        WeaponInput.Disable();

        WeaponInput.Weapon.Shoot.performed -= e => SetShot();
        WeaponInput.Weapon.Reload.performed -= e => RequestReload();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (!_isInitialized)
        {
            return;
        }

        CheckConditions();
    }

    private void CheckConditions()
    {
        if (_requestingReload && CurrentAmmuniotion < MagazineSize && !_reloading && AmmunitionReserve > 0 &&
            !_controller.IsRunning && !_controller.IsFortified && _controller.Controller.isGrounded &&
            _controller.Input_Movement == Vector2.zero)
        {
            Reload();
        }
        else
        {
            _requestingReload = false;
        }

        if (_ready && _shooting && !_reloading && CurrentAmmuniotion > 0 && !_controller.IsRunning &&
            !_controller.IsFortified && _controller.Controller.isGrounded)
        {
            Shoot();
        }
        else
        {
            _shooting = false;
        }
    }

    private void Reload()
    {
        _reloading = true;
        _requestingReload = false;
        _controller.CharacterAnimator.SetTrigger(ConstantValuesHolder.reloadTrigger);
        Invoke(nameof(EndReload), _reloadTime);
    }

    private void EndReload()
    {
        AmmunitionReserve -= (MagazineSize - CurrentAmmuniotion);
        CurrentAmmuniotion = MagazineSize;
        _reloading = false;
    }

    private void Shoot()
    {
        _controller.CharacterAnimator.SetTrigger(ConstantValuesHolder.fireTrigger);

        _ready = false;
        _shooting = false;

        float spreadValue = _controller.Input_Movement == Vector2.zero
            ? _movingSpreadModifier * _spreadAmount
            : _spreadAmount;
        float xDeviation = Random.Range(-spreadValue, spreadValue);
        float yDeviation = Random.Range(-spreadValue, spreadValue);

        Vector3 shotDirection = _camera.transform.forward + new Vector3(xDeviation, yDeviation, 0);

        _muzzleFlash.Play();
        _audioSource.Play();

        Physics.Raycast(_camera.transform.position, shotDirection, out RaycastHit hit, _range);
        ServerShoot(_camera.transform.position, shotDirection, _pawn.controllingPlayer);

        if(hit.point != Vector3.zero)
        {
            GameObject impact = Instantiate(_impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 1f);
        }

        //if (Physics.Raycast(_camera.transform.position, shotDirection, out _hit, _range))
        //{
        //    //Debug.Log(hit.transform.tag);
        //    IEnemy enemy = _hit.transform.GetComponentInParent<IEnemy>();

        //    if (enemy != null)
        //    {
        //        enemy.TakeDamage(Damage);
        //    }

        //    GameObject impact = Instantiate(_impactEffect, _hit.point, Quaternion.LookRotation(_hit.normal));
        //    Destroy(impact, 1f);
        //}

        CurrentAmmuniotion--;

        Invoke(nameof(ReadyReset), _timeBetweenShots);
    }

    [ServerRpc]
    private void ServerShoot(Vector3 firePointPosition, Vector3 firePointDirection, Player player)
    {
        if (Physics.Raycast(firePointPosition, firePointDirection, out RaycastHit hit, _range))
        {
            //Debug.Log(hit.transform.tag);
            AbstractEnemy enemy = hit.transform.GetComponentInParent<AbstractEnemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(Damage, player);
            }
        }
    }

    private void ReadyReset()
    {
        _ready = true;
    }

    private void SetShot()
    {
        if (!IsOwner)
        {
            return;
        }

        _shooting = true;
    }

    private void RequestReload()
    {
        if (!IsOwner)
        {
            return;
        }

        _requestingReload = true;
    }
}