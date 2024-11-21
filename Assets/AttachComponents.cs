using Photon.Voice.Unity; // Speakerクラスのための名前空間
using UnityEngine;

public class AttachComponents : MonoBehaviour
{
    void Start()
    {
        // メインカメラを取得
        GameObject mainCamera = Camera.main.gameObject;

        // CameraFollow スクリプトを追加
        if (mainCamera.GetComponent<CameraFollow2>() == null) // 既にアタッチされていない場合のみ追加
        {
            mainCamera.AddComponent<CameraFollow2>();
            Debug.Log("CameraFollow script added to the Main Camera.");
        }
    }
}
