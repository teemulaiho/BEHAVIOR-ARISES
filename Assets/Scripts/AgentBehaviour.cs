using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public class AgentBehaviour : MonoBehaviour
{
    public GameManagerBehaviour gameManager;
    public AgentState agentState;
    public MeshRenderer meshRenderer;
    public Rigidbody rigidBody;
    public Collider agentCollider;
    public Collision agentCollision;

    public GoalBehaviour targetGoal;
    public BallBehaviour targetBall;
    public AgentBehaviour targetAgent;
    public PowerupBehaviour targetPowerup;

    public Vector3 targetPos;
    public Quaternion originalRotation;
    public float agentSpeed;
    public float agentRecovery;
    public float recoveryTime;
    public int score;

    public Vector3 kickForce;
    public bool colliding;


    // Agent Behaviour Tree Begin
    static BT_Node bt_root;
    //static Selector bt_root;
    // Agent Behaviour Tree End

    public void Init(GameManagerBehaviour p_gameManager)
    {
        Debug.Log("Init AgentBehaviour.");

        this.name = "Agent";
        gameManager = p_gameManager;
        agentState = AgentState.Idle;
        targetBall = null;
        targetAgent = null;
        targetPowerup = gameManager.GetPowerup();
        agentSpeed = 2.5f;
        agentRecovery = 2f;
        recoveryTime = 0;
        targetGoal = gameManager.GetGoal(this);
        meshRenderer = GetComponent<MeshRenderer>();
        rigidBody = GetComponent<Rigidbody>();
        agentCollider = GetComponent<CapsuleCollider>();
        originalRotation = transform.rotation;
        score = 0;
        colliding = false;

        kickForce = new Vector3(50, 0, 50);

        targetBall = gameManager.GetBall();


        // Agent Behaviour Tree Begin  
        if (bt_root == null)
        {
            Selector root = new Selector();     // Temporary root.

            Sequencer DoIHaveBall = new Sequencer();
            DoIHaveBall.children.Add(new NodeDoIHaveBall());
            DoIHaveBall.children.Add(new NodeTargetGoal());
            DoIHaveBall.children.Add(new NodeMoveTowardsTarget());
            DoIHaveBall.children.Add(new NodeScoreGoal());


            Sequencer IsPowerUpNearby = new Sequencer();
            IsPowerUpNearby.children.Add(new NodeIsPowerupCloseEnough());
            IsPowerUpNearby.children.Add(new NodeTargetPowerup());
            IsPowerUpNearby.children.Add(new NodeMoveTowardsTarget());
            IsPowerUpNearby.children.Add(new NodeCapturePowerup());

            Sequencer DoesSomeoneElseHaveBall = new Sequencer();
            DoesSomeoneElseHaveBall.children.Add(new NodeDoesSomeoneElseHaveBall());
            DoesSomeoneElseHaveBall.children.Add(new NodeTargetAgent());
            DoesSomeoneElseHaveBall.children.Add(new NodeMoveTowardsTarget());
            DoesSomeoneElseHaveBall.children.Add(new NodeKickAgent());

            Sequencer IsBallFree = new Sequencer();
            IsBallFree.children.Add(new NodeIsBallFree());
            IsBallFree.children.Add(new NodeTargetBall());
            IsBallFree.children.Add(new NodeMoveTowardsTarget());
            IsBallFree.children.Add(new NodeCaptureBall());

            Selector IsBallNearby = new Selector();
            IsBallNearby.children.Add(new NodeIsBallCloseEnough());
            IsBallNearby.children.Add(DoesSomeoneElseHaveBall);
            IsBallNearby.children.Add(IsBallFree)   ;


            root.children.Add(DoIHaveBall);
            root.children.Add(IsBallNearby);
            root.children.Add(IsPowerUpNearby);
            //root.children.Add(DoesSomeoneElseHaveBall);
            //root.children.Add(IsBallFree);
            bt_root = root;
        }
        // Agent Behaviour Tree End
    }

    public void AgentUpdate()
    {
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
                colliding = false;
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
        if (!colliding)
        {
            if (agentState != AgentState.Fallen)
            {
                if (agentState == AgentState.Fetching)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetBall.transform.position, agentSpeed * Time.deltaTime);
                }
                else if (agentState == AgentState.Returning)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetGoal.transform.position, agentSpeed * Time.deltaTime);
                }
                else if (agentState == AgentState.Attacking)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetAgent.transform.position, 1.1f * agentSpeed * Time.deltaTime);
                }
                else
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * agentSpeed);
            }
            else
            {
                rigidBody.useGravity = false;
                rigidBody.angularVelocity.Set(0, 0, 0);
                //transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * agentSpeed);
                transform.Rotate(0, 0, 1);
            }
        }
        else
        {
            agentState = AgentState.Idle;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        agentCollision = collision;

        if (collision.gameObject.CompareTag("Agent"))
        {
            if (targetAgent != null)
            {
                if (collision.collider == targetAgent.agentCollider)
                    colliding = true;
            }   
        }


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
    }

    public void AddScore()
    {
        score++;
        Debug.Log("Agent " + meshRenderer.material.color.ToString() + " Score: " + score);
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

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
}
