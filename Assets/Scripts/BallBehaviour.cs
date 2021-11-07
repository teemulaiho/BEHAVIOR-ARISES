﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState
{
    Free,
    Targeted,
    Captured
}

public class BallBehaviour : MonoBehaviour
{
    public GameManagerBehaviour gameManager;
    public Rigidbody rigidBody;
    public BallState ballState;
    public AgentBehaviour agent;
    public Vector3 fleefrom;
    public Vector3 newPos;
    public float speed = 2.5f;
    public float defaultSpeed = 2.5f;

    public void Init(GameManagerBehaviour p_gameManger)
    {
        Debug.Log("Init BallBehaviour.");
        gameManager = p_gameManger;
        rigidBody = GetComponent<Rigidbody>();
        ballState = BallState.Free;
        agent = null;
    }

    public void BallUpdate()
    {
        if (agent != null)
        {
            rigidBody.velocity = Vector3.zero;
            transform.position = Vector3.MoveTowards(transform.position, agent.transform.position, speed * Time.deltaTime);
        }

        if (agent == null)
        {
            if (Vector3.Distance(transform.position, gameManager.GetGoalPosition()) > 50)
            {
                ResetBall();
            }
        }
    }

    public BallState GetState()
    {
        return ballState;
    }

    public void SetAgent(AgentBehaviour p_agent)
    {
        if(agent != p_agent)
        {
            agent = p_agent;
            speed = p_agent.agentSpeed;
        }
    }

    public bool RemoveAgent(AgentBehaviour p_agent)
    {
        if(agent != null)
        {
            if (agent == p_agent)
            {
                agent = null;
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Goal"))
        {
            if (other.gameObject.CompareTag("Goal"))
            {
                agent.AddScore();
                RemoveAgent(agent);
                ResetBall();
            }
        }
    }

    public void ResetBall()
    {
        transform.position = new Vector3(25, 3, -25);
        speed = defaultSpeed;
        ballState = BallState.Free;
    }
}
