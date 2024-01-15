using System;
using Newtonsoft.Json;

namespace GameForgeAI.OpenAISdk
{
    public enum MessageResultType
    {
        None,
        TextResult,
        RequiredAction
    }

    [Serializable]
    public class EmptyModel
    {
    }

    [Serializable]
    public class CreateTranscriptionResponse
    {
        public string text;
    }

    [Serializable]
    public class CreateThreadResponse
    {
        public string id;
    }

    [Serializable]
    public class CreateSpeechRequest
    {
        public string input;
        public string voice;
        public string model;
    }

    [Serializable]
    public class ToolCallOutput
    {
        [JsonProperty("tool_call_id")] public string id;
        public string output;
    }

    [Serializable]
    public class MessageContentModel
    {
        public string role = "user";
        public string content;

        public MessageContentModel(string message)
        {
            content = message;
        }
    }

    [Serializable]
    public class AssistantInstructionModel
    {
        [JsonProperty("assistant_id")] public string assistantId;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string instructions;

        public AssistantInstructionModel(string assistId, string instruction)
        {
            assistantId = assistId;
            instructions = instruction;
        }
    }

    [Serializable]
    public class AssistantRunResponse
    {
        public string id;
        [JsonProperty("thread_id")] public string threadId;
        public string status;

        [JsonProperty("required_action")] public RequiredAction requiredAction;
    }

    [Serializable]
    public class MessagesResponse
    {
        public MessageResponse[] data;

        public string GetLatestResponse()
        {
            if (data == null || data.Length == 0) return null;
            return data[0]?.GetLatestResponse();
        }
    }

    [Serializable]
    public class MessageResponse
    {
        public string id;
        public string role;
        [JsonProperty("thread_id")] public string threadId;

        public MessageResponseContent[] content;

        public string GetLatestResponse()
        {
            if (content == null || content.Length == 0) return null;

            return content[0].text?.value;
        }
    }

    [Serializable]
    public class MessageResponseContent
    {
        public string type;
        public MessageResponseTextContent text;
    }

    [Serializable]
    public class MessageResponseTextContent
    {
        public string value;
        // annotations
    }

    public class RequiredAction
    {
        public string type;
        [JsonProperty("submit_tool_outputs")] public SubmitToolOutputs submitToolOutputs;
    }

    [Serializable]
    public class SubmitToolOutputs
    {
        [JsonProperty("tool_calls")] public ToolCall[] toolCalls;
    }

    [Serializable]
    public class ToolCall
    {
        public string id;
        public string type;
        public FunctionCall function;
    }

    [Serializable]
    public class FunctionCall
    {
        public string name;
        public string arguments;
    }

    [Serializable]
    public class SendMessageResult
    {
        public string RunId;
        public MessageResultType Type;
        public string Message;
        public RequiredAction RequiredAction;
    }
}