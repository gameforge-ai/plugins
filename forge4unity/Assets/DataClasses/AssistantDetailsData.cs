using System;
using System.Collections.Generic;

[Serializable]
public class AssistantDetailsData
{
    public string id;
    public string openai_assistant_id;
    public string last_updated;
    public string model;
    public string name;
    public string selected_voice;
    public string instructions;
    public string recent_events;
    public string campaign_id;
    public string entity_id;
    public Dictionary<string, string> metadata;
}
