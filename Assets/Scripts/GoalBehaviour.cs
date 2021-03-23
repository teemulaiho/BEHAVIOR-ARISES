using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBehaviour : MonoBehaviour
{
    GameManagerBehaviour gameManager;

    public void Init(GameManagerBehaviour p_gameManager)
    {
        Debug.Log("Init GoalBehaviour.");
        gameManager = p_gameManager;
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
