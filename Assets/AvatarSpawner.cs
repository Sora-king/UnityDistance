using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab; // プレイヤーPrefab
    [SerializeField] private Transform[] spawnPoints; // スポーン位置
    private GameObject avatar;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // サーバーに接続
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("RoomName", new RoomOptions(), TypedLobby.Default); // ルームに接続または作成
    }

    public override void OnJoinedRoom()
    {
        // プレイヤーのスポーン位置を決定
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Vector3 spawnPosition = spawnPoints[playerIndex % spawnPoints.Length].position;
        Quaternion spawnRotation = spawnPoints[playerIndex % spawnPoints.Length].rotation;

        // アバターを生成
        avatar = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);
        /*

        // 自分のアバターにのみ物理法則を適用、他のアバターの Rigidbody を削除
        if (!avatar.GetComponent<PhotonView>().IsMine)
        {
            SetTriggerForOtherAvatars(avatar);
        }


        // 自分のアバターを TagObject に設定
        PhotonNetwork.LocalPlayer.TagObject = avatar;
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber}'s TagObject set to {avatar.name}");
        */
    }

    /*

    // 他のアバターの物理法則を削除するメソッド
    private void RemovePhysics(GameObject targetAvatar)
    {
        Rigidbody rb = targetAvatar.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb); // Rigidbody を削除
        }

        Collider collider = targetAvatar.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider); // Collider も削除（必要なら）
        }

        Debug.Log("物理法則を削除しました: " + targetAvatar.name);
    }

    

    // 他のアバターの Collider を Is Trigger に設定するメソッド
    private void SetTriggerForOtherAvatars(GameObject targetAvatar)
    {
        Collider collider = avatar.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true; // Collider の Is Trigger を ON に設定
        }

        Debug.Log("Collider を Is Trigger に設定しました: " + targetAvatar.name);
    }
    private IEnumerator WaitForPhotonView(GameObject avatar)
    {
        // PhotonViewが割り当てられるまで待つ
        PhotonView photonView = avatar.GetComponent<PhotonView>();
        while (photonView == null || photonView.IsMine)
        {
            yield return new WaitForSeconds(0.5);
        }

        // 自分以外のアバターに物理法則を適用
        if (!photonView.IsMine)
        {
            SetTriggerForOtherAvatars(avatar);
        }
    }*/
}
