using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class OTPLogin : Login
{
    private const string LOGIN_URL = "https://apidev.gameforge.ai/v1/api/login/";


    /// <summary>
    /// Overriding Initialize function from Login to carry out One-time Password (OTP)-based login
    /// </summary>
    public override void Initialize(string username, string secret, string campaignId, bool force = false)
    {
        base.Initialize(username, secret, campaignId, force);

        bool requiresLogin = PlayerPrefs.GetString("username") == username && PlayerPrefs.GetString("secret") == secret && PlayerPrefs.GetString("campaignId") == campaignId;
        requiresLogin |= string.IsNullOrEmpty(PlayerPrefs.GetString("token"));
        requiresLogin |= force;

        if (requiresLogin)
        {
            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetString("secret", secret);
            PlayerPrefs.SetString("campaignId", campaignId);
            StartCoroutine(LoginCoroutine());
        }
        else
        {
            //EditorUtility.DisplayDialog("Login successful", "Already logged in", "Ok");
            CreateImporter();
            DestroyImmediate(gameObject);
        }
    }

    /// <summary>
    /// Corourine to send an HTTP request to GameForge API with a username and a OTP (one-time password) and get a token
    /// </summary>
    IEnumerator LoginCoroutine()
    {
        WWWForm form = new();
        form.AddField("username", PlayerPrefs.GetString("username"));
        form.AddField("login_code", PlayerPrefs.GetString("secret"));

        UnityWebRequest uwr = UnityWebRequest.Post(LOGIN_URL, form);
        yield return uwr.SendWebRequest();
        if (uwr.error != null)
            EditorUtility.DisplayDialog("Login required", "Your username or one-time password are incorrect or have expired. Please, regenerate a new one from Gameforme web platform.", "Ok");
        else
        {
            //EditorUtility.DisplayDialog("Login successful", "Your login was successful", "Ok");
            SetToken(uwr.downloadHandler.text);
            CreateImporter();
        }
        DestroyImmediate(gameObject);
    }

    /// <summary>
    /// Sets the API Token received using the OTP in Application Memory.
    /// </summary>

    void SetToken(string json)
    {
        LoginData stub = JsonUtility.FromJson<LoginData>(json);
        PlayerPrefs.SetString("token", stub.token);
    }
}
