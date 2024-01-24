using System.Collections;
using TMPro;
using UnityEngine;

public class RuntimeCharacter : MonoBehaviour
{
    [SerializeField, Tooltip("Name of the Game Object in Scene with the Audio Source")]
    string audioSourceObjectName;
    [SerializeField]
    VoiceChatController voiceController;
    [SerializeField]
    TextMeshPro textWidget;
    [SerializeField]
    SpriteRenderer thumbnailWidget;
    [SerializeField]
    EntityDetailsData entityData;
    [SerializeField]
    bool listening;

    private Character character;

    /// <summary>
    /// Initializes the run-time version of Character
    /// </summary>
    public void Initialize(Character character)
    {
        this.character = character; 
        this.entityData = character.GetEntityData();

        if (textWidget != null)
            textWidget.text = entityData.name;

        if (voiceController != null && !string.IsNullOrEmpty(entityData.assistant))
            voiceController.Initialize(entityData.assistant, audioSourceObjectName);

        StartCoroutine(WaitToPaintImage());

        if (FindFirstObjectByType<CharacterDropdown>() != null)
            FindFirstObjectByType<CharacterDropdown>().AddCharacter(character);
    }

    /// <summary>
    /// Paints the image on top of the mesh
    /// </summary>
    IEnumerator WaitToPaintImage()
    {
        yield return new WaitWhile(() => character.IsLoadingImage());
        if (thumbnailWidget != null && character.GetEntityImage() != null && character.GetEntityImage().sprite != null)
            thumbnailWidget.sprite = character.GetEntityImage().sprite;
    }


    /// <summary>
    /// Returns the name of the character
    /// </summary>
    public string GetName()
    {
        return entityData.name;
    }

    /// <summary>
    /// Makes this character listen to you, while disabling the listening for others.
    /// </summary>
    public void Listen()
    {
        foreach (RuntimeCharacter item in FindObjectsByType<RuntimeCharacter>(FindObjectsSortMode.None))
            item.SetVoiceControllerListening(item.entityData.id == entityData.id);
    }

    /// <summary>
    /// Enables or Disables Voice Controller listening
    /// </summary>
    public void SetVoiceControllerListening(bool listening)
    {
        voiceController.SetListening(listening);
    }
}
