using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISaving : MonoBehaviour
{
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private Text dotTxt;
    [SerializeField] private float dotInterval = 0.4f;
    [SerializeField] private float duration = 3f;
    
    private void Start()
    {
        uiRoot.SetActive(false);
        SaveManager.Instance.OnMasterSave -= ShowUI; 
        SaveManager.Instance.OnMasterSave += ShowUI;
    }

    private void ShowUI()
    {
        uiRoot.SetActive(true);
        StartCoroutine(EllipsisLoop());
        StartCoroutine(CloseAfterDelay());
    }
    
    private void HideUI()
    {
        StopAllCoroutines();
        uiRoot.SetActive(false);
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(duration);
        HideUI();
    }
    
    private IEnumerator EllipsisLoop()
    {
        int dotCount = 1;

        while (true)
        {
            // 점 개수 1~3 반복
            if (dotCount > 3)
                dotCount = 1;

            string dots = new string('.', dotCount);
            dotTxt.text = $"{dots}";

            dotCount++;
            yield return new WaitForSeconds(dotInterval);
        }
    }
}
