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
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{

    public Action OnSetupFinished;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;

    [SerializeField] private LayerMask groundMask;


    [SerializeField] private Rigidbody rb;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private GameObject model;


    [SerializeField] private TextMeshProUGUI usernameText;

    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackSpeedMultiplier;
    [SerializeField] private float attackStartPercent;
    [SerializeField] private float attackDurationPercent;
    [SerializeField] private Collider weaponCollider;
    private float attackTimer;
    private float attackClipDuration;



    private Vector2 inputVector;
    private PlayerInputActions playerInputActions;

    [SerializeField] private float cameraYOffset;

    private Camera playerCamera;
    private void Awake()
    {

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Attack.performed += Attack;
        weaponCollider.enabled = false;

    }



    public void SetupPlayer()
    {
        Debug.Log("SetupPlayer foi chamado");
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, playerCamera.transform.position.z);
        playerCamera.transform.SetParent(transform);
        transform.position = GameManager.instance.playerSpawnPoint.position;
        GameManager.instance.alliesTransformList.Add(transform);

        FillAttackAnimationInfo();
    }

    public void SetUsername(string username)
    {
        usernameText.text = username;
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
                attackClipDuration =  clip.length;
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
        networkAnimator.SetTrigger("Attack");
        animator.SetFloat("AttackSpeedMultiplier", attackSpeedMultiplier);
        await Awaitable.WaitForSecondsAsync(start);

        weaponCollider.enabled = true;
        await Awaitable.WaitForSecondsAsync(duration);

        weaponCollider.enabled = false;
        var end = (attackClipDuration / attackSpeedMultiplier ) - (start + duration) ;
        await Awaitable.WaitForSecondsAsync(end);
    }


    private void InputHandler()
    {
        inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    private void RotateToMousePosition()
    {
        var (success, position) = GetMousePosition();
        if (success)
        {
            // Calculate the direction
            var direction = position - model.transform.position;

            // You might want to delete this line.
            // Ignore the height difference.
            direction.y = 0;

            // Make the transform look in the direction.
            model.transform.forward = direction;
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

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient Foi Chamado");
        base.OnStartClient();
        if (base.IsOwner)
        {
            SetupPlayer();
        }
        else
        {
            this.enabled = false;
        }
    }

}
