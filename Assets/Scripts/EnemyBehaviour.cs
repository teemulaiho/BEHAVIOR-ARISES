using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    GoToTorch,
    AttackAgent,
    ReturnToBase,
    ChaseAfterBall,
    None
}

public class EnemyBehaviour : MonoBehaviour, Agent
{
    [SerializeField] GameManagerBehaviour       gameManager;
    [SerializeField] int                        enemyID;
    [SerializeField] EnemyState                 enemyState;

    [SerializeField] ProjectileBehaviour        bulletPrefab;
    [SerializeField] List<ProjectileBehaviour>  bulletList;

    [SerializeField] List<TorchBehaviour>       torchList;

    [SerializeField] BallBehaviour              ball;
    [SerializeField] bool                       hasBall;

    [SerializeField] NavMeshAgent               navMeshAgent;

    [SerializeField] SphereCollider             rangeCollider;
    [SerializeField] List<AgentBehaviour>       agentsInRangeList;
    [SerializeField] AgentBehaviour             targetAgent;

    [SerializeField] public HealthbarBehaviour  healthBar;
    [SerializeField] public float               enemyMaxHealth;
    [SerializeField] public float               enemyCurrentHealth;
    [SerializeField] public float               enemyRange;
    [SerializeField] public float               enemyAttackCooldown;
    [SerializeField] public float               enemyAttackCoolDownDT;

    [SerializeField] public BT_Node             bt_root;


    public void Init(GameManagerBehaviour gm, int id)
    {
        gameManager                             = gm;
        enemyID                                 = id;

        enemyState                              = EnemyState.Idle;

        bulletPrefab                            = Resources.Load<ProjectileBehaviour>("Prefabs/Bullet");
        bulletList                              = new List<ProjectileBehaviour>();
        bulletPrefab                            = Instantiate(bulletPrefab);
        bulletList.Add(bulletPrefab);
        bulletPrefab.gameObject.SetActive(false);

        torchList                               = new List<TorchBehaviour>();
        torchList.AddRange(gameManager.GetTorches());

        ball                                    = gameManager.GetBall();

        navMeshAgent                            = GetComponent<NavMeshAgent>();

        healthBar                               = GetComponentInChildren<HealthbarBehaviour>();

        enemyMaxHealth                          = 1000f;
        enemyCurrentHealth                      = enemyMaxHealth;

        enemyAttackCooldown                     = 1;
        enemyAttackCoolDownDT                   = 0;

        enemyRange                              = 4;
        rangeCollider                           = GetComponent<SphereCollider>();
        rangeCollider.radius                    = enemyRange;

        agentsInRangeList                       = new List<AgentBehaviour>();

        healthBar.SetMaxHealth(enemyMaxHealth);

        if (bt_root == null)
        {
            Selector root = new Selector();

            root.children.Add(AttackBranch());
            root.children.Add(TorchBranch());
            root.children.Add(BallBranch());
            root.children.Add(ReturnToBaseBranch());
            bt_root = root; 
        }
    }

    public void EnemyUpdate()
    {
        Float();

        bt_root.Run(this);
    }

    private void Float()
    {
        float yOffset = 1 + 0.5f * transform.localScale.y;

        float x = transform.position.x;
        float y = Mathf.PingPong(Time.time, 2) + yOffset;
        float z = transform.position.z;

        transform.position = new Vector3(x, y, z);
    }

    public EnemyState SetEnemyState(EnemyState state)
    {
        enemyState = state;

        return enemyState;
    }

    public EnemyState GetEnemyState()
    {
        return enemyState;
    }

    public bool IsTargetInRange()
    {
        if (agentsInRangeList.Count > 0)
        {
            return true;
        }

        return false;
    }

    public bool TargetAgent()
    {
        float minDist = float.MaxValue;

        if (agentsInRangeList.Count >0)
        {
            foreach (AgentBehaviour a in agentsInRangeList)
            {
                float dist = Vector3.Distance(transform.position, a.transform.position);
                
                if (dist < minDist)
                {
                    minDist = dist;
                    targetAgent = a;
                    return true;
                }
            }
        }

        return false;
    }

