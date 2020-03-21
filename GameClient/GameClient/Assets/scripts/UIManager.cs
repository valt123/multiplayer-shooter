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
    public Image hpBg;
    public Image hpBar;

    public GameObject reticle;

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

        //hud.SetActive(false);
    }

    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;

        if (ipAddress.text == "localhost")
        {
            ipAddress.text = "127.0.0.1";
        }

        try
        {
            Client.instance.ConnectToServer(ipAddress.text);
            //hud.SetActive(true);
        }
        catch(Exception _ex)
        {
            Debug.Log($"There was an error connecting to the server: {_ex}");
            startMenu.SetActive(true);
        }
        
    }
}
