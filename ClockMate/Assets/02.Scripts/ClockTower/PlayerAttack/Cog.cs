using System.Collections;
using DefineExtension;
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

    private PhotonView _pv;
    private Rigidbody _rb;
    private bool _carried;

    // 위치 동기화용
    public Vector3 NetPos {get; private set;}
    private Quaternion _netRot;
    // 각 클라가 보낸 월드 이동 벡터
    private Vector3 _worldMoveA = Vector3.zero;
    private Vector3 _worldMoveB = Vector3.zero;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        _rb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        if (!_carried) return;
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

        if (!_carried) return;
        
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
        if (!gripA.IsOccupied || !gripB.IsOccupied)
        {
            _carried = false;
            _rb.isKinematic = false;
            character.Anim.SetCarry(false);
            return;
        }

        _carried = true;
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        character.Anim.PlayPickUp();
        StartCoroutine(PickUpThenSetPos(character));
    }
    
    private IEnumerator PickUpThenSetPos(CharacterBase character)
    {
        yield return new WaitForSeconds(1.0f);
        // 들어올리는 애니메이션 재생 기다린 뒤 물건 위치 이동
        
        // 톱니 키네틱 전환
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        
        // 들어올림
        Vector3 pos = transform.position;
        pos.y += carryHeight;
        transform.position = pos;
        
        character.Anim.SetCarry(true);
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
            NetPos = (Vector3)stream.ReceiveNext();
            _netRot = (Quaternion)stream.ReceiveNext();
        }
    }

    private void LateUpdate()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // 누적 오차 제거
            transform.position = NetPos;
            transform.rotation = _netRot;
        }
    }
}
