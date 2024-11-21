using Photon.Pun;
using UnityEngine;

public class CameraFollow2 : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 5, -5); // カメラのオフセット
    public float smoothSpeed = 0.1f; // 追尾スムーズ速度

    private Transform target; // 追尾対象（自分のアバター）
    private Vector3 currentVelocity;

    void Start()
    {
        StartCoroutine(FindTarget());
    }
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

    private System.Collections.IEnumerator FindTarget()
    {
        while (true)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                PhotonView photonView = obj.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine) // 自分のアバターを特定
                {
                    target = obj.transform;
                    Debug.Log("CameraFollow: Target set to " + obj.name);
                    yield break; // ターゲットを見つけたらコルーチンを終了
                }
            }

            Debug.Log("CameraFollow: Waiting for target...");
            yield return new WaitForSeconds(0.5f); // 0.5秒ごとに再試行
        }
    }
}

