using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState
{
    Free,
    Targeted
}

public class BallBehaviour : MonoBehaviour
{
    GameManagerBehaviour gameManager;
    BallState ballState;
    AgentBehaviour agent;
    Vector3 fleefrom;

    float speed = 2.5f;

    public void Init(GameManagerBehaviour p_gameManger)
    {
        gameManager = p_gameManger;
        ballState = BallState.Free;
        agent = null;
    }

    public void BallUpdate()
    {
        if(gameManager.AmIBeingChased(this, ref fleefrom))
        {
            // fleefrom no has the data stored as a reference


            //Debug.Log(fleefrom);

            if (Vector3.Distance(transform.position, fleefrom) > 5.0f)
            {
                Debug.Log(fleefrom);

                transform.position = Vector3.MoveTowards(transform.position, fleefrom + transform.position, speed * Time.deltaTime);
            }
        }
    }

    public BallState GetState()
    {
        return ballState;
    }

    public void SetAgent(AgentBehaviour p_agent)
    {
        agent = p_agent;
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
