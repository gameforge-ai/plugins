using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class Map : MonoBehaviour
{
    private const string WHITEBOX = "GameForgeMapWhitebox";

    private const float distance = 50f;

    public EntityDetailsData entityData;

    /// <summary>
    /// Initializes the Map Entity
    /// </summary>
    public void Initialize(EntityDetailsData data, int entityTypeCounter)
    {
        entityData = data;
        InstantiatePlane(data.map.url, entityTypeCounter);
    }
    /// <summary>
    /// Initializes the Map Plance
    /// </summary>
    void InstantiatePlane(string imageUrl, int entityTypeCounter)
    {
        Vector3 position = new(distance * entityTypeCounter, 0, 0);
        MapPlaceholder[] placeholders = FindObjectsByType<MapPlaceholder>(FindObjectsSortMode.None);
        GameObject plane = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(WHITEBOX)))); // default dummy
        plane.GetComponent<MapSpriteRenderer>().Initialize(entityData.id);
        bool found = false;
        foreach (MapPlaceholder placeholder in placeholders)
        {
            if (placeholder.map.ToLower() == entityData.name.ToLower())
            {
                position = placeholder.transform.position;
                plane.transform.parent = placeholder.gameObject.transform;
                found = true;
                break;
            }
        }
        if (!found)
            plane.transform.parent = transform;
        plane.transform.SetPositionAndRotation(position, Quaternion.Euler(90f, 0f, 0f));
        plane.name = entityData.name;
        StartCoroutine(PaintMapOnPlane(imageUrl, plane));
    }

    /// <summary>
    /// Paints the image on top of the mesh
    /// </summary>
    IEnumerator PaintMapOnPlane(string imageUrl, GameObject plane)
    {
        using UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Get the downloaded texture
            Texture2D texture = DownloadHandlerTexture.GetContent(www);

            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            //plane.transform.localScale = new Vector3(texture.width, texture.height, 1f);
            plane.transform.localScale = Vector3.one;

            plane.GetComponent<MapSpriteRenderer>().PaintSprite(sprite);
        }
        else
            Debug.LogError("Failed to load image. Error: " + www.error);
    }
}
