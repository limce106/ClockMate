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
    
    public void SetImagePosition(BreakPoint breakPoint)
    {
        img[_index].transform.position = Camera.main.WorldToScreenPoint(breakPoint.transform.position);
        _images[breakPoint] = img[_index++];
    }
    
    public void HideImage(BreakPoint breakPoint)
    {
        _images[breakPoint].gameObject.SetActive(false);
    }
}
