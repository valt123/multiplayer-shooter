using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth;
    public MeshRenderer model;
    public GameObject shootOrigin;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
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
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }

    public void Shoot(Vector3 _endPosition)
    {
        LineRenderer line = shootOrigin.GetComponent<LineRenderer>();

        if (line == null)
        {
            line = shootOrigin.AddComponent<LineRenderer>();
        }
        
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(1, 0, 0);
        line.endColor = new Color(1, 0, 0);

        line.SetPosition(0, shootOrigin.transform.position);
        line.SetPosition(1, _endPosition);

        Destroy(line, 0.5f);
    }
}
