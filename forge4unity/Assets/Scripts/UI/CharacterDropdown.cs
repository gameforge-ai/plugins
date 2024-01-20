using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(TMP_Dropdown))]
public class CharacterDropdown : MonoBehaviour
{
    private static float LOOK_AT_DISTANCE = 1f;
    private static float LOOK_FROM_ABOVE = 0.3f;
    private static string UNSELECTED_TEXT = "Unselected";

    void Awake()
    {
        GetComponent<TMP_Dropdown>().onValueChanged.RemoveAllListeners();
        GetComponent<TMP_Dropdown>().onValueChanged.AddListener((index) => GoToCharacter(index));
    }

    public void AddCharacter(Character character)
    {
        GetComponent<TMP_Dropdown>().AddOptions(new List<TMP_Dropdown.OptionData>() { new (character.name) });
    }

    public void ClearOptions()
    {
        GetComponent<TMP_Dropdown>().ClearOptions();
        // First always option for unselected
        GetComponent <TMP_Dropdown>().AddOptions(new List<TMP_Dropdown.OptionData>() { new(UNSELECTED_TEXT) });
    }
    public void GoToCharacter(int index)
    {
        if (index == 0)
            return;

        TMP_Dropdown.OptionData option = GetComponent<TMP_Dropdown>().options[index];
        foreach(InstantiatedCharacter character in FindObjectsByType<InstantiatedCharacter>(FindObjectsSortMode.None))
        {
            if( character.GetName() == option.text)
            {
                //Camera.main.transform.LookAt(character.transform.position);
                //Camera.main.transform.position = character.transform.position;
                // Calculate the desired position based on the object's position and forward vector
                Vector3 desiredPosition = character.transform.position + character.transform.forward * LOOK_AT_DISTANCE;

                // Set the camera's position to the desired position
                Camera.main.transform.position = new Vector3(desiredPosition.x, desiredPosition.y + LOOK_FROM_ABOVE, desiredPosition.z);

                // Calculate the desired rotation based on the object's forward vector
                Quaternion desiredRotation = Quaternion.LookRotation(-1*character.transform.forward);

                // Set the camera's rotation to the desired rotation
                Camera.main.transform.rotation = desiredRotation;
                break;
            }
        }
    }
}