    public AgentBehaviour GetTargetAgent()
    {
        return targetAgent;
    }

    public bool AttackAgent()
    {
        if (targetAgent.GetAgentCurrentHealth() > 0)
        {
            Debug.Log("Attacking taget agent.");
            foreach (ProjectileBehaviour b in bulletList)
            {
                b.gameObject.SetActive(true);
                b.Init(this.transform.position, targetAgent.transform.position);
            }
            return true;
        }

        return false;
    }

    public bool IsEnemyInCooldown()
    {
        enemyAttackCoolDownDT += Time.deltaTime;

        if (enemyAttackCoolDownDT > enemyAttackCooldown)
        {
            enemyAttackCoolDownDT = 0f;
            return false;
        }

        return true;
    }

    public void ResetEnemyCooldown()
    {
        enemyAttackCoolDownDT = 0f;
    }

    public Vector3 GetNavMeshDestination()
    {
        return navMeshAgent.destination;
    }

    public void SetNewNavMeshDestination()
    {
        int i = 0;
        Vector3 newDest = Vector3.zero;

        foreach (TorchBehaviour t in torchList)
        {
            if (navMeshAgent.destination.x == t.transform.position.x &&
                navMeshAgent.destination.z == t.transform.position.z)
            {
                if (torchList.Count > 0)
                {
                    if (i < torchList.Count - 1)
                    {
                        newDest = torchList[i+1].transform.position;
                        break;
                    }
                    else
                    {
                        newDest = torchList[0].transform.position;
                        break;
                    }
                }
            }

            i++;
        }

        if (newDest == Vector3.zero)
        {
            newDest = torchList[0].transform.position;
        }

        navMeshAgent.SetDestination(newDest);
    }

    public void SetNewNavMeshDestination(Vector3 dest)
    {
        navMeshAgent.destination = dest;
    }

    public float GetNavMeshAgentSpeed()
    {
        return navMeshAgent.speed;
    }

    public bool CompareDestinationToTorchPositions(Vector3 destination)
    {
        foreach (TorchBehaviour t in torchList)
        {
            if (t.transform.position.x == destination.x &&
                t.transform.position.z == destination.z)
                return true;
        }

        return false;
    }

    public bool CheckForUnlitTorches()
    {
        foreach(TorchBehaviour t in torchList)
        {
            if (t.GetTorchLightIntensity() < 1)
            {
                return true;
            }
        }

        return false;
    }

    public bool SetDestinationToTorch()
    {
        float minDist = float.MaxValue;
        TorchBehaviour nearestTorch = null; 


        foreach (TorchBehaviour t in torchList)
        {
            if (t.GetTorchLightIntensity() < 1)
            {
                float dist = Vector3.Distance(transform.position, t.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestTorch = t;
                }
            }
        }

        if (nearestTorch != null && nearestTorch.GetTorchLightIntensity() < 1)
        {
            navMeshAgent.destination = nearestTorch.transform.position;
            return true;
        }

        return false;
    }

    public bool LightUpTorch()
    {
        Vector3 destination = GetNavMeshDestination();

        foreach (TorchBehaviour t in torchList)
        {
            destination.y = t.transform.position.y;

            if (t.transform.position == destination)
            {
                t.LightUpTorch();
                return true;
            }
        }

        return false;
    }

    public bool CheckIfBallAwayFromBase()
    {
        Vector3 basePos = new Vector3(25, 1.1f, -25);

        float dist = Vector3.Distance(ball.transform.position,basePos);

        if (dist > 5)
            return true;

        return false;
    }

    public BallBehaviour GetBall()
    {
        return ball;
    }

    public bool CheckDistanceBetween(Vector3 a, Vector3 b, float withinDist)
    {
        float dist = Vector3.Distance(a, b);

        if (dist < withinDist)
            return true;

        return false;
    }

    public void CaptureBall(bool isBallCaptured)
    {
        hasBall = isBallCaptured;
        ball = gameManager.GetBall();
        ball.SetAgent(this);
    }

    public bool HasBall()
    {
        return hasBall;
    }
    
