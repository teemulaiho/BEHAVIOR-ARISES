using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBehaviour : MonoBehaviour
{
    GameManagerBehaviour gameManager;
    Rigidbody rigidBody;
    Vector3 kickForce;

    public void Init(GameManagerBehaviour p_gameManager)
    {
        gameManager = p_gameManager;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        kickForce = new Vector3(500, 10, 500);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rigidBody.AddForce(kickForce);
        }
    }
}
