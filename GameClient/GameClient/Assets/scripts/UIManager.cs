using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region Startmenu variables
    public GameObject startMenu;
    public InputField usernameField;
    public InputField ipAddress;
    #endregion

    #region Hud variables
    public GameObject hud;
    public Text ammoCapacity;
    public Text healthText;
    #endregion

    #region Scoreboard variables
    public GameObject scoreBoard;

    public Transform entryContainer;
    public Transform entryTemplate;
    #endregion

    #region Killfeed variables
    public GameObject killFeed;
    public Transform killFeedTemplate;
    public Transform killFeedContainer;
    #endregion

    public GameObject pauseMenu;
    public GameObject deathScreen;

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
    }

    #region Hud stuff
    public static void AmmoCapacity(string _ammoCapacity)
    {
        instance.ammoCapacity.text = _ammoCapacity;
    }

    public static void Health(float _health, float _maxHealth)
    {
        instance.healthText.text = $"{_health}/{_maxHealth}";
    }

    public static void DeathScreen(bool _isDead)
    {
        instance.deathScreen.SetActive(_isDead);
    }
    #endregion

    #region Start menu
    public void ConnectToServer()
    {
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
        catch (Exception _ex)
        {
            Debug.Log($"There was an error connecting to the server: {_ex}");
        }

    }
    #endregion

    #region Pause menu stuff
    public static void PauseMenu()
    {
        instance.pauseMenu.SetActive(Cursor.lockState == CursorLockMode.None);
    }

    public void DisconnectFromServer()
    {
        Client.instance.Disconnect();

        startMenu.SetActive(true);
        hud.SetActive(false);
        pauseMenu.SetActive(false);
    }

    public void Respawn()
    {
        ClientSend.PlayerSuicide();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Options()
    {

    }
    #endregion

    #region Scoreboard
    public static void Scoreboard(bool _active)
    {
        if (_active)
        {
            instance.scoreBoard.SetActive(true);

            float templateHeight = 50f;

            foreach (var i in GameManager.players.Keys)
            {
                var _player = GameManager.players[i];

                Transform entryTransform = Instantiate(instance.entryTemplate, instance.entryContainer);
                RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * i);

                var name = _player.username;
                var kills = _player.kills.ToString();
                var deaths = _player.deaths.ToString();

                entryTransform.Find("name").GetComponent<Text>().text = name;
                entryTransform.Find("kills").GetComponent<Text>().text = kills;
                entryTransform.Find("deaths").GetComponent<Text>().text = deaths;

                entryTransform.gameObject.SetActive(true);
            }
        }
        else
        {
            instance.scoreBoard.SetActive(false);
            foreach (Transform child in instance.entryContainer.transform)
            {
                Destroy(child.gameObject);
            }

        }
    }
    #endregion

    #region Killfeed
    public static void KillFeed(string _killer, string _killed)
    {
        instance.killFeed.SetActive(true);

        float templateHeight = 35f;

        var i = 0;

        Transform entryTransform = Instantiate(instance.killFeedTemplate, instance.killFeedContainer);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * i);

        entryTransform.Find("Killer").GetComponent<Text>().text = _killer;
        entryTransform.Find("Killed").GetComponent<Text>().text = _killed;

        entryTransform.gameObject.SetActive(true);
    }
    #endregion
}
