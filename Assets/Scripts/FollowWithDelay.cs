using System.Collections.Generic;
using UnityEngine;

public class FollowWithDelay : MonoBehaviour
{
    public Transform target;
    public float delayInSeconds = 1f;

    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    private Queue<Quaternion> rotationHistory = new Queue<Quaternion>();
    private float timer = 0f;

    void Update()
    {
        if (target == null) return;

        timer += Time.deltaTime;

        // Simpan posisi dan rotasi target
        positionHistory.Enqueue(target.position);
        rotationHistory.Enqueue(target.rotation);

        // Jika sudah lewat waktu delay
        if (timer >= delayInSeconds)
        {
            if (positionHistory.Count > 0 && rotationHistory.Count > 0)
            {
                Vector3 delayedPosition = positionHistory.Dequeue();
                delayedPosition.y = 1.0f; // Tetap di ketinggian 2
                transform.position = delayedPosition;

                transform.rotation = rotationHistory.Dequeue();
            }
        }
    }
}
