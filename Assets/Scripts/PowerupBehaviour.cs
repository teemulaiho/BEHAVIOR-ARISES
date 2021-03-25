using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupState
{
    Speed,
    Kick
}

public class PowerupBehaviour : MonoBehaviour
{
    GameManagerBehaviour gameManager;
    MeshRenderer meshRenderer;
    public PowerupState state;

    public void Init(GameManagerBehaviour p_gameManager)
    {
        gameManager = p_gameManager;
        meshRenderer = GetComponent<MeshRenderer>();

        state = (PowerupState)Random.Range(0, 2);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (state == PowerupState.Speed)
        {
            meshRenderer.material.color = Color.Lerp(Color.yellow, Color.black, Mathf.PingPong(Time.time, 1));
        }
        else if (state == PowerupState.Kick)
        {
            meshRenderer.material.color = Color.Lerp(Color.magenta, Color.black, Mathf.PingPong(Time.time, 1));
        }
    }
}
