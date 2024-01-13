using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Character : Entity
{
    private const string DUMMY = "GameForgeCharacterDummy";
    private const string MESH = "[Mesh]";

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
    /// Adds a Dummy Mesh to the Entity
    /// </summary>
    void InitializeMesh()
    {
        Vector3 position = new(distance * entityTypeCounter, 0, 0);
        CharacterPlaceholder[] placeholders = FindObjectsByType<CharacterPlaceholder>(FindObjectsSortMode.None);
        foreach(CharacterPlaceholder placeholder in placeholders)
            if(placeholder.character.ToLower() == entityData.name.ToLower())
                position = placeholder.transform.position;
        GameObject dummy = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(DUMMY))));
        dummy.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 180f, 0f));
        dummy.name = MESH + entityData.name;
        dummy.transform.parent = transform;
        if(dummy.GetComponentInChildren<TextMeshPro>() != null) 
            dummy.GetComponentInChildren<TextMeshPro>().text = entityData.name;
        StartCoroutine(WaitToPaintImage(dummy));
    }

    IEnumerator WaitToPaintImage(GameObject dummy)
    {
        yield return new WaitWhile(() => loadingImage);
        if (dummy.GetComponentInChildren<SpriteRenderer>() != null && entityImage.sprite != null)
            dummy.GetComponentInChildren<SpriteRenderer>().sprite = entityImage.sprite;
    }
}
