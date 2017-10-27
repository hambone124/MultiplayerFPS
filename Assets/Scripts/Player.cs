using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour {

    [SyncVar]
    private bool _isDead = false;
    public bool IsDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField]
    private int maxHealth = 100;
    [SyncVar]
    private int currentHealth;
    [SerializeField]
    private GameObject deathEffect;
    [SerializeField]
    private GameObject spawnEffect;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectOnDeath;

    private bool isFirstSetup = true;

    [ClientRpc]
    public void RpcTakeDamage(int _amount)
    {
        if (IsDead)
        {
            return;
        }
        currentHealth -= _amount;
        Debug.Log(transform.name + " has " + currentHealth + " health.");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }
        for (int i = 0; i < disableGameObjectOnDeath.Length; i++)
        {
            disableGameObjectOnDeath[i].SetActive(false);
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = true;
        }
        Debug.Log(transform.name + " is DEAD!");

        GameObject _gfxIns = Instantiate<GameObject>(deathEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 3f);

        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
        Debug.Log(transform.name + " has respawned.");
        SetupPlayer();
    }

    private void Update()
    {
        if (!isLocalPlayer) { return; }
        if (Input.GetKey(KeyCode.K))
        {
            RpcTakeDamage(999);
        }
    }

    public void SetupPlayer()
    {
        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        }
        CmdBroadcastNewPlayerSetup();
    }

    [Command]
    private void CmdBroadcastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if (isFirstSetup)
        {
            wasEnabled = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDeath[i].enabled;
            }
            isFirstSetup = false;
        }
        SetDefaults();
    }

    public void SetDefaults()
    {
        IsDead = false;
        currentHealth = maxHealth;
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }
        for (int i = 0; i < disableGameObjectOnDeath.Length; i++)
        {
            disableGameObjectOnDeath[i].SetActive(true);
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = true;
        }

        GameObject _gfxIns = Instantiate<GameObject>(spawnEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 3f);

    }
}
