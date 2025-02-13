using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : NetworkBehaviour
{
    [AllowMutableSyncType]
    [SerializeField]
    private SyncVar<float> health = new SyncVar<float>();
    [AllowMutableSyncType]
    [SerializeField]
    private SyncVar<bool> canBeDamaged = new SyncVar<bool>(true);

    [SerializeField] private bool isPlayer = false;

    [SerializeField] private float maxHealth;


    [SerializeField] private Image healthSlider;
    private void Awake()
    {
    }

    private void Health_OnChange(float prev, float next, bool asServer)
    {
        if (asServer || base.IsClientOnlyStarted)
        {

            UpdateHealthBar(next);
        }
    }

    public override void OnStartClient()
    {
        health.OnChange += Health_OnChange;
        SetCanBeDamaged(this, true);
        if (!isPlayer) return;
        base.OnStartClient();
        if (!base.IsOwner) this.enabled = false;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            DealDamage(10);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            HealDamage(10);
        }
    }
    public void DealDamage(float amount)
    {
        if (canBeDamaged.Value)
        {
            var health = this.health.Value;
            health -= amount;
            health = Mathf.Clamp(health, 0, maxHealth);
            if (health <= 0)
            {
                GameManager.instance.DespawnEnemy(this.gameObject);
            }
            else
            {
                UpdateHealth(this, health);
            }
            //OnHealthChanged?.Invoke();
        }
    }
    [ObserversRpc]
    private void UpdateHealthBar(float amount)
    {         
        healthSlider.fillAmount = amount / maxHealth;
    }

    public void HealDamage(float amount)
    {
        var health = this.health.Value;
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealth(this, health);

        //OnHealthChanged?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealth(HealthController script, float value) {script.health.Value = value; }
    [ServerRpc(RequireOwnership = false)]
    public void SetCanBeDamaged(HealthController script, bool value) => script.canBeDamaged.Value = value;

}
