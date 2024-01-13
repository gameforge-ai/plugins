using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class MenuItemImport : EditorWindow
{
    private const string LOGIN = "login";

    private static EditorWindow wnd;

    private TextField username;
    private TextField secret;
    private TextField campaignId;
    private Toggle useOTP;

    private bool isOTP = false;
    
    private const string logoName = "gameforge_logo";
    private const int width = 450;
    private const int height = 200;


    /// <summary>
    /// Registers an new tab in Unity's Editor Window which opens a custom menu to connect to Gameforge and import 1 campaign
    /// </summary>
    [MenuItem("Gameforge/Import/Campaign")]
    static void ImportCampaign()
    {
        wnd = GetWindow<MenuItemImport>();
        wnd.titleContent = new GUIContent("Connect to GameForge AI");
        // Limit size of the window
        Vector2 size = new(width, height);
        wnd.minSize = size;
        wnd.maxSize = size;
    }

    /// <summary>
    /// GUI function to show the content of the Editor Window
    /// </summary>
    public void CreateGUI()
    {
        Sprite logo = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(logoName)));
        
        Image spriteImage = new()
        {
            scaleMode = ScaleMode.ScaleToFit,
            sprite = logo
        };
        rootVisualElement.Add(spriteImage);

        useOTP = new Toggle("Log in using One-time password");
        useOTP.RegisterValueChangedCallback(ToggleChange);

        rootVisualElement.Add(useOTP);

        username = new TextField("Username")
        {
            value = PlayerPrefs.GetString("username")
        };

        rootVisualElement.Add(username);

        secret = new("API Token", int.MaxValue, false, true, '*')
        {
            value = PlayerPrefs.GetString("token")
        };
        rootVisualElement.Add(secret);

        campaignId = new ("Campaign Id/Name")
        {
            value = PlayerPrefs.GetString("campaignId")
        };
        rootVisualElement.Add(campaignId);

        Button button = new(() => Submit())
        {
            text = "Synchronize",
        };
        rootVisualElement.Add(button);
    }


    /// <summary>
    /// Button callback to connect to GameForge and retrieve assets
    /// </summary>
    void Submit()
    {
        if (GameObject.Find(LOGIN))
            DestroyImmediate(GameObject.Find(LOGIN));
        
        GameObject loginGameObject = new (LOGIN);
        
        Login login;
        if(useOTP.value)
            login = loginGameObject.AddComponent<OTPLogin>();
        else
            login = loginGameObject.AddComponent<APITokenLogin>();
        login.Initialize(username.value, secret.value, campaignId.value);
        wnd.Close();
    }

    /// <summary>
    /// Updates form based on One
    /// </summary>
    private void ToggleChange(ChangeEvent<bool> evt)
    {
        isOTP = evt.newValue;
        secret.value = isOTP ? PlayerPrefs.GetString("secret") : PlayerPrefs.GetString("token");
        secret.label = isOTP ? "One-time Password" : "API Token";
    }
}
