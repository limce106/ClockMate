using Define;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleRevive : UIBase
{
    [SerializeField] private Image timer;
    [SerializeField] private Image reviveIcon;

    private Coroutine _fillTimerCoroutine;

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        timer.fillAmount = 0f;
        Sprite reviveBegin = Resources.Load<Sprite>("UI/Sprites/Revive_Icon_Beginning");
        reviveIcon.sprite = reviveBegin;
    }

    public void PlayReviveTimerUI(float duration, Vector3 uiPos)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = uiPos;

        _fillTimerCoroutine = StartCoroutine(FillTimer(duration));
    }

    public void StopReviveTimerUI()
    {
        if(_fillTimerCoroutine != null)
        {
            StopCoroutine(_fillTimerCoroutine);
        }

        UIManager.Instance.Close(this);
    }

    private IEnumerator FillTimer(float duration)
    {
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            timer.fillAmount = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }

        timer.fillAmount = 1f;

        Sprite reviveComplete = Resources.Load<Sprite>("UI/Sprites/Revive_Icon_Complete");
        reviveIcon.sprite = reviveComplete;

        yield return new WaitForSeconds(0.2f);

        UIManager.Instance.Close(this);
        _fillTimerCoroutine = null;
    }
}
