using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerBehaviour : MonoBehaviour
{
    [SerializeField] Canvas screenUI;
    Canvas screenUIPrefab;

    public HealthbarBehaviour[]healthBars;
    public TMP_Text[] agentScoreTexts;
    public Image[] agentImages;

    EnemyBehaviour enemyPrefab;
    LeadAgentBehaviour leadAgentPrefab;
    SupportAgentBehaviour supportAgentPrefab;
    BallBehaviour ballPrefab;
    GoalBehaviour goalPrefab;
    PowerupBehaviour powerupPrefab;
    CubeBehaviour cubePrefab;
    GameObject safepointPrefab;
    RectTransform agentUIPrefab;
    TorchBehaviour torchPrefab;

    List<EnemyBehaviour> enemies                                        = new List<EnemyBehaviour>();
    List<LeadAgentBehaviour> leadAgents                                 = new List<LeadAgentBehaviour>();
    List<SupportAgentBehaviour> supportAgents                           = new List<SupportAgentBehaviour>();
    List<BallBehaviour> balls                                           = new List<BallBehaviour>();
    List<GoalBehaviour> goals                                           = new List<GoalBehaviour>();
    List<PowerupBehaviour> powerups                                     = new List<PowerupBehaviour>();
    List<CubeBehaviour> cubes                                           = new List<CubeBehaviour>();
    List<GameObject> safepoints                                         = new List<GameObject>();
    List<TorchBehaviour> torches                                        = new List<TorchBehaviour>();
    
    public List<Sprite> uiSprites                                       = new List<Sprite>();
    
    public Dictionary<LeadAgentBehaviour, BallBehaviour> chaseInfo      = new Dictionary<LeadAgentBehaviour, BallBehaviour>();
    public Dictionary<LeadAgentBehaviour, BallBehaviour> captureInfo    = new Dictionary<LeadAgentBehaviour, BallBehaviour>();
    public Dictionary<LeadAgentBehaviour, GoalBehaviour> goalInfo       = new Dictionary<LeadAgentBehaviour, GoalBehaviour>();

    public Dictionary<TMP_Text, LeadAgentBehaviour> scoreInfo           = new Dictionary<TMP_Text, LeadAgentBehaviour>();
    public Dictionary<Image, LeadAgentBehaviour> agentTargetInfo        = new Dictionary<Image, LeadAgentBehaviour>();

    int leadAgentAmount                                                 = 0;
    int supportAgentAmount                                              = 1;
    int enemyAmount                                                     = 1;
    int torchAmount                                                     = 4;
    
    bool toggle;
    public bool isTeamPlayActive;


    // Is Called Before Start()
    private void Awake()
    {
        Debug.Log("Awake GameManager.");

        //screenUIPrefab = Resources.Load<Canvas>("Prefabs/Canvas");
        //screenUI = Instantiate(screenUIPrefab);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start GameManager.");

        // Instantiate GameObjects
        {
            // UI Instantiate
            {
                //agentUIPrefab = Resources.Load<RectTransform>("Prefabs/UI/AgentUI");
                //agentUIPrefab = Instantiate(agentUIPrefab);
                //agentUIPrefab.transform.parent = screenUI.transform;
                //agentUIPrefab.localPosition = Vector3.zero;
                //agentUIPrefab.anchorMin = new Vector2(0, 1);
                //agentUIPrefab.anchorMax = new Vector2(0, 1);
            }

            // Torch Instantiate
            {
                GameObject torchParent = new GameObject("TORCHES");
                torchPrefab = Resources.Load<TorchBehaviour>("Prefabs/Torch");
                for (int i = 0; i < torchAmount; i++)
                {
                    torchPrefab = Instantiate(torchPrefab);
                    torchPrefab.transform.SetParent(torchParent.transform);
                    torchPrefab.name = "Torch " + i;
                    torchPrefab.Init(this, i);
                    torches.Add(torchPrefab);

                    if (i == 0)
                        torchPrefab.transform.position = new Vector3(45, 2, -45);
                    else if (i == 1)
                        torchPrefab.transform.position = new Vector3(5, 2, -45);
                    else if (i == 2)
                        torchPrefab.transform.position = new Vector3(5, 2, -5);
                    else if (i == 3)
                        torchPrefab.transform.position = new Vector3(45, 2, -5);
                }            
            }

            // Enemy Instantiate.
            {
                GameObject enemyParent = new GameObject("ENEMIES");
                enemyPrefab = Resources.Load<EnemyBehaviour>("Prefabs/Enemy");

                for (int i = 0; i < enemyAmount; i++)
                {
                    enemyPrefab = Instantiate(enemyPrefab);
                    enemyPrefab.name = "Enemy";
                    enemyPrefab.transform.SetParent(enemyParent.transform);
                    enemyPrefab.transform.position = new Vector3(Random.Range(10, 20), 2, Random.Range(-20, -30));
                    enemies.Add(enemyPrefab);
                }
            }

            // Agent Instantiate.
            {
                GameObject agentParent = new GameObject("AGENTS");
                leadAgentPrefab = Resources.Load<LeadAgentBehaviour>("Prefabs/LeadAgent");
                for (int i = 0; i < leadAgentAmount; i++)
                {
                    leadAgentPrefab = Instantiate(leadAgentPrefab);
                    leadAgentPrefab.transform.SetParent(agentParent.transform);
                    leadAgentPrefab.transform.position = new Vector3(Random.Range(1, 50), 2, Random.Range(-1, -50));
                    leadAgentPrefab.team = i % 2;
                    leadAgents.Add(leadAgentPrefab);
                }

                supportAgentPrefab = Resources.Load<SupportAgentBehaviour>("Prefabs/SupportAgent");
                for (int i = 0; i < supportAgentAmount; i++)
                {
                    supportAgentPrefab = Instantiate(supportAgentPrefab);
                    supportAgentPrefab.transform.SetParent(agentParent.transform);
                    supportAgentPrefab.transform.position = new Vector3(Random.Range(1, 50), 2, Random.Range(-1, -50));
                    supportAgentPrefab.SetAgentTeam(i % 2);
                    supportAgents.Add(supportAgentPrefab);
                }

                //GameObject 
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
                goalPrefab.transform.position = new Vector3(45, 1.1f, -25);
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
                //GameObject cubeParent = new GameObject("CUBES");
                //cubePrefab = Resources.Load<CubeBehaviour>("Prefabs/Cube");
                //for (int i = 0; i < 100; i++)
                //{
                //    cubePrefab = Instantiate(cubePrefab);
                //    cubePrefab.transform.SetParent(cubeParent.transform);
                //    cubePrefab.transform.position = new Vector3(Random.Range(1, 50), Random.Range(2, 10), Random.Range(-1, -50));
                //    cubes.Add(cubePrefab);
                //}
            }

            // Safe Point Instantiate
            {
                safepointPrefab = Resources.Load<GameObject>("Prefabs/SafePoint");
                safepointPrefab = Instantiate(safepointPrefab);
                safepointPrefab.tag = "SafePoint";
                safepointPrefab.transform.position = new Vector3(5,1.1f,-25);
                safepoints.Add(safepointPrefab);
            }
        }

        // Initialize GameObjects
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Init(this, i);
            }

            for (int i = 0; i < leadAgents.Count; i++)
            {
                goalInfo[leadAgents[i]] = goals[0];
            }

            for (int i = 0; i < leadAgents.Count; i++)
            {
                leadAgents[i].Init(this, i);
            }

            for (int i = 0; i < supportAgents.Count; i++)
            {
                supportAgents[i].Init(this, i);
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
                for (int i = 0; i < leadAgents.Count; i++)
                {
                    healthBars[i].SetMaxHealth(leadAgents[i].agentMaxHealth);
                }
            }
        }

        // Initialize UI ScoreTexts With Agents
        {
            for (int i = 0; i < leadAgents.Count; i++)
            {
                scoreInfo.Add(agentScoreTexts[i], leadAgents[i]);
            }
        }

        // Initialize UI AgentTargets With Agents
        {
            for (int i = 0; i < leadAgents.Count; i++)
            {
                agentTargetInfo.Add(agentImages[i], leadAgents[i]);
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

        foreach (TorchBehaviour t in torches)
        {
            t.TorchUpdate();
        }

        foreach (EnemyBehaviour e in enemies)
        {
            e.EnemyUpdate();
        }

        for (int i = 0; i < leadAgents.Count; i++)
        {
            leadAgents[i].AgentUpdate();
        }

        for (int i = 0; i < supportAgents.Count; i++)
        {
            supportAgents[i].AgentUpdate();
        }

        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].BallUpdate();
        }

        for (int i = 0; i < powerups.Count; i++)
        {
            powerups[i].PowerupUpdate();
        }

        // Update UI Health bars
        {
            for (int i = 0; i < leadAgents.Count; i++)
            {
                healthBars[i].SetHealth(leadAgents[i].agentCurrentHealth);
            }
        }

        // Update UI Score Texts
        {
            foreach (TMP_Text t in scoreInfo.Keys)
            {
                t.text = scoreInfo[t].GetScore().ToString();
            }
        }

        // Update UI Agent Targets
        {
            foreach (Image i in agentTargetInfo.Keys)
            {
                if (agentTargetInfo[i].GetTargetType() == TargetType.Ball)
                {
                    i.sprite = uiSprites[4];
                }
                else if (agentTargetInfo[i].GetTargetType() == TargetType.Goal)
                {
                    i.sprite = uiSprites[7];
                }
                else if (agentTargetInfo[i].GetTargetType() == TargetType.Safe)
                {
                    i.sprite = uiSprites[6];
                }
                else if (agentTargetInfo[i].GetTargetType() == TargetType.Powerup)
                {
                    if (agentTargetInfo[i].targetPowerup.GetPowerupState() == PowerupState.Speed)
                        i.sprite = uiSprites[8];
                    else if (agentTargetInfo[i].targetPowerup.GetPowerupState() == PowerupState.Kick)
                        i.sprite = uiSprites[9];

                }
                else if (agentTargetInfo[i].GetTargetType() == TargetType.Agent)
                {
                    if (agentTargetInfo[i].GetTargetAgent() != null)
                    {
                        int spriteID = agentTargetInfo[i].GetTargetAgent().GetAgentID();
                        i.sprite = uiSprites[spriteID];
                    }  
  
                }
            }
        }

        //for (int i = 0; i < cubes.Count; i++)
        //{
        //    cubes[i].CubeUpdate();
        //}
    }

    public BallBehaviour GetFreeBall(LeadAgentBehaviour agent)
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

    public List<EnemyBehaviour> GetEnemy()
    {
        return enemies;
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

        for (int i = 0; i < leadAgents.Count; i++)
        {
            if (leadAgents[i].team == team)
            {
                if (leadAgents[i].agentRole == AgentRole.None)
                {
                    noneCount++;
                }
                else if (leadAgents[i].agentRole == AgentRole.Lead)
                {
                    leadCount++;
                }
                else if (leadAgents[i].agentRole == AgentRole.Support)
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

    public void SetAgent(LeadAgentBehaviour p_agent)
    {
        balls[0].SetAgent(p_agent);
    }

    public LeadAgentBehaviour GetNearestEnemyAgent(LeadAgentBehaviour p_agent)
    {
        float distance = float.MaxValue;
        int closestAgentIndex = -1;

        if (leadAgents != null && leadAgents.Count > 0)
        {
            for (int i = 0; i < leadAgents.Count; i++)
            {
                if (leadAgents[i].team != p_agent.team &&
                    leadAgents[i] != p_agent)
                {
                    if (Vector3.Distance(p_agent.transform.position, leadAgents[i].transform.position) < distance)
                    {
                        distance = Vector3.Distance(p_agent.transform.position, leadAgents[i].transform.position);
                        closestAgentIndex = i;
                    }
                }
            }

            return leadAgents[closestAgentIndex];
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

    public bool AmICaptured(BallBehaviour ball, out LeadAgentBehaviour agent)
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

    public void UpdateChaseData(LeadAgentBehaviour agent, BallBehaviour ball) // agent behavior calling this
    {
        if (!chaseInfo.ContainsKey(agent))
            chaseInfo[agent] = ball;

        if (captureInfo.ContainsKey(agent))
            captureInfo.Remove(agent);
    }

    public void UpdateCaptureData(LeadAgentBehaviour agent, BallBehaviour ball)
    {
        captureInfo[agent] = ball;

        if (chaseInfo.ContainsKey(agent))
            chaseInfo.Remove(agent);
    }

    public void RemoveCaptureData(LeadAgentBehaviour agent)
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

    public void Goal(BallBehaviour ball, LeadAgentBehaviour agent)
    {
        captureInfo.Remove(agent);
        agent.AddScore();
    }

    public GoalBehaviour GetGoal(LeadAgentBehaviour agent)
    {
        return goalInfo[agent];
    }

    public Vector3 GetGoalPosition()
    {
        if (goals.Count > 0)
            return goals[0].transform.position;

        return Vector3.zero;
    }

    public GameObject GetSafePoint()
    {
        if (safepoints.Count > 0)
            return safepoints[0];

        return null;
    }

    public bool GetTeamPlayMode()
    {
        return isTeamPlayActive;
    }

    public List<TorchBehaviour> GetTorches()
    {
        return torches;
    }
}
