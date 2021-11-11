using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    Vector3                         targetDirection;
    float                           projectileSpeed;
    float                           projectileDamage;
    bool                            fired;

    public void Init(Vector3 startPos, Vector3 targetPos)
    {
        if (!fired)
        {
            fired                   = true;
            transform.position      = startPos;
            targetDirection         = (targetPos - transform.position).normalized;
            projectileSpeed         = 4f;
            projectileDamage        = 200f;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.position += targetDirection * projectileSpeed * Time.deltaTime;
        
        if (Vector3.Distance(transform.position, Vector3.zero) > 50)
        {
            DisableGameobject();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Agent"))
        {
            other.GetComponent<LeadAgentBehaviour>().AgentTakeDamage(projectileDamage);
        }

        if(!other.gameObject.CompareTag("Enemy"))
        {
            DisableGameobject();
        }
    }

    private void DisableGameobject()
    {
        fired = false;
        this.gameObject.SetActive(false);
    }
}
