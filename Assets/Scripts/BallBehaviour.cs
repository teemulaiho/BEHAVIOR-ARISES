using System.Collections;
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
    [SerializeField] GameManagerBehaviour   gameManager;
    [SerializeField] Rigidbody              rigidBody;
    [SerializeField] BallState              ballState;
    [SerializeField] LeadAgentBehaviour     agent;
    [SerializeField] EnemyBehaviour         enemy;
    [SerializeField] Vector3                fleeFrom;
    [SerializeField] Vector3                newPos;
    [SerializeField] float                  speed = 2.5f;
    [SerializeField] float                  defaultSpeed = 2.5f;

    public void Init(GameManagerBehaviour p_gameManger)
    {
        gameManager                         = p_gameManger;
        rigidBody                           = GetComponent<Rigidbody>();
        ballState                           = BallState.Free;
        agent                               = null;
    }

    public void BallUpdate()
    {
        if (agent != null)
        {
            rigidBody.velocity = Vector3.zero;
            transform.position = Vector3.MoveTowards(transform.position, agent.transform.position, speed * Time.deltaTime);
        }
        else if (enemy != null)
        {
            rigidBody.velocity = Vector3.zero;
            transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
        }

        if (agent == null || enemy == null)
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

    public void RemoveAgent()
    {
        agent = null;
    }

    public void SetAgent(Agent a)
    {
        EnemyBehaviour e_agent = a as EnemyBehaviour;
        LeadAgentBehaviour p_agent = a as LeadAgentBehaviour;

        if (p_agent && agent != p_agent)
        {
            agent = p_agent;
            speed = p_agent.GetNavMeshAgentSpeed();

            enemy = null;
        }

        if (e_agent)
        {
            enemy = e_agent;
            speed = e_agent.GetNavMeshAgentSpeed();

            agent = null;
        }
    }

    public bool RemoveAgent(Agent a)
    {
        EnemyBehaviour e_agent = a as EnemyBehaviour;
        LeadAgentBehaviour p_agent = a as LeadAgentBehaviour;

        if (agent != null)
        {
            if (agent == p_agent)
            {
                agent = null;
                return true;
            }
        }
        else if (enemy != null)
        {
            if (enemy == e_agent)
            {
                enemy = null;
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

    public BallBehaviour Getball()
    {
        return this;
    }

    public LeadAgentBehaviour GetAgent()
    {
        return agent;
    }
}
