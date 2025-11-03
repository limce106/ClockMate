using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자용 스테이지 이동 치트키 
/// </summary>
public class UIStageDebugLoader : UIBase
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private InputField stageInput; // 이동한 스테이지 id 입력 필드
    [SerializeField] private Button goButton;
    [SerializeField] private Text statusText;
    [SerializeField] private Text infoText;
    
    [SerializeField] private int minStageId = 1;
    [SerializeField] private int maxStageId = 8;

    private bool _isActive = false;
    private void Awake()
    {
        goButton.onClick.AddListener(OnClickGo);
    }

    private void Start()
    {
        SetInfo();
    }

    private void SetInfo()
    {
        SaveData saveData = SaveManager.Instance.Load();
        int saved = saveData.stageId;
        string savedCh = saveData.character.ToString();
        if (GameManager.Instance.CurrentStage == null)
        {
            infoText.text = "게임 시작되지 않음" +
                            $"\nsaved: {saved}" +
                            $"\nsavedCh: {savedCh}";;
            return;
        }
        int current = GameManager.Instance.CurrentStage.ID;

        infoText.text = $"current: {current}" +
                        $"\nsaved: {saved}" +
                        $"\nsavedCh: {savedCh}";
    }

    private void OnDestroy()
    {
        goButton.onClick.RemoveListener(OnClickGo);
    }

    private void OnClickGo()
    {
        TrySubmit(stageInput.text);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            ToggleStageLoader();
        }
    }
    
    /// <summary>
    /// InputField에 입력한 스테이지로의 이동을 처리한다.
    /// 잘못된 값이 입력되면 status text로 알린다.
    /// </summary>
    private void TrySubmit(string input)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (input == string.Empty)
        {
            SetStatus("id 입력 필요");
            return;
        }
        input = input.Trim();
        if (!int.TryParse(input, out var stageId))
        {
            SetStatus("숫자만 입력 가능");
            return;
        }

        if (stageId < minStageId || stageId > maxStageId)
        {
            SetStatus($"입력 범위: {minStageId}~{maxStageId}");
            return;
        }

        SetInteractable(false);
        SetStatus("이동 중…");

        RPCManager rpcManager = RPCManager.Instance;

        if (GameManager.Instance.CurrentStage.ID == 4)
        {
            FindObjectOfType<ChaseControlModule>().StopChase();
        }

        try
        {
            rpcManager.photonView.RPC(nameof(rpcManager.RPC_MoveToStage), RpcTarget.All, stageId);
            SaveManager.Instance.SaveStage(stageId);
            rpcManager.photonView.RPC(nameof(rpcManager.RPC_SyncReset), RpcTarget.All);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            SetInteractable(true);
        }

        ToggleStageLoader();
        StartCoroutine(CheckStageLoadFinish(stageId));
    }
    
    private IEnumerator CheckStageLoadFinish(int stageId)
    {
        yield return GameManager.Instance.CurrentStage.ID == stageId; // 이동 완료까지 기다리기
        
        stageInput.text = string.Empty;
        SetStatus("id 입력 필요");
        SetInfo();
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    private void SetInteractable(bool isInteractable)
    {
        stageInput.interactable = isInteractable;
        goButton.interactable = isInteractable;
    }
    
    /// <summary>
    /// UI 토글
    /// </summary>
    private void ToggleStageLoader()
    {
        if (_isActive)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
        }
        else
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        _isActive = !_isActive;
    }
}
