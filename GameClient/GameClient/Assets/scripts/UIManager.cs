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
    public Image hitMarker;
    #endregion

    #region Scoreboard variables
    public GameObject scoreBoard;

    public Transform entryContainer;
    public Transform entryTemplate;
    #endregion

    #region Killfeed variables
    public Transform killFeedTemplate;
    public Transform killFeedContainer;
    public List<GameObject> killFeedItemList = new List<GameObject>();
    #endregion

    #region Pause menu
    public GameObject pauseMenu;

    public GameObject optionsMenu;
    public Slider volume;
    public Slider sensitivity;
    #endregion

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

        hitMarker.canvasRenderer.SetAlpha(0);
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

    public static void HitMark()
    {
        instance.hitMarker.canvasRenderer.SetAlpha(1);
        FadeOut(instance.hitMarker, 0.2f);
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

    public void OptionsMenu()
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

                entryTransform.Find("name").GetComponent<Text>().text = _player.IsLocalPlayer() ? $"<color=orange>{name}</color>" : $"<color=red> {name} </color>";
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
        float templateHeight = 35f;
        
        Transform killFeedItem = Instantiate(instance.killFeedTemplate, instance.killFeedContainer);
        RectTransform killFeedItemRect = killFeedItem.GetComponent<RectTransform>();
        killFeedItemRect.anchoredPosition = new Vector2(0, -templateHeight);

        killFeedItem.Find("Killer").GetComponent<Text>().text = _killer;
        killFeedItem.Find("Killed").GetComponent<Text>().text = _killed;

        foreach(var item in instance.killFeedItemList)
        {
            item.GetComponent<RectTransform>().position += new Vector3(0, -templateHeight, 0);
        }

        instance.killFeedItemList.Add(killFeedItem.gameObject);
        instance.StartCoroutine(instance.RemoveKillFeedItem(killFeedItem.gameObject));
    }

    IEnumerator RemoveKillFeedItem(GameObject _killFeedItem)
    {
        yield return new WaitForSeconds(5f);

        instance.killFeedItemList.Remove(_killFeedItem.gameObject);
        Destroy(_killFeedItem.gameObject);
    }
    #endregion

    public static void FadeIn(Image _image, float _duration)
    {
        _image.CrossFadeAlpha(1, _duration, false);
    }

    public static void FadeOut(Image _image, float _duration)
    {
        _image.CrossFadeAlpha(0, _duration, false);
    }
}
