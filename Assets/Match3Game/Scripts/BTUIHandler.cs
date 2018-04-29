using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BTUIHandler : MonoBehaviour
{
    //BTHome
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Image settingsButton;
    [SerializeField] private Sprite settingsButtonDefault;
    private bool isSettingsPanelOn;

    //BTGameplay
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    private void Start()
    {
        isSettingsPanelOn = false;
        settingsPanel.SetActive(false);
    }

    public void PlayButtonClick()
    {
        SceneManager.LoadScene("BTMap");
    }
    public void EditAvatarButtonClick()
    {
        SceneManager.LoadScene("BTEditAvatar");
    }

    public void MainMenuButtonClick()
    {
        SceneManager.LoadScene("BTHome");
    }


    public void ShowSettingsPanel()
    {
        if (!isSettingsPanelOn)
        {
            settingsPanel.SetActive(true);
            isSettingsPanelOn = !isSettingsPanelOn;
        }
        else
        {
            settingsPanel.SetActive(false);
            isSettingsPanelOn = !isSettingsPanelOn;
            settingsButton.sprite = settingsButtonDefault;

        }
    }

    public void FacebookButton()
    {
        Application.OpenURL("https:www.facebook.com/BotsLeague/");
    }

    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }

    public void PlayNextLevel()
    {
        SceneManager.LoadScene("BTGameplay");
    }
}



