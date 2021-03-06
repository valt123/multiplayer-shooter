﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;

    public float health;
    public float maxHealth;
    public float stamina;
    public float maxStamina;

    public int kills;
    public int deaths;
    public bool isDead = false;

    public Transform weaponHolder;

    public Transform tommyGun;
    public int ammoCapacity = 50;
    public int maxAmmoCapacity = 50;
    public GameObject magazine;
    public Animator boltAnimator;
    private Coroutine changeGunRotationCoroutine;

    public GameObject knife;
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

    private Quaternion lastRot;
    public Vector3 velocity;
    public bool isGrounded;
    public float idleToRun;
    private bool isIdleToRunChanging;

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

        tommyGun.GetComponent<Animator>().keepAnimatorControllerStateOnDisable = true;
        knife.GetComponent<Animator>().keepAnimatorControllerStateOnDisable = true;
    }

    private void Update()
    {
        if (IsLocalPlayer())
        {
            UIManager.DamageOverlay(health, maxHealth);
            UIManager.StaminaOverlay(stamina, maxStamina);
        }

        if (!IsLocalPlayer())
        {
            healthSlider.value = CalculateHealthbarValue();
        }

        TurnNameTextTowardLocalPlayer();

        MovementAnimationAndSounds();

        if (IsLocalPlayer() && Cursor.lockState == CursorLockMode.Locked)
        {
            var horizontalTurn = Input.GetAxis("Mouse X");
            var verticalTurn = -Input.GetAxis("Mouse Y");

            if (changeGunRotationCoroutine == null)
            {
                changeGunRotationCoroutine = StartCoroutine(LerpGunRotation(Quaternion.Euler(verticalTurn, horizontalTurn * 2, 0), .3f, true, false));
            }
        }
    }

    private void MovementAnimationAndSounds()
    {
        if (velocity.magnitude > 0f && isGrounded && !isDead)
        {
            playerAudioSource.volume = PlayerPrefs.GetFloat("Volume", 0.5f);

            if (velocity.magnitude > 7)
            {
                playerAudioSource.maxDistance = 50;
                playerAudioSource.pitch = Random.Range(1f, 1.1f);

                if (!isIdleToRunChanging && idleToRun < 1) StartCoroutine(ChangeIdleToRun(idleToRun, 1f, .5f));
            }
            else if (velocity.magnitude < 7)
            {
                playerAudioSource.maxDistance = 30;
                playerAudioSource.pitch = Random.Range(0.6f, 0.8f);

                if (!isIdleToRunChanging && idleToRun > 0) StartCoroutine(ChangeIdleToRun(idleToRun, 0f, .5f));
            }

            if (!playerAudioSource.isPlaying)
            {
                playerAudioSource.Play();
            }
            //Debug.Log(velocity.magnitude);
        }
        else
        {
            if (!isIdleToRunChanging && idleToRun != 0) StartCoroutine(ChangeIdleToRun(idleToRun, 0f, .5f));
        }

        tommyGun.GetComponent<Animator>().SetFloat("IdleToRun", idleToRun);
    }

    IEnumerator ChangeIdleToRun(float value, float targetValue, float duration)
    {
        isIdleToRunChanging = true;
        var t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            idleToRun = Mathf.Lerp(value, targetValue, t / duration);
            yield return null;
        }
        isIdleToRunChanging = false;
        idleToRun = targetValue;
        yield break;
    }

    private float CalculateHealthbarValue()
    {
        return health / maxHealth;
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
        // Hide player models
        ToggleAllChildMeshrenderers(transform, false);

        // Disable collider
        GetComponent<Collider>().enabled = false;

        // Hide name text
        nameText.SetActive(false);

        isDead = true;

        SpawnCorpse();

        if (IsLocalPlayer())
        {
            SpawnSpectator();

            // Disable camera and audio listener to avoid console spam
            cameraTransform.GetComponent<Camera>().enabled = false;
            cameraTransform.GetComponent<AudioListener>().enabled = false;

            UIManager.DeathScreen(true);
        }
    }

    void ToggleAllChildMeshrenderers(Transform _transform, bool toggle)
    {
        ToggleMeshrender(_transform.root, toggle);
    }

    private void ToggleMeshrender(Transform _transform, bool toggle)
    {
        var renderer =_transform.GetComponent<Renderer>();

        if (renderer != null) renderer.enabled = toggle;

        for (int i = 0; i < _transform.childCount; i++)
        {
            ToggleMeshrender(_transform.GetChild(i), toggle);
        }
    }

    public void TakeDamage(int _enemy)
    {
        var enemy = GameManager.players[_enemy];

        Debug.Log( (transform.position - enemy.transform.position).normalized );
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
        // Enable all player models
        ToggleAllChildMeshrenderers(transform, true);
        
        GetComponent<Collider>().enabled = true;

        DestroySpectator();

        SetHealth(maxHealth);

        nameText.SetActive(true);

        if (IsLocalPlayer())
        {
            cameraTransform.GetComponent<Camera>().enabled = true;
            cameraTransform.GetComponent<AudioListener>().enabled = true;

            UIManager.DeathScreen(false);
        }

        isDead = false;
    }

    public void ShootReceived(Vector3 _endPosition, bool _didHitPlayer)
    {
        if (changeGunRotationCoroutine != null)
        {
            StopCoroutine(changeGunRotationCoroutine);
            changeGunRotationCoroutine = null;
        }

        var _lookRotation = Quaternion.LookRotation(_endPosition - weaponHolder.position);

        changeGunRotationCoroutine = StartCoroutine(LerpGunRotation(_lookRotation, .1f, false, false));
        
        AddForceToRigidbody(shootOrigin.transform.position, _endPosition, 25f);

        ShotTracer(_endPosition);
        shootOrigin.GetComponent<ParticleSystem>().Play();

        shootOriginAudioSource.pitch = Random.Range(1.1f, 1.2f);
        shootOriginAudioSource.PlayOneShot(gunShotSounds[Random.Range(0, gunShotSounds.Length)], PlayerPrefs.GetFloat("Volume", 0.5f));

        boltAnimator.Play("Bolt_firing");

        if (_didHitPlayer)
        {
            //AudioSource.PlayClipAtPoint(playerHitSound[Random.Range(0, gunShotSounds.Length)], _endPosition, PlayerPrefs.GetFloat("Volume", 0.5f));
        }


        if (changeGunRotationCoroutine != null)
        {
            StopCoroutine(changeGunRotationCoroutine);
            changeGunRotationCoroutine = null;
        }
        changeGunRotationCoroutine = StartCoroutine(LerpGunRotation( Quaternion.Euler(0,0,0), .5f ));
    }

    private IEnumerator LerpGunRotation(Quaternion target, float duration, bool localRotation = true , bool waitBefore = true)
    {
        if (waitBefore)
        {
            yield return new WaitForSeconds(.5f);
        }
        Quaternion _currentRotation;

        _currentRotation = localRotation ? weaponHolder.localRotation : weaponHolder.rotation;

        var t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime / duration;

            if (localRotation)
            {
                weaponHolder.localRotation = Quaternion.Slerp(_currentRotation, target, t / duration);
            }
            else
            {
                weaponHolder.rotation = Quaternion.Slerp(_currentRotation, target, t / duration);
            }
            
            yield return null;
        }

        if (localRotation)
        {
            weaponHolder.localRotation = target;
        }
        else
        {
            weaponHolder.rotation = target;
        }

        changeGunRotationCoroutine = null;
        yield break;
    }

    public void PlayerMeleed()
    {
        knife.GetComponent<Animator>().Play("Stab");
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
        if (!LocalPlayer().isDead)
        {
            nameText.transform.LookAt(2 * nameText.transform.position - LocalPlayer().transform.position);
        }
        else
        {
            nameText.transform.LookAt(2 * nameText.transform.position - LocalPlayer().spectator.transform.position);
        }
        
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
        return Client.IsLocalPlayer(id);
    }

    public PlayerManager LocalPlayer()
    {
        return GameManager.players[Client.instance.myId];
    }
}
