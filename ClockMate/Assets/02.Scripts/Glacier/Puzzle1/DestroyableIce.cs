using System;
using Photon.Pun;
using UnityEngine;

public class DestroyableIce : MonoBehaviourPun
{
    [SerializeField] private ParticleSystem[] breakParticles;
    [SerializeField] private GameObject iceGo;
    [SerializeField] private BreakPoint[] breakPoints;
    [SerializeField] private TargetDetector targetDetector;
    [SerializeField] private GameObject[] afterDestroyGos;
    
    private int _breakPointCount;
    public UIIceBreakPoint UiIceBreakPoint { get; private set;}

    private void Awake()
    {
        Init();
    }
    
    private void Init()
    {
        _breakPointCount = breakPoints.Length;
    }
    
    public void SetDestroyable()
    {
        UiIceBreakPoint = UIManager.Instance.Show<UIIceBreakPoint>("UIIceBreakPoint");

        for (int i = 0; i < _breakPointCount; i++)
        {
            breakPoints[i].Init(this, targetDetector);
        }
    }
    
    private void AllDestroyed()
    {
        foreach (ParticleSystem particle in breakParticles)
        {
            particle.gameObject.SetActive(true);
            particle.Play();
        }
        iceGo.SetActive(false);
        foreach (GameObject go in afterDestroyGos)
        {
            go.SetActive(true);
        }
    }

    public void ReduceBreakPoint()
    {
        if (--_breakPointCount <= 0)
        {
            AllDestroyed();
        }
    }
}
