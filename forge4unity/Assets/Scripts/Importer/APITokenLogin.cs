using UnityEngine;

public class APITokenLogin : Login
{
    public override void Initialize(string username, string secret, string campaignId, bool force = false)
    {
        base.Initialize(username, secret, campaignId, force);
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("token", secret);
        PlayerPrefs.SetString("campaignId", campaignId);
        CreateImporter();
        DestroyImmediate(gameObject);
    }
}