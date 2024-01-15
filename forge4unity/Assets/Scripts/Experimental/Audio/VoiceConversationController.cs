using System.Threading.Tasks;
using GameForgeAI.OpenAISdk;
using UnityEngine;

public class VoiceConversationController : MonoBehaviour
{
    [Header("Assistant id assigned in Game Forge")]
    public string assistantId;
    
    private AudioSource audioSource;
    private ConversationThread conversationThread;
    private string currentDevice;
    private bool isRecording;
    private AudioClip recordedClip;

    private OpenAI openAI;

    private void Awake()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("openAIApiKey")))
        {
            Debug.Log(string.Format("Voice chat disabled for {0}. Reason: OpenAI key was not introduced.", gameObject.name));
            return;
        }
        
        if (string.IsNullOrEmpty(assistantId))
        {
            Debug.Log(string.Format("Voice chat disabled for {0}. Reason: Assistant id not populated from GameForge.", gameObject.name));
            return;
        }
        audioSource = FindFirstObjectByType<AudioSource>();
        if(audioSource == null)
        {
            GameObject audio = new ("AudioSource");
            audioSource = audio.AddComponent<AudioSource>();
            return;
        }
        
        openAI = new OpenAI(PlayerPrefs.GetString("openAIApiKey"));
        conversationThread = new(openAI, assistantId)
        {
            textResultDelegate = result =>
            {
                Debug.Log($"text response received: {result}");
                _ = ReadText(result);
            },

            toolCallsDelegate = (functionName, arguments) =>
            {
                Debug.Log($"function call received: {functionName} arguments: {arguments}");
                return "{'success': 'true'}";
            }
        };

        var devices = Microphone.devices;
        if (devices != null && devices.Length > 0)
            currentDevice = devices[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRecording)
            StartRecording();
        else if (Input.GetKeyUp(KeyCode.Space) && isRecording)
            StopRecording();
    }
    
    private void StartRecording()
    {
        if (string.IsNullOrEmpty(currentDevice))
            return;

        recordedClip = Microphone.Start(currentDevice, false, 10, 44100);
        isRecording = true;
    }

    private void StopRecording()
    {
        Microphone.End(currentDevice);
        isRecording = false;
    
        var clip = recordedClip;
        recordedClip = null;

        _ = conversationThread.SendVoiceMessageAsync(clip);
    }

    private async Task ReadText(string text)
    {
        var clip = await openAI.CreateSpeechAsync(text);
        audioSource.clip = clip;
        audioSource.Play();
    }
}