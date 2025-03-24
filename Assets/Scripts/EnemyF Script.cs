using UnityEngine;

public class EnemyFScript : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent;
    public Transform player;

    //public LayerMask whatIsGround, whatIsPlayer;

   // public float health;

    //Attacking 
   // public float timeBetweenAttacks;
   // bool alreadyAttacked;
    //public GameObject projectile; 

    //States 
   // public float sightRange, attackRange;
   // public bool playerInSightRange;
   // public bool playerInAttackRange;

    //private void Awake()
   //{
    //    player = GameObject.Find("Player").transform;
    //    agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
  //  }


    //private void Update()
   // {
        //Check for sight and attack range
        //playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        //Debug.Log(Physics.CheckSphere(transform.position, sightRange, whatIsPlayer));

        //playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
       // Debug.Log(Physics.CheckSphere(transform.position, attackRange, whatIsPlayer));

        //Debug.Log($"Player in sight range: {playerInSightRange}");
       // Debug.Log($"Player in attack range: {playerInAttackRange}");

        //if (playerInSightRange && !playerInAttackRange) ChasePlayer();
       // if (playerInAttackRange && playerInSightRange) AttackPlayer();
  //  }

  
 //   private void ChasePlayer()
  ///  {
 //       agent.SetDestination(player.position);
  //      Debug.Log("chaseing player");
  //  }


   // private void AttackPlayer()
   // {
        //Make sure enemy doesn't move
   //     agent.SetDestination(transform.position);
//
//        transform.LookAt(player);
//
  //      if (!alreadyAttacked)
    //    {
      //      alreadyAttacked = true;
//          Invoke(nameof(ResetAttack), timeBetweenAttacks);
//    }
//    }


   // private void ResetAttack()
   // {
   //     alreadyAttacked = false;
  //  }


  //  public void TakeDamage(int damage)
    //{
     //   health -= damage;
//
    //    if (health <= 0) Invoke(nameof(DestroyEnemy), .5f);
   // }


  //  private void DestroyEnemy()
  //  {
  //      Destroy(gameObject);
  //  }


   // private void OnDrawGizmosSelected()
  //  {
   //     Gizmos.color = Color.red;
   //     Gizmos.DrawWireSphere(transform.position, attackRange);
   //     Gizmos.color = Color.yellow;
   //     Gizmos.DrawWireSphere(transform.position, sightRange);
   // }


}