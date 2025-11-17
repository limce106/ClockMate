using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class UIMapDescription : UIBase
{
    [SerializeField] private GameObject _mapDescriptionObj;
    [SerializeField] private TMP_Text _mapName;

    private CanvasGroup _canvasGroup;
    private Dictionary<string, string> _mapNames;
    private Coroutine _fadeInOut;

    private const float FadeDuration = 1.0f;
    private const float DisplayDuration = 2.0f;

    void Awake()
    {
        _mapNames = new Dictionary<string, string>()
        {
            { "Desert", "¹ö·ÁÁø ¸ð·¡ ¾ð´ö" },
            { "Glacier", "²Ç²Ç ¾ó¾îºÙÀº ¹Ù´Ù" },
            { "Forest", "ÀØÈù ½£ÀÇ ¼Ó»èÀÓ" },
            { "ClockTower", "½Ã°£ÀÌ ¸ØÃá Å¾" }
        };

        _canvasGroup = _mapDescriptionObj.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        ShowMapDescription();
    }

    private void OnDisable()
    {
        if(_fadeInOut != null)
            StopCoroutine(_fadeInOut);
    }

    private void SetMapNameForScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        _mapName.text = _mapNames[currentScene];
    }

    private void ShowMapDescription()
    {
        SetMapNameForScene();
        _fadeInOut = StartCoroutine(FadeInOut());
    }

    private IEnumerator FadeInOut()
    {
        float time = 0f;

        while (time < FadeDuration)
        {
            time += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / FadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(DisplayDuration);

        time = 0f;
        while (time < FadeDuration)
        {
            time += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / FadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _fadeInOut = null;
        gameObject.SetActive(false);
    }
}
