using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class FollowObject : MonoBehaviour {
    public Transform target;
    Vector3 destination;
    NavMeshAgent agent;

    void Start()
    {
        // Cache agent component and destination
        if (GameObject.FindGameObjectWithTag("Player"))
            target = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        destination = agent.destination;
    }

    void Update()
    {
        // Update destination if the target moves one unit
        if (target)
        {
            if (Vector3.Distance(destination, target.position) > 1.0f)
            {
                destination = target.position;
                agent.destination = destination;
            }
        }
    }
}
