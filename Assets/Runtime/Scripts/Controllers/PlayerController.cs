using FishNet.Component.Animating;
using FishNet.Object;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;



public class PlayerController : NetworkBehaviour
{

    public Action OnSetupFinished;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;

    [SerializeField] private LayerMask groundMask;


    [SerializeField] private Rigidbody rb;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private GameObject modelObj;
    [SerializeField] private GameObject worldCanvasObj;
    [SerializeField] private GameObject mainCanvasObj;
    [SerializeField] public PlayerClientManager playerClientManagerRef;

    [SerializeField] private TextMeshProUGUI usernameText;

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

    [SerializeField] private float cameraYOffset;

    private Camera mainCamera;
    private CinemachineCamera playerCamera;

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
            weaponCollider.enabled = false;
            
            await SetupPlayer();
        }
        else
        {
            mainCanvasObj.SetActive(false);
            this.enabled = false;
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
        //playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, playerCamera.transform.position.z);
        //playerCamera.transform.SetParent(transform);

        while (!GameManager.Instance.alliesNetworkObjectList.Contains(this.NetworkObject))
        {
            await Awaitable.WaitForSecondsAsync(0.1f);
        }

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
        rb.AddForce(force);

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
        var (success, position) = GetMousePosition();
        if (success)
        {

            Vector3 targetDirection = position - modelObj.transform.position;
            targetDirection.y = 0;
            float singleStep = rotationSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(modelObj.transform.forward, targetDirection, singleStep, 0.0f);
            modelObj.transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        if (mainCamera == null) return (success: false, Vector3.zero);

        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            // The Raycast hit something, return with the position.
            return (success: true, position: hitInfo.point);
        }
        else
        {
            // The Raycast did not hit anything.
            return (success: false, position: Vector3.zero);
        }
    }
}
