using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour, Agent
{
    [SerializeField] GameManagerBehaviour        gameManager;
    [SerializeField] int                         enemyID;

    [SerializeField] ProjectileBehaviour         bulletPrefab;
    [SerializeField] List<ProjectileBehaviour>   bulletList;

    [SerializeField] List<GameObject>            torchList;

    [SerializeField] NavMeshAgent                navMeshAgent;

    [SerializeField] SphereCollider              rangeCollider;
    [SerializeField] List<AgentBehaviour>        agentsInRangeList;
    [SerializeField] AgentBehaviour              targetAgent;

    [SerializeField] public HealthbarBehaviour   healthBar;
    [SerializeField] public float                enemyMaxHealth;
    [SerializeField] public float                enemyCurrentHealth;
    [SerializeField] public float                enemyRange;
    [SerializeField] public float                enemyAttackCooldown;
    [SerializeField] public float                enemyAttackCoolDownDT;

    [SerializeField] public BT_Node              bt_root;


    public void Init(GameManagerBehaviour gm, int id)
    {
        gameManager                             = gm;
        enemyID                                 = id;

        bulletPrefab                            = Resources.Load<ProjectileBehaviour>("Prefabs/Bullet");
        bulletList                              = new List<ProjectileBehaviour>();
        bulletPrefab                            = Instantiate(bulletPrefab);
        bulletList.Add(bulletPrefab);
        bulletPrefab.gameObject.SetActive(false);

        torchList                               = new List<GameObject>();
        torchList.AddRange(gameManager.GetTorches());

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
            root.children.Add(SearchBranch());
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

        foreach (GameObject t in torchList)
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

    public bool CompareDestinationToTorchPositions(Vector3 destination)
    {
        foreach (GameObject go in torchList)
        {
            if (go.transform.position == destination)
                return true;
        }

        return false;
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

    private Selector SearchBranch()
    {
        Selector SearchBranch = new Selector();
        SearchBranch.children.Add(new NodeEnemyIsMyDestinationATorch());
        SearchBranch.children.Add(SearchForAgent());

        return SearchBranch;
    }

    private Sequencer SearchForAgent()
    {
        Sequencer SearchForAgent = new Sequencer();
        SearchForAgent.children.Add(new NodeEnemyHasReachedDestination());
        SearchForAgent.children.Add(new NodeEnemyMoveTowardsDestination());
        SearchForAgent.children.Add(new NodeSetNewDestination());

        return SearchForAgent;
    }
}
