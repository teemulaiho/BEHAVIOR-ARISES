using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ReturnState
{
    FAILURE,
    SUCCESS,
    RUNNING
}

public abstract class BT_Node
{
    public abstract ReturnState Run(AgentBehaviour agent);
}

public class Selector : BT_Node // Choose reaction.
{
    public List<BT_Node> children = new List<BT_Node>();

    public override ReturnState Run(AgentBehaviour agent)
    {
        bool running = false;

        for (int i = 0; i < children.Count; i++)
        {
            ReturnState state = children[i].Run(agent);

            if (state == ReturnState.SUCCESS)
                return state;
            if (state == ReturnState.RUNNING)
                running = true;
        }

        if (running)
        {
            return ReturnState.RUNNING;
        }

        return ReturnState.FAILURE;
        // run all children stop if condition of return
    }
}

public class Sequencer : BT_Node // Run reaction steps.
{
    public List<BT_Node> children = new List<BT_Node>();

    public override ReturnState Run(AgentBehaviour agent)
    {
        for (int i = 0; i < children.Count; i++)
        {
            ReturnState state = children[i].Run(agent);

            if (state == ReturnState.FAILURE)
            {
                return ReturnState.FAILURE;
            }
            if (state == ReturnState.RUNNING)
            {
                return ReturnState.RUNNING;
            }
        }

        return ReturnState.SUCCESS;
    }
}

// Filippo's Shenanigans
//public class Composite : BT_Node
//{

//}


//public class Node : BT_Node
//{

//}

public class NodePrint : BT_Node
{
    private string text = "";
    public NodePrint(string text)
    {
        this.text = text;
    }
    public override ReturnState Run(AgentBehaviour agent)
    {
        Debug.Log(text);
        return ReturnState.SUCCESS;
    }
}


