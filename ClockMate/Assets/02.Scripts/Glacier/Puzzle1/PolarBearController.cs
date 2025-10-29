using Photon.Pun;
using UnityEngine;

public class PolarBearController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private SledController sled;
    [SerializeField] private float followDistance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private SnowballShooter shooter;
//    [SerializeField] private float jumpForce;
//    [SerializeField] private Transform front;
    
    private Rigidbody _rb;
    //private bool _isJumping;
    //private LayerMask _layerMask;
    private bool _chaseSled;
    private Vector3 _initPos;
    private Quaternion _initRot;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _rb = GetComponent<Rigidbody>();
        //_layerMask = LayerMask.GetMask("Ground");
        _initPos = transform.position;
        _initRot = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Instance.IsInRoomAndReady() && !photonView.IsMine) return;

        if (!_chaseSled) return; 

        // 썰매를 따라서 이동
        Vector3 toSled = sled.transform.position - transform.position;
        toSled.y = 0f;
        float dist = toSled.magnitude;

        // dist가 followDistance보다 클 때만 추격
        if (dist <= followDistance)
        {
            // 멈춤 처리
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
            return;
        }
        
        // if (NeedToJump())
        // {
        //     Jump();
        // }
        // else 
        // {
        //     Move(toSled.normalized);
        // }
        Move(toSled.normalized);
    }

    private void Move(Vector3 direction)
    {
        _rb.MovePosition(_rb.position + direction * (Time.fixedDeltaTime * moveSpeed));
        transform.forward = direction;
    }

    // private bool NeedToJump()
    // {
    //     Ray ray = new Ray(front.position, Vector3.down);
    //     return !_isJumping && Physics.Raycast(ray, 5.5f, _layerMask) == false;
    // }

    // private void Jump()
    // {
    //     _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    //     _isJumping = true;
    // }

    // private void OnCollisionEnter(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Ground"))
    //     {
    //         _isJumping = false;
    //     }
    // }

    public void SetChaseState(bool isActive)
    {
        //_rb.isKinematic = false;
        _chaseSled = isActive;
        shooter.SetActive(isActive);
    }

    public void ResetTransform()
    {
        if (NetworkManager.Instance.IsInRoomAndReady() && !photonView.IsMine) return;

        transform.position = _initPos;
        transform.rotation = _initRot;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
