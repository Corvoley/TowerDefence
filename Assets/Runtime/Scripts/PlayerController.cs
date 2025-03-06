using FishNet.CodeGenerating;
using FishNet.Component.Animating;
using FishNet.Example.ColliderRollbacks;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem;

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
    [SerializeField] private GameObject canvasObj;


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

    private Camera playerCamera;

    public override async void OnStartClient()
    {
        Debug.Log("OnStartClient Foi Chamado");
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
            this.enabled = false;
        }
    }

    public async Task SetupPlayer()
    {        
        Debug.Log("SetupPlayer foi chamado");
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, playerCamera.transform.position.z);
        playerCamera.transform.SetParent(transform);

        while (!GameManager.Instance.alliesNetworkObjectList.Contains(this.NetworkObject))
        {
            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        var index = GameManager.Instance.alliesNetworkObjectList.IndexOf(this.NetworkObject);       
        transform.position = GameManager.Instance.playerSpawnPoint[index].position;
        modelObj.SetActive(true);
        canvasObj.SetActive(true);
        FillAttackAnimationInfo();
    }

    public void SetUsername(string username)
    {
        usernameText.text = username;
    }
    private async Task AddPlayerToList()
    {
        await Awaitable.WaitForSecondsAsync(0.5f);
        GameManager.Instance.AddPlayerToList(this.NetworkObject);
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
            Debug.Log("Attack!!!!");
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
        particle.Play();
        weaponCollider.enabled = true;
        await Awaitable.WaitForSecondsAsync(duration);
        particle.Stop();
        weaponCollider.enabled = false;
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
            /*
            // Calculate the direction
            var direction = position - model.transform.position;

            // You might want to delete this line.
            // Ignore the height difference.
            direction.y = 0;

            // Make the transform look in the direction.
            model.transform.forward = direction;*/


            Vector3 targetDirection = position - modelObj.transform.position;
            targetDirection.y = 0;
            float singleStep = rotationSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(modelObj.transform.forward, targetDirection, singleStep, 0.0f);
            modelObj.transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        if (playerCamera == null) return (success: false, Vector3.zero);

        var ray = playerCamera.ScreenPointToRay(Input.mousePosition);

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
