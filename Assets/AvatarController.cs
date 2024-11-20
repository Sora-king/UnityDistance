using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    public float moveSpeed = 5f; // 移動速度
    public float rotationSpeed = 300f; // 回転速度

    void Update()
    {
         // 矢印キーでの移動処理
        float horizontal = Input.GetAxis("Horizontal"); // 左右移動（A/Dキーまたは矢印キー）
        float vertical = Input.GetAxis("Vertical"); // 前後移動（W/Sキーまたは矢印キー）

        // 前後方向の移動（キャラクターの向きを基準にする）
        Vector3 moveDirection = transform.forward * vertical; // 前後移動
        Vector3 strafeDirection = transform.right * horizontal; // 左右移動

        // 移動を合成
        Vector3 finalMoveDirection = (moveDirection + strafeDirection).normalized;

        // キャラクターの移動実行
        transform.position += finalMoveDirection * moveSpeed * Time.deltaTime;
        if (Input.GetMouseButton(0)) // 左クリックを検知（0は左ボタン、1は右ボタン、2は中ボタン）
        {
            // マウスの移動による回転処理
            float mouseX = Input.GetAxis("Mouse X"); // マウスの横移動
            transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime); // Y軸で回転
        }
        
    }
}
