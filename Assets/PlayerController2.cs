using Photon.Pun;
using UnityEngine;

public class PlayerController2 : MonoBehaviourPun
{
    private Rigidbody rb;
    public float moveSpeed = 5f; // 移動速度
    public float rotationSpeed = 300f; // 回転速度

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!photonView.IsMine)
        {
            Destroy(rb); // 他のプレイヤーのRigidbodyを削除
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleMovement(); // キャラクターの移動処理
            HandleRotation(); // マウスによる回転処理
        }
    }

    void HandleMovement()
    {
        // 入力に基づいて移動を計算
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // アバターのローカル座標系を基準に移動
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.Self);
    }

    void HandleRotation()
    {
        // マウスの左ボタンを押している間だけ回転を有効に
        if (Input.GetMouseButton(0)) // 左クリック (0は左クリック, 1は右クリック, 2は中央クリック)
        {
            float mouseX = Input.GetAxis("Mouse X"); // マウスの横方向の動き
            float rotationY = mouseX * rotationSpeed * Time.deltaTime;

            // Y軸を基準に回転
            transform.Rotate(0, rotationY, 0);
        }
    }
}
