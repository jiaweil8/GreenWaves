using UnityEngine;
using UnityEngine.AI;

public class AITarget : MonoBehaviour
{
    public Transform target;
    public float AttackDistance;
    
    private NavMeshAgent m_agent;
    private Animator m_animator;
    private float m_distance;
    
    // Start is called before the first frame update
    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        m_distance = Vector3.Distance(m_agent.transform.position, target.position);
        if (m_distance < AttackDistance)
        {
            m_agent.isStopped = true;
            //m_animator.SetBool("Attack", true);
        }
        else
        {
            m_agent.isStopped = false;
            //m_animator.SetBool("Attack", false);
            m_agent.SetDestination(target.position);
        }

        // void OnAnimatiorMove()
        // {
        //     
        // }
    }
}
