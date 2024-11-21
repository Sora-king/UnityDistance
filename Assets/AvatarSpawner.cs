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
    Transform[] playerPositions;

    GameObject player;

    void Start()
    {
        //PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();     
    }

    //マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        // "OnoTest"という名前のルームに参加する（ルームが無ければ作成してから参加する）
        PhotonNetwork.JoinOrCreateRoom("OnoTest", new RoomOptions(), TypedLobby.Default);
        print("ルーム作成完了");
    }

    //部屋に入ったらアバター生成
    public override void OnJoinedRoom()
    {
        int othersCount = PhotonNetwork.PlayerListOthers.Length;
        GameObject spawnedPlayer = PhotonNetwork.Instantiate(networkPlayer.name, playerPositions[othersCount].position, Quaternion.identity);
        cameraRig = spawnedPlayer.GetComponentInChildren<Camera>().transform;

        if (cameraRig != null)
        {
            // カメラリグの位置をスポーン位置に設定
            cameraRig.position = playerPositions[othersCount].position;
            Debug.Log("CameraRig position set successfully.");
        }
        else
        {
            Debug.LogError("CameraRig could not be found in the spawned player.");
        }
        //cameraRig.position = playerPositions[othersCount].position;
    }
}