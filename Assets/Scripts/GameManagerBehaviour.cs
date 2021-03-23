using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerBehaviour : MonoBehaviour
{
    AgentBehaviour agentPrefab;
    BallBehaviour ballPrefab;

    List<AgentBehaviour> agents = new List<AgentBehaviour>();
    List<BallBehaviour> balls = new List<BallBehaviour>();

    Dictionary<AgentBehaviour, BallBehaviour> chaseInfo = new Dictionary<AgentBehaviour, BallBehaviour>();

    // Is Called Before Start()
    private void Awake()
    {
        Debug.Log("Awake");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");

        agentPrefab = Resources.Load<AgentBehaviour>("Prefabs/Agent");
        agentPrefab = Instantiate(agentPrefab);
        agentPrefab.Init(this);
        agents.Add(agentPrefab);

        ballPrefab = Resources.Load<BallBehaviour>("Prefabs/Ball");
        ballPrefab = Instantiate(ballPrefab);
        ballPrefab.Init(this);
        balls.Add(ballPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].AgentUpdate();
        }

        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].BallUpdate();
        }
    }

    public BallBehaviour GetFreeBall(AgentBehaviour agent)
    {
        for (int i = 0; i < balls.Count; i++)
        {
            if (balls[i].GetState() == BallState.Free)
                return balls[i];
        }
        
        return null;
    }

    public void SetAgent(AgentBehaviour p_agent)
    {
        balls[0].SetAgent(p_agent);
    }

    // If vector3zero == not being chased
    public bool AmIBeingChased(BallBehaviour ball, ref Vector3 pos)
    {
        pos = Vector3.zero;

        if (ball == null)
            return false;

        if (chaseInfo.Count == 0)
            return false;

        foreach(var pair in chaseInfo)
        {
            if (pair.Value == ball)
            {
                pos = pair.Key.transform.position;
                return true;
            }
        }

        return false;
    }

    public void UpdateChaseData(AgentBehaviour agent, BallBehaviour ball) // agent behavior calling this
    {
        chaseInfo[agent] = ball;
    }
}
