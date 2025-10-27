using System.Collections;
using Cinemachine;
using DefineExtension;
using Photon.Pun;
using UnityEngine;
using static Define.Character;
using static Define.Icon;

[RequireComponent(typeof(PhotonView))]
public class ChaseControlModule : MonoBehaviourPun
{
    [SerializeField] private SledController sled;
    [SerializeField] private PolarBearController bear;
    [SerializeField] private CinemachineVirtualCamera VcamHour;
    [SerializeField] private CinemachineVirtualCamera VcamMilli;
    [SerializeField] private float restartWaitTime = 3f;

    private UIControlHelp _uiControlHelp; 
    /// <summary>
    /// 북극곰 썰매 추격 메서드를 호출한다. 서버 연결 상태에 따라 분기한다.
    /// </summary>
    public void StartChase()
    {
        if (NetworkManager.Instance.IsInRoomAndReady())
        {
            photonView.RPC(nameof(RPC_StartChase), RpcTarget.All);
            // 썰매 소유권은 아워가 가지도록 설정
            int houActorNr = GameManager.Instance.Characters[CharacterName.Hour].photonView.OwnerActorNr;
            if (sled.photonView.OwnerActorNr != houActorNr)
            {
                sled.photonView.TransferOwnership(houActorNr);
            }
        }
        else
        {
            RPC_StartChase();
        }
    }
    
    /// <summary>
    /// 실제 북극곰 썰매 추격 로직을 실행한다.
    /// 썰매와 북극곰을 활성화하고 움직이는 상태로 만든다.
    /// 선택 캐릭터에 맞는 카메라와 UI를 활성화한다.
    /// </summary>
    [PunRPC]
    private void RPC_StartChase()
    {
        sled.gameObject.SetActive(true);
        sled.SetSledMoveState(true);
        bear.gameObject.SetActive(true);
        bear.StartChase();
        // 아워 & 밀리 각자 카메라 세팅
        CharacterName character = GameManager.Instance.SelectedCharacter;
        if (character is CharacterName.Hour)
        {
            VcamHour.gameObject.SetActive(true);
        }
        else
        {
            VcamMilli.gameObject.SetActive(true);
        }
        // UI 표시 - 체력 & 조작 도움
        ActivateChaseUI();
    }
    
    public void StopChase()
    {
        
    }
    
    /// <summary>
    /// 썰매의 체력이 0이 될 시 호출한다.
    /// 썰매와 북극곰을 비활성화 한다.
    /// 정해진 시간만큼 대기 후 시작 상태로 초기화한다.
    /// </summary>
    public void RestartChase()
    {
        if (NetworkManager.Instance.IsInRoomAndReady())
        {
            photonView.RPC(nameof(RPC_RestartChase), RpcTarget.All);
        }
        else
        {
            RPC_RestartChase();
        }
    }
    
    [PunRPC]
    private void RPC_RestartChase()
    {
        // 썰매와 북극곰 비활성화
        sled.gameObject.SetActive(false);
        bear.gameObject.SetActive(false);

        StartCoroutine(WaitAndReset(restartWaitTime));
    }

    private IEnumerator WaitAndReset(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Reset();
    } 
    
    /// <summary>
    /// 실제 초기화 로직을 실행한다.
    /// 썰매와 북극곰을 초기 위치로 이동시키고 활성화한다.
    /// 썰매의 체력 및 눈덩이 풀도 초기화한다.
    /// </summary>
    private void Reset()
    {
        // 시작 위치로 이동 & 활성화
        sled.ResetTransform();
        bear.ResetTransform();
        sled.gameObject.SetActive(true);
        bear.gameObject.SetActive(true);
        
        // 체력 리셋
        sled.Hp.Init();
        
        // 눈덩이 풀 리셋
        SnowballPool.Instance.ReturnAll();
    }
    
    /// <summary>
    /// 썰매 추격 퍼즐의 끝에 도달하여 종료 트리거에 접촉했을 시 호출한다.
    /// 빙하 퍼즐 1 완료 컷신을 재생하고 컷신 재생이 끝나면 스테이지 클리어 메서드를 호출한다.
    /// </summary>
    public void FinishChase()
    {
        CutsceneSyncManager.Instance.PlayForAll(
            "SledChaseFinish",
            0f,
            () =>
            {
                photonView.RPC(nameof(RPC_OnChaseFinCutsceneEnd), RpcTarget.All);
            }
        );
        // 아워 & 밀리 각자 카메라
        CharacterName character = GameManager.Instance.SelectedCharacter;
        if (character is CharacterName.Hour)
        {
            VcamHour.gameObject.SetActive(false);
        }
        else
        {
            VcamMilli.gameObject.SetActive(false);
        }
        _uiControlHelp?.Close();
        sled.Hp.CloseUI();
        sled.SetSledMoveState(false);
        sled.gameObject.SetActive(false);
        bear.gameObject.SetActive(false);
    }
    
    private void ActivateChaseUI()
    {
        sled.Hp.Init();
        
        _uiControlHelp = UIManager.Instance.Show<UIControlHelp>("UIControlHelp");
        bool isHour = GameManager.Instance.SelectedCharacter == CharacterName.Hour;
        Sprite s1 = isHour ? Key.AD.LoadSprite() : Key.WASD.LoadSprite();
        string t1 = isHour ? "좌우이동" : "조준하기";
        Sprite s2 = Key.Space.LoadSprite(Style.Outline);
        string t2 = isHour ? "점프하기" : "발사하기";
        _uiControlHelp.SetControl(s1, t1, s2, t2);
    }
    
    [PunRPC]
    private void RPC_OnChaseFinCutsceneEnd()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        GameManager.Instance.StageComplete();
        GameManager.Instance.ResetStage();
    }
}
