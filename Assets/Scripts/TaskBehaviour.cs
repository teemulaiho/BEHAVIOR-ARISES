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
        for (int i = 0; i < children.Count; i++)
        {
            ReturnState state = children[i].Run(agent);

            if (state != ReturnState.FAILURE)
            {
                return state;
            }              
        }

        return ReturnState.FAILURE;
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

            if (state != ReturnState.SUCCESS)
            {
                return state;
            }
        }

        return ReturnState.SUCCESS;
    }
}

public class Inverter : BT_Node
{
    public List<BT_Node> children = new List<BT_Node>();

    public override ReturnState Run(AgentBehaviour agent)
    {
        for (int i = 0; i < children.Count; i++)
        {
            ReturnState state = children[i].Run(agent);

            if (state != ReturnState.SUCCESS)
            {
                return ReturnState.SUCCESS;
            }
        }

        return ReturnState.FAILURE;
    }
}

public class NodePrint : BT_Node
{
    private string text = "";
    public NodePrint(string text)
    {
        this.text = text;
    }
    public override ReturnState Run(AgentBehaviour agent)
    {
        return ReturnState.SUCCESS;
    }
}

// Generic Behaviour
public class NodeCheckHealthAbove25Percent : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.agentCurrentHealth / agent.agentMaxHealth > 0.25f &&
            !agent.isHealing)
        {
            return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;
    }
}

public class NodeCheckHealthAbove50Percent : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.agentCurrentHealth / agent.agentMaxHealth > 0.5f &&
            !agent.isHealing)
        {
            return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;
    }
}

public class NodeTargetHealthPotion : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        return ReturnState.FAILURE;
    }
}

public class NodeTargetSafeArea : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        agent.targetPos = agent.safePos;
        agent.targetType = TargetType.Safe;
        return ReturnState.SUCCESS;
    }
}

public class NodeHealAgent : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.agentCurrentHealth < agent.agentMaxHealth)
        {
            if (agent.collidingWithSafePoint)
            {
                if (!agent.isHealing)
                    agent.isHealing = true;

                agent.agentCurrentHealth++;
                agent.healthBar.SetHealth(agent.agentCurrentHealth);
            }
            return ReturnState.RUNNING;
        }
        else
        {
            agent.isHealing = false;
            return ReturnState.SUCCESS;
        }
    }
}


// Lead Agent
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
            if (agent.navMeshAgent.destination != agent.targetPos)
                agent.navMeshAgent.SetDestination(agent.targetPos);

            if (agent.transform.position != agent.targetGoal.transform.position)
            {
                return ReturnState.RUNNING;
            }
            else
                return ReturnState.SUCCESS;
        }
        else if (agent.HasAgentReachedCurrentTarget())
        {
            return ReturnState.SUCCESS;
        }
        else
        {
            if (agent.navMeshAgent.destination != agent.targetPos)
                agent.navMeshAgent.SetDestination(agent.targetPos);
            return ReturnState.RUNNING;
        }
    }
}

public class NodeTargetBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        agent.targetPos = agent.targetBall.transform.position;
        agent.targetType = TargetType.Ball;
        return ReturnState.SUCCESS;
    }
}

public class NodeTargetAgent : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.agentRole == AgentRole.Lead)
        {
            if (agent.targetBall.agent != null &&
                agent.targetBall.agent != agent)
            {
                agent.targetPos = agent.targetBall.agent.transform.position;
                agent.targetAgent = agent.targetBall.agent;
                agent.targetType = TargetType.Agent;
                return ReturnState.SUCCESS;
            }
        }
        else if (agent.agentRole == AgentRole.Support)
        {
            if (agent.targetAgent == agent)
            {
                agent.targetPos = agent.targetAgent.transform.position;
                return ReturnState.SUCCESS;
            }
        }

        return ReturnState.FAILURE;
    }
}

public class NodeTargetGoal : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        agent.targetPos = agent.targetGoal.transform.position;
        agent.targetType = TargetType.Goal;
        return ReturnState.SUCCESS;
    }
}

public class NodeTargetPowerup : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetPowerup != null)
        {
            agent.targetPos = agent.targetPowerup.transform.position;
            agent.targetType = TargetType.Powerup;
            return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;
    }
}

public class NodeCaptureBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.collidingWithBall)
        {
            agent.targetBall.SetAgent(agent);
            return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;           
    }
}

public class NodeCapturePowerup : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.collidingWithPowerup)
        {
            agent.AgentPowerup(agent.targetPowerup);
            agent.targetPowerup.ResetPowerup();
            return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;
    }
}

public class NodeCapture : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        return ReturnState.FAILURE;
    }
}

public class NodeScoreGoal : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (Vector3.Distance(agent.transform.position, agent.targetGoal.transform.position) < 0.02)
        {
            return ReturnState.SUCCESS;
        }
        else
        {
            return ReturnState.FAILURE;
        }
    }
}

public class NodeKickAgent : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.collidingWithAgent)
        {
            agent.targetAgent.rb.AddExplosionForce(2000f, agent.transform.position, 10f);
            agent.targetAgent.AgentTakeDamage(agent.kickForce.x);

            if (!agent.agentKickParticles.isPlaying)
                agent.agentKickParticles.Play();

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

        if (timeToBall > 10f)
        {
            return ReturnState.FAILURE;
        }

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

            if (timeToPowerUp > 20)
            {
                return ReturnState.FAILURE;
            }
        }

        return ReturnState.SUCCESS;
    }
}

public class NodeIsPowerupSpeed : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetPowerup != null &&
            agent.targetPowerup.IsActive())
        {
            if (agent.targetPowerup.state.ToString() == "Speed")
                return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;
    }
}

public class NodeIsPowerupKick : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent.targetPowerup != null &&
            agent.targetPowerup.IsActive())
        {
            if (agent.targetPowerup.state.ToString() == "Kick")
                return ReturnState.SUCCESS;
        }

        return ReturnState.FAILURE;
    }
}

// Support Agent
public class NodeDoesMyTeamHaveBall : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        if (agent != null &&
            agent.targetBall != null &&
            agent.targetBall.agent != null)
        {
            if (agent.targetBall.agent.team == agent.team)
            {
                return ReturnState.SUCCESS;
            }
        }

        return ReturnState.FAILURE;
    }
}

public class NodeIsEnemyTeamAgentCloseEnough : BT_Node
{
    public override ReturnState Run(AgentBehaviour agent)
    {
        AgentBehaviour nearestEnemyAgent = agent.gameManager.GetNearestEnemyAgent(agent);
        float timeToTarget = Vector3.Distance(agent.transform.position, nearestEnemyAgent.transform.position) / agent.agentSpeed;

        if (timeToTarget < 5)
        {
            agent.targetAgent = nearestEnemyAgent;
            return ReturnState.FAILURE;
        }

        return ReturnState.SUCCESS;
    }
}