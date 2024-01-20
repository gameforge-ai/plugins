using System.Collections;
using TMPro;
using UnityEngine;

public class InstantiatedCharacter : MonoBehaviour
{
    [SerializeField]
    VoiceConversationController voiceController;
    [SerializeField]
    TextMeshPro textWidget;
    [SerializeField]
    SpriteRenderer thumbnailWidget;
    [SerializeField]
    EntityDetailsData entityData;

    private Character character;

    // Start is called before the first frame update
    public void Initialize(Character character)
    {
        this.character = character; 
        this.entityData = character.GetEntityData();

        if (textWidget != null)
            textWidget.text = entityData.name;
        
        if (voiceController != null && !string.IsNullOrEmpty(entityData.openai_assistant_id))
            voiceController.assistantId = entityData.openai_assistant_id;

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

    public string GetName()
    {
        return entityData.name;
    }

}
