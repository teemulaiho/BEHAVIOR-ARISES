using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    Vector3 targetDirection;
    float projectileSpeed;
    bool fired;

    public void Init(Vector3 startPos, Vector3 targetPos)
    {
        if (!fired)
        {
            fired = true;
            transform.position = startPos;
            targetDirection = (targetPos - transform.position).normalized;
            projectileSpeed = 4f;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.position += targetDirection * projectileSpeed * Time.deltaTime; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.gameObject.CompareTag("Enemy"))
        {
            fired = false;
            this.gameObject.SetActive(false);
        }
    }
}
