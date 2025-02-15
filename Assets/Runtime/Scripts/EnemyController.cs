using FishNet.Component.Animating;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float attackRate;
    private float attackCooldown;
    [SerializeField] private LayerMask attackableMask;
    [SerializeField] private Vector3 attackExtends;
    [SerializeField] public Transform attackPoint;
    [SerializeField] public Transform defaultTarget;
    [SerializeField] private Transform currentTarget;


    [SerializeField] private float refreshRateAI = 0.2f;
    private float timerAI;



    private List<Transform> positionsList = new List<Transform>();
    private NavMeshAgent agent;
    private NetworkAnimator networkAnimator;


    public EnemyAIState currentEnemyState;

    private int positionIndex = 0;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }
    public override void OnStartClient()
    {
        
        base.OnStartClient();
        SetupEnemy();
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

    private void Attack()
    {

        if (Time.time >= attackCooldown)
        {
            //isAttacking = true;
            Collider[] hit = Physics.OverlapBox(attackPoint.position, attackExtends, attackPoint.rotation, attackableMask);
            networkAnimator.SetTrigger("Attack");
            //networkAnimator.SetTrigger("Attack");
            foreach (Collider enemy in hit)
            {
                enemy.GetComponent<HealthController>().DealDamage(5);
            }
            attackCooldown = Time.time + 1f / attackRate;
            //isAttacking = false;
        }

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

        Gizmos.matrix = attackPoint.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, attackExtends * 2);
    }
}
