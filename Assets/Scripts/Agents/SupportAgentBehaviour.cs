using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SupportAgentState
{
    GoToTorch,
    ChaseAfterEnemy,
    None
}

public class SupportAgentBehaviour : MonoBehaviour, Agent
{
    [SerializeField] GameManagerBehaviour       gameManager;
    [SerializeField] int                        agentID;
    [SerializeField] int                        team;
    [SerializeField] SupportAgentState          agentState;

    [SerializeField] MeshRenderer               meshRenderer;
    [SerializeField] NavMeshAgent               navMeshAgent;

    [SerializeField] List<TorchBehaviour>       torchList;
    [SerializeField] TorchBehaviour             targetTorch;

    [SerializeField] float                      agentSpeed;

    public BT_Node                              bt_root;

    public void Init(GameManagerBehaviour gm, int id)
    {
        gameManager                             = gm;
        agentID                                 = id;

        meshRenderer                            = GetComponent<MeshRenderer>();
        navMeshAgent                            = GetComponent<NavMeshAgent>();

        torchList                               = new List<TorchBehaviour>();
        torchList.AddRange(gameManager.GetTorches());

        agentSpeed                              = 2f;
        navMeshAgent.speed                      = agentSpeed;

        if (gameManager.GetTeamPlayMode())
        {
            if (team == 0)
            {
                meshRenderer.material.color = Color.blue;
            }
            else if (team == 1)
            {
                meshRenderer.material.color = Color.red;
            }
        }
        else
        {
            meshRenderer.material.color = Color.yellow;
            //if (agentID == 0)
            //    meshRenderer.material.color = Color.green;
            //else if (agentID == 1)
            //    meshRenderer.material.color = Color.red;
            //else if (agentID == 2)
            //    meshRenderer.material.color = Color.blue;
            //else if (agentID == 3)
            //    meshRenderer.material.color = Color.black;
        }


        if (bt_root == null)
        {
            Selector root = new Selector();     // Temporary root.

            root.children.Add(SabotageBranch());
            root.children.Add(TorchBranch());
            bt_root = root;
        }
    }

    public void AgentUpdate()
    {
        if (bt_root != null)
            bt_root.Run(this);
    }

    public void SetAgentTeam(int i)
    {
        team = i;
    }

    public void SetAgentState(SupportAgentState state)
    {
        agentState = state;
    }

    public bool CheckForLitTorches()
    {
        foreach (TorchBehaviour t in torchList)
        {
            if (t.GetTorchLightIntensity() >= 1)
            {
                return true;
            }
        }

        return false;
    }

    public bool SetDestinationToTorch(int newLightIntensityValue)
    {
        float minDist = float.MaxValue;
        TorchBehaviour nearestTorch = null;

        foreach (TorchBehaviour t in torchList)
        {
            if (newLightIntensityValue == 1)
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
            else if (newLightIntensityValue == 0)
            {
                if (t.GetTorchLightIntensity() > 0)
                {
                    float dist = Vector3.Distance(transform.position, t.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestTorch = t;
                    }
                }
            }
        }

        if (nearestTorch != null)
        {
            targetTorch = nearestTorch;
            navMeshAgent.destination = targetTorch.transform.position;
            return true;
        }

        return false;
    }

    public bool LightDownTorch()
    {
        Vector3 destination = GetNavMeshDestination();

        foreach (TorchBehaviour t in torchList)
        {
            destination.y = t.transform.position.y;

            if (t.transform.position == destination)
            {
                t.LightDownTorch();
                RemoveTargetTorch();
                return true;
            }
        }

        return false;
    }

    public Vector3 GetNavMeshDestination()
    {
        return navMeshAgent.destination;
    }

    public bool CheckDistanceBetween(Vector3 a, Vector3 b, float withinDist)
    {
        float dist = Vector3.Distance(a, b);

        if (dist < withinDist)
            return true;

        return false;
    }

    public List<EnemyBehaviour> GetEnemy()
    {
        return gameManager.GetEnemy();
    }

    public Vector3 SetNavMeshDestination(Vector3 dest)
    {
        navMeshAgent.destination = dest;

        return navMeshAgent.destination;
    }

    public EnemyBehaviour GetNearestEnemy()
    {
        float min = float.MaxValue;
        EnemyBehaviour nearestEnemy = null;

        foreach (EnemyBehaviour e in GetEnemy())
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);

            if (dist < min)
            {
                dist = min;
                nearestEnemy = e;
            }
        }

        return nearestEnemy;

    }

    public bool Sabotage(EnemyBehaviour target)
    {
        target.GetSabotaged(EnemySabotageState.Slowed);

        return false;
    }

    public TorchBehaviour GetTargetTorch()
    {
        return targetTorch;
    }

    public void RemoveTargetTorch()
    {
        targetTorch = null;
    }

    public Selector TorchBranch()
    {
        Selector TorchBranch = new Selector();
        TorchBranch.children.Add(SearchForTorches());
        TorchBranch.children.Add(GoLightDownTorch());

        return TorchBranch;
    }

    private Sequencer SearchForTorches()
    {
        Sequencer SearchForTorches = new Sequencer();
        //SearchForTorches.children.Add(new NodeEnemyAmIGoingToBase());

        SearchForTorches.children.Add(new NodeEnemyIsThereAUnlitTorch());

        Inverter InvertTorchTargetResult = new Inverter();
        InvertTorchTargetResult.children.Add(new NodeEnemyAmIAlreadyTargetingATorch());
        SearchForTorches.children.Add(InvertTorchTargetResult);
        SearchForTorches.children.Add(new NodeEnemySetDestinationToNearestUnlitTorch());

        return SearchForTorches;
    }

    private Sequencer GoLightDownTorch()    
    {
        Sequencer GoLightDownTorch = new Sequencer();
        GoLightDownTorch.children.Add(new NodeEnemyAmIAlreadyTargetingATorch());
        GoLightDownTorch.children.Add(new NodeEnemyHaveIReachedTorch());

        Inverter InvertTorchLightUpResult = new Inverter();
        InvertTorchLightUpResult.children.Add(new NodeEnemyLightUpDownTorch());

        GoLightDownTorch.children.Add(InvertTorchLightUpResult);

        return GoLightDownTorch;
    }

    public Sequencer SabotageBranch()
    {
        Sequencer SabotageBranch = new Sequencer();

        Sequencer IsEnemyNearby = new Sequencer();
        Inverter IsEnemySabotagedInverter = new Inverter();
        IsEnemySabotagedInverter.children.Add(new NodeIsEnemyAlreadySabotaged());

        IsEnemyNearby.children.Add(IsEnemySabotagedInverter);
        IsEnemyNearby.children.Add(new NodeIsEnemyNearby());
        IsEnemyNearby.children.Add(new NodeTargetNearestEnemy());

        Sequencer SabotageEnemy = new Sequencer();
        SabotageEnemy.children.Add(new NodeIsEnemyInRange());
        SabotageEnemy.children.Add(IsEnemySabotagedInverter);
        SabotageEnemy.children.Add(new NodeSabotageEnemy());

        SabotageBranch.children.Add(IsEnemyNearby);
        SabotageBranch.children.Add(SabotageEnemy);

        return SabotageBranch;
    }
}
