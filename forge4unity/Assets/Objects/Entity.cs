using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    [SerializeField]
    protected Image entityImage;
    [SerializeField]
    protected EntityDetailsData entityData;
    [SerializeField]
    protected int entityTypeCounter;

    protected bool loadingImage;

    /// <summary>
    /// Initializes the Entity
    /// </summary>
    public virtual void Initialize(EntityDetailsData data, int entityCounter)
    {
        loadingImage = true;

        entityData = data;
        entityTypeCounter = entityCounter;

        if (data.image != null && data.image.thumbnail_url != null)
        {
            entityImage = gameObject.AddComponent<Image>();
            StartCoroutine(LoadThumbnail(data.image.thumbnail_url));
        }
        else
            loadingImage = false;
        
        if (data.map != null && data.map.thumbnail_url != null)
        {
            GameObject go = new(string.Format("{0} Map", gameObject.name));
            go.transform.parent = transform;
            Map entityMap = go.AddComponent<Map>();
            entityMap.Initialize(data.map.thumbnail_url);
        }
    }

    /// <summary>
    /// Coroutine to ask for the Entity Thumbnail
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
            if(entityImage != null)
                entityImage.sprite = sprite;
        }
        else
        {
            Debug.LogError("Failed to load image. Error: " + www.error);
        }
        loadingImage = false;
    }
}