public class NodeDoIHaveBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetBall.agent == agent)
        {
            agent.RemoveTargetAgent();
            return ReturnState.SUCCESS;
        }
        else
            return ReturnState.FAILURE;
    }
}
public class NodeDoesSomeoneElseHaveBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetBall.agent != null && 
            agent.targetBall.agent != agent)
        {
            return ReturnState.SUCCESS;
        }
        else
            return ReturnState.FAILURE;
    }
}
public class NodeIsBallFree : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetBall.agent == null)
        {
            return ReturnState.SUCCESS;
        }
        else
            return ReturnState.FAILURE;      
    }
}
public class NodeMoveTowardsTarget : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetPos == agent.targetGoal.transform.position)
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, agent.targetPos, agent.agentSpeed * Time.deltaTime);

            if (agent.transform.position != agent.targetGoal.transform.position)
            {
                return ReturnState.RUNNING;
            }
            else
                return ReturnState.SUCCESS;
        }
        else if (Vector3.Distance(agent.transform.position, agent.targetPos) < 1)
        {
            //Debug.Log("Reached Target: " + agent.targetPos);
            return ReturnState.SUCCESS;
        }
        else
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, agent.targetPos, agent.agentSpeed * Time.deltaTime);
            return ReturnState.RUNNING;
        }
    }
}
public class NodeTargetBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        Debug.Log("Targeting Ball.");
        agent.targetPos = agent.targetBall.transform.position;
        return ReturnState.SUCCESS;
    }
}
public class NodeTargetAgent : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        Debug.Log("Targeting Agent.");

        if (agent.targetBall.agent != null &&
            agent.targetBall.agent != agent)
        {
            Debug.Log("Targeting agent who has ball.");
            agent.targetPos = agent.targetBall.agent.transform.position;
            agent.targetAgent = agent.targetBall.agent;
            return ReturnState.SUCCESS;
        }
        else if (agent.targetBall.agent == agent)
        {
            Debug.Log("I have ball, why would I target myself?");
            return ReturnState.FAILURE;
        }

        return ReturnState.FAILURE;
    }
}
public class NodeTargetGoal : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        Debug.Log("Targeting Goal.");
        agent.targetPos = agent.targetGoal.transform.position;
        return ReturnState.SUCCESS;
    }
}
public class NodeTargetPowerup : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetPowerup != null)
        {
            Debug.Log("Targeting Powerup: " + agent.targetPowerup.state);
            agent.targetPos = agent.targetPowerup.transform.position;
            return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;
    }
}
public class NodeCaptureBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (Vector3.Distance(agent.transform.position, agent.targetBall.transform.position) < 1)
        {
            //Debug.Log("Agent:" + agent.name + " captured ball at: " + agent.targetBall.transform.position);
            agent.targetBall.SetAgent(agent);
            return ReturnState.SUCCESS;
        }
        else
        {
            Debug.Log("Could not capture, distance to ball is: " + Vector3.Distance(agent.transform.position, agent.targetBall.transform.position));
            return ReturnState.FAILURE;
        }           
    }
}
public class NodeCapturePowerup : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.agentCollision != null)
        {
            if (agent.agentCollision.collider.CompareTag("Powerup"))
            {
                Debug.Log("Collided with powerup.");
                return ReturnState.SUCCESS;
            }
        }

        Debug.Log("Powerup too for away for collision.");
        return ReturnState.FAILURE;
    }
}
public class NodeScoreGoal : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (Vector3.Distance(agent.transform.position, agent.targetGoal.transform.position) < 0.02)
        {
            Debug.Log("Agent:" + agent.name + " scored goal!");
            //agent.targetBall.ResetBall();
            return ReturnState.SUCCESS;
        }
        else
        {
            //Debug.Log("Could not score, distance to goal is: " + Vector3.Distance(agent.transform.position, agent.targetGoal.transform.position));
            return ReturnState.FAILURE;
        }
    }
}
public class NodeKickAgent : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.colliding)
        {
            Debug.Log("Reahed agent, proceed to kick.");
            agent.targetAgent.rigidBody.AddExplosionForce(2000f, agent.transform.position, 10f);
            agent.targetAgent.targetBall.RemoveAgent(agent.targetAgent);
            agent.targetAgent.RemoveBall();
            agent.RemoveTargetAgent();
            return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;
    }
}
public class NodeIsBallCloseEnough: BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        float timeToBall = Vector3.Distance(agent.transform.position, agent.targetBall.transform.position) / agent.agentSpeed;

        if (timeToBall > 10)
        {
            Debug.Log("Ball is too far away from me.");
            return ReturnState.FAILURE;
        }

        Debug.Log("Ball is close enough.");
        return ReturnState.SUCCESS;
    }
}
public class NodeIsPowerupCloseEnough : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetPowerup == null)
        {
            return ReturnState.FAILURE;
        }
        else
        {
            float timeToPowerUp = Vector3.Distance(agent.transform.position, agent.targetPowerup.transform.position) / agent.agentSpeed;

            if (timeToPowerUp > 10)
            {
                Debug.Log("Powerup is to far away from me.");
                return ReturnState.FAILURE;
            }
        }

        Debug.Log("Powerup is close enough.");
        return ReturnState.SUCCESS;
    }
}
public class NodeIsPowerupSpeed : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetPowerup.state.ToString() == "Speed")
            return ReturnState.SUCCESS;

        return ReturnState.FAILURE;
    }
}

public class NodeIsPowerupKick : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetPowerup.state.ToString() == "Kick")
            return ReturnState.SUCCESS;

        return ReturnState.FAILURE;
    }
}


/*
 * main()
 * {
 BT_Node root = null;

    Seqeuence s = new Sequence(); // will run all its children if they return success
    PrintNode a = new PrintNode("Hello"); // will print and always return success
    PrintNode b = new PrintNode("World");
    
    s.children.add(a);
    s.children.add(b);
 
 root.Run(); // running the BT
 }
 */

// Decorators
// Composites
// Nodes




// I Have Ball Branch
// Selector a;
// Selector b;
// Leaf Node;

// I Don't Have Ball Branch
// Selector c;
// Selector d;
// Leaf Node;

// Ball Is Free Branch
// Selector e;
// Selector f;
// Leaf Node;