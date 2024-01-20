using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class Importer : MonoBehaviour
{
    private static readonly string CAMPAIGNS_URL = "https://apidev.gameforge.ai/v1/campaigns/";
    private static readonly string CAMPAIGN_URL = "https://apidev.gameforge.ai/v1/campaigns/{id}/";
    private static readonly string ENTITIES_URL = "https://apidev.gameforge.ai/v1/campaigns/{id}/entities/";
    private static readonly string ENTITY_DETAILS_URL = "https://apidev.gameforge.ai/v1/entities/{id}/";

    private Dictionary<EntityEnum, int> spawnCounter;
    /// <summary>
    /// Initializes the Importer class, which retrieves the token and triggers the pulling of objects
    /// </summary>
    public void Initialize()
    {
        spawnCounter = new Dictionary<EntityEnum, int>();
        ImportErrorEnum errors;
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("token")))
            errors = ImportErrorEnum.AUTHENTICATION_ERROR;
        else
            errors = PullCampaigns();

        ImporterResultsDialog results = ScriptableObject.CreateInstance<ImporterResultsDialog>();
        results.Initialize(errors);
        results.Show();

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(0f, 1f, -5f);

        DestroyImmediate(gameObject);
    }
    /// <summary>
    /// Calls GameForge API and retrieves all campaigns to look for the selected one
    /// </summary>
    ImportErrorEnum PullCampaigns()
    {
        ImportErrorEnum errors = ImportErrorEnum.NO_ERROR;
        UnityWebRequest uwr = UnityWebRequest.Get(CAMPAIGNS_URL);
        uwr.SetRequestHeader("Authorization", Utils.GetAuthHeader());
        uwr.SendWebRequest();

        while (!uwr.isDone) { };

        if (uwr.error != null)
        {
            Debug.LogError("Unable to retrieve campaigns");
            errors = ImportErrorEnum.AUTHENTICATION_ERROR;
        }
        else
        {
            string response = "{\"results\":" + uwr.downloadHandler.text + "}";
            CampaignsData campaignsData = JsonUtility.FromJson<CampaignsData>(response);
            bool found = false;
            foreach(CampaignData campaign in campaignsData.results)
            {
                if(campaign.id == PlayerPrefs.GetString("campaignId") || campaign.name == PlayerPrefs.GetString("campaignId"))
                {
                    errors = PullCampaign(campaign.id);
                    if(errors == ImportErrorEnum.NO_ERROR)
                        found = true;
                }                    
            }
            if (!found)
            {
                if (errors == ImportErrorEnum.CANCELLED_ERROR)
                {
                    Debug.Log(string.Format("Campaign {0} already exists and was cancelled by user.", PlayerPrefs.GetString("campaignId")));
                }
                if (errors == ImportErrorEnum.NO_ERROR)
                {
                    errors = ImportErrorEnum.CAMPAIGN_ERROR;
                    Debug.Log(string.Format("Campaign not found: {0}", PlayerPrefs.GetString("campaignId")));
                }                
            }
        }
        
        return errors;
    }

    /// <summary>
    /// Calls GameForge API and retrieves the selected campaign
    /// </summary>
    ImportErrorEnum PullCampaign(string campaignId)
    {
        ImportErrorEnum errors;
        string url = CAMPAIGN_URL.Replace("{id}", campaignId);
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        uwr.SetRequestHeader("Authorization", Utils.GetAuthHeader());
        uwr.SendWebRequest();

        while (!uwr.isDone) { };

        if (uwr.error != null)
        {
            Debug.LogError(string.Format("Campaign: {0} Error {1} Endpoint: {2}", campaignId, uwr.error, url));
            errors = ImportErrorEnum.CAMPAIGN_ERROR;
        }
        else
        {
            CampaignData campaignData = JsonUtility.FromJson<CampaignData>(uwr.downloadHandler.text);

            if(CheckIfRecreateCampaign(campaignData.name))
            {
                // DESTROY PREVIOUS CAMPAIGN
                if (GameObject.Find(campaignData.name))
                    DestroyImmediate(GameObject.Find(campaignData.name));

                // CLEAR DROPDOWNS
                if (FindFirstObjectByType<CharacterDropdown>() != null)
                    FindFirstObjectByType<CharacterDropdown>().ClearOptions();

                GameObject go = new(campaignData.name);

                Campaign campaign = go.AddComponent<Campaign>();
                campaign.Initialize(campaignData);
                errors = PullEntities(campaignData, go.transform);
            }
            else
                errors = ImportErrorEnum.CANCELLED_ERROR;
        }
        return errors;
    }

    /// <summary>
    /// Returns true if the campaign has not been imported yet or if the user decided to recreate.
    /// </summary>
    bool CheckIfRecreateCampaign(string name)
    {
        bool res = true;

        if (GameObject.Find(name))
            res = EditorUtility.DisplayDialog("Recreate campaign?", "This campaign is already present in Unity. Do you want to reimport? ALL CHANGES WILL BE LOST", "Recreate", "Cancel");
        
        return res;
    }


    /// <summary>
    /// Calls GameForge API and retrieves the entities from a given campaign
    /// </summary>
    ImportErrorEnum PullEntities(CampaignData campaignData, Transform t)
    {
        ImportErrorEnum errors;

        UnityWebRequest uwr = UnityWebRequest.Get(ENTITIES_URL.Replace("{id}", campaignData.id));
        uwr.SetRequestHeader("Authorization", Utils.GetAuthHeader());
        uwr.SendWebRequest();
        
        while (!uwr.isDone) { };
        
        if (uwr.error != null)
        {
            Debug.LogError(string.Format("Campaign: {0} Error: {1}", campaignData.id, uwr.error));
            errors = ImportErrorEnum.ENTITY_ERROR;
        }
        else
        {
            string jsonstr = "{\"results\":" + uwr.downloadHandler.text + "}";
            EntitiesData entitiesData = JsonUtility.FromJson<EntitiesData>(jsonstr);
            errors = InstantiateEntities(entitiesData.results, t);
        }
        return errors;
    }

    /// <summary>
    /// Creates Unity GameObjects based on GameForge Entities
    /// </summary>
    ImportErrorEnum InstantiateEntities(EntityData[] entitiesData, Transform t)
    {
        ImportErrorEnum errors = ImportErrorEnum.NO_ERROR;
        foreach (EntityData entity in entitiesData)
        {
            EditorUtility.DisplayCancelableProgressBar("Importing from GameForge", string.Format("Downloading {0}", entity.name), 0.5f);
            EntityDetailsData entityData = PullEntityDetails(entity);
            if(entityData == null)
            {
                errors = ImportErrorEnum.ENTITY_ERROR;
                continue;
            }

            GameObject go = new(entity.name);
            go.transform.parent = t;
            if (System.Enum.TryParse(entity.entity_type, out EntityEnum entityEnum))
            {
                if (!spawnCounter.ContainsKey(entityEnum))
                    spawnCounter.Add(entityEnum, 0);

                switch (entityEnum)
                {
                    case EntityEnum.location:
                        go.AddComponent<Location>().Initialize(entityData, spawnCounter[entityEnum]);
                        break;
                    case EntityEnum.quest:
                        go.AddComponent<Quest>().Initialize(entityData, spawnCounter[entityEnum]);
                        break;
                    case EntityEnum.organization:
                        go.AddComponent<Organization>().Initialize(entityData, spawnCounter[entityEnum]);
                        break;
                    case EntityEnum.character:
                        go.AddComponent<Character>().Initialize(entityData, spawnCounter[entityEnum]);
                        break;
                    case EntityEnum.item:
                        go.AddComponent<Item>().Initialize(entityData, spawnCounter[entityEnum]);
                        break;
                    case EntityEnum.puzzle:
                        go.AddComponent<Puzzle>().Initialize(entityData, spawnCounter[entityEnum]);
                        break;
                }
                spawnCounter[entityEnum]++;
            }

            InstantiateEntities(entity.children, go.transform);
        }

        EditorUtility.ClearProgressBar();
        return errors;
    }

    /// <summary>
    /// Retrieve the details (image, description, etc) of each Entity
    /// </summary>
    EntityDetailsData PullEntityDetails(EntityData entity)
    {
        EntityDetailsData res = null;
        UnityWebRequest uwr = UnityWebRequest.Get(ENTITY_DETAILS_URL.Replace("{id}", entity.id));
        uwr.SetRequestHeader("Authorization", Utils.GetAuthHeader());
        uwr.SendWebRequest();

        while (!uwr.isDone) { };

        if (uwr.error != null)
            Debug.LogError(string.Format("Entity: {0} Error: {1}", entity.id, uwr.error));            
        else
            res = JsonUtility.FromJson<EntityDetailsData>(uwr.downloadHandler.text);
        
        return res;
    }

}
