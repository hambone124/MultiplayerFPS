using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

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
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

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
        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = true;
        }
        Debug.Log(transform.name + " is DEAD!");

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);
        SetDefaults();
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
        Debug.Log(transform.name + " has respawned."); 
    }    

    private void Update()
    {
        if (!isLocalPlayer) { return; }
        if (Input.GetKey(KeyCode.K))
        {
            RpcTakeDamage(999);
        }
    }

    public void Setup()
    {
        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
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
        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = true;
        }
    }
}
