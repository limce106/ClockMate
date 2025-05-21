using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIStartGameTest : UIBase
{
    [SerializeField] private Button startGameButton;
    private void Awake()
    {
        startGameButton.onClick.AddListener(TestBtnClick);
    }

    private void TestBtnClick()
    {
        gameObject.SetActive(false);
        GameManager.Instance.StartGame();
    }
}