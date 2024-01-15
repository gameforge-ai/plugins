using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Character : Entity
{
    private const string DUMMY = "GameForgeCharacterDummy";

    private const float distance = 3f;

    /// <summary>
    /// Initializes the Character Entity
    /// </summary>
    public override void Initialize(EntityDetailsData data, int entityCounter)
    {
        base.Initialize(data, entityCounter);
        InitializeMesh();
    }

    /// <summary>
    /// Adds the Mesh to the entity
    /// </summary>
    void InitializeMesh()
    {
        Vector3 position = new(distance * entityTypeCounter, 0, 0);
        CharacterPlaceholder[] placeholders = FindObjectsByType<CharacterPlaceholder>(FindObjectsSortMode.None);
        GameObject dummy = null;
        foreach (CharacterPlaceholder placeholder in placeholders)
        {
            if (placeholder.character.ToLower() == entityData.name.ToLower())
            {
                position = placeholder.transform.position;

                if (placeholder.characterPrefab != null)
                    dummy = Instantiate(placeholder.characterPrefab); // Placeholder with a predefined prefab
                else
                    dummy = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(DUMMY)))); // default dummy
                
                dummy.transform.parent = placeholder.gameObject.transform;
            }
        }
        // No placeholder was set - we instantiate the default dummy
        if (dummy == null)
        {
            dummy = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(DUMMY))));
            dummy.transform.parent = transform;
        }

        dummy.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, 0f));
        dummy.transform.LookAt(Camera.main.transform);
        dummy.name = entityData.name;
        if (dummy.GetComponentInChildren<TextMeshPro>() != null)
            dummy.GetComponentInChildren<TextMeshPro>().text = entityData.name;
        if (dummy.GetComponentInChildren<VoiceConversationController>() == null)
            dummy.AddComponent<VoiceConversationController>();
        if (!string.IsNullOrEmpty(entityData.openai_assistant_id))
            dummy.GetComponent<VoiceConversationController>().assistantId = entityData.openai_assistant_id;
        StartCoroutine(WaitToPaintImage(dummy));
    }


    /// <summary>
    /// Paints the image on top of the mesh
    /// </summary>
    IEnumerator WaitToPaintImage(GameObject dummy)
    {
        yield return new WaitWhile(() => loadingImage);
        if (dummy.GetComponentInChildren<SpriteRenderer>() != null && entityImage.sprite != null)
            dummy.GetComponentInChildren<SpriteRenderer>().sprite = entityImage.sprite;
    }
}
