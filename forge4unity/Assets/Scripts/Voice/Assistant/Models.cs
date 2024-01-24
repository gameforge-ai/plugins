using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameForgeAI.Assistant
{
    public static class Role
    {
        public const string USER = "user";
        public const string ASSISTANT = "assistant";
    }
    
    public class MetadataModel : Dictionary<string, string>
    {
    }

    [Serializable]
    public class EmptyModel
    {
    }
    
    [Serializable]
    public class AssistantModel
    {
        public string id;
        public string model;
        public string name;
        public string selectedVoice;
        public string instructions;
        public MetadataModel metadata;

        [JsonProperty("openai_assistant_id")] public string openAiAssistantId;
        [JsonProperty("recent_events")] public string recentEvents;
        [JsonProperty("campaign_id")] public string campaignId;
        [JsonProperty("entity_id")] public string entityId;
        [JsonProperty("last_updated")] public DateTime lastUpdated;
    }

    [Serializable]
    public class ThreadModel
    {
        public string id;
        
        [JsonProperty("assistant_id")] public string assistantId;
        [JsonProperty("created_at")] public DateTime createdAt;
        [JsonProperty("is_complete")] public bool isComplete;
        [JsonProperty("in_voice_mode")] public bool isVoiceMode;
        [JsonProperty("messages_pending")] public bool messagesPending;
        [JsonProperty("current_run_id")] public string currentRunId;
        
        public MetadataModel metadata;

        [JsonProperty("thread_messages")] public ThreadMessageModel[] threadMessages;
        [JsonProperty("thread_runs")] public ThreadRunModel[] threadRuns;

        public ThreadMessageModel LatestMessage()
        {
            return threadMessages?.Length > 0 ? threadMessages[^1] : null;
        }
        
        public ThreadRunModel LatestRun()
        {
            return threadRuns?.Length > 0 ? threadRuns[0] : null;
        }

        public ThreadMessageModel GetMessage(string messageId)
        {
            if (threadMessages == null) return null;

            foreach (var msg in threadMessages)
            {
                if (msg.id == messageId)
                    return msg;
            }

            return null;
        }

        public ThreadMessageModel GetResponseForRunId(string runId)
        {
            if (threadMessages == null) return null;

            foreach (var msg in threadMessages)
            {
                if (msg.runId == runId)
                    return msg;
            }

            return null;
        }
    }

    [Serializable]
    public class ThreadMessageModel
    {
        public string id;
        [JsonProperty("created_at")] public DateTime createdAt;
        public string role;
        [JsonProperty("is_voice")] public bool isVoice;
        [JsonProperty("voice_audio_url")] public string voiceAudioUrl;

        public string content;

        // file_ids type?
        [JsonProperty("run_id")] public string runId;
        public MetadataModel metadata;
        [JsonProperty("thread_id")] public string threadId;
        [JsonProperty("assistant_id")] public string assistantId;
    }

    [Serializable]
    public class ThreadRunModel
    {
        public string id;
        [JsonProperty("created_at")] public DateTime createdAt;
        [JsonProperty("updated_at")] public DateTime? updatedAt;
        [JsonProperty("started_at")] public DateTime? startedAt;
        [JsonProperty("expires_at")] public DateTime? expiredAt;
        [JsonProperty("failed_at")] public DateTime? failedAt;
        [JsonProperty("cancelled_at")] public DateTime? cancelledAt;
        [JsonProperty("completed_at")] public DateTime? completedAt;

        public string status;
        [JsonProperty("run_model")] public string runModel;
        [JsonProperty("last_error_code")] public string lastErrorCode; // Type?
        [JsonProperty("last_error_message")] public string lastErrorMessage;

        [JsonProperty("thread_id")] public string threadId;
        [JsonProperty("assistant_id")] public string assistantId;
        public MetadataModel metadata;
    }
    
    [Serializable]
    public class VoiceOptionsModel
    {
        [JsonProperty("voice_options")] public string[] options;
    }
    
    [Serializable]
    public class SelectVoiceResponse
    {
        public AssistantModel assistant;
    }
}