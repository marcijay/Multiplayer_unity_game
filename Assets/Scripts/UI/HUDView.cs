using FishNet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDView : View
{
    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private TextMeshProUGUI armorText;

    [SerializeField]
    private TextMeshProUGUI fortifyText;

    [SerializeField]
    private TextMeshProUGUI currentAmmoText;

    [SerializeField]
    private TextMeshProUGUI reserveAmmoText;

    [SerializeField]
    private TextMeshProUGUI currentEventText;

    [SerializeField]
    private TextMeshProUGUI currentObjectiveText;

    [SerializeField]
    private Button _disconnectButton;

    [SerializeField]
    private Button _quitButton;

    [SerializeField]
    private GameObject _menu;

    private bool _isMenuOn;

    private Player _player;

    public override void Initialize()
    {
        _quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        _disconnectButton.onClick.AddListener(() =>
        {
            if (InstanceFinder.IsServer)
            {
                InstanceFinder.ServerManager.StopConnection(true);
            }
            else if (InstanceFinder.IsClient)
            {
                InstanceFinder.ClientManager.StopConnection();
            }
        });

        _isMenuOn = false;

        _player = Player.LocalInstance;

        base.Initialize();
    }

    private void Update()
    {
        if (_player == null || _player.controlledPawn == null || _player.controlledPawn.Controller == null || _player.controlledPawn.Controller.Weapon == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isMenuOn = !_isMenuOn;
            if (_isMenuOn)
            {
                _player.controlledPawn.Controller.DefaultInput.Disable();
                _player.controlledPawn.Controller.Weapon.WeaponInput.Disable();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _player.controlledPawn.Controller.DefaultInput.Enable();
                _player.controlledPawn.Controller.Weapon.WeaponInput.Enable();
            }
            _menu.SetActive(_isMenuOn);
        }

        healthText.text = $"HP: {_player.controlledPawn.Health} / {_player.controlledPawn.MaxHealth}";
        armorText.text = $"Armor: {_player.controlledPawn.Armour}";
        fortifyText.enabled = _player.controlledPawn.Controller.IsFortified;
        currentAmmoText.text = $"{_player.controlledPawn.Controller.Weapon.CurrentAmmuniotion} / {_player.controlledPawn.Controller.Weapon.MagazineSize} ";
        reserveAmmoText.text = $"{_player.controlledPawn.Controller.Weapon.AmmunitionReserve}";

        // TODO - current event text
        currentEventText.text = _player.CurrentEventText;

        if(ObjectiveManager.Instance != null)
        {
            currentObjectiveText.text = ObjectiveManager.Instance.objectiveText;
        }
        if(WaveManager.Instance != null)
        {
            currentObjectiveText.text = $"Survive enemy swarms. Wave {WaveManager.Instance.Wave}/{WaveManager.Instance.MaxWave}. Enemies left: {WaveManager.Instance.CurrentEnemies}";
        }
    }
}
