using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameForgeAI.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace GameForgeAI.Assistant
{
    public class AssistantAPI
    {
        private const string API_URL_V1 = "https://apidev.gameforge.ai/v1";
        private readonly string apiKey;

        private readonly bool debugMode = false;

        public AssistantAPI(string apiKey, bool debug)
        {
            this.apiKey = apiKey;
            debugMode = debug;

        }

        public async Task<AssistantModel> GetAssistants()
        {
            var url = $"{API_URL_V1}/assistants/";
            return await GetAsync<AssistantModel>(url);
        }

        public async Task<ThreadModel> CreateThread(string assistId)
        {
            var url = $"{API_URL_V1}/assistants/{assistId}/threads/";
            return await PostAsync<ThreadModel>(url, "");
        }

        public async Task<ThreadModel> CreateAndRun(string assistId, string message)
        {
            var url = $"{API_URL_V1}/assistants/{assistId}/threads/create_and_run/";
            var payload = new Dictionary<string, string>
            {
                {"message_content", message}
            };
            return await PostAsync<ThreadModel>(url, payload);
        }
        
        public async Task<ThreadModel> GetThread(string assistId, string threadId)
        {
            var url = $"{API_URL_V1}/assistants/{assistId}/threads/{threadId}/";
            return await GetAsync<ThreadModel>(url);
        }
        
        public async Task<ThreadModel[]> GetThreads(string assistId)
        {
            var url = $"{API_URL_V1}/assistants/{assistId}/threads/";
            return await GetAsync<ThreadModel[]>(url);
        }
        
        public async Task<EmptyModel> DeleteThread(string assistId, string threadId)
        {
            var url = $"{API_URL_V1}/assistants/{assistId}/threads/{threadId}/";
            return await DeleteAsync<EmptyModel>(url);
        }

        public async Task<ThreadModel> AddMessage(string assistId, string threadId, string message)
        {
            var url = $"{API_URL_V1}/assistants/{assistId}/threads/{threadId}/add_message/";
            var payload = new Dictionary<string, string>
            {
                {"message_content", message}
            };
            return await PostAsync<ThreadModel>(url, payload);
        }

        public async Task<AudioClip> CreateSpeech(string assistId, string threadId, string messageId)
        {
            await GenerateSpeech(assistId, threadId, messageId);
            var thread = await GetThread(assistId, threadId);
            return await GetAudioForMessage(thread.GetMessage(messageId));
        }

        public async Task<AudioClip> GetAudioForMessage(ThreadMessageModel model)
        {
            var voiceUrl = model?.voiceAudioUrl;
            if (string.IsNullOrEmpty(voiceUrl))
                throw new Exception("generate speech failed");
            
            var www = UnityWebRequestMultimedia.GetAudioClip(voiceUrl, AudioType.MPEG);
            await www.SendWebRequest();
            
            if (!string.IsNullOrEmpty(www.error))
                throw new Exception($"Web request error: {www.url} {www.error}");
            
            return DownloadHandlerAudioClip.GetContent(www);
        }

        public async Task<ThreadMessageModel> TranscribeAndWaitForResponse(string assistId, string threadId, AudioClip clip, int delayMilliseconds = 500)
        {
            var thread = await Transcribe(assistId, threadId, clip);
            var runId = thread.currentRunId;
            
            if (!string.IsNullOrEmpty(runId))
                return await PollForRunId(thread, runId);
            
            var message = thread.GetResponseForRunId(runId);
            while (message == null)
            {
                await Task.Delay(delayMilliseconds);
                thread = await GetThread(assistId, threadId);
                message = thread.GetResponseForRunId(runId);
            }

            return message;
        }
        
        public async Task<ThreadModel> Transcribe(string assistId, string threadId, AudioClip clip)
        {
            var audioData = AudioClipConverter.ToWavBytes(clip);
            var url = $"{API_URL_V1}/assistants/{assistId}/threads/{threadId}/transcribe/";
            
            var filename = "voice.wav";
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("audio_file", audioData, filename, "audio/wav"),
                new MultipartFormDataSection("file_name", filename)
            };
            
            var www = UnityWebRequest.Post(url, form);
            AddHeader(www);
            return await SendWebRequestAsync<ThreadModel>(www);
        }
        
        public async Task<ThreadMessageModel> PollForRunId(ThreadModel threadModel, string runId, int delayMilliseconds = 500)
        {
            var responseMessage = threadModel.GetResponseForRunId(runId);
            while (responseMessage == null)
            {
                await Task.Delay(delayMilliseconds);
                var updatedThread = await GetThread(threadModel.assistantId, threadModel.id);
                responseMessage = updatedThread.GetResponseForRunId(runId);
            }

            return responseMessage;
        }

        public async Task<VoiceOptionsModel> GetVoiceOptions()
        {
            var url = $"{API_URL_V1}/assistants/voice_options/";
            return await GetAsync<VoiceOptionsModel>(url);
        }
        
        public async Task<SelectVoiceResponse> SelectVoice(string assistId, string voice)
        {
            var url = $"{API_URL_V1}/assistants/{assistId}/select_voice/";
            var payload = new Dictionary<string, string>
            {
                {"selected_voice", voice}
            };
            return await PostAsync<SelectVoiceResponse>(url, payload);
        }
        
        private async Task<EmptyModel> GenerateSpeech(string assistId, string threadId, string messageId)
        {
            var url = $"{API_URL_V1}/assistants/{assistId}/threads/{threadId}/messages/{messageId}/speech/";
            return await GetAsync<EmptyModel>(url);
        }

        private async Task<T> GetAsync<T>(string url)
        {
            var www = UnityWebRequest.Get(url);
            AddHeader(www);
            return await SendWebRequestAsync<T>(www);
        }

        private async Task<T> PostAsync<T>(string url, object model)
        {
            var json = JsonConvert.SerializeObject(model);
            var www = UnityWebRequest.Post(url, json, "application/json");
            AddHeader(www);
            return await SendWebRequestAsync<T>(www);
        }
        
        private async Task<T> DeleteAsync<T>(string url)
        {
            var www = UnityWebRequest.Delete(url);
            AddHeader(www);
            return await SendWebRequestAsync<T>(www);
        }

        private void AddHeader(UnityWebRequest www)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("missing auth token");

            www.SetRequestHeader("Authorization", $"Token {apiKey}");
        }

        private async Task<T> SendWebRequestAsync<T>(UnityWebRequest www)
        {
            await www.SendWebRequest();

            if (!string.IsNullOrEmpty(www.error))
            {
                var errorMsg = $"Web request error: {www.url} {www.error}";
                Debug.LogError(errorMsg);
                throw new Exception(errorMsg);
            }

            var json = www.downloadHandler.text;
            if (debugMode)
                Debug.Log(json);

            if (string.IsNullOrEmpty(json))
                return default;
            
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
