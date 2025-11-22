using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView)), RequireComponent(typeof(Rigidbody))]
public class Cog : MonoBehaviourPun, IPunObservable
{
    [field: SerializeField] public int Id {get; set;}

    [Header("Carry Settings")]
    [Tooltip("두 명이 붙었을 때 들어올릴 높이")]
    [SerializeField] private float carryHeight = 0.35f;
    [SerializeField] private float moveSpeed = 3.0f;
    
    [Header("Grips")]
    [SerializeField] private IACogGrip gripA;
    [SerializeField] private IACogGrip gripB;

    [field: SerializeField] public Slot Slot {get; private set; }
    // 마스터만 사용(집계)
    private HashSet<int> _finishedPlayers; // 완료 인원

    private Rigidbody _rb;
    

    // 위치 동기화용
    private Vector3 _netPos;
    private Quaternion _netRot;
    // 각 클라가 보낸 월드 이동 벡터
    private Vector3 _worldMoveA = Vector3.zero;
    private Vector3 _worldMoveB = Vector3.zero;

    public bool Carried {get; private set;}

    // 톱니바퀴가 올바른 위치에 끼워졌는지 여부
    public bool Fitted {get; private set;}

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _finishedPlayers = new HashSet<int>();
    }

    private void OnEnable()
    {
        Carried = false;
        Fitted = false;
        gripA.gameObject.SetActive(true);
        gripB.gameObject.SetActive(true);
        _rb.isKinematic = false;
    }

    private void Update()
    {
        if (!Carried) return;
        // 들어올려진 상태라면 플레이어 입력 읽기
        
        // 카메라 기준 입력 => 월드 벡터
        Vector2 moveAxis = ReadMoveAxis();

        Vector3 camF = Vector3.forward;
        Vector3 camR = Vector3.right;

        if (Camera.main is not null)
        {
            camF = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            camR = Vector3.ProjectOnPlane(Camera.main.transform.right,   Vector3.up).normalized;
        }

        Vector3 world = camF * moveAxis.y + camR * moveAxis.x;
        bool isA = PhotonView.Find(gripA.HolderViewId).IsMine;
        photonView.RPC(nameof(RPC_SetWorldMove), RpcTarget.MasterClient, isA, world);
    }
    
    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!Carried) return;
        
        Vector3 sum = _worldMoveA + _worldMoveB;
        if (sum.sqrMagnitude > 0.0001f)
        {
            transform.position += sum.normalized * (moveSpeed * Time.fixedDeltaTime);
        }
    }
    
    private Vector2 ReadMoveAxis()
    {
        // 이동 입력 처리
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) input.y += 1;
        if (Input.GetKey(KeyCode.S)) input.y -= 1;
        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        if (Input.GetKey(KeyCode.D)) input.x += 1;
        return input.normalized;
    }
    
    [PunRPC]
    private void RPC_SetWorldMove(bool isA, Vector3 worldMove)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (isA) _worldMoveA = worldMove;
        else     _worldMoveB = worldMove;
    }

    /// <summary>
    /// 두 개의 Grip이 모두 점유되었다면 들어올려지도록 한다.
    /// 둘 중 하나라도 점유되지 않았다면 운반 상태를 취소한다.
    /// </summary>
    public void OnGripStateChange()
    {
        CharacterBase character = GameManager.Instance.Characters[GameManager.Instance.SelectedCharacter];
        if (gripA.IsOccupied || gripB.IsOccupied)
        {
            // _rb.velocity = Vector3.zero;
            // _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, -1.4f, pos.z);
            transform.rotation = new Quaternion(0, 0, 0, 0);
//            GetComponent<Collider>().enabled = false;
        }
        else if(!Fitted)
        {
            _rb.isKinematic = false;
  //          GetComponent<Collider>().enabled = true;
        }
        if (!gripA.IsOccupied || !gripB.IsOccupied)
        {
            Carried = false;
//            _rb.velocity = Vector3.zero;
            transform.rotation = new Quaternion(0, 0, 0, 0);
            character.Anim.SetCarry(false);
            _finishedPlayers.Clear(); // 내려놓았으면 끼우기 완료 취소
            Slot.ActivateTrigger(false);
            return;
        }

        Carried = true;
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        character.Anim.PlayPickUp();
        StartCoroutine(PickUpThenSetPos(character));
    }
    
    private IEnumerator PickUpThenSetPos(CharacterBase character)
    {
        yield return new WaitForSeconds(1.0f);
        // 들어올리는 애니메이션 재생 기다린 뒤 물건 위치 이동
        
        // 기울기 제거
        transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        
        // 들어올림
        Vector3 pos = transform.position;
        pos.y += carryHeight;
        transform.position = pos;

        character.Anim.SetCarry(true);
    }
    
    /// <summary>
    ///  톱니바퀴가 올바른 슬롯에 끼워졌을 때 호출한다.
    ///  두 플레이어 모두 끼우기 상호작용을 완료했다면 
    ///  톱니바퀴를 슬롯 위치에 고정시키고 플레이어와의 상호작용을 비활성화한다.
    /// </summary>
    [PunRPC]
    private void RPC_FitCogToSlot()
    {
        Fitted = true;
        
        // 톱니바퀴 grip 두개 모두 비활성화
        gripA.gameObject.SetActive(false);
        gripA.Release();
        gripB.gameObject.SetActive(false);
        gripB.Release();
        
        _rb.isKinematic = true;
//        GetComponent<Collider>().enabled = true;
        
        // 톱니바퀴 슬롯에 끼워져야하는 위치/회전값으로 fix, 키네틱 전환
        gameObject.transform.position = Slot.transform.position;
        gameObject.transform.rotation = Slot.transform.rotation;
        
        // todo 톱니바퀴 끼워지는 이펙트와 사운드 추가
    }
    
    [PunRPC]
    public void RPC_ReportFitCog(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _finishedPlayers.Add(viewID); // 완료 저장
        TryFitCog(); // 종료 조건 만족 여부 확인
    }

    /// <summary>
    /// 종료 집계 후 처리
    /// </summary>
    private void TryFitCog()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // 기대 인원이 0이거나 모든 기대 인원이 모두 완료하였다면 종료
        if (_finishedPlayers.Count == 2)
        {
            photonView.RPC(nameof(RPC_FitCogToSlot), RpcTarget.All);
            _finishedPlayers.Clear();
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 마스터
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else // 클라
        {
            _netPos = (Vector3)stream.ReceiveNext();
            _netRot = (Quaternion)stream.ReceiveNext();
        }
    }

    private void LateUpdate()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // 누적 오차 제거
            transform.position = _netPos;
            transform.rotation = _netRot;
        }
    }
}
