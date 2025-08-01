using System.Collections.Generic;
using UnityEngine;

public class SnowballPoolModule : MonoBehaviour
{
    [SerializeField] private Snowball snowballPrefab;
    [SerializeField] private int poolSize;
    [SerializeField] private SledHP sledHP;

    private readonly Queue<Snowball> _pool = new();
    public List<Snowball> ActiveSnowballs {get; private set;}

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        ActiveSnowballs = new List<Snowball>();
        for (int i = 0; i < poolSize; i++)
        {
            Snowball snowball = Instantiate(snowballPrefab);
            snowball.Init(this, sledHP);
            snowball.gameObject.SetActive(false);
            _pool.Enqueue(snowball);
        }
    }

    public Snowball Get()
    {
        if (_pool.Count > 0)
        {
            Snowball sb = _pool.Dequeue();
            sb.gameObject.SetActive(true);
            ActiveSnowballs.Add(sb);
            return sb;
        }
        else
        {
            Snowball snowball = Instantiate(snowballPrefab);
            snowball.Init(this, sledHP);
            ActiveSnowballs.Add(snowball);
            return snowball;
        }
    }

    public void ReturnToPool(Snowball snowball)
    {
        snowball.gameObject.transform.position = Vector3.zero;
        snowball.gameObject.SetActive(false);
        ActiveSnowballs.Remove(snowball);
        _pool.Enqueue(snowball);
    }
}