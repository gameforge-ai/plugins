using System.Collections;
using System.Linq;
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
        CharacterPlaceholder[] placeholders = FindObjectsByType<CharacterPlaceholder>(FindObjectsSortMode.None);
        GameObject dummy = null;
        foreach (CharacterPlaceholder placeholder in placeholders)
        {
            if (placeholder.character.ToLower() == entityData.name.ToLower())
            {
                dummy.transform.SetPositionAndRotation(placeholder.transform.position, Quaternion.Euler(0f, 0f, 0f));

                if (placeholder.characterPrefab != null)
                    dummy = Instantiate(placeholder.characterPrefab); // Placeholder with a predefined prefab
                else
                    dummy = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(DUMMY)))); // default dummy
                
                dummy.transform.parent = placeholder.gameObject.transform;
                break;
            }
        }
        // No placeholder was set - we instantiate the default dummy and take the coordinates from GameForge Point object
        if (dummy == null)
        {
            dummy = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(DUMMY))));
            StartCoroutine(WaitToPaintOnMap(dummy));
        }

        dummy.name = entityData.name;

        if (dummy.GetComponentInChildren<InstantiatedCharacter>() != null)
            dummy.GetComponentInChildren<InstantiatedCharacter>().Initialize(this);
    }

    /// <summary>
    /// Paints the image on top of the mesh
    /// </summary>
    IEnumerator WaitToPaintOnMap(GameObject dummy)
    {
        if (string.IsNullOrEmpty(entityData.parent_map_id))
        {
            SpawnWithNoPosition(dummy);
            yield return null;
        }

        MapSpriteRenderer map;
        int timeout = 30;
        while(true)
        {
            map = GetMapById(entityData.parent_map_id);
            if(map == null)
            {
                timeout--;
                if (timeout > 0)
                    yield return new WaitForSecondsRealtime(1);
                else
                    break;
            }                
            else
                break;
        }

        if (map == null)
        {
            Debug.LogError(string.Format("Timeout expired waiting for the map {0} associated to character `{1}`", entityData.parent_map_id, entityData.name));
            SpawnWithNoPosition(dummy);
        }
        else
        {
            // dummy.transform.parent = map.gameObject.transform;
            Vector3 position = new(
                entityData.point.latitude + map.transform.parent.position.x,
                0f,
                entityData.point.longitude + map.transform.parent.position.y);
            dummy.transform.position = position;
            dummy.transform.SetParent(map.transform, true);
            dummy.transform.position /= 100f;
        }
    }

    MapSpriteRenderer GetMapById(string id)
    {
        return FindObjectsByType<MapSpriteRenderer>(FindObjectsSortMode.None).DefaultIfEmpty(null).Where(x => x.mapId == id).FirstOrDefault();
    }

    void SpawnWithNoPosition(GameObject dummy)
    {
        Vector3 position = new(distance * entityTypeCounter, 0, 0);
        Debug.LogWarning(string.Format("Character {0} does not have a placehoder or any map associated in GameForge. Instantiating at {1}", entityData.name, position));
        dummy.transform.position = position;
        dummy.transform.parent = transform;
    }
}
