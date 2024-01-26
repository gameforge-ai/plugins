using System;
using System.Threading.Tasks;
using UnityEngine;
using GameForgeAI.Assistant;

public class VoiceChatController : MonoBehaviour
{
    [SerializeField]
    private string assistantId;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private bool debugMode;
    [SerializeField]
    private bool initialized = false;
    
    private ConversationThread conversationThread;
    private string currentDevice;
    private bool isRecording;
    private AudioClip recordedClip;

    private AssistantAPI api;

    [SerializeField]
    private bool listening;
    
        
    public void Initialize(string assistantId, string audioSourceObjectName)
    {
        initialized = false;
        listening = false;

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("token")))
        {
            Debug.LogError(string.Format("{0}'s voice disabled. Reason: Missing API Token", gameObject.name));
            return;
        }
                    
        if (string.IsNullOrEmpty(assistantId))
        {
            Debug.LogWarning(string.Format("{0}'s voice disabled. Reason: Assistant not defined in GameForge", gameObject.transform.parent.name));
            return;
        }            

        if(GameObject.Find(audioSourceObjectName) != null && GameObject.Find(audioSourceObjectName).GetComponent<AudioSource>() != null)
            this.audioSource = GameObject.Find(audioSourceObjectName).GetComponent<AudioSource>();
        else
        {
            this.audioSource = GameObject.FindFirstObjectByType<AudioSource>();
            if (this.audioSource == null)
            {
                Debug.LogError(string.Format("{0}'s voice disabled. Reason: No `AudioSource`s were found in the scene", gameObject.transform.parent.name));
                return;
            }
        }
        this.assistantId = assistantId;

        initialized = true;
    }

    private void Awake()
    {
        if (!initialized)
            return;

        api = new AssistantAPI(PlayerPrefs.GetString("token"), debugMode);
        conversationThread = new(api, this.assistantId)
        {
            voiceResultDelegate = result =>
            {
                _ = ReadText(result);
            }
        };

        var devices = Microphone.devices;
        if (devices != null && devices.Length > 0)
            currentDevice = devices[0];
    }

    private void Update()
    {
        if (!initialized || !listening)
            return;
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isRecording)
            StartRecording();
        else if (Input.GetKeyUp(KeyCode.LeftAlt) && isRecording)
            StopRecording();
    }
    
    private void StartRecording()
    {
        if (string.IsNullOrEmpty(currentDevice))
            return;

        Debug.Log(string.Format("START recording chat with {0} with assistant id {1}", transform.parent.name, assistantId));

        recordedClip = Microphone.Start(currentDevice, false, 10, 44100);
        isRecording = true;
    }

    private async void StopRecording()
    {
        Debug.Log(string.Format("STOP recording chat with {0} with assistant id {1}", transform.parent.name, assistantId));

        Microphone.End(currentDevice);
        isRecording = false;
    
        var clip = recordedClip;
        recordedClip = null;

        try
        {
            await conversationThread.SendVoiceMessageAsync(clip);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private async Task ReadText(ThreadMessageModel model)
    {
        if (!initialized)
            return;
        try
        {
            var clip = await api.GetAudioForMessage(model);
            audioSource.clip = clip;
            audioSource.Play();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void SetListening(bool listening)
    {
        this.listening = listening;
    }
}