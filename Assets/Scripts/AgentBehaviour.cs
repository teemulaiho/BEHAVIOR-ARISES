using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public enum AgentState
{
    Idle,
    Deciding,
    Observing,
    Fetching,
    Returning,
    Chasing,
    Attacking,
    Fallen
}
public enum AgentRole
{
    None,
    Lead,
    Support,
    Healer
}

public enum TargetType
{
    None,
    Agent,
    Ball,
    Goal,
    Safe,
    Powerup
}

public class AgentBehaviour : MonoBehaviour, Agent
{
    public GameManagerBehaviour         gameManager;
    public int                          agentID;
    public AgentState                   agentState;
    public AgentRole                    agentRole;
    public PowerupState                 powerupState;
    public MeshRenderer                 meshRenderer;
    public Rigidbody                    rb;
    public Collider                     agentCollider;
    public Collision                    agentCollision;
    public ParticleSystem               agentKickParticles;
    public NavMeshAgent                 navMeshAgent;

    public HealthbarBehaviour           healthBar;
    public TMP_Text                     scoreText;

    public TargetType                   targetType;
    public GoalBehaviour                targetGoal;
    public BallBehaviour                targetBall;
    public AgentBehaviour               targetAgent;
    public PowerupBehaviour             targetPowerup;
    public GameObject                   targetSafePoint;

    public Vector3                      targetPos;
    public Vector3                      safePos;
    public Quaternion                   originalRotation;

    public float                        agentSpeed;
    public float                        agentRecovery;
    public float                        agentMaxHealth;
    public float                        agentCurrentHealth;
    public float                        recoveryTime;

    public int                          score;
    public int                          team;

    public Vector3                      kickForce;
    public Vector3                      agentScale;

    public bool                         collidingWithBall;
    public bool                         collidingWithPowerup;
    public bool                         collidingWithAgent;
    public bool                         collidingWithSafePoint;
    public bool                         isHealing;

    // Agent Behaviour Tree Begin
    public BT_Node                      bt_root;
    // Agent Behaviour Tree End

    public void Init(GameManagerBehaviour p_gameManager, int ID)
    {
        this.name                       = "Agent";
        gameManager                     = p_gameManager;
        agentID                         = ID;
        agentState                      = AgentState.Idle;
        //agentRole                     = gameManager.GetRole(team);
        agentRole                       = AgentRole.Lead;
        targetType                      = TargetType.None;
        targetBall                      = null;
        targetAgent                     = null;
        targetPowerup                   = gameManager.GetPowerup();
        agentSpeed                      = 2.5f;
        agentRecovery                   = 2f;
        agentMaxHealth                  = 1000f;
        agentCurrentHealth              = agentMaxHealth;
        recoveryTime                    = 0;
        score                           = 0;

        targetGoal                      = gameManager.GetGoal(this);
        meshRenderer                    = GetComponent<MeshRenderer>();
        rb                              = GetComponent<Rigidbody>();
        agentCollider                   = GetComponent<CapsuleCollider>();
        agentCollision                  = new Collision();
        agentKickParticles              = GetComponentInChildren<ParticleSystem>();
        navMeshAgent                    = GetComponent<NavMeshAgent>();
        healthBar                       = GetComponentInChildren<HealthbarBehaviour>();
        originalRotation                = transform.rotation;

        collidingWithBall               = false;
        collidingWithPowerup            = false;
        collidingWithAgent              = false;

        kickForce                       = new Vector3(50, 0, 50);
        agentScale                      = new Vector3(1, 1, 1);

        targetBall                      = gameManager.GetBall();
        targetSafePoint                 = gameManager.GetSafePoint();
        safePos                         = targetSafePoint.transform.position;

        healthBar.SetMaxHealth(agentMaxHealth);

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
            if (agentID == 0)
                meshRenderer.material.color = Color.green;
            else if (agentID == 1)
                meshRenderer.material.color = Color.red;
            else if (agentID == 2)
                meshRenderer.material.color = Color.blue;
            else if (agentID == 3)
                meshRenderer.material.color = Color.black;
        }

