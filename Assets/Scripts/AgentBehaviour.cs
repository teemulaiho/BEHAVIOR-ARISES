using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentState
{
    Idle,
    Deciding,
    Observing,
    Fetching,
    Returning
}
public class AgentBehaviour : MonoBehaviour
{
    public GameManagerBehaviour gameManager;
    AgentState agentState;

    public BallBehaviour targetBall;
    float agentSpeed;

    public void Init(GameManagerBehaviour p_gameManager)
    {
        gameManager = p_gameManager;
        agentState = AgentState.Idle;
        targetBall = null;
        agentSpeed = 2.5f;
    }

    public void AgentUpdate()
    {
        Sense();
        Act();
    }

    void Sense()
    {
        if (targetBall == null)
        {
            targetBall = gameManager.GetFreeBall(this);
            gameManager.UpdateChaseData(this, targetBall);
        }
    }

    void Decide()
    {

    }

    void Act()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetBall.transform.position, Time.deltaTime * agentSpeed);
    }

    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
