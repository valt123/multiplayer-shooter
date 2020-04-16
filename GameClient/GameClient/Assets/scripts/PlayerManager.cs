using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;

    public float health;
    public float maxHealth;

    public int kills;
    public int deaths;
    public bool isDead = false;

    public Transform tommyGun;
    public int ammoCapacity = 50;
    public int maxAmmoCapacity = 50;
    public GameObject magazine;
    public Animator boltAnimator;

    public AudioClip[] gunShotSounds;
    public AudioClip[] playerHitSound;
    public AudioClip[] movementSounds;
    public AudioSource shootOriginAudioSource;
    public AudioSource playerAudioSource;

    public MeshRenderer[] model;

    public GameObject shootOrigin;
    public GameObject nameText;
    public Slider healthSlider;
    public Transform cameraTransform;
    public Vector3 velocity;
    public bool isGrounded;

    public GameObject spectatorPrefab;
    public GameObject spectator;

    public GameObject playerCorpsePrefab;
    private GameObject playerCorpse;

    public void Initialize(int _id, string _username, int _kills, int _deaths)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        kills = _kills;
        deaths = _deaths;

        playerAudioSource.clip = movementSounds[0];

        if (!IsLocalPlayer())
        {
            TextMesh _text = nameText.GetComponent<TextMesh>();
            _text.text = username;
        }

        UIManager.AmmoCapacity(ammoCapacity.ToString());
    }

    private void Update()
    {
        if (IsLocalPlayer())
        {
            UIManager.DamageOverlay(health, maxHealth);
        }

        if (!IsLocalPlayer())
        {
            healthSlider.value = CalculateHealth();
        }

        TurnNameTextTowardLocalPlayer();
    }

    float CalculateHealth()
    {
        return health / maxHealth;
    }

    void FixedUpdate()
    {

        if (velocity.magnitude > 0f && !playerAudioSource.isPlaying && isGrounded )
        {
            playerAudioSource.volume = PlayerPrefs.GetFloat("Volume", 0.5f);

            if (velocity.magnitude > 7)
            {
                playerAudioSource.maxDistance = 50;
                playerAudioSource.pitch = Random.Range(1f, 1.1f);
            }
            else
            {
                playerAudioSource.maxDistance = 30;
                playerAudioSource.pitch = Random.Range(0.6f, 0.8f);
            }
            
            playerAudioSource.Play();
        }
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        foreach (MeshRenderer _model in model )
        {
            _model.enabled = false;
        }
        nameText.SetActive(false);
        cameraTransform.gameObject.SetActive(false);

        isDead = true;

        SpawnCorpse();
        if (IsLocalPlayer())
        {
            SpawnSpectator();
            UIManager.DeathScreen(true);
        }
    }

    public void SpawnCorpse()
    {
        playerCorpse = Instantiate(playerCorpsePrefab, transform.position - new Vector3(0, 0.9f, 0), transform.rotation);

        var _children = playerCorpse.GetComponentsInChildren<Transform>();
        foreach (var _child in _children)
        {
            _child.gameObject.AddComponent<Rigidbody>();
        }
        Destroy(playerCorpse, 60f);
    }

    public void SpawnSpectator()
    {
        spectator = Instantiate(spectatorPrefab, transform.position - transform.forward * 5f + transform.up * 5f, transform.rotation);
        spectator.transform.LookAt(transform.position);
        UIManager.ToggleHud(false);
    }

    public void DestroySpectator()
    {
        Destroy(spectator);
        UIManager.ToggleHud(true);
    }

    public void Respawn()
    {
        DestroySpectator();
        cameraTransform.gameObject.SetActive(true);
        foreach (MeshRenderer _model in model)
        {
            _model.enabled = true;
        }

        SetHealth(maxHealth);
        nameText.SetActive(true);

        if (IsLocalPlayer())
        {
            UIManager.DeathScreen(false);
        }

        isDead = false;
    }

    public void ShootReceived(Vector3 _endPosition, bool _didHitPlayer)
    {
        tommyGun.LookAt(_endPosition);

        AddForceToRigidbody(shootOrigin.transform.position, _endPosition, 25f);

        ShotTracer(_endPosition);
        shootOrigin.GetComponent<ParticleSystem>().Play();

        shootOriginAudioSource.pitch = Random.Range(1.1f, 1.2f);
        shootOriginAudioSource.PlayOneShot(gunShotSounds[Random.Range(0, gunShotSounds.Length)], PlayerPrefs.GetFloat("Volume", 0.5f));

        boltAnimator.Play("Bolt_firing");

        if (_didHitPlayer)
        {
            AudioSource.PlayClipAtPoint(playerHitSound[Random.Range(0, gunShotSounds.Length)], _endPosition, PlayerPrefs.GetFloat("Volume", 0.5f));
        }
    }

    public void AddForceToRigidbody(Vector3 _position, Vector3 _endPosition, float _force)
    {
        Debug.DrawRay(_position, (_endPosition - _position), Color.red, 10f);
        if (Physics.Raycast(_position, _endPosition - _position, out RaycastHit _hit, 100f))
        {
            Rigidbody rb = _hit.collider.attachedRigidbody;

            if (rb != null)
            {
                var _forceDirection = rb.transform.position - _position;

                rb.AddForce(_forceDirection * 50f);
            }
        }
    }

    // AddForceToRigidbodiesWithinRadius(_endPosition, 3f, 1000f);
    public void AddForceToRigidbodiesWithinRadius(Vector3 _position, float _radius, float _force)
    {
        Collider[] hitColliders = Physics.OverlapSphere(_position, _radius);
        foreach(var _collider in hitColliders)
        {
            Rigidbody rb = _collider.gameObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                var _forceDirection = rb.transform.position - _position;
                rb.AddForceAtPosition(_forceDirection * _force, _position);
            }
        }
    }

    public void ShotTracer(Vector3 _endPosition)
    {
        LineRenderer line = shootOrigin.GetComponent<LineRenderer>();

        if (line == null)
        {
            line = shootOrigin.AddComponent<LineRenderer>();
        }

        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(1, 1, 1, 1);
        line.endColor = new Color(1, 1, 1, 1);

        line.SetPosition(0, shootOrigin.transform.position);
        line.SetPosition(1, _endPosition);

        Destroy(line, 0.05f);
    }

    void TurnNameTextTowardLocalPlayer()
    {
        nameText.transform.LookAt(2 * nameText.transform.position - GameManager.players[Client.instance.myId].transform.position);
    }

    public void PlayerReloading()
    {
        GameObject _magazine = Instantiate(magazine, magazine.transform.position, magazine.transform.rotation);
        magazine.GetComponent<MeshRenderer>().enabled = false;

        _magazine.AddComponent<Rigidbody>();

        Destroy(_magazine, 20f);
    }

    public void AmmoCapacity(int _ammoCapacity, int _maxAmmoCapacity)
    {
        ammoCapacity = _ammoCapacity;
        maxAmmoCapacity = _maxAmmoCapacity;
        magazine.GetComponent<MeshRenderer>().enabled = true;

        if (IsLocalPlayer())
        {
            UIManager.AmmoCapacity(ammoCapacity.ToString());
        }
    }

    public bool IsLocalPlayer()
    {
        return id == Client.instance.myId;
    }
}
