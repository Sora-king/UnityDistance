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

        // 自分のアバターにのみ物理法則を適用、他のアバターの Rigidbody を削除
        if (!avatar.GetComponent<PhotonView>().IsMine)
        {
            RemovePhysics(avatar);
        }

        // 自分のアバターを TagObject に設定
        PhotonNetwork.LocalPlayer.TagObject = avatar;
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber}'s TagObject set to {avatar.name}");
    }

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
}
