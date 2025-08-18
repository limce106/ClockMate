using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;
using static Define.Character;

/// <summary>
/// 썰매 상호작용 승인 및 컷신 동기 재생, 추격 시작 동작을 처리한다.
/// </summary>
public class SledChaseOrchestrator : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform sledStart;
    [SerializeField] private Transform bearStart;
    [SerializeField] private Collider chaseEndTrigger;

    [SerializeField] private SledController sled;
    [SerializeField] private Collider sledTriggerCollider;
    [SerializeField] private PolarBearController bear;
    [SerializeField] private SledHP sledHP;
    [SerializeField] private TargetDetector targetDetector;
    [SerializeField] private GameObject characterModels;
    [SerializeField] private SnowballShooter snowballShooter;
    
    [Header("Cutscene")]
    [SerializeField] private VideoCutscenePlayer cutscenePlayer;
    [SerializeField] private VideoClip cutsceneClip;  

    [Header("Cameras")]
    [SerializeField] private GameObject vCamFront;
    [SerializeField] private GameObject vCamBack;

    private bool _locked;
    private int _finishedCount;
    
    private int _hourActor = -1;
    private int _milliActor = -1;
    
    private void Start()
    {
        StartCoroutine(ReportSelectionOnceReadyRoutine());
    }

    private IEnumerator ReportSelectionOnceReadyRoutine()
    {
        // 네트워크 준비 대기
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom);

        // 로컬 선택값 보고
        CharacterName selectedCharacter = GameManager.Instance.SelectedCharacter;
        photonView.RPC(nameof(RPC_ReportSelection), RpcTarget.MasterClient, 
            PhotonNetwork.LocalPlayer.ActorNumber, (int)selectedCharacter);
    }

    /// <summary>
    /// 상호작용 시작 요청
    /// </summary>
    public void RequestStartFromTrigger()
    {
        photonView.RPC(nameof(RPC_RequestStart), RpcTarget.MasterClient);
    }


    [PunRPC]
    private void RPC_ReportSelection(int actorNumber, int characterEnum)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        CharacterName c = (CharacterName)characterEnum;
        if (c == CharacterName.Hour)  _hourActor  = actorNumber;
        if (c == CharacterName.Milli) _milliActor = actorNumber;
    }

    [PunRPC]
    private void RPC_RequestStart()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_locked) return; // 재진입 방지
        if (_hourActor < 0 || _milliActor < 0) return; // 선택 미수신 시 보류

        _locked = true;
        // 컷신 재생
        CutsceneSyncManager.Instance.PlayForAll(
            "PolarBearAwake",
            0f,
            () =>
            {
                // 추격 시작 위치로 텔레포트
                photonView.RPC(nameof(RPC_TeleportAll), RpcTarget.All,
                    sledStart.position, sledStart.rotation.eulerAngles,
                    bearStart.position, bearStart.rotation.eulerAngles);
            }
        );
        photonView.RPC(nameof(RPC_PrepareChase), RpcTarget.All);
    }
    
    [PunRPC]
    private void RPC_PrepareChase()
    {
        // 썰매 콜라이더 비활성화
        if (sledTriggerCollider != null)
        {
            sledTriggerCollider.enabled = false;
        }
        // 플레이어 캐릭터 비활성화
        GameManager.Instance.SetAllCharactersActive(false); 
    }

    [PunRPC]
    private void RPC_TeleportAll(Vector3 sledPos, Vector3 sledEuler, Vector3 bearPos, Vector3 bearEuler)
    {
        characterModels.SetActive(true);
        sled.transform.SetPositionAndRotation(sledPos, Quaternion.Euler(sledEuler));
        bear.transform.SetPositionAndRotation(bearPos, Quaternion.Euler(bearEuler));
        bear.GetComponent<Rigidbody>().isKinematic = false;
        photonView.RPC(nameof(RPC_TeleportDone), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_TeleportDone()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        _finishedCount++;
        if (_finishedCount == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            // 썰매 소유권을 Hour에게 양도
            if (_hourActor > 0)
            {
                PhotonView sledView = sled.GetComponent<PhotonView>();
                if (sledView != null && sledView.OwnerActorNr != _hourActor)
                    sledView.TransferOwnership(_hourActor);
            }

            photonView.RPC(nameof(RPC_BeginChase), RpcTarget.All, _hourActor, _milliActor);
        }
    }

    [PunRPC]
    private void RPC_BeginChase(int hourActor, int milliActor)
    {
        // Hour: 썰매 조작 권한
        bool isHour = PhotonNetwork.LocalPlayer.ActorNumber == hourActor;
        bool isMilli = PhotonNetwork.LocalPlayer.ActorNumber == milliActor;

        sled.SetControl(isHour);           
        sled.StartSled();                  
        bear.StartChase(); 

        // 카메라
        vCamFront.SetActive(isHour);
        vCamBack.SetActive(isMilli);

        // UI
        sledHP.Init();
        if (isMilli && targetDetector != null)
        {
            targetDetector.enabled = true;
            targetDetector.Init();
        }
        
        snowballShooter.SetActive(true);
        
        sled.GetComponent<PhotonTransformView>().enabled = true;
    }
}
