using Define;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleRevive : UIBase
{
    [SerializeField] private Image timer;
    [SerializeField] private Image characterIcon;

    private Color _originalColor;
    private Image characterImg;
    private Coroutine _fillTimerCoroutine;

    public void Init(string deadCharacter, float duration)
    {
        Sprite character = Resources.Load<Sprite>("UI/Sprites/Character/Revive_" + deadCharacter + "_Face");
        characterIcon.sprite = character;
        ApplyQuarterSize();

        _originalColor = characterIcon.color;
        timer.fillAmount = 0f;

        characterImg = characterIcon.GetComponent<Image>();
        characterImg.color = Color.gray;

        PlayReviveTimerUI(duration);
    }

    public void PlayReviveTimerUI(float duration)
    {
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
        characterImg.color = _originalColor;

        yield return new WaitForSeconds(0.2f);

        UIManager.Instance.Close(this);
        _fillTimerCoroutine = null;
    }

    /// <summary>
    /// 캐릭터 얼굴 이미지를 원본 크기/4 값으로 설정
    /// </summary>
    private void ApplyQuarterSize()
    {
        if (characterImg == null) return;

        characterImg.SetNativeSize();

        RectTransform rt = characterImg.rectTransform;
        rt.sizeDelta = rt.sizeDelta / 4;
    }
}
