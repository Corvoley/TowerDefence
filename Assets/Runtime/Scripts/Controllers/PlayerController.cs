using FishNet.Component.Animating;
using FishNet.Object;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;



public class PlayerController : NetworkBehaviour, IEnemyTarget
{

    public Action OnSetupFinished;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask interactMask;

    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;

    [Header("Model")]
    [SerializeField] private GameObject modelObj;
    [SerializeField] private GameObject worldCanvasObj;
    [SerializeField] private GameObject mainCanvasObj;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] public PlayerClientManager playerClientManagerRef;


    [Header("Attack")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackSpeedMultiplier;
    [SerializeField] private float attackStartPercent;
    [SerializeField] private float attackDurationPercent;
    [SerializeField] private Collider weaponCollider;
    [SerializeField] private ParticleSystem particle;


    private float attackTimer;
    private float attackClipDuration;
    private Vector2 inputVector;
    private bool canRotate = true;

    private PlayerInputActions playerInputActions;

    [Header("Camera")]
    private Camera mainCamera;
    private CinemachineCamera playerCamera;
    [SerializeField] private float cameraYOffset;

    [Header("Interaction")]
    [SerializeField] private float interactRange;

    public IEnemyTarget.TargetType Type => IEnemyTarget.TargetType.Player;

    public bool IsActive => true;

    public override async void OnStartClient()
    {        
        base.OnStartClient();
        if (base.IsServerInitialized)
        {
            await AddPlayerToList();
        }
        if (base.IsOwner)
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Enable();
            playerInputActions.Player.Attack.performed += Attack;
            playerInputActions.Player.Interact.performed += Interact_performed;
            weaponCollider.enabled = false;
            
            await SetupPlayer();
        }
        else
        {
            mainCanvasObj.SetActive(false);
            this.enabled = false;
        }
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        
        (bool success, Vector3 position, RaycastHit hitInfo) = UtilsClass.GetMouseWorldPosition(interactMask);
        if (success)
        {
            if (Vector3.Distance(transform.position, position) <= interactRange)
            {
                IInteractable interactable = hitInfo.collider.gameObject.GetComponentInParent<IInteractable>();               
                if (interactable != null)
                {
                    interactable.OnInteract(this);
                }
                
            }
           
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        GameManager.Instance.RemovePlayerFromAlliesList(this.NetworkObject);
        if (base.IsOwner)
        {
            playerInputActions.Player.Disable();
            playerInputActions.Player.Attack.performed -= Attack;          
        }
    }

    public async Task SetupPlayer()
    {     
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<CinemachineCamera>();

        playerCamera.Follow = transform;
        playerCamera.transform.position = Vector3.zero;


        while (!GameManager.Instance.alliesNetworkObjectList.Contains(this.NetworkObject))
        {
            await Awaitable.WaitForSecondsAsync(0.1f);
        }
        await Awaitable.WaitForSecondsAsync(0.1f);
        var index = GameManager.Instance.alliesNetworkObjectList.IndexOf(this.NetworkObject);
        transform.position = GameManager.Instance.playerSpawnPoint[index].position;
        await Awaitable.WaitForSecondsAsync(0.1f);
        ActivatePlayerChilds();


        mainCanvasObj.SetActive(true);
        var button = mainCanvasObj.transform.Find("exitButton").GetComponent<Button>();
        button.onClick.AddListener(async () => { await playerClientManagerRef.LeaveMatch(); });

        FillAttackAnimationInfo();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivatePlayerChilds()
    {
        NetworkObject modelObjNetworkObject = transform.GetChild(0).GetComponent<NetworkObject>();
        Spawn(modelObjNetworkObject.gameObject, Owner);

        
        NetworkObject canvasObjNetworkObject = transform.GetChild(1).GetComponent<NetworkObject>();
        Spawn(canvasObjNetworkObject.gameObject);
    }

    public void SetUsername(string username)
    {
        usernameText.text = username;
    }
    private async Task AddPlayerToList()
    {
        await Awaitable.WaitForSecondsAsync(0.5f);
        GameManager.Instance.AddPlayerToAlliesList(this.NetworkObject);
        Debug.Log("Id Adicionado do player: " + this.NetworkObject);
    }
    private void Update()
    {
        InputHandler();
        RotateToMousePosition();
    }
    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {

        Vector3 force = new Vector3(movementSpeed * inputVector.x, 0, movementSpeed * inputVector.y);
        rb.AddForce(force, ForceMode.Force);

    }

    private void FillAttackAnimationInfo()
    {

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Attack")
            {
                attackClipDuration = clip.length;
            }
        }


    }
    private async void Attack(InputAction.CallbackContext context)
    {

        if (Time.time >= attackTimer)
        {
            attackTimer = Time.time + attackCooldown + (attackClipDuration / attackSpeedMultiplier);
            await AttackTask(attackStartPercent * (attackClipDuration / attackSpeedMultiplier), attackDurationPercent * (attackClipDuration / attackSpeedMultiplier), weaponCollider);
            
        }
    }
    private async Task AttackTask(float start, float duration, Collider weaponCollider)
    {

        canRotate = false;
        networkAnimator.SetTrigger("Attack");
        animator.SetFloat("AttackSpeedMultiplier", attackSpeedMultiplier);
        await Awaitable.WaitForSecondsAsync(start);
        if (particle != null) particle.Play();
        if (weaponCollider != null) weaponCollider.enabled = true;
        await Awaitable.WaitForSecondsAsync(duration);
        if (particle != null) particle.Stop();
        if (weaponCollider != null) weaponCollider.enabled = false;
        var end = (attackClipDuration / attackSpeedMultiplier) - (start + duration);
        await Awaitable.WaitForSecondsAsync(end);
        canRotate = true;
    }


    private void InputHandler()
    {
        if (playerInputActions != null)
        {

            inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        }
    }

    private void RotateToMousePosition()
    {
        if (!canRotate) return;
        (bool success, Vector3 position, RaycastHit hitInfo) = UtilsClass.GetMouseWorldPosition(groundMask);
        if (success)
        {

            Vector3 targetDirection = position - modelObj.transform.position;
            targetDirection.y = 0;
            float singleStep = rotationSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(modelObj.transform.forward, targetDirection, singleStep, 0.0f);
            modelObj.transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    
}