        // Agent Behaviour Tree Begin  
        if (bt_root == null)
        {
            Selector root = new Selector();     // Temporary root.

            // Lead Agent Behaviour Tree
            if (agentRole == AgentRole.Lead)
            {
                Debug.Log("I Am A Lead Agent.");

                root.children.Add(HealthBranch());
                root.children.Add(BallBranch());
                root.children.Add(PowerupBranch());
            }

            // Support Agent Behaviour Tree
            else if (agentRole == AgentRole.Support)
            {
                Debug.Log("I am a Support Agent.");

                root.children.Add(DefenceBranch());
            }

            // Healer Agent
            else if (agentRole == AgentRole.Healer)
            {
                Debug.Log("I am a Healer Agent.");
            }

            bt_root = root;
        }
        // Agent Behaviour Tree End
    }

    public void AgentUpdate()
    {
        if (Vector3.Distance(transform.position, targetGoal.transform.position) > 50)
        {
            transform.position = new Vector3(Random.Range(0, 49), 4, Random.Range(-1, -49));
            rb.velocity = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AgentTakeDamage(20);
        }

        if (transform.position.y < -10 ||
            transform.position.x > 50 ||
            transform.position.x < 0 ||
            transform.position.z > 50 ||
            transform.position.z < -50)
        {
            gameManager.RemoveCaptureData(this);
        }

        bt_root.Run(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            collidingWithBall = true;
        }
        else if (collision.gameObject.CompareTag("Powerup"))
        {
            collidingWithPowerup = true;
        }
        else if (collision.gameObject.CompareTag("Agent"))
        {
            collidingWithAgent = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        agentCollision = null;

        if (collidingWithBall)
            collidingWithBall = false;

        if (collidingWithPowerup)
            collidingWithPowerup = false;

        if (collidingWithAgent)
            collidingWithAgent = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("SafePoint"))
        {
            collidingWithSafePoint = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (collidingWithSafePoint)
            collidingWithSafePoint = false;
    }

    public void AddScore()
    {
        score++;
    }

    public int GetScore()
    {
        return score;
    }

    public void RemoveBall()
    {
        if (targetBall != null)
        {
            if (targetBall.agent == this)
            {
                targetBall.agent = null;
            }
        }
    }

    public void RemoveTargetAgent()
    {
        if (targetAgent != null)
            targetAgent = null;
    }

    public void RemoveTargetPowerup()
    {
        if (targetPowerup != null)
            targetPowerup = null;
    }

    public void AgentPowerup(PowerupBehaviour powerup)
    {
        if (powerup.state == PowerupState.Speed)
        {
            agentSpeed *= 2f;
        }
        else if (powerup.state == PowerupState.Kick)
        {
            kickForce *= 2f;
            transform.localScale = kickForce.x / 50 * agentScale;
        }
        else
        {

        }
    }

    public void AgentTakeDamage(float damage)
    {
        agentCurrentHealth -= damage;
        healthBar.SetHealth(agentCurrentHealth);
    }

    public bool HasAgentReachedCurrentTarget()
    {
        if (targetPos == targetBall.transform.position)
        {
            if (collidingWithBall)
                return true;
        }
        else if (targetPos == targetPowerup.transform.position)
        {
            if (collidingWithPowerup)
                return true;
        }
        else if (targetAgent != null &&
                 targetPos == targetAgent.transform.position)
        {
            if (collidingWithAgent)
                return true;
        }
        else if (targetPos == safePos)
        {
            if (collidingWithSafePoint)
                return true;
        }

        return false;
    }

    public AgentBehaviour GetTargetAgent()
    {
        return targetAgent;
    }

    public int GetAgentID()
    {
        return agentID;
    }

    public TargetType GetTargetType()
    {
        return targetType;
    }

    private Selector HealthBranch()
    {
        Selector HealthBranch = new Selector();
        HealthBranch.children.Add(IsMyHealthBelow25Percent());

        return HealthBranch;
    }

    private Selector BallBranch()
    {
        Selector BallBranch = new Selector();
        BallBranch.children.Add(DoIHaveBall());
        BallBranch.children.Add(DoesSomeoneElseHaveBall());
        BallBranch.children.Add(IsBallFree());

        return BallBranch;
    }

    private Selector PowerupBranch()
    {
        Selector PowerupBranch = new Selector();
        PowerupBranch.children.Add(IsPowerupSpeed());
        PowerupBranch.children.Add(IsPowerupKick());

        return PowerupBranch;
    }
    
    private Selector DefenceBranch()
    {
        Selector DefenceBranch = new Selector();
        DefenceBranch.children.Add(DoesMyTeamHaveBall());
        DefenceBranch.children.Add(AttackNearbyEnemyAgent());

        return DefenceBranch;
    }

    private Sequencer IsMyHealthBelow25Percent()
    {
        Inverter HealthAbove25PercentInverter = new Inverter();
        HealthAbove25PercentInverter.children.Add(new NodeCheckHealthAbove25Percent());

        Sequencer IsMyHealthBelow25Percent = new Sequencer();
        IsMyHealthBelow25Percent.children.Add(HealthAbove25PercentInverter);
        IsMyHealthBelow25Percent.children.Add(new NodeTargetSafeArea());
        IsMyHealthBelow25Percent.children.Add(new NodeMoveTowardsTarget());
        IsMyHealthBelow25Percent.children.Add(new NodeHealAgent());

        return IsMyHealthBelow25Percent;
    }

    private Sequencer DoIHaveBall()
    {
        Sequencer DoIHaveBall = new Sequencer();
        DoIHaveBall.children.Add(new NodeDoIHaveBall());
        DoIHaveBall.children.Add(new NodeTargetGoal());
        DoIHaveBall.children.Add(new NodeMoveTowardsTarget());
        DoIHaveBall.children.Add(new NodeScoreGoal());

        return DoIHaveBall;
    }

    private Sequencer IsPowerupKick()
    {
        Sequencer IsPowerupKick = new Sequencer();
        IsPowerupKick.children.Add(new NodeIsPowerupKick());
        IsPowerupKick.children.Add(new NodeIsPowerupCloseEnough());
        IsPowerupKick.children.Add(new NodeTargetPowerup());
        IsPowerupKick.children.Add(new NodeMoveTowardsTarget());
        IsPowerupKick.children.Add(new NodeCapturePowerup());

        return IsPowerupKick;
    }

    private Sequencer IsPowerupSpeed()
    {
        Sequencer IsPowerupSpeed = new Sequencer();
        IsPowerupSpeed.children.Add(new NodeIsPowerupSpeed());
        IsPowerupSpeed.children.Add(new NodeIsPowerupCloseEnough());
        IsPowerupSpeed.children.Add(new NodeTargetPowerup());
        IsPowerupSpeed.children.Add(new NodeMoveTowardsTarget());
        IsPowerupSpeed.children.Add(new NodeCapturePowerup());

        return IsPowerupSpeed;
    }

    private Sequencer DoesSomeoneElseHaveBall()
    {
        Sequencer DoesSomeoneElseHaveBall = new Sequencer();
        DoesSomeoneElseHaveBall.children.Add(new NodeIsBallCloseEnough());
        DoesSomeoneElseHaveBall.children.Add(new NodeDoesSomeoneElseHaveBall());
        DoesSomeoneElseHaveBall.children.Add(new NodeTargetAgent());
        DoesSomeoneElseHaveBall.children.Add(new NodeMoveTowardsTarget());
        DoesSomeoneElseHaveBall.children.Add(new NodeKickAgent());

        return DoesSomeoneElseHaveBall;
    }

    private Sequencer IsBallFree()
    {
        Sequencer IsBallFree = new Sequencer();
        IsBallFree.children.Add(new NodeIsBallCloseEnough());
        IsBallFree.children.Add(new NodeIsBallFree());
        IsBallFree.children.Add(new NodeTargetBall());
        IsBallFree.children.Add(new NodeMoveTowardsTarget());
        IsBallFree.children.Add(new NodeCaptureBall());

        return IsBallFree;
    }

    private Sequencer DoesMyTeamHaveBall()
    {
        Sequencer DoesMyTeamHaveBall = new Sequencer();
        DoesMyTeamHaveBall.children.Add(new NodeDoesMyTeamHaveBall());
        DoesMyTeamHaveBall.children.Add(new NodeIsEnemyTeamAgentCloseEnough());
        DoesMyTeamHaveBall.children.Add(new NodeTargetAgent());
        DoesMyTeamHaveBall.children.Add(new NodeMoveTowardsTarget());

        return DoesMyTeamHaveBall;
    }

    private Sequencer AttackNearbyEnemyAgent()
    {
        Inverter InvertNearbyEnemyResult = new Inverter();
        InvertNearbyEnemyResult.children.Add(new NodeIsEnemyTeamAgentCloseEnough());

        Sequencer AttackNearbyEnemyAgent = new Sequencer();
        AttackNearbyEnemyAgent.children.Add(new NodeDoesMyTeamHaveBall());
        AttackNearbyEnemyAgent.children.Add(InvertNearbyEnemyResult);
        AttackNearbyEnemyAgent.children.Add(new NodeMoveTowardsTarget());
        AttackNearbyEnemyAgent.children.Add(new NodeKickAgent());

        return AttackNearbyEnemyAgent;
    }
}
