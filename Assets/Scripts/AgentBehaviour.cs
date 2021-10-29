using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

public class AgentBehaviour : MonoBehaviour
{
    public GameManagerBehaviour gameManager;
    public AgentState agentState;
    public AgentRole agentRole;
    public PowerupState powerupState;
    public MeshRenderer meshRenderer;
    public Rigidbody rb;
    public Collider agentCollider;
    public Collision agentCollision;
    public ParticleSystem agentKickParticles;

    public HealthbarBehaviour healthBar;
    public TMP_Text scoreText;

    public GoalBehaviour targetGoal;
    public BallBehaviour targetBall;
    public AgentBehaviour targetAgent;
    public PowerupBehaviour targetPowerup;
    public GameObject targetSafePoint;

    public Vector3 targetPos;
    public Vector3 safePos;
    public Quaternion originalRotation;
    public float agentSpeed;
    public float agentRecovery;
    public float agentMaxHealth;
    public float agentCurrentHealth;
    public float recoveryTime;
    public int score;
    public int team;

    public Vector3 kickForce;
    public Vector3 agentScale;

    public bool collidingWithBall;
    public bool collidingWithPowerup;
    public bool collidingWithAgent;
    public bool collidingWithSafePoint;
    public bool isHealing;

    // Agent Behaviour Tree Begin
    public BT_Node bt_root;
    //static Selector bt_root;
    // Agent Behaviour Tree End

