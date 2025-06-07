using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    [Header("Button")]
    public Button startButton;
    public Button settingButton;
    public Button exitButton;

    [Header("Panel")]
    public GameObject titlePanel;
    public GameObject lobbyPanel;

    [Header("Manager")]
    public GameObject titleManager;
    public GameObject matchManager;

    public void OnClick_Start()
    {
        titlePanel.SetActive(false);
        lobbyPanel.SetActive(true);

        titleManager.SetActive(false);
        matchManager.SetActive(true);
    }
    public void OnClick_Setting()
    {

    }
    public void OnClick_Exit()
    {
        Application.Quit();
    }
}
