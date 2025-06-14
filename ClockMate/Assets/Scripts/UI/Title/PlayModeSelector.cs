using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayModeSelector : MonoBehaviour
{
    [SerializeField]
    private MatchManager matchManager;
    public Button continueButton;

    public static bool IsNewGameRoom { get; private set; }

    private void Start()
    {
        if (SaveManager.Instance == null)
            return;

        if(!SaveManager.Instance.SaveDataExist())
        {
            continueButton.interactable = false;
        }
    }

    public void OnClick_NewGame()
    {
        IsNewGameRoom = true;
        matchManager.OnClick_CreateRoom();
    }
    public void OnClick_Continue()
    {
        if (SaveManager.Instance == null)
            return;

        IsNewGameRoom = false;
        matchManager.OnClick_CreateRoom();
    }
}
