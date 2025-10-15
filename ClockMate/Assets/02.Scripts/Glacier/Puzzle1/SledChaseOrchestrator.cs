using System.Collections;
using DefineExtension;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;
using static Define.Character;
using static Define.Icon;

/// <summary>
/// 썰매 상호작용 승인 및 컷신 동기 재생, 추격 시작 동작을 처리한다.
/// </summary>
public class SledChaseOrchestrator : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform sledStart;
    [SerializeField] private Transform bearStart;
    
    [SerializeField] private DestroyableIce destroyableIce;
    [SerializeField] private SledController sled;
    [SerializeField] private Collider sledTriggerCollider;
    [SerializeField] private PolarBearController bear;
    [SerializeField] private SledHp sledHp;
    [SerializeField] private TargetDetector targetDetector;
    [SerializeField] private GameObject characterModels;
    [SerializeField] private GameObject visual;
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
    
    public void RequestIceEventFromTrigger()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_ActivateBreakPoints), RpcTarget.All, _milliActor);
    }

    public void RequestRestart()
    {
        // TODO 리셋 과정 자연스럽게 처리
        _finishedCount = 0;
        photonView.RPC(nameof(RPC_WaitAndRestart), RpcTarget.All);
    }
    
    [PunRPC]
    private void RPC_WaitAndRestart()
    {
        sled.SetSledMoving(false);
        visual.SetActive(false);
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(WaitForRestart());
    }
    IEnumerator WaitForRestart()
    {
        yield return new WaitForSeconds(1f);
        photonView.RPC(nameof(RPC_TeleportAll), RpcTarget.All,
            sledStart.position, sledStart.rotation.eulerAngles,
            bearStart.position, bearStart.rotation.eulerAngles);
        SnowballPool.Instance.ReturnAll(); // 눈덩이 풀도 리셋
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
        photonView.RPC(nameof(RPC_PrepareChase), RpcTarget.All);
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
                GameManager.Instance.SetAllCharactersActive(false);
            }
        );
    }
    
    [PunRPC]
    private void RPC_PrepareChase()
    {
        // 썰매 콜라이더 비활성화
        if (sledTriggerCollider != null)
        {
            sledTriggerCollider.enabled = false;
        }
    }

    [PunRPC]
    private void RPC_TeleportAll(Vector3 sledPos, Vector3 sledEuler, Vector3 bearPos, Vector3 bearEuler)
    {
        visual.SetActive(true);
        characterModels.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            sled.transform.SetPositionAndRotation(sledPos, Quaternion.Euler(sledEuler));
        }
        
        bear.transform.SetPositionAndRotation(bearPos, Quaternion.Euler(bearEuler));
        sled.GetComponent<Rigidbody>().isKinematic = false;
        bear.GetComponent<Rigidbody>().isKinematic = false;
        photonView.RPC(nameof(RPC_TeleportDone), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_TeleportDone()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        _finishedCount++;
        Debug.Log($"[teleportDone] finCount: {_finishedCount}");
        if (_finishedCount != PhotonNetwork.CurrentRoom.PlayerCount) return;
        
        // 썰매 소유권을 Hour에게 양도
        if (_hourActor > 0)
        {
            PhotonView sledView = sled.GetComponent<PhotonView>();
            if (sledView != null && sledView.OwnerActorNr != _hourActor)
                sledView.TransferOwnership(_hourActor);
        }

        photonView.RPC(nameof(RPC_BeginChase), RpcTarget.All, _hourActor, _milliActor);
    }

    [PunRPC]
    private void RPC_BeginChase(int hourActor, int milliActor)
    {
        // Hour: 썰매 조작 권한
        bool isHour = PhotonNetwork.LocalPlayer.ActorNumber == hourActor;
        bool isMilli = PhotonNetwork.LocalPlayer.ActorNumber == milliActor;

        sled.SetControl(isHour);           
        sled.SetSledMoving(true);                  
        bear.StartChase(); 

        // 카메라
        vCamFront.SetActive(isHour);
        vCamBack.SetActive(isMilli);

        // UI
        sledHp.Init();
        
        var uiControlHelp = UIManager.Instance.Show<UIControlHelp>("UIControlHelp");
        Sprite s1 = isHour ? Key.AD.LoadSprite() : Key.WASD.LoadSprite();
        string t1 = isHour ? "좌우이동" : "조준하기";
        Sprite s2 = Key.Space.LoadSprite(Style.Outline);
        string t2 = isHour ? "점프하기" : "발사하기";
        uiControlHelp.SetControl(s1, t1, s2, t2);
        
        if (isMilli && targetDetector != null)
        {
            targetDetector.enabled = true;
            targetDetector.Init();
        }
        
        snowballShooter.SetActive(true);
    }
    
    [PunRPC]
    private void RPC_ActivateBreakPoints(int milliActor)
    {
        snowballShooter.SetActive(false);

        if (PhotonNetwork.LocalPlayer.ActorNumber == milliActor)
        {
            destroyableIce.SetDestroyable();
        }
        else
        {
            sled.SetControl(false);
        }
    }

}
