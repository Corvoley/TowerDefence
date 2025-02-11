using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : NetworkBehaviour
{
    private List<Transform> positionsList = new List<Transform>();
    private NavMeshAgent agent;

    private int positionIndex = 0;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        var positions = GameObject.Find("WalkPoints");
        for (int i = 0; i < positions.transform.childCount; i++)
        {
            positionsList.Add(positions.transform.GetChild(i));
        }
        GoToNextPosition();
    }

    private void Update()
    {
        CheckNewPosition();
    }
    private void CheckNewPosition()
    {
        if (Vector3.Distance(agent.destination, transform.position) < 1f)
        {
            GoToNextPosition();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GoToNextPosition()
    {
        positionIndex++;
        if (positionIndex >= positionsList.Count) { positionIndex = 0;}
        agent.SetDestination(positionsList[positionIndex].position);

    }
}
