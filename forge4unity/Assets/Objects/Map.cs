using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public Image mapImage;

    /// <summary>
    /// Initializes the Map Entity
    /// </summary>
    public void Initialize(string imageUrl)
    {
        mapImage = gameObject.AddComponent<Image>();
        StartCoroutine(LoadThumbnail(imageUrl));
    }

    /// <summary>
    /// Coroutine to ask for the Thumbnail and show it in the Editor
    /// </summary>

    IEnumerator LoadThumbnail(string imageUrl)
    {
        using UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Get the downloaded texture
            Texture2D texture = DownloadHandlerTexture.GetContent(www);

            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // Apply the sprite to the Image component
            if (mapImage != null)
                mapImage.sprite = sprite;
        }
        else
        {
            Debug.LogError("Failed to load image. Error: " + www.error);
        }
    }

}
