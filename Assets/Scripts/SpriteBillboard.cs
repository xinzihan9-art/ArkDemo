
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [Header("Billboard Settings")]
    [Range(-45f, 45f)]
    public float tiltAngle = 15f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // 计算摄像机水平方向
        Vector3 direction =
            mainCamera.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // 保持图片正面朝向摄像机
        Quaternion faceCamera =
            Quaternion.LookRotation(-direction.normalized);

        // 添加固定的倾斜角度
        transform.rotation =
            faceCamera * Quaternion.Euler(tiltAngle, 0f, 0f);
    }
}
