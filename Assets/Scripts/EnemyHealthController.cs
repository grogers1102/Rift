using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    public float health = 100;
    public void DamageToHealth(float dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
       
    }
}
