using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using Photon.Pun;

public class PuzzleLifeManager : MonoBehaviourPun
{
    [SerializeField] private int stageId;
    [SerializeField] private bool isTest;
    private BoStage _currentStage;

    private static PuzzleLifeManager _instance;
    public static PuzzleLifeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = FindObjectOfType<PuzzleLifeManager>();
                if (obj != null)
                    _instance = obj;
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        if (isTest)
        {
            _currentStage = new BoStage(stageId);
        }
        else
        {
            _currentStage = GameManager.Instance.CurrentStage;
        }
    }

    public void HandleDeath(CharacterBase character)
    {
        character.photonView.RPC("SetCharacterActive", RpcTarget.All, false);
        UIManager.Instance.Show<UIGameOver>("UIGameOver");
    }
}
