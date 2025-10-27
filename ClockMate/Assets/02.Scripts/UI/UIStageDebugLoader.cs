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
    
    [SerializeField] private int minStageId = 1;
    [SerializeField] private int maxStageId = 8;

    private bool _isActive = false;
    private void Awake()
    {
        goButton.onClick.AddListener(OnClickGo);
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
        if(Input.GetKeyDown(KeyCode.Alpha1))
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

        BoStage stage = new BoStage(stageId);
        if (stage.Map != GameManager.Instance.CurrentStage.Map)
        {
            // 다른 맵의 스테이지라면 해당 맵(씬)으로 이동
            rpcManager.photonView.RPC(
                nameof(rpcManager.RPC_MoveToMap), RpcTarget.All, stage.Map.ToString()
            );
        }
        rpcManager.photonView.RPC(nameof(rpcManager.RPC_SyncStage), RpcTarget.All, stageId);
        rpcManager.photonView.RPC(nameof(rpcManager.RPC_SyncReset), RpcTarget.All);
        SetInteractable(true);

        ToggleStageLoader();
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
