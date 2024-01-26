using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class Character : Entity
{
    private const string ASSISTANT_ID = "https://apidev.gameforge.ai/v1/entities/{id}/get_assistant/";
    private const string DUMMY = "GameForgeCharacterDummy";
    private const string PLACING = "[WAITING ON MAP]";

    private const float distance = 3f;
    
    private string meshName;

    public AssistantDetailsData assistantDetailsData;

    /// <summary>
    /// Initializes the Character Entity
    /// </summary>
    public override void Initialize(EntityDetailsData data, int entityCounter)
    {
        base.Initialize(data, entityCounter);
        meshName = string.Format("mesh::{0}", entityData.name);
        AssignAssistantId();
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
                dummy.name = meshName;
                break;
            }
        }
        // No placeholder was set - we instantiate the default dummy and take the coordinates from GameForge Point object
        if (dummy == null)
        {
            dummy = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(DUMMY))));
            dummy.name = PLACING + meshName;
            StartCoroutine(WaitToPaintOnMap(dummy));
        }


        if (dummy.GetComponentInChildren<RuntimeCharacter>() != null)
            dummy.GetComponentInChildren<RuntimeCharacter>().Initialize(this);
    }

    /// <summary>
    /// Adds the Mesh to the entity
    /// </summary>
    void AssignAssistantId()
    {
        if (!string.IsNullOrEmpty(entityData.assistant))
            return;

        UnityWebRequest uwr = UnityWebRequest.Get(ASSISTANT_ID.Replace("{id}", entityData.id));
        uwr.SetRequestHeader("Authorization", Utils.GetAuthHeader());
        uwr.SendWebRequest();

        while (!uwr.isDone) { };

        // Check for errors
        if (uwr.result != UnityWebRequest.Result.Success)
            Debug.LogError(string.Format("Unable to create a Conversational Assistant for {0}. Error: {1}", entityData.name, uwr.error));
        else
        {
            AssistantData assistantData = JsonUtility.FromJson<AssistantData>(uwr.downloadHandler.text);
            
            if(assistantData.assistant == null)
                Debug.Log(string.Format("Unable to retrieve assistant details for {0}", entityData.name));
            else
            {
                assistantDetailsData = assistantData.assistant;
                entityData.assistant = assistantDetailsData.id;
                entityData.openai_assistant_id= assistantDetailsData.openai_assistant_id;
            }
        }
    }


    /// <summary>
    /// Paints the image on top of the mesh
    /// </summary>
    IEnumerator WaitToPaintOnMap(GameObject dummy)
    {
        if (string.IsNullOrEmpty(entityData.parent_map_id))
        {
            dummy.name = meshName;
            SpawnWithNoPosition(dummy);
            yield return null;
        }

        MapSpriteRenderer map;
        int timeout = 5;
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
        
        dummy.name = meshName;

        if (map == null)
            SpawnWithNoPosition(dummy);
        else
        {
            Vector3 position = new(
                entityData.point.latitude + map.transform.parent.position.x,
                0f,
                entityData.point.longitude + map.transform.parent.position.y);
            dummy.transform.position = position;
            dummy.transform.SetParent(map.transform, true);            
            dummy.transform.position /= 100f;
            dummy.transform.position = new Vector3(
                dummy.transform.position.x + map.transform.position.x,
                0f,
                dummy.transform.position.z + map.transform.position.z);
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