    public void DropBall()
    {
        if (hasBall && ball)
        {
            ball.RemoveAgent(this);
            hasBall = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        AgentBehaviour collidingAgent = other.GetComponent<AgentBehaviour>();

        if (collidingAgent && !agentsInRangeList.Contains(collidingAgent))
        {
            agentsInRangeList.Add(collidingAgent);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AgentBehaviour collidingAgent = other.GetComponent<AgentBehaviour>();

        if (collidingAgent && agentsInRangeList.Contains(collidingAgent))
        {
            agentsInRangeList.Remove(collidingAgent);
        }
    }

    private Selector AttackBranch()
    {
        Selector AttackBranch = new Selector();
        AttackBranch.children.Add(AttackAgentInRange());

        return AttackBranch;
    }

    private Sequencer AttackAgentInRange()
    {
        Sequencer AttackAgentInRange = new Sequencer();
        AttackAgentInRange.children.Add(new NodeEnemyIsAgentInRange());
        AttackAgentInRange.children.Add(new NodeEnemyTargetAgent());
        AttackAgentInRange.children.Add(new NodeEnemyAttackTargetAgent());

        return AttackAgentInRange;
    }

    private Selector TorchBranch()
    {
        Selector TorchBranch = new Selector();
        TorchBranch.children.Add(SearchForTorches());
        TorchBranch.children.Add(GoLightUpTorch());

        return TorchBranch;
    }

    private Sequencer SearchForTorches()
    {
        Sequencer SearchForTorches = new Sequencer();
        SearchForTorches.children.Add(new NodeEnemyAmIGoingToBase());
        SearchForTorches.children.Add(new NodeEnemyIsThereAUnlitTorch());
        Inverter InvertTorchTargetResult = new Inverter();
        InvertTorchTargetResult.children.Add(new NodeEnemyAmIAlreadyTargetingATorch());
        SearchForTorches.children.Add(InvertTorchTargetResult);
        SearchForTorches.children.Add(new NodeEnemySetDestinationToNearestUnlitTorch());

        return SearchForTorches;
    }

    private Sequencer GoLightUpTorch()
    {
        Sequencer GoLightUpTorch = new Sequencer();
        GoLightUpTorch.children.Add(new NodeEnemyAmIAlreadyTargetingATorch());
        GoLightUpTorch.children.Add(new NodeEnemyHaveIReachedTorch());

        Inverter InvertTorchLightUpResult = new Inverter();
        InvertTorchLightUpResult.children.Add(new NodeEnemyLightUpTorch());

        GoLightUpTorch.children.Add(InvertTorchLightUpResult);

        return GoLightUpTorch;
    }

    private Selector BallBranch()
    {
        Selector BallBranch = new Selector();
        BallBranch.children.Add(CheckOnBall());

        return BallBranch;
    }

    private Sequencer CheckOnBall()
    {
        Sequencer CheckOnBall = new Sequencer();

        Inverter InvertDoIHaveBall = new Inverter();
        InvertDoIHaveBall.children.Add(new NodeEnemyDoIHaveBall());

        CheckOnBall.children.Add(InvertDoIHaveBall);
        CheckOnBall.children.Add(new NodeEnemyIsBallAwayFromBase());
        CheckOnBall.children.Add(new NodeEnemyTargetBall());
        CheckOnBall.children.Add(new NodeEnemyChaseAfterBall());
        CheckOnBall.children.Add(new NodeEnemyCaptureBall());

        return CheckOnBall;
    }

    private Sequencer ReturnToBaseBranch()
    {
        Sequencer ReturnToBaseBranch = new Sequencer();
        ReturnToBaseBranch.children.Add(ReturnToBase());

        return ReturnToBaseBranch;
    }

    private Sequencer ReturnToBase()
    {
        Sequencer ReturnToBase = new Sequencer();

        Inverter InvertAmIAlreadyAtBase = new Inverter();
        InvertAmIAlreadyAtBase.children.Add(new NodeEnemyAmIAlreadyAtBase());

        ReturnToBase.children.Add(InvertAmIAlreadyAtBase);
        ReturnToBase.children.Add(new NodeEnemySetDestinationToBase());
        ReturnToBase.children.Add(new NodeEnemyHaveIReachedDestinationBase());

        return ReturnToBase;
    }
}
