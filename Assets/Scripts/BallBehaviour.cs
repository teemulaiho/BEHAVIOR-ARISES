using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState
{
    Free,
    Targeted,
    Captured
}

public class BallBehaviour : MonoBehaviour
{
    public GameManagerBehaviour gameManager;
    public Rigidbody rigidBody;
    public BallState ballState;
    public AgentBehaviour agent;
    public Vector3 fleefrom;
    public Vector3 newPos;
    public float speed = 2.5f;
    public float defaultSpeed = 2.5f;

    public void Init(GameManagerBehaviour p_gameManger)
    {
        Debug.Log("Init BallBehaviour.");
        gameManager = p_gameManger;
        rigidBody = GetComponent<Rigidbody>();
        ballState = BallState.Free;
        agent = null;
    }

    public void BallUpdate()
    {
        if (agent != null)
        {
            rigidBody.velocity = Vector3.zero;
            transform.position = Vector3.MoveTowards(transform.position, agent.transform.position, speed * Time.deltaTime);
        }

        //if (gameManager.AmIBeingChased(this, ref fleefrom))
        //{
        //    ballState = BallState.Targeted;
        //}
        //else if (gameManager.AmICaptured(this, out agent))
        //{
        //    ballState = BallState.Captured;
        //}
        //else
        //    ballState = BallState.Free;


        //else if (ballState != BallState.Targeted)
        //    ballState = BallState.Free;

        // fleefrom no has the data stored as a reference
        //Debug.Log(fleefrom);

        //if (ballState == BallState.Targeted)
        //{
        //    if (Vector3.Distance(transform.position, fleefrom) > 3.0f)
        //    {
        //        Debug.Log(fleefrom);

        //        newPos = Vector3.MoveTowards(transform.position, fleefrom + transform.position, speed * Time.deltaTime);
        //        newPos.y = transform.position.y;
        //        transform.position = newPos;
        //    }
        //}
        //else if (ballState == BallState.Captured)
        //{
        //    rigidBody.velocity = Vector3.zero;
        //    transform.position = Vector3.MoveTowards(transform.position, agent.transform.position, speed * Time.deltaTime);
        //}
    }

    public BallState GetState()
    {
        return ballState;
    }

    public void SetAgent(AgentBehaviour p_agent)
    {
        if(agent != p_agent)
        {
            agent = p_agent;
            speed = p_agent.agentSpeed;
        }
   
    }

    public bool RemoveAgent(AgentBehaviour p_agent)
    {
        if(agent != null)
        {
            if (agent == p_agent)
            {
                agent = null;
                return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            Debug.Log("GOOOOOOOOOOOOOOOOALLL!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Goal"))
        {
            if (other.gameObject.CompareTag("Goal"))
            {
                Debug.Log("GOOOOOOOOOOOOOOOOALLL!");
                agent.AddScore();
                RemoveAgent(agent);
                ResetBall();
            }
        }
    }

    public void ResetBall()
    {
        transform.position = new Vector3(Random.Range(1, 49), 3, Random.Range(-1, -49));
        speed = defaultSpeed;
        ballState = BallState.Free;
        //gameManager.Goal(this, agent);
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
