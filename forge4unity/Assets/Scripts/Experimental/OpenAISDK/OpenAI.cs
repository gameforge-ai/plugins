using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GameForgeAI.OpenAISdk
{
    public class OpenAI
    {
        private const string RUN_STATUS_COMPLETED = "completed";
        private const string RUN_STATUS_REQUIRES_ACTION = "requires_action";

        private readonly string _apiKey;

        public OpenAI(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<CreateThreadResponse> CreateThreadAsync()
        {
            var url = "https://api.openai.com/v1/threads";

            var www = PostEmptyJson(url, true);

            return await SendWebRequestAsync<CreateThreadResponse>(www);
        }

        public async Task<SendMessageResult> SendMessageAsync(string assistantId, string threadId, string message,
            string instruction = null)
        {
            await SendMessageAsync(threadId, message);
            var runResponse = await RunAssistantAsync(assistantId, threadId, instruction);
            return await GetRunResultAsync(runResponse);
        }

        public async Task SendMessageAsync(string threadId, string message)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages";
            var model = new MessageContentModel(message);

            var www = PostJson(url, model, true);

            await SendWebRequestAsync<EmptyModel>(www);
        }

        public async Task<AssistantRunResponse> RunAssistantAsync(string assistantId, string threadId,
            string instruction = null)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs";

            var model = new AssistantInstructionModel(assistantId, instruction);

            var www = PostJson(url, model, true);

            return await SendWebRequestAsync<AssistantRunResponse>(www);
        }

        public async Task<string> GetLatestMessagesFromThreadAsync(string threadId, int limit = 1)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages?limit={limit}";

            var www = Get(url, true);

            var response = await SendWebRequestAsync<MessagesResponse>(www);

            return response.GetLatestResponse();
        }

        public async Task<AssistantRunResponse> RetrieveRunAsync(string threadId, string runId)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}";

            var www = Get(url, true);

            return await SendWebRequestAsync<AssistantRunResponse>(www);
        }

        public async Task<SendMessageResult> SubmitToolsOutputAsync(string threadId, string runId,
            Dictionary<string, string> outputs)
        {
            var list = new List<ToolCallOutput>();

            foreach (var (key, value) in outputs)
            {
                list.Add(new ToolCallOutput
                {
                    id = key,
                    output = value
                });
            }

            var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}/submit_tool_outputs";

            var model = new Dictionary<string, List<ToolCallOutput>>
            {
                ["tool_outputs"] = list
            };

            var www = PostJson(url, model, true);

            var response = await SendWebRequestAsync<AssistantRunResponse>(www);

            return await GetRunResultAsync(response);
        }

        public async Task<CreateTranscriptionResponse> CreateTranscriptionAsync(AudioClip clip,
            string sttModel = "whisper-1")
        {
            var audioData = AudioClipConverter.ToWavBytes(clip);
            return await CreateTranscriptionAsync(audioData, sttModel);
        }

        public async Task<CreateTranscriptionResponse> CreateTranscriptionAsync(byte[] audioData,
            string sttModel = "whisper-1")
        {
            var url = "https://api.openai.com/v1/audio/transcriptions";

            var form = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", audioData, "recording", "audio/wav"),
                new MultipartFormDataSection("model", sttModel)
            };

            var www = UnityWebRequest.Post(url, form);
            AddHeader(www);

            return await SendWebRequestAsync<CreateTranscriptionResponse>(www);
        }

        public async Task<AudioClip> CreateSpeechAsync(string text, string ttsModel = "tts-1", string voice = "onyx")
        {
            var url = "https://api.openai.com/v1/audio/speech";
            var request = new CreateSpeechRequest
            {
                input = text,
                model = ttsModel,
                voice = voice
            };

            var www = PostJson(url, request, false);

            www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

            await www.SendWebRequest();

            if (!string.IsNullOrEmpty(www.error))
                throw new Exception($"Web request error: {www.url} {www.error}");

            return DownloadHandlerAudioClip.GetContent(www);
        }

        public async Task<SendMessageResult> GetRunResultAsync(AssistantRunResponse response)
        {
            while (response.status != RUN_STATUS_COMPLETED && response.status != RUN_STATUS_REQUIRES_ACTION)
            {
                await Task.Delay(500);
                response = await RetrieveRunAsync(response.threadId, response.id);
            }

            if (response.status == RUN_STATUS_REQUIRES_ACTION)
            {
                return new SendMessageResult
                {
                    RunId = response.id,
                    Type = MessageResultType.RequiredAction,
                    RequiredAction = response.requiredAction
                };
            }

            var latestMessage = await GetLatestMessagesFromThreadAsync(response.threadId);
            return new SendMessageResult
            {
                RunId = response.id,
                Type = MessageResultType.TextResult,
                Message = latestMessage
            };
        }

        private void AddHeader(UnityWebRequest www)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("missing auth token");
            }

            www.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
        }

        private static async Task<T> SendWebRequestAsync<T>(UnityWebRequest www)
        {
            await www.SendWebRequest();
            
            if (!string.IsNullOrEmpty(www.error))
                throw new Exception($"Web request error: {www.url} {www.error}");

            Debug.LogWarning(www.downloadHandler.text);
            return JsonUtility.FromJson<T>(www.downloadHandler.text);
        }

        private UnityWebRequest PostEmptyJson(string url, bool isAssistant)
        {
            var www = UnityWebRequest.Post(url, "", "application/json");
            AddHeader(www);
            if (isAssistant)
            {
                www.SetRequestHeader("OpenAI-Beta", "assistants=v1");
            }

            return www;
        }

        private UnityWebRequest Get(string url, bool isAssistant)
        {
            var www = UnityWebRequest.Get(url);
            AddHeader(www);
            if (isAssistant)
            {
                www.SetRequestHeader("OpenAI-Beta", "assistants=v1");
            }

            return www;
        }

        private UnityWebRequest PostJson<T>(string url, T model, bool isAssistant)
        {
            var json = JsonUtility.ToJson(model);
            var www = UnityWebRequest.Post(url, json, "application/json");
            AddHeader(www);
            if (isAssistant)
            {
                www.SetRequestHeader("OpenAI-Beta", "assistants=v1");
            }

            return www;
        }
    }
}