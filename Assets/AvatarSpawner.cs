using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class AvatarSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameObject networkPlayer;

    [SerializeField]
    Transform cameraRig;

    [SerializeField]
    Transform[] playerPositions; // 配列に全プレイヤーの位置情報を保持

    GameObject player;

    void Start()
    {
        // Photonサーバーに接続
        PhotonNetwork.ConnectUsingSettings();
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster() {
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
    }

    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom() {
        // ランダムな座標に自身のアバター（ネットワークオブジェクト）を生成する
        var position = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
        PhotonNetwork.Instantiate(networkPlayer.name, position, Quaternion.identity);
    }
}

