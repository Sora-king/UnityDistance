using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AvatarSync : MonoBehaviourPun, IPunObservable
{
    private Vector3 networkPosition; // ネットワークで受信した位置
    private Quaternion networkRotation; // ネットワークで受信した回転

    void Start()
    {
        if (!photonView.IsMine)
        {
            // 他クライアントのアバター用に初期化
            networkPosition = transform.position;
            networkRotation = transform.rotation;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            // 他クライアントのアバターの位置と回転を補間して滑らかに動かす
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10);
        }
    }

    // データ送受信を定義
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 自分のアバターのデータを送信
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // 他プレイヤーのアバターのデータを受信
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
