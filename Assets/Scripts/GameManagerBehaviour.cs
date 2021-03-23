﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerBehaviour : MonoBehaviour
{
    AgentBehaviour agentPrefab;
    BallBehaviour ballPrefab;
    GoalBehaviour goalPrefab;
    CubeBehaviour cubePrefab;

    List<AgentBehaviour> agents = new List<AgentBehaviour>();
    List<BallBehaviour> balls = new List<BallBehaviour>();
    List<GoalBehaviour> goals = new List<GoalBehaviour>();
    List<CubeBehaviour> cubes = new List<CubeBehaviour>();

    public Dictionary<AgentBehaviour, BallBehaviour> chaseInfo = new Dictionary<AgentBehaviour, BallBehaviour>();
    public Dictionary<AgentBehaviour, BallBehaviour> captureInfo = new Dictionary<AgentBehaviour, BallBehaviour>();
    public Dictionary<AgentBehaviour, GoalBehaviour> goalInfo = new Dictionary<AgentBehaviour, GoalBehaviour>();

    // Is Called Before Start()
    private void Awake()
    {
        Debug.Log("Awake GameManager.");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start GameManager.");

        // Agent Instantiate.
        {
            agentPrefab = Resources.Load<AgentBehaviour>("Prefabs/Agent");
            for (int i = 0; i < 3; i++)
            {
                agentPrefab = Instantiate(agentPrefab);
                agentPrefab.transform.position = new Vector3(Random.Range(1, 50), 2, Random.Range(-1, -50));
                agents.Add(agentPrefab);
            }
        }

        // Ball Instantiate.
        {
            ballPrefab = Resources.Load<BallBehaviour>("Prefabs/Ball");
            ballPrefab = Instantiate(ballPrefab);
            ballPrefab.transform.position = new Vector3(Random.Range(1, 50), 1, Random.Range(-1, -50));
            balls.Add(ballPrefab);
        }

        // Goal Instantiate.
        {
            goalPrefab = Resources.Load<GoalBehaviour>("Prefabs/Goal");
            goalPrefab = Instantiate(goalPrefab);
            goalPrefab.transform.position = new Vector3(25, 1.1f, -25);
            goals.Add(goalPrefab);
        }

        // Cube Instantiate.
        {
            cubePrefab = Resources.Load<CubeBehaviour>("Prefabs/Cube");
            for (int i = 0; i < 50; i++)
            {
                cubePrefab = Instantiate(cubePrefab);
                cubePrefab.transform.position = new Vector3(Random.Range(1, 50), Random.Range(2, 10), Random.Range(-1, -50));
                cubes.Add(cubePrefab);
            }
        }

        // Initialize GameObjects
        {
            for (int i = 0; i < agents.Count; i++)
            {
                goalInfo[agents[i]] = goals[0];
            }

            for (int i = 0; i < agents.Count; i++)
            {
                agents[i].Init(this);

                if (i == 0)
                    agents[i].meshRenderer.material.color = Color.green;
                else if (i == 1)
                    agents[i].meshRenderer.material.color = Color.red;
                else if (i == 2)
                    agents[i].meshRenderer.material.color = Color.blue;
                else if (i == 3)
                    agents[i].meshRenderer.material.color = Color.black;
            }

            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].Init(this);
            }

            for (int i = 0; i < goals.Count; i++)
            {
                goals[i].Init(this);
            }

            for (int i = 0; i < cubes.Count; i++)
            {
                cubes[i].Init(this);
            }
        }
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
            else
                return balls[i];
        }   
        
        return null;
    }

    public BallBehaviour GetBall()
    {
        if (balls.Count > 0)
        {
            return balls[0];
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

    public bool AmICaptured(BallBehaviour ball, out AgentBehaviour agent)
    {
        agent = null;

        if (ball == null)
            return false;

        if (captureInfo.Count == 0)
            return false;

        foreach (var pair in captureInfo)
        {
            if (pair.Value == ball)
            {
                agent = pair.Key;
                return true;
            }
        }

        return false;
    }

    public void UpdateChaseData(AgentBehaviour agent, BallBehaviour ball) // agent behavior calling this
    {
        if (!chaseInfo.ContainsKey(agent))
            chaseInfo[agent] = ball;

        if (captureInfo.ContainsKey(agent))
            captureInfo.Remove(agent);
    }

    public void UpdateCaptureData(AgentBehaviour agent, BallBehaviour ball)
    {
        captureInfo[agent] = ball;

        if (chaseInfo.ContainsKey(agent))
            chaseInfo.Remove(agent);
    }

    public void RemoveCaptureData(AgentBehaviour agent)
    {
        if (captureInfo.ContainsKey(agent))
        {
            Debug.Log("GameManagerBehavior.cs - captureInfo contains. Agent: " + agent.meshRenderer.material.color); 
            if (captureInfo.Remove(agent))
            {
                Debug.Log("GameManagerBehavior.cs - Successfully removed agent: " + agent.meshRenderer.material.color + " from captureInfo.");
            }
        }
        else
            Debug.Log("GameManagerBehavior.cs - Cannot find agent: " + agent.meshRenderer.material.color + " from captureInfo.");
    }

    public void Goal(BallBehaviour ball, AgentBehaviour agent)
    {
        captureInfo.Remove(agent);
        agent.AddScore();
    }

    public GoalBehaviour GetGoal(AgentBehaviour agent)
    {
        return goalInfo[agent];
    }
}
