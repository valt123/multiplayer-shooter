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
    public GameObject startCamera;
    #endregion

    #region Hud variables
    [Header("Hud variables")]
    public GameObject hud;
    public Text ammoCapacity;
    public Image hitMarker;
    public Image damageOverlay;
    public Image staminaOverlay;
    #endregion

    #region Scoreboard variables
    [Header("Scoreboard variables")]
    public GameObject scoreBoard;

    public Transform entryContainer;
    public Transform entryTemplate;
    #endregion

    #region Killfeed variables
    [Header("Killfeed variables")]
    public Transform killFeedTemplate;
    public Transform killFeedContainer;
    private List<GameObject> killFeedItemList = new List<GameObject>();
    #endregion

    #region Pause menu variables
    [Header("Pause menu variables")]
    public GameObject pauseMenu;

    public GameObject optionsMenu;
    public Slider volume;
    public Slider sensitivity;
    #endregion

    #region Misc variables
    [Header("Misc variables")]
    public GameObject deathScreen;
    #endregion

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
        damageOverlay.canvasRenderer.SetAlpha(0);
        staminaOverlay.canvasRenderer.SetAlpha(0);

        usernameField.text = PlayerPrefs.GetString("Username");
        ipAddress.text = PlayerPrefs.GetString("IPaddress");
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && !startMenu.activeSelf)
        {
            ToggleCursorMode();
            PauseMenu();
        }
    }

    private void ToggleCursorMode()
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #region Hud stuff
    public static void AmmoCapacity(string _ammoCapacity)
    {
        instance.ammoCapacity.text = _ammoCapacity;
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

    public static void DamageOverlay(float _health, float _maxhealth)
    {
        var alpha = -(_health / _maxhealth) + 1;

        instance.damageOverlay.CrossFadeAlpha(Math.Abs(alpha), 0.1f, false);
    }

    public static void StaminaOverlay(float _stamina, float _maxStamina)
    {
        var alpha = -(_stamina / _maxStamina) + 1;

        instance.staminaOverlay.canvasRenderer.SetAlpha(alpha);
    }

    public static void ToggleHud(bool toggle)
    {
        instance.hud.SetActive(toggle);

        if (toggle)
        {
            instance.hitMarker.canvasRenderer.SetAlpha(0);
        }
    }

    #endregion

    #region Start menu
    public void ConnectToServer()
    {
        startMenu.SetActive(true);
        hitMarker.canvasRenderer.SetAlpha(0);
        damageOverlay.canvasRenderer.SetAlpha(0);
        staminaOverlay.canvasRenderer.SetAlpha(0);

        if (ipAddress.text == "")
        {
            ipAddress.text = "127.0.0.1";
        }

        try
        {
            Client.instance.ConnectToServer(ipAddress.text);

            startMenu.SetActive(false);
            hud.SetActive(true);

            PlayerPrefs.SetString("Username", usernameField.text);
            PlayerPrefs.SetString("IPaddress", ipAddress.text);

            startCamera.SetActive(false);
        }

        catch
        {
            Debug.Log($"There was an error connecting to the server");
        }

    }
    #endregion

    #region Pause menu
    public static void PauseMenu()
    {
        instance.pauseMenu.SetActive(Cursor.lockState == CursorLockMode.None);

        if (Cursor.lockState == CursorLockMode.Locked && !IsPauseMenuOn())
        {
            instance.CloseOptions();
        }

    }

    public static bool IsPauseMenuOn()
    {
        return instance.startMenu.activeSelf;
    }

    public void DisconnectFromServer()
    {
        Client.instance.Disconnect();

        startMenu.SetActive(true);
        startCamera.SetActive(true);
        hud.SetActive(false);
        pauseMenu.SetActive(false);
        deathScreen.SetActive(false);
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
        instance.optionsMenu.SetActive(true);
        instance.sensitivity.value = PlayerPrefs.GetFloat("Sensitivity", 100f);
        instance.volume.value = PlayerPrefs.GetFloat("Volume", .5f);

        instance.optionsMenu.transform.Find("VolumeValue").GetComponent<Text>().text = instance.volume.value.ToString("F2");
        instance.optionsMenu.transform.Find("SensitivityValue").GetComponent<Text>().text = instance.sensitivity.value.ToString("F0");
    }

    public void SaveSensitivityOption()
    {
        PlayerPrefs.SetFloat("Sensitivity", instance.sensitivity.value);
        instance.optionsMenu.transform.Find("SensitivityValue").GetComponent<Text>().text = instance.sensitivity.value.ToString("F0");
    }

    public void SaveVolumeOption()
    {
        PlayerPrefs.SetFloat("Volume", instance.volume.value);
        instance.optionsMenu.transform.Find("VolumeValue").GetComponent<Text>().text = instance.volume.value.ToString("F2");
        Debug.Log(instance.volume.value);
    }

    public void CloseOptions()
    {
        instance.optionsMenu.SetActive(false);
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
