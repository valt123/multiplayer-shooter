using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;

    public float health;
    public float maxHealth;

    public int kills;
    public int deaths;

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
    public Transform cameraTransform;
    private Vector3 lastPosition;
    public float speed;
    public bool isGrounded;

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
    }

    void FixedUpdate()
    {
        speed = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;
        Debug.Log(speed);

        if (speed > 0f && !playerAudioSource.isPlaying && isGrounded )
        {
            playerAudioSource.volume = PlayerPrefs.GetFloat("Volume", 0.5f);

            if (speed > 0.1f)
            {
                playerAudioSource.pitch = Random.Range(1f, 1.1f);
            }
            else
            {
                playerAudioSource.pitch = Random.Range(0.7f, 0.9f);
            }
            
            playerAudioSource.Play();
        }

        TurnNameTextTowardLocalPlayer();
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

        if (IsLocalPlayer())
        {
            UIManager.DeathScreen(true);
        }
    }

    public void Respawn()
    {
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
    }

    public void ShootReceived(Vector3 _endPosition, bool _didHitPlayer)
    {
        tommyGun.LookAt(_endPosition);

        ShotTracer(_endPosition);
        shootOrigin.GetComponent<ParticleSystem>().Play();

        shootOriginAudioSource.pitch = Random.Range(1, 1.1f);
        shootOriginAudioSource.PlayOneShot(gunShotSounds[Random.Range(0, gunShotSounds.Length)], PlayerPrefs.GetFloat("Volume", 0.5f));

        boltAnimator.Play("Bolt_firing");

        if (_didHitPlayer)
        {
            AudioSource.PlayClipAtPoint(playerHitSound[Random.Range(0, gunShotSounds.Length)], _endPosition, PlayerPrefs.GetFloat("Volume", 0.5f));
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