    public void Init(GameManagerBehaviour p_gameManager)
    {
        //Debug.Log("Init AgentBehaviour.");

        this.name = "Agent";
        gameManager = p_gameManager;
        agentState = AgentState.Idle;
        //agentRole = gameManager.GetRole(team);
        agentRole = AgentRole.Lead;
        //Debug.Log(agentRole);
        targetBall = null;
        targetAgent = null;
        targetPowerup = gameManager.GetPowerup();
        agentSpeed = 2.5f;
        agentRecovery = 2f;
        agentMaxHealth = 1000f;
        agentCurrentHealth = agentMaxHealth;
        healthBar.SetMaxHealth(agentMaxHealth);
        recoveryTime = 0;
        targetGoal = gameManager.GetGoal(this);
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
        agentCollider = GetComponent<CapsuleCollider>();
        agentCollision = new Collision();
        agentKickParticles = GetComponentInChildren<ParticleSystem>();
        originalRotation = transform.rotation;
        score = 0;
        collidingWithBall = false;
        collidingWithPowerup = false;
        collidingWithAgent = false;

        kickForce = new Vector3(50, 0, 50);
        agentScale = new Vector3(1, 1, 1);

        targetBall = gameManager.GetBall();
        targetSafePoint = gameManager.GetSafePoint();
        safePos = targetSafePoint.transform.position;

        //if (team == 0)
        //{
        //    meshRenderer.material.color = Color.blue;
        //}
        //else if (team == 1)
        //{
        //    meshRenderer.material.color = Color.red;
        //}

        // Agent Behaviour Tree Begin  
        if (bt_root == null)
        {
            Selector root = new Selector();     // Temporary root.

            // Lead Agent Behaviour Tree
            if (agentRole == AgentRole.Lead)
            {
                Debug.Log("I Am A Lead Agent.");

                Inverter HealthAbove25PercentInverter = new Inverter();
                HealthAbove25PercentInverter.children.Add(new NodeCheckHealthAbove25Percent());

                Inverter inverter = new Inverter();
                inverter.children.Add(new NodeCheckHealthAbove50Percent());

                Sequencer IsMyHealthBelow25Percent = new Sequencer();
                IsMyHealthBelow25Percent.children.Add(HealthAbove25PercentInverter);
                IsMyHealthBelow25Percent.children.Add(new NodeTargetSafeArea());
                IsMyHealthBelow25Percent.children.Add(new NodeMoveTowardsTarget());
                IsMyHealthBelow25Percent.children.Add(new NodeHealAgent());

                Sequencer DoIHaveBall = new Sequencer();
                DoIHaveBall.children.Add(new NodeDoIHaveBall());
                DoIHaveBall.children.Add(new NodeTargetGoal());
                DoIHaveBall.children.Add(new NodeMoveTowardsTarget());
                DoIHaveBall.children.Add(new NodeScoreGoal());

                Sequencer IsPowerupKick = new Sequencer();
                IsPowerupKick.children.Add(new NodeIsPowerupKick());
                IsPowerupKick.children.Add(new NodeIsPowerupCloseEnough());
                IsPowerupKick.children.Add(new NodeTargetPowerup());
                IsPowerupKick.children.Add(new NodeMoveTowardsTarget());
                IsPowerupKick.children.Add(new NodeCapturePowerup());

                Sequencer IsPowerupSpeed = new Sequencer();
                IsPowerupSpeed.children.Add(new NodeIsPowerupSpeed());
                IsPowerupSpeed.children.Add(new NodeIsPowerupCloseEnough());
                IsPowerupSpeed.children.Add(new NodeTargetPowerup());
                IsPowerupSpeed.children.Add(new NodeMoveTowardsTarget());
                IsPowerupSpeed.children.Add(new NodeCapturePowerup());

                Sequencer DoesSomeoneElseHaveBall = new Sequencer();
                DoesSomeoneElseHaveBall.children.Add(new NodeIsBallCloseEnough());
                DoesSomeoneElseHaveBall.children.Add(new NodeDoesSomeoneElseHaveBall());
                DoesSomeoneElseHaveBall.children.Add(new NodeTargetAgent());
                DoesSomeoneElseHaveBall.children.Add(new NodeMoveTowardsTarget());
                DoesSomeoneElseHaveBall.children.Add(new NodeKickAgent());

                Sequencer IsBallFree = new Sequencer();
                IsBallFree.children.Add(new NodeIsBallCloseEnough());
                IsBallFree.children.Add(new NodeIsBallFree());
                IsBallFree.children.Add(new NodeTargetBall());
                IsBallFree.children.Add(new NodeMoveTowardsTarget());
                IsBallFree.children.Add(new NodeCaptureBall());

                Selector HealthBranch = new Selector();
                HealthBranch.children.Add(IsMyHealthBelow25Percent);

                Selector BallBranch = new Selector();
                BallBranch.children.Add(DoIHaveBall);
                BallBranch.children.Add(DoesSomeoneElseHaveBall);
                BallBranch.children.Add(IsBallFree);

                Selector PowerupBranch = new Selector();
                PowerupBranch.children.Add(IsPowerupSpeed);
                PowerupBranch.children.Add(IsPowerupKick);

                root.children.Add(IsMyHealthBelow25Percent);
                root.children.Add(BallBranch);
                root.children.Add(PowerupBranch);
            }

            // Support Agent Behaviour Tree
            else if (agentRole == AgentRole.Support)
            {
                Debug.Log("I am a Support Agent.");
                
                Sequencer DoesMyTeamHaveBall = new Sequencer();
                DoesMyTeamHaveBall.children.Add(new NodeDoesMyTeamHaveBall());
                DoesMyTeamHaveBall.children.Add(new NodeIsEnemyTeamAgentCloseEnough());
                DoesMyTeamHaveBall.children.Add(new NodeTargetAgent());
                DoesMyTeamHaveBall.children.Add(new NodeMoveTowardsTarget());

                Inverter InvertNearbyEnemyResult = new Inverter();
                InvertNearbyEnemyResult.children.Add(new NodeIsEnemyTeamAgentCloseEnough());

                Sequencer AttackNearbyEnemyAgent = new Sequencer();
                AttackNearbyEnemyAgent.children.Add(new NodeDoesMyTeamHaveBall());
                AttackNearbyEnemyAgent.children.Add(InvertNearbyEnemyResult);
                AttackNearbyEnemyAgent.children.Add(new NodeMoveTowardsTarget());
                AttackNearbyEnemyAgent.children.Add(new NodeKickAgent());

                Selector defenceBranch = new Selector();
                defenceBranch.children.Add(DoesMyTeamHaveBall);
                defenceBranch.children.Add(AttackNearbyEnemyAgent);

                root.children.Add(defenceBranch);
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

        //Sense();
        //Act();
    }

    void Sense()
    {
        if (agentState == AgentState.Idle)
        {
            recoveryTime += Time.deltaTime;

            if (recoveryTime > agentRecovery)
            {
                targetBall = null;
                recoveryTime = 0;
                //colliding = false;
            }
        }

        if (targetBall == null)
        {
            targetBall = gameManager.GetBall();
            agentState = AgentState.Fetching;
        }

        if (targetBall.agent != this)
        {
            if (targetBall != null)
                if (targetBall.agent != null)
                {
                    targetAgent = targetBall.agent;
                    agentState = AgentState.Attacking;
                }           
        }



        //Debug.Log("Rotation.eulerAngles.z: " + transform.rotation.eulerAngles.z);

        //if (transform.rotation.eulerAngles.z < -45 )
        //{
        //    agentState = AgentState.Fallen;
        //    Debug.Log("Rotation.eulerAngles.z: " + transform.rotation.eulerAngles.z);
        //}
        //else if (transform.rotation.eulerAngles.z > 45)
        //{
        //    agentState = AgentState.Fallen;
        //    Debug.Log("Rotation.eulerAngles.z: " + transform.rotation.eulerAngles.z);
        //}
        //else
        //{
        //    agentState = AgentState.Fetching;
        //}

        //if (targetBall == null && agentState != AgentState.Returning)
        //{
        //    //targetBall = gameManager.GetFreeBall(this);
        //    targetBall = gameManager.GetBall();
        //    targetPos = targetBall.transform.position;            
        //    gameManager.UpdateChaseData(this, targetBall);
        //    agentState = AgentState.Fetching;
        //}

        //if (agentState == AgentState.Fetching && Vector3.Distance(transform.position, targetBall.transform.position) <= 2)
        //{
        //    gameManager.UpdateCaptureData(this, targetBall);
        //    agentState = AgentState.Returning;
        //    targetPos = targetGoal.transform.position;
        //}
    }

    void Decide()
    {

    }

    void Act()
    {
        //if (!colliding)
        //{
        //    if (agentState != AgentState.Fallen)
        //    {
        //        if (agentState == AgentState.Fetching)
        //        {
        //            transform.position = Vector3.MoveTowards(transform.position, targetBall.transform.position, agentSpeed * Time.deltaTime);
        //        }
        //        else if (agentState == AgentState.Returning)
        //        {
        //            transform.position = Vector3.MoveTowards(transform.position, targetGoal.transform.position, agentSpeed * Time.deltaTime);
        //        }
        //        else if (agentState == AgentState.Attacking)
        //        {
        //            transform.position = Vector3.MoveTowards(transform.position, targetAgent.transform.position, 1.1f * agentSpeed * Time.deltaTime);
        //        }
        //        else
        //            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * agentSpeed);
        //    }
        //    else
        //    {
        //        rigidBody.useGravity = false;
        //        rigidBody.angularVelocity.Set(0, 0, 0);
        //        //transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * agentSpeed);
        //        transform.Rotate(0, 0, 1);
        //    }
        //}
        //else
        //{
        //    agentState = AgentState.Idle;
        //}
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


        //Debug.Log(agentCollision.gameObject.tag);

        //if (agentCollision.gameObject.CompareTag("Agent"))
        //{
        //    if (targetAgent != null)
        //    {
        //        if (collision.collider == targetAgent.agentCollider)
        //            colliding = true;
        //    }   
        //}


        //if (collision.gameObject.CompareTag("Ball") &&
        //    agentState != AgentState.Attacking &&
        //    agentState != AgentState.Idle)
        //{
        //    gameManager.UpdateCaptureData(this, targetBall);
        //    agentState = AgentState.Returning;
        //    targetPos = targetGoal.transform.position;
        //}

        //if (collision.gameObject.CompareTag("Agent"))
        //{
        //    colliding = true;
        //    if (agentState == AgentState.Attacking)
        //    {
        //        collision.collider.attachedRigidbody.AddExplosionForce(2000f, transform.position, 10f);
        //        agentState = AgentState.Idle;
        //        agentRecovery = 1f;
        //    }
        //    else if (agentState == AgentState.Fetching)
        //    {
        //        agentRecovery = 3f;
        //    }
        //    else if (agentState == AgentState.Returning)
        //    {
        //        Debug.Log("AgentBehaviour.cs - Calling RemoveCaptureData. " + meshRenderer.material.color);
        //        gameManager.RemoveCaptureData(this);
        //        agentRecovery = 3f;
        //    }
        //    else if (agentState == AgentState.Idle)
        //    {
        //        collision.collider.attachedRigidbody.AddExplosionForce(1000f, transform.position, 10f);
        //        agentRecovery = 3f;
        //    }
        //}
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

    //private void OnTriggerEnter(Collider other)
    //{
    //    agentCollider = other;

    //    if (other.gameObject.CompareTag("Powerup"))
    //    {
    //        if (targetAgent != null)
    //        {
    //            if (other == targetAgent.agentCollider)
    //                colliding = true;
    //        }
    //    }
    //}

    public void AddScore()
    {
        score++;
        //Debug.Log("Agent " + meshRenderer.material.color.ToString() + " Score: " + score);
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
            
            //targetBall = null;
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

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
}
