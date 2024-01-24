using TMPro;
using UnityEngine;


[RequireComponent (typeof(TextMeshProUGUI))]
public class CloseToTalkNotification : MonoBehaviour
{
    /// <summary>
    /// Sets the text of the notification widget about who are you talking to (if any)
    /// </summary>
    public void SetText(string text)
    {
        GetComponent<TextMeshProUGUI>().text = text;
    }

    /// <summary>
    /// Hides or Shows the text
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        GetComponent<TextMeshProUGUI>().enabled = enabled;
    }
}
