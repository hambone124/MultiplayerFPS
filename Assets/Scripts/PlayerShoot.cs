using System;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerShoot : NetworkBehaviour {

    private const string PLAYER_TAG = "Player";

    [SerializeField]
    private Camera cam;
    public PlayerWeapon weapon;
    [SerializeField]
    private LayerMask mask;

    private void Start() {
        if (cam == null)
        {
            Debug.Log("PlayerShoot: No camera referenced.");
            this.enabled = false;
        }
    }

    private void Update() {
        if (Input.GetButton("Fire1"))
        {
            Debug.Log("Fire button pressed.");
            Shoot();
        }
    }

    [Client]
    private void Shoot()
    {
        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, weapon.range, mask))
        {
            if (_hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(_hit.collider.name, weapon.damage);
            }
        }
    }

    [Command]
    private void CmdPlayerShot(string _playerID, int _damage)
    {
        Debug.Log(_playerID + " has been shot.");
        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }
}
