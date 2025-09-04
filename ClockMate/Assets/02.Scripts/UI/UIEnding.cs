using Define;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEnding : UIBase
{
    [SerializeField] private Button btnToTitle;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        UIType = UI.UIType.FullScreen;
        btnToTitle.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("TitleMatch");
        });
    }
}
