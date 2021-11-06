using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, Agent
{
    GameManagerBehaviour        gameManager;
    int                         enemyID;

    AgentBehaviour              targetAgent;

    public HealthbarBehaviour   healthBar;
    public float                enemyMaxHealth;
    public float                enemyCurrentHealth;

    public void Init(GameManagerBehaviour gm, int id)
    {
        gameManager         = gm;
        enemyID             = id;
        healthBar           = GetComponentInChildren<HealthbarBehaviour>();

        enemyMaxHealth      = 1000f;
        enemyCurrentHealth  = enemyMaxHealth;

        healthBar.SetMaxHealth(enemyMaxHealth);
    }

    public void EnemyUpdate()
    {
        Float();
    }

    private void Float()
    {
        float yOffset = 1 + 0.5f * transform.localScale.y;

        float x = transform.position.x;
        float y = Mathf.PingPong(Time.time, 2) + yOffset;
        float z = transform.position.z;

        transform.position = new Vector3(x, y, z);
    }
}
