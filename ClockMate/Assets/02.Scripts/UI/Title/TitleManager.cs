using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class TitleManager : MonoBehaviour
{
    [Header("Text")]
    public TMP_InputField joinCodeInputField;

    [Header("Button")]
    public Button startButton;
    public Button settingButton;
    public Button exitButton;

    public Button joinCodeOkButton;

    [Header("Panel")]
    public GameObject titlePanel;
    public GameObject roomPanel;
    public GameObject playTypePanel;
    public GameObject joinCodePanel;

    private bool suppressCallback = false;
    private GameObject prevPanel;
    private GameObject curPanel;

    void Start()
    {
        SoundManager.Instance.PlayBgm("title_bgm");

        joinCodeInputField.onValueChanged.AddListener(OnInputValueChanged);
        CheckInput(joinCodeInputField.text);
    }

    void OnInputValueChanged(string value)
    {
        if (suppressCallback)
            return;

        string upper = value.ToUpper();
        if (value != upper)
        {
            suppressCallback = true;
            joinCodeInputField.text = upper;
            // 커서가 뒤로 밀리는 문제 방지
            joinCodeInputField.caretPosition = upper.Length;
            suppressCallback = false;
        }

        CheckInput(upper);
    }

    void CheckInput(string text)
    {
        joinCodeOkButton.interactable = !string.IsNullOrEmpty(text);
    }

    public void OnClick_Start()
    {
        titlePanel.SetActive(false);
        roomPanel.SetActive(true);

        prevPanel = titlePanel;
        curPanel = roomPanel;

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }
    public void OnClick_Setting()
    {
        UISetting uiSetting = UIManager.Instance?.Show<UISetting>("UISetting");
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }
    public void OnClick_Exit()
    {
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
        Application.Quit();
    }
    public void OnClick_MakeRoom()
    {
        roomPanel.SetActive(false);
        playTypePanel.SetActive(true);

        prevPanel = roomPanel;
        curPanel = playTypePanel;

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }

    public void OnClick_JoinRoom()
    {
        roomPanel.SetActive(false);
        joinCodePanel.SetActive(true);

        prevPanel = roomPanel;
        curPanel = joinCodePanel;

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }

    public void OnClick_Back()
    {
        curPanel.SetActive(false);
        prevPanel.SetActive(true);

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }
}
