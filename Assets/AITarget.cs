using UnityEngine;
using UnityEngine.AI;

public class AITarget : MonoBehaviour
{
    public Transform target;
    public float AttackDistance;
    public int curStage;
    
    private NavMeshAgent m_agent;
    //private Animator m_animator;
    private float m_distance;
    private bool m_stageChanged;
    private float m_speed_s1 = 5f;
    private float m_speed_s2 = 10f;
    private float m_speed_s3 = 15f;
    
    // Start is called before the first frame update
    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();
        //m_animator = GetComponent<Animator>();
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

        if(m_stageChanged)
            CheckStage();
    }

    void CheckStage()
    {
        if (curStage == 1)
            m_agent.speed = m_speed_s1;
        else if (curStage == 2)
            m_agent.speed = m_speed_s2;
        else if (curStage == 3)
            m_agent.speed = m_speed_s3;
    }
}
