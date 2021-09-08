using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerBehaviour : MonoBehaviour
{
    static GameManagerBehaviour instance;

    [SerializeField] Canvas screenUI;

    public HealthbarBehaviour[] healthBars;
    
    AgentBehaviour agentPrefab;
    BallBehaviour ballPrefab;
    GoalBehaviour goalPrefab;
    PowerupBehaviour powerupPrefab;
    CubeBehaviour cubePrefab;
    GameObject safepointPrefab;

    List<AgentBehaviour> agents = new List<AgentBehaviour>();
    List<BallBehaviour> balls = new List<BallBehaviour>();
    List<GoalBehaviour> goals = new List<GoalBehaviour>();
    List<PowerupBehaviour> powerups = new List<PowerupBehaviour>();
    List<CubeBehaviour> cubes = new List<CubeBehaviour>();
    List<GameObject> safepoints = new List<GameObject>();

    public Dictionary<AgentBehaviour, BallBehaviour> chaseInfo = new Dictionary<AgentBehaviour, BallBehaviour>();
    public Dictionary<AgentBehaviour, BallBehaviour> captureInfo = new Dictionary<AgentBehaviour, BallBehaviour>();
    public Dictionary<AgentBehaviour, GoalBehaviour> goalInfo = new Dictionary<AgentBehaviour, GoalBehaviour>();

    int agentAmount = 4;

    bool toggle;


    // Is Called Before Start()
    private void Awake()
    {
        Debug.Log("Awake GameManager.");
        //CheckForDuplicateGameObjects();     // Remove DontDestroyOnLoad Duplicates.
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start GameManager.");

        // Instantiate GameObjects
        {
            // Agent Instantiate.
            {
                GameObject agentParent = new GameObject("AGENTS");
                agentPrefab = Resources.Load<AgentBehaviour>("Prefabs/Agent");
                for (int i = 0; i < agentAmount; i++)
                {
                    agentPrefab = Instantiate(agentPrefab);
                    agentPrefab.transform.SetParent(agentParent.transform);
                    agentPrefab.transform.position = new Vector3(Random.Range(1, 50), 2, Random.Range(-1, -50));
                    agentPrefab.team = i % 2;
                    agents.Add(agentPrefab);
                }
            }

            // Ball Instantiate.
            {
                GameObject ballParent = new GameObject("BALLS");
                ballPrefab = Resources.Load<BallBehaviour>("Prefabs/Ball");
                ballPrefab = Instantiate(ballPrefab);
                ballPrefab.transform.SetParent(ballParent.transform);
                ballPrefab.transform.position = new Vector3(Random.Range(1, 50), 1, Random.Range(-1, -50));
                balls.Add(ballPrefab);
            }

            // Goal Instantiate.
            {
                GameObject goalParent = new GameObject("GOALS");
                goalPrefab = Resources.Load<GoalBehaviour>("Prefabs/Goal");
                goalPrefab = Instantiate(goalPrefab);
                goalPrefab.transform.SetParent(goalParent.transform);
                goalPrefab.transform.position = new Vector3(25, 1.1f, -25);
                goals.Add(goalPrefab);
            }

            // Powerup Instantiate.
            {
                GameObject powerupParent = new GameObject("POWERUPS");
                powerupPrefab = Resources.Load<PowerupBehaviour>("Prefabs/Powerup");
                powerupPrefab = Instantiate(powerupPrefab);
                powerupPrefab.transform.SetParent(powerupParent.transform);
                powerupPrefab.transform.position = new Vector3(Random.Range(1, 50), Random.Range(2, 10), Random.Range(-1, -50));
                //powerupPrefab.transform.position = new Vector3(25, 1.1f, -25);
                powerups.Add(powerupPrefab);
            }

            // Cube Instantiate.
            {
                GameObject cubeParent = new GameObject("CUBES");
                cubePrefab = Resources.Load<CubeBehaviour>("Prefabs/Cube");
                for (int i = 0; i < 100; i++)
                {
                    cubePrefab = Instantiate(cubePrefab);
                    cubePrefab.transform.SetParent(cubeParent.transform);
                    cubePrefab.transform.position = new Vector3(Random.Range(1, 50), Random.Range(2, 10), Random.Range(-1, -50));
                    cubes.Add(cubePrefab);
                }
            }

            // Safe Point Instantiate
            {
                safepointPrefab = Resources.Load<GameObject>("Prefabs/SafePoint");
                safepointPrefab = Instantiate(safepointPrefab);
                safepoints.Add(safepointPrefab);
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

            for (int i = 0; i < powerups.Count; i++)
            {
                powerups[i].Init(this);
            }

            for (int i = 0; i < cubes.Count; i++)
            {
                cubes[i].Init(this);
            }
        }

        // Initialize Screen UI
        {
            // Initialize Health bars
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    healthBars[i].SetMaxHealth(agents[i].agentMaxHealth);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            toggle = !toggle;
            Debug.Log("toggle: " + toggle);
        }

        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].AgentUpdate();
        }

        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].BallUpdate();
        }

        for (int i = 0; i < powerups.Count; i++)
        {
            powerups[i].PowerupUpdate();
        }

        // Update Health bars
        {
            for (int i = 0; i < agents.Count; i++)
            {
                healthBars[i].SetHealth(agents[i].agentCurrentHealth);
            }
        }

        //for (int i = 0; i < cubes.Count; i++)
        //{
        //    cubes[i].CubeUpdate();
        //}
    }

    private void CheckForDuplicateGameObjects()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
            Destroy(gameObject);
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

    public PowerupBehaviour GetPowerup()
    {
        if (powerups.Count > 0)
        {
            return powerups[0];
        }

        return null;
    }

    public void SpawnPowerup()
    {

    }

    public AgentRole GetRole(int team)
    {
        int noneCount = 0;
        int leadCount = 0;
        int supportCount = 0;

        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i].team == team)
            {
                if (agents[i].agentRole == AgentRole.None)
                {
                    noneCount++;
                }
                else if (agents[i].agentRole == AgentRole.Lead)
                {
                    leadCount++;
                }
                else if (agents[i].agentRole == AgentRole.Support)
                {
                    supportCount++;
                }
            } 
        }

        if(leadCount == supportCount)
        {
            return AgentRole.Lead;
        }
        else if (leadCount > supportCount)
        {
            return AgentRole.Support;
        }
        else if (supportCount > leadCount)
        {
            return AgentRole.Lead;
        }

        return AgentRole.None;
    }

    public void SetAgent(AgentBehaviour p_agent)
    {
        balls[0].SetAgent(p_agent);
    }

    public AgentBehaviour GetNearestEnemyAgent(AgentBehaviour p_agent)
    {
        float distance = float.MaxValue;
        int closestAgentIndex = -1;

        if (agents != null && agents.Count > 0)
        {
            for (int i = 0; i < agents.Count; i++)
            {
                if (agents[i].team != p_agent.team &&
                    agents[i] != p_agent)
                {
                    if (Vector3.Distance(p_agent.transform.position, agents[i].transform.position) < distance)
                    {
                        distance = Vector3.Distance(p_agent.transform.position, agents[i].transform.position);
                        closestAgentIndex = i;
                    }
                }
            }

            return agents[closestAgentIndex];
        } 

        return null;
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
            //Debug.Log("GameManagerBehavior.cs - captureInfo contains. Agent: " + agent.meshRenderer.material.color); 
            if (captureInfo.Remove(agent))
            {
                //Debug.Log("GameManagerBehavior.cs - Successfully removed agent: " + agent.meshRenderer.material.color + " from captureInfo.");
            }
        }
        else
        {

            //Debug.Log("GameManagerBehavior.cs - Cannot find agent: " + agent.meshRenderer.material.color + " from captureInfo.");
        }
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

    public GameObject GetSafePoint()
    {
        if (safepoints.Count > 0)
            return safepoints[0];

        return null;
    }
}
