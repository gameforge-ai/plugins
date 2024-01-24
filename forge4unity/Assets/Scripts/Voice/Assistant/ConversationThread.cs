using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GameForgeAI.Assistant
{
    public class ConversationThread
    {
        private readonly AssistantAPI _api;
        private readonly string _assistantId;

        private string _threadId;
        
        public TextResultDelegate textResultDelegate;
        public VoiceResultDelegate voiceResultDelegate;
        
        public ConversationThread(AssistantAPI api, string assistantId, string threadId = null)
        {
            _api = api;
            _assistantId = assistantId;
            _threadId = threadId;

            if (string.IsNullOrEmpty(_threadId))
            {
                _ = InitializeThreadAsync();
            }
        }
        
        public async Task SendVoiceMessageAsync(AudioClip clip)
        {
            try
            {
                var response = await _api.TranscribeAndWaitForResponse(_assistantId, _threadId, clip);
                await HandleSendMessageResult(response);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
        
        public async Task<VoiceOptionsModel> GetVoiceOptions()
        {
            return await _api.GetVoiceOptions();
        }
        
        public async Task<SelectVoiceResponse> SelectVoice(string voice)
        {
            return await _api.SelectVoice(_assistantId, voice);
        }
        
        private async Task InitializeThreadAsync()
        {
            var response = await _api.CreateThread(_assistantId);
            _threadId = response.id;
        }
        
        #pragma warning disable 1998
        private async Task HandleSendMessageResult(ThreadMessageModel model)
        {
            if (model.isVoice && !string.IsNullOrEmpty(model.voiceAudioUrl))
            {
                voiceResultDelegate?.Invoke(model);
            }
            else
            {
                textResultDelegate?.Invoke(model);
            }
        }
        #pragma warning restore 1998
    }
    
    public delegate void TextResultDelegate(ThreadMessageModel model);
    public delegate void VoiceResultDelegate(ThreadMessageModel model);
}