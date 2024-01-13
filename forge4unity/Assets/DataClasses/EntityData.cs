using System;

[Serializable]
public class EntityData
{
    public string id;
    public string name;
    public string entity_type;
    public string parent_id;
    public EntityData[] children;
    public string parent_map_id;
}
