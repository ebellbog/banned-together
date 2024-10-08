using UnityEngine;

public class WebLink : MonoBehaviour
{
    public string url;

    public void OpenURL()
    {
        Application.OpenURL(url);
        Debug.Log($"Opened {url} in browser");
    }
}
