using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField usernameField;
    public InputField ipAddress;

    public GameObject hud;
    public Text ammoCapacity;
    public Text healthText;
    //public Image hpBg;
    //public Image hpBar;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists");
            Destroy(this);
        }

        hud.SetActive(false);
    }

    public static void AmmoCapacity(string _ammoCapacity)
    {
        instance.ammoCapacity.text = _ammoCapacity;
    }

    public static void Health(float _health, float _maxHealth)
    {
        instance.healthText.text = $"{_health}/{_maxHealth}";
    }

    

    public void ConnectToServer()
    {
        usernameField.interactable = false;

        if (ipAddress.text == "localhost" || ipAddress.text == "")
        {
            ipAddress.text = "127.0.0.1";
        }

        try
        {
            Client.instance.ConnectToServer(ipAddress.text);

            startMenu.SetActive(false);
            hud.SetActive(true);
        }
        catch(Exception _ex)
        {
            Debug.Log($"There was an error connecting to the server: {_ex}");
        }
        
    }
}
