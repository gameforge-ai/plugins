using UnityEditor;
using UnityEngine;

public static class Utils
{
    /// <summary>
    /// Loads objects from disk given a name and returns their ids
    /// </summary>    
    public static string[] RetrieveGUIDs(string name)
    {
        // Get the logo sprite
        string[] allObjectGuids = AssetDatabase.FindAssets(name);
        if (allObjectGuids.Length < 1)
            Debug.LogError(string.Format("`{0}` not found. Something is wrong with the plugin. Please, reinstall.", name));
        return allObjectGuids;
    }
    /// <summary>
    /// Loads objects from disk given a name and returns its id or raises an exception
    /// </summary>    
    public static string RetrieveGUID(string name)
    {
        string[] guids = RetrieveGUIDs(name);
        if (guids.Length < 1)
        {
            FileMissing();
            throw new System.Exception();
        }
        return guids[0];
    }

    /// <summary>
    /// Gets the Authorization Header to send requests to GameForge API
    /// </summary>
    public static string GetAuthHeader()
    {
        return string.Format("Token {0}", PlayerPrefs.GetString("token"));
    }

    /// <summary>
    /// Shows a popup saying that something is not right with the plugin files
    /// </summary>
    public static void FileMissing()
    {
        EditorUtility.DisplayDialog("Logo not found", "Some Gameforge files are missing. Please reinstall the plugin.", "Ok");
    }
}
