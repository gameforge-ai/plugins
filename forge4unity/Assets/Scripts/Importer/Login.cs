using UnityEngine;


public class Login : MonoBehaviour
{
    private const string IMPORTER = "importer";


    /// <summary>
    /// Abstract (Virtual) function for Initalize - never call it directly
    /// </summary>
    public virtual void Initialize(string username, string secret, string campaignId, bool force=false)
    {
        return;
    }

    /// <summary>
    /// Instantiates the Importer object
    /// </summary>
    protected void CreateImporter()
    {
        if (GameObject.Find(IMPORTER))
            DestroyImmediate(GameObject.Find(IMPORTER));

        GameObject importerGameObject = new(IMPORTER);

        Importer importer = importerGameObject.AddComponent<Importer>();
        importer.Initialize();
    }
}
