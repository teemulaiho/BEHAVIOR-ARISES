using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupState
{
    Speed,
    Kick,
    // Pull
}

public class PowerupBehaviour : MonoBehaviour
{
    GameManagerBehaviour gameManager;
    MeshRenderer meshRenderer;
    Rigidbody rigidBody;
    public PowerupState state;
    bool isActive;
    float powerupSpawnTime;
    float dt;

    public void Init(GameManagerBehaviour p_gameManager)
    {
        gameManager = p_gameManager;
        meshRenderer = GetComponent<MeshRenderer>();
        rigidBody = GetComponent<Rigidbody>();

        state = (PowerupState)Random.Range(0, 2);
        isActive = true;
        powerupSpawnTime = 5f;
        dt = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void PowerupUpdate()
    {
        if (!isActive)
        {
            dt += Time.deltaTime;
        }  

        if (dt > powerupSpawnTime)
        {
            dt = 0f;
            SpawnPowerup();
        }

        if (state == PowerupState.Speed)
        {
            meshRenderer.material.color = Color.Lerp(Color.yellow, Color.black, Mathf.PingPong(Time.time, 1));
        }
        else if (state == PowerupState.Kick)
        {
            meshRenderer.material.color = Color.Lerp(Color.magenta, Color.black, Mathf.PingPong(Time.time, 1));
        }
    }

    public void ResetPowerup()
    {
        transform.position = new Vector3(Random.Range(1, 49), 5, Random.Range(-1, -49));
        meshRenderer.enabled = false;
        rigidBody.useGravity = false;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        isActive = false;

        state = (PowerupState)Random.Range(0, 2);
    }

    public void SpawnPowerup()
    {
        meshRenderer.enabled = true;
        rigidBody.useGravity = true;

        isActive = true;
    }

    public bool IsActive()
    {
        return isActive;
    }
}
