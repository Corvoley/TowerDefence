using FishNet.CodeGenerating;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{

    public Action OnSetupFinished;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;

    [SerializeField] private Vector3 attackExtends;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRate;
    private float attackCooldown;
    private bool isAttacking;


    [SerializeField] private Rigidbody rb;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private GameObject model;


    [SerializeField] private TextMeshProUGUI usernameText;



    private Vector2 inputVector;
    private PlayerInputActions playerInputActions;

    [SerializeField] private float cameraYOffset;

    private Camera playerCamera;
    private void Awake()
    {

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Attack.performed += Attack;

    }



    public void SetupPlayer()
    {
        Debug.Log("SetupPlayer foi chamado");
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, playerCamera.transform.position.z);
        playerCamera.transform.SetParent(transform);
        transform.position = GameManager.instance.playerSpawnPoint.position;
        GameManager.instance.alliesTransformList.Add(transform);
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
    private void Attack(InputAction.CallbackContext context)
    {

        if (Time.time >= attackCooldown)
        {
            isAttacking = true;
            Collider[] hit = Physics.OverlapBox(attackPoint.position, attackExtends, attackPoint.rotation, enemyMask);
            networkAnimator.SetTrigger("Attack");
            //networkAnimator.SetTrigger("Attack");
            foreach (Collider enemy in hit)
            {
                enemy.GetComponent<HealthController>().DealDamage(25);
            }
            attackCooldown = Time.time + 1f / attackRate;
            isAttacking = false;
        }

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




    private void OnDrawGizmos()
    {
        Gizmos.matrix = attackPoint.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, attackExtends * 2);


    }
}
