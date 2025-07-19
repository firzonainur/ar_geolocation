using UnityEngine;

public class CharacterControllerByDistance : MonoBehaviour
{
    public Transform player;              // Kamera AR
    public float activationDistance = 3f; // Jarak trigger
    public float moveSpeed = 1.5f;        // Kecepatan gerak
    public Transform pointA;              // Titik awal (waypoint 1)
    public Transform pointB;              // Titik tujuan (waypoint 2)

    private Animator animator;
    private bool isMoving = false;
    private Transform currentTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
        transform.position = pointA.position; // Mulai dari point A
        currentTarget = pointB;               // Bergerak ke point B
    }

    void Update()
    {
        if (player == null || pointA == null || pointB == null) return;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= activationDistance)
        {
            if (!isMoving)
            {
                isMoving = true;
                if (animator != null)
                    animator.SetBool("isWalking", true);
            }

            MoveTowards(currentTarget);
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                if (animator != null)
                    animator.SetBool("isWalking", false);
            }
        }
    }

    void MoveTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(target); // Opsional: menghadap target

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance < 0.1f)
        {
            // Tukar target jika sudah sampai
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }
}
