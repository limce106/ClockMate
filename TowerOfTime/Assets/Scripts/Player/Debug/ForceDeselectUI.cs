using UnityEngine;
using UnityEngine.EventSystems;

public class ForceDeselectUI : MonoBehaviour
{
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}