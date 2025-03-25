using FishNet.Object;
using System.Threading.Tasks;
using UnityEngine;

public class NodeController : NetworkBehaviour
{
    public GameManager gameManager; 

    [SerializeField] private ResourceNodeOS resourceNodeOS;

    public override void OnStopClient()
    {
        base.OnStopClient();       
        gameManager.SpawnResources(resourceNodeOS, transform.position);
    }

    
}
