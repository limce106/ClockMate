using UnityEngine;

public class AudioListenerEnabler : MonoBehaviour
{
    private void OnDisable()
    {
        if (Camera.main != null) Camera.main.GetComponent<AudioListener>().enabled = true;
    }
    
    private void OnEnable()
    {
        if (Camera.main != null) Camera.main.GetComponent<AudioListener>().enabled = false;
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (var audioListener in audioListeners)
        {
            audioListener.enabled = false;
        }
        GetComponent<AudioListener>().enabled = true;
    }
}