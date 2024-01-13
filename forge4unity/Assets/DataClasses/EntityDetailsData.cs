using System;

[Serializable]
public class EntityDetailsData
{
    public string id;
    public string campaign_id;
    public string entity_type;
    public string name;
    public string description;
    public string target_text;
    public GeneratedImageData image;
    public GeneratedImageData map;
    public GeneratedImageData[] generated_images;
    public GeneratedImageData[] generated_maps;
    public string selected_image_id;
    public string selected_map_id;
    public PointData point;
    public bool is_hidden;
    public int generation_count;
    public string parent_id;
    public string assistant;
    public string openai_assistant_id;
    public string parent_map_id;
}
