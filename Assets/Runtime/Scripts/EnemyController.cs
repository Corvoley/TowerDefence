using FishNet.Component.Animating;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[Serializable]
public enum EnemyAIState
{
    Idle,
    Walking,
    Chasing,
    Attacking,
}
public class EnemyController : NetworkBehaviour
{
    [SerializeField] private GameObject model;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float aggroDistance;
    [SerializeField] private float attackDistance;

    [SerializeField] public Transform defaultTarget;
    [SerializeField] private Transform currentTarget;


    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackSpeedMultiplier;
    [SerializeField] private float attackStartPercent;
    [SerializeField] private float attackDurationPercent;
    [SerializeField] private Collider weaponCollider;
    private float attackTimer;
    [SerializeField] private float attackClipDuration;


    [SerializeField] private float refreshRateAI = 0.2f;
    private float timerAI;



    private List<Transform> positionsList = new List<Transform>();
    private NavMeshAgent agent;
    private NetworkAnimator networkAnimator;
    private Animator animator;


    public EnemyAIState currentEnemyState;

    private int positionIndex = 0;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        networkAnimator = GetComponent<NetworkAnimator>();
        animator = GetComponent<Animator>();
    }
    public override void OnStartClient()
    {
        
        base.OnStartClient();
        SetupEnemy();
        FillAttackAnimationInfo();
    }


    [ServerRpc(RequireOwnership = false)]
    public void SetupEnemy()
    {
        currentTarget = defaultTarget;
        currentEnemyState = EnemyAIState.Walking;
       
    }

    private void Update()
    {

        ModelDirectionHandler();
        StateMachine();
    }
    
    private void StateMachine()
    {
        if (Time.time >= timerAI)
        {
            switch (currentEnemyState)
            {
                case EnemyAIState.Idle:
                    break;
                case EnemyAIState.Walking:
                    WalkingStateHandler();
                    break;
                case EnemyAIState.Chasing:
                    ChasingStateHandler();
                    break;
                case EnemyAIState.Attacking:
                    AttackingStateHandler();
                    break;
                default:
                    break;
            }
            timerAI = Time.time + refreshRateAI;
        }
        
        
    }
    private void FillAttackAnimationInfo()
    {

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "AttackAxe")
            {
                attackClipDuration = clip.length;
            }
        }


    }
    private void AttackingStateHandler()
    {
        agent.ResetPath();
        if (Vector3.Distance(transform.position, currentTarget.position) > attackDistance)
        {
            currentEnemyState = EnemyAIState.Walking;
        }        
        Attack();
    }

    private void WalkingStateHandler()
    {
        Transform closestTarget = GetClosestTarget(GameManager.instance.alliesTransformList);
        if (closestTarget != null)
        {
            currentTarget = closestTarget;
            currentEnemyState = EnemyAIState.Chasing;
        }
       
        agent.SetDestination(currentTarget.position);
        if (Vector3.Distance(transform.position, currentTarget.position) <= attackDistance)
        {
            currentEnemyState = EnemyAIState.Attacking;
        }
    }
    private void ChasingStateHandler()
    {
        
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > aggroDistance * 1.5f)
        {
            currentTarget = defaultTarget;
            currentEnemyState = EnemyAIState.Walking;
        }
        
        agent.SetDestination(currentTarget.position);
        if (Vector3.Distance(transform.position, currentTarget.position) <= attackDistance)
        {
            currentEnemyState = EnemyAIState.Attacking;
        }

    }

    private async void Attack()
    {

        if (Time.time >= attackTimer)
        {
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
        var end = (attackClipDuration / attackSpeedMultiplier) - (start + duration);
        await Awaitable.WaitForSecondsAsync(end);

        // parar a task quando destruir o inimigo
    }

    private Transform GetClosestTarget(List<Transform> targetList)
    {
        if (targetList.Count == 0) return null;

        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < targetList.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, targetList[i].position);
            if (distance <= aggroDistance)
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = targetList[i];
                }
            }

        }
        return closestTarget;
    }

    private void ModelDirectionHandler()
    {
        if (agent.velocity == Vector3.zero)
        {
            return;
        }
        model.transform.rotation = Quaternion.Slerp(model.transform.rotation, Quaternion.LookRotation(currentEnemyState != EnemyAIState.Attacking ? agent.velocity.normalized : currentTarget.position - transform.position), Time.deltaTime * rotationSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroDistance);

    
    }
}
