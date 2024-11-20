using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 追尾対象（キャラクター）
    public Vector3 offset = new Vector3(0, 5, -5); // カメラの初期オフセット（後頭部の斜め上）
    public float smoothSpeed = 0.1f; // カメラの追尾スムーズ速度

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // 目標位置を計算（ターゲットの位置＋オフセット）
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // 現在のカメラ位置をスムーズに更新
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed);

        // カメラの位置を更新
        transform.position = smoothedPosition;

        // キャラクターの方向を向くように回転
        transform.LookAt(target.position + Vector3.up * 1.5f); // キャラクターの少し上を見るよう調整
    }
}
