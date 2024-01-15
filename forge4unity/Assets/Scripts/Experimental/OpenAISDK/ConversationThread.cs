using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameForgeAI.OpenAISdk
{
    public class ConversationThread
    {
        private readonly OpenAI _openAI;
        private readonly string _assistantId;

        private string _threadId;

        public TextResultDelegate textResultDelegate;
        public ToolCallsDelegate toolCallsDelegate;

        public ConversationThread(OpenAI openAI, string assistantId, string threadId = null)
        {
            _openAI = openAI;
            _assistantId = assistantId;
            _threadId = threadId;

            if (string.IsNullOrEmpty(_threadId))
            {
                _ = InitializeThreadAsync();
            }
        }

        private async Task InitializeThreadAsync()
        {
            var response = await _openAI.CreateThreadAsync();
            _threadId = response.id;
        }

        public async Task SendVoiceMessageAsync(AudioClip clip)
        {
            var transcriptionResponse = await _openAI.CreateTranscriptionAsync(clip);
            await SendMessageAsync(transcriptionResponse.text);
        }
        
        public async Task SendMessageAsync(string message, string instruction = null)
        {
            var response = await _openAI.SendMessageAsync(_assistantId, _threadId, message, instruction);
            await HandleSendMessageResult(response);
        }

        private async Task HandleSendMessageResult(SendMessageResult result)
        {
            if (result.Type == MessageResultType.TextResult)
            {
                textResultDelegate?.Invoke(result.Message);
            }
            else if (result.Type == MessageResultType.RequiredAction)
            {
                await HandleRequiredAction(result.RunId, result.RequiredAction);
            }
        }

        private async Task HandleRequiredAction(string runId, RequiredAction requiredAction)
        {
            if (toolCallsDelegate == null) return;

            if (requiredAction.submitToolOutputs.toolCalls == null ||
                requiredAction.submitToolOutputs.toolCalls.Length == 0) return;

            var outputs = new Dictionary<string, string>();

            foreach (var toolCall in requiredAction.submitToolOutputs.toolCalls)
            {
                var function = toolCall.function;
                outputs[toolCall.id] = toolCallsDelegate(function.name, function.arguments);
            }

            var response = await _openAI.SubmitToolsOutputAsync(_threadId, runId, outputs);
            await HandleSendMessageResult(response);
        }
    }
}

public delegate string ToolCallsDelegate(string functionName, string arguments);

public delegate void TextResultDelegate(string textResult);