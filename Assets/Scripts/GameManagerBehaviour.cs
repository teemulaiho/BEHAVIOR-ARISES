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
    public Image[] agentIcons;

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
    public Dictionary<Image, LeadAgentBehaviour> agentIconInfo          = new Dictionary<Image, LeadAgentBehaviour>();
    
    int leadAgentAmount                                                 = 3;
    int supportAgentAmount                                              = 1;
    int enemyAmount                                                     = 1;
    int torchAmount                                                     = 4;
    
    public bool isTeamPlayActive;

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate GameObjects
        {
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
        }

        // Initialize Screen UI
        {
            // Initialize Health bars
            {
                foreach (HealthbarBehaviour hb in healthBars)
                {
                    hb.gameObject.SetActive(false);
                }

                for (int i = 0; i < leadAgents.Count; i++)
                {
                    healthBars[i].gameObject.SetActive(true);
                    healthBars[i].SetMaxHealth(leadAgents[i].agentMaxHealth);
                }
            }

            // Initialize UI ScoreTexts With Agents
            {
                foreach (TMP_Text a in agentScoreTexts)
                {
                    a.gameObject.SetActive(false);
                }

                for (int i = 0; i < leadAgents.Count; i++)
                {
                    agentScoreTexts[i].gameObject.SetActive(true);
                    scoreInfo.Add(agentScoreTexts[i], leadAgents[i]);
                }
            }

            // Initialize UI AgentIcons With Agent
            {
                foreach (Image i in agentIcons)
                {
                    i.gameObject.SetActive(false);
                }

                for (int i = 0; i < leadAgents.Count; i++)
                {
                    agentIcons[i].gameObject.SetActive(true);
                    agentIconInfo.Add(agentIcons[i], leadAgents[i]);
                }
            }

            // Initialize UI AgentTargets With Agents
            {
                foreach (Image i in agentImages)
                {
                    i.gameObject.SetActive(false);
                }

                for (int i = 0; i < leadAgents.Count; i++)
                {
                    agentImages[i].gameObject.SetActive(true);
                    agentTargetInfo.Add(agentImages[i], leadAgents[i]);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
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

    public void RemoveCaptureData(LeadAgentBehaviour agent)
    {
        if (captureInfo.ContainsKey(agent))
        {
            captureInfo.Remove(agent);
        }
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
