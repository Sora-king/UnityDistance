using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class AvatarSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab; // プレイヤーPrefab
    [SerializeField] private Transform[] spawnPoints; // スポーン位置

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

        // アバターを生成
        GameObject avatar = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        // 自分のアバターを TagObject に設定
        PhotonNetwork.LocalPlayer.TagObject = avatar;
    }
}
