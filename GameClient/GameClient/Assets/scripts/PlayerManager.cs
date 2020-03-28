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
    public GameObject magazine;
    public Animator boltAnimator;

    public MeshRenderer[] model;

    public GameObject shootOrigin;
    public GameObject nameText;
    public Transform cameraTransform;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        NameText();
        UIManager.Health(health, maxHealth);
        UIManager.AmmoCapacity(ammoCapacity.ToString());
    }

    private void Update()
    {
        TurnNameTextTowardLocalPlayer();
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }

        if (id == Client.instance.myId)
        {
            UIManager.Health(health, maxHealth);
        }
    }

    public void Die()
    {
        foreach (MeshRenderer _model in model )
        {
            _model.enabled = false;
        }
        nameText.SetActive(false);

        if (id == Client.instance.myId)
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

        if (id == Client.instance.myId)
        {
            UIManager.DeathScreen(false);
        }
    }

    public void ShootReceived(Vector3 _endPosition)
    {
        tommyGun.LookAt(_endPosition);

        ShotTracer(_endPosition);
        shootOrigin.GetComponent<ParticleSystem>().Play();
        boltAnimator.Play("Bolt_firing");
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

    public void NameText()
    {
        TextMesh _text = nameText.GetComponent<TextMesh>();
        if (Client.instance.myId != id)
        {
            _text.text = username;
        }
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

    public void AmmoCapacity(int _ammoCapacity)
    {
        ammoCapacity = _ammoCapacity;
        magazine.GetComponent<MeshRenderer>().enabled = true;

        if (id == Client.instance.myId)
        {
            UIManager.AmmoCapacity(ammoCapacity.ToString());
        }
    }
}
