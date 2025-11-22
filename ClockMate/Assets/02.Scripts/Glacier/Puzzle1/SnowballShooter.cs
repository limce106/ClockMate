using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using static Define.Character;
using Random = UnityEngine.Random;

public class SnowballShooter : MonoBehaviourPun
{
    [SerializeField] private Transform sledTargetPos;
    [SerializeField] private TargetDetector targetDetector;
    [SerializeField] private Transform[] snowballGenPositions;

    [SerializeField] private ParticleSystem breathEffect;
    [SerializeField] private ParticleSystem roarEffect;
    [SerializeField] private string attackSfx;
    [SerializeField] private float attackSfxVolume;

    [SerializeField] private float fireInterval;

    private bool _active;
    private float _fireTimer;
    private static readonly bool[,] PATTERNS = new bool[,]
    {
        {false, false, false, true, false, false, false, false, false},
        {false, false, false, false, true, false, false, false, false},
        {true, false, false, false, false, false, false,false, false},
        {false, false, true, false, false, false, false, false, false},
        {false, false, false, false, false, false, true, false, false},
        {false, false, false, false, false, false, false, false, true},
    };

    private void Update()
    {
        if (!_active) return;
        if (!PhotonNetwork.IsMasterClient) return;

        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireInterval)
        {
            _fireTimer -= fireInterval;
            StartCoroutine(PrepareThenFire(1.2f));
        }
    }

    private void FireSnowball(int index)
    {
        // int pattern = Random.Range(0, PATTERNS.GetLength(0));
        //
        // for (int i = 0; i < snowballGenPositions.Length; i++)
        // {
        //     if (!PATTERNS[pattern, i]) continue;
        //
        //     Transform spawn = snowballGenPositions[i];
        //     Snowball snowball = SnowballPool.Instance.Get(
        //         spawn.position,
        //         Quaternion.identity
        //     );
        //     
        //     photonView.RPC(nameof(RPC_InitForAll), RpcTarget.All, snowball.photonView.ViewID);
        // }
        Transform spawn = snowballGenPositions[index];
        Snowball snowball = SnowballPool.Instance.Get(
            spawn.position,
            Quaternion.identity
        );
        
        photonView.RPC(nameof(RPC_InitForAll), RpcTarget.All, snowball.photonView.ViewID);
    }
    
    private IEnumerator PrepareThenFire(float prepareTime)
    {
        SoundManager.Instance.PlaySfx(key: attackSfx, volume: attackSfxVolume, sync: true);
        breathEffect.Play();
        yield return new WaitForSeconds(prepareTime);
        roarEffect.Play();
        int count = Random.Range(1, 4);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, snowballGenPositions.Length);
            FireSnowball(index);
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    public void SetActive(bool active)
    {
        _active = active;
        if (!active)
        {
            SnowballPool.Instance.ReturnAll();
        }
    }
    
    [PunRPC]
    private void RPC_InitForAll(int snowballViewID)
    {
        PhotonView pv = PhotonView.Find(snowballViewID);
        if (pv == null) return;
        if (!pv.TryGetComponent(out Snowball snowball)) return;
        
        // 각 클라이언트 로컬에서 눈덩이 타겟 세팅
        snowball.SetTarget(sledTargetPos);
        if (GameManager.Instance.SelectedCharacter != CharacterName.Milli) return;
        // 밀리만 조준을 위한 타겟 등록
        targetDetector.AddTarget(snowball);
    }
}

