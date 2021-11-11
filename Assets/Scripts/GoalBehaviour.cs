using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBehaviour : MonoBehaviour
{
    GameManagerBehaviour gameManager;

    public void Init(GameManagerBehaviour p_gameManager)
    {
        gameManager = p_gameManager;
    }
}
