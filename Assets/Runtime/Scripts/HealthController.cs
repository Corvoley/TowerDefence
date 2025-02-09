using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class HealthController : NetworkBehaviour
{
    [AllowMutableSyncType]
    [SerializeField] 
    private SyncVar<float> health = new SyncVar<float>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(!base.IsOwner) this.enabled = false;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdateHealth(this, -1);
        }
    }


    [ServerRpc]
    public void UpdateHealth(HealthController script, float amountToChange) =>  script.health.Value += amountToChange;
    

}
