using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ImporterResultsDialog : EditorWindow
{
    private Label status;
    private Label whereToFind;
    private Label entities;
    private Image hierarchy;
    private Label characters;
    private Image scene;

    private const string entitiesName = "gameforge_hierarchy";
    private const string sceneName = "gameforge_scene";
    private const int width = 450;
    private const int longHeight = 600;
    private const int shortHeight = 75;

    private static ImportErrorEnum errorsEnum;


    /// <summary>
    /// Editor Window Constructor for the Results of the Import
    /// </summary>
    public void Initialize(ImportErrorEnum errors)
    {
        SetLongWindow();
        titleContent = new GUIContent("Importing results");
        errorsEnum = errors;
    }

    /// <summary>
    /// Sets a long size for the window message
    /// </summary>
    public void SetLongWindow()
    {
        Vector2 size = new(width, longHeight);
        minSize = size;
        maxSize = size;
    }

    /// <summary>
    /// Sets a short size for the window message
    /// </summary>
    public void SetShortWindow()
    {
        Vector2 size = new(width, shortHeight);
        minSize = size;
        maxSize = size;
    }

    /// <summary>
    /// GUI function to show the content of the Editor Window
    /// </summary>
    public void CreateGUI()
    {
        Sprite hierarchySprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(entitiesName)));
        Sprite sceneSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(Utils.RetrieveGUID(sceneName)));

        string title = "IMPORT SUCCESSFULL";
        switch (errorsEnum)
        {
            case ImportErrorEnum.AUTHENTICATION_ERROR:
                title = "There was an authentication problem.\nPlease check your credentials and try again.";
                break;
            case ImportErrorEnum.CAMPAIGN_ERROR:
                title = "Unable to find the campaign.\nPlease check the campaign name/id and try again.";
                break;
            case ImportErrorEnum.ENTITY_ERROR:
                title = "Some entities could not be imported.";
                break;
            case ImportErrorEnum.CANCELLED_ERROR:
                title = "Import cancelled by the user";
                break;
        }

        status = new Label(string.Format("\n{0}\n", title));
        status.style.unityFontStyleAndWeight = FontStyle.Bold;
        status.style.unityTextAlign = TextAnchor.UpperCenter;

        rootVisualElement.Add(status);

        if (errorsEnum == ImportErrorEnum.NO_ERROR || errorsEnum == ImportErrorEnum.ENTITY_ERROR)
        {
            entities = new Label("ENTITIES:\nAll the entities have been placed in the Scene Explorer");
            rootVisualElement.Add(entities);
            entities.style.unityFontStyleAndWeight = FontStyle.Bold;
            entities.style.unityTextAlign = TextAnchor.UpperCenter;

            hierarchy = new()
            {
                scaleMode = ScaleMode.ScaleToFit,
                sprite = hierarchySprite
            };
            rootVisualElement.Add(hierarchy);

            characters = new Label("CHARACTERS:\nYour characters have been instantiated as dummies in the scene.");
            rootVisualElement.Add(characters);
            characters.style.unityFontStyleAndWeight = FontStyle.Bold;
            characters.style.unityTextAlign = TextAnchor.UpperCenter;

            scene = new()
            {
                scaleMode = ScaleMode.StretchToFill,
                sprite = sceneSprite
            };
            rootVisualElement.Add(scene);
        }
        else
            SetShortWindow();
        
        Button button = new(() => Close())
        {
            text = "Ok",
        };
        rootVisualElement.Add(button);

    }
}
