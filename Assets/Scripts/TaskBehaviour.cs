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
    BallBehaviour ball;

    public override ReturnState Run(AgentBehaviour agent)
    {
        return ReturnState.SUCCESS;
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

public class NodeTargetBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        agent.targetPos = agent.targetBall.transform.position;
        return ReturnState.SUCCESS;
    }
}

public class NodeMoveTowardsTarget : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (Vector3.Distance(agent.transform.position, agent.targetPos) < 1)
        {
            Debug.Log("Reached Target: " + agent.targetPos);
            return ReturnState.SUCCESS;
        }
        else
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, agent.targetPos, agent.agentSpeed * Time.deltaTime);
            return ReturnState.RUNNING;
        }
    }
}

public class NodeCaptureBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (Vector3.Distance(agent.transform.position, agent.targetBall.transform.position) < 1)
        {
            Debug.Log("Agent:" + agent.name + " captured ball at: " + agent.targetBall.transform.position);
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