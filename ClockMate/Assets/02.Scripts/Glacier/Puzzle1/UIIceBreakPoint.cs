using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIceBreakPoint : UIBase
{
    [SerializeField] private Image[] img;
    private Dictionary<BreakPoint, Image> _images;
    private int _index;
    
    private void Awake()
    {
        Init();
    }
    
    private void Init()
    {
        _index = 0;
        _images = new Dictionary<BreakPoint, Image>();
    }
    
    private void Update()
    {
        if (_images.Count <= 0) return;
            
        foreach (var kv in _images)
        {
            kv.Value.transform.position = Camera.main.WorldToScreenPoint(kv.Key.transform.position);
        }
    }
    
    public void SetImage(BreakPoint breakPoint)
    {
        _images[breakPoint] = img[_index++];
    }
    
    public void HideImage(BreakPoint breakPoint)
    {
        _images[breakPoint].gameObject.SetActive(false);
    }
}
