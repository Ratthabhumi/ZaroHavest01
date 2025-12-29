using UnityEngine;

public class GravityObject : MonoBehaviour
{
    public float gravityStrength = 5f; // ความแรงแรงดึงดูด

    void FixedUpdate()
    {
        // คำนวณทิศทางเข้าหาจุดศูนย์กลาง (0,0)
        Vector2 direction = (Vector2.zero - (Vector2)transform.position).normalized;

        // ใส่แรงดูด
        GetComponent<Rigidbody2D>().AddForce(direction * gravityStrength);

        // หมุน object ให้หันเท้าเข้าหาดาว (ต้านแรงโน้มถ่วง)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}