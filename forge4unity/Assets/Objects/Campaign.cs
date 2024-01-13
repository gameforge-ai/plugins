using UnityEngine;

public class Campaign : MonoBehaviour
{
    [SerializeField]
    private CampaignData campaign;

    public void Initialize(CampaignData data)
    {
        campaign = data;
    }
}
