using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class GroupFormationManager : MonoBehaviourPunCallbacks
{
    public float clusteringDistance = 10f; // 特定のプレイヤーからの距離閾値
    private const string GROUP_KEY = "Groups"; // ルームカスタムプロパティのキー

    void Update()
    {
        // 特定のプレイヤー（マスタークライアント）がグループ化を開始
        if (PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.Y))
        {
            FormAndShareGroups();
        }
    }

    void FormAndShareGroups()
    {
        List<int> groupIds = new List<int>(); // 同じグループのプレイヤーのIDを保持
        Hashtable allGroups = new Hashtable(); // 全グループ情報

        // 特定のプレイヤーをマスタークライアントとする
        Player masterPlayer = PhotonNetwork.MasterClient;
        Transform masterAvatarTransform = FindAvatarTransformByPlayer(masterPlayer);

        if (masterAvatarTransform != null)
        {
            Vector3 masterPosition = masterAvatarTransform.position;

            // 他のプレイヤーとの距離を計算
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player != masterPlayer)
                {
                    Transform avatarTransform = FindAvatarTransformByPlayer(player);
                    if (avatarTransform != null)
                    {
                        float distance = Vector3.Distance(masterPosition, avatarTransform.position);
                        if (distance <= clusteringDistance)
                        {
                            groupIds.Add(player.ActorNumber); // 距離が閾値以下のプレイヤーをグループに追加
                        }
                    }
                }
            }

            // マスター自身をグループに追加
            groupIds.Add(masterPlayer.ActorNumber);

            // グループ情報をルームプロパティに保存
            allGroups[masterPlayer.ActorNumber] = groupIds.ToArray();
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { GROUP_KEY, allGroups } });

            Debug.Log("【デバッグ】グループ情報をルームカスタムプロパティに保存しました。");
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GROUP_KEY))
        {
            Hashtable allGroups = (Hashtable)propertiesThatChanged[GROUP_KEY];
            if (allGroups.ContainsKey(PhotonNetwork.MasterClient.ActorNumber))
            {
                int[] group = (int[])allGroups[PhotonNetwork.MasterClient.ActorNumber];
                ArrangeInCircle(group);
            }
        }
    }

    void ArrangeInCircle(int[] group)
    {
        List<Transform> groupPlayers = new List<Transform>();

        // グループメンバーを収集し、IDの小さい順にソート
        List<int> sortedGroup = new List<int>(group);
        sortedGroup.Sort();

        foreach (int actorNumber in sortedGroup)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player != null)
            {
                Transform avatarTransform = FindAvatarTransformByPlayer(player);
                if (avatarTransform != null)
                {
                    groupPlayers.Add(avatarTransform);
                }
            }
        }

        // グループの中心をマスターの位置に設定
        Vector3 center = Vector3.zero;
        Transform masterAvatarTransform = FindAvatarTransformByPlayer(PhotonNetwork.MasterClient);
        if (masterAvatarTransform != null)
        {
            center = masterAvatarTransform.position;
        }

        // グループ人数に応じて円の半径を動的に計算
        float dynamicRadius = Mathf.Max(5f, groupPlayers.Count * 1.5f);

        // 円形に配置
        float angleStep = 360f / groupPlayers.Count;
        for (int i = 0; i < groupPlayers.Count; i++)
        {
            // 配置する角度を計算
            float angle = i * angleStep * Mathf.Deg2Rad;

            // 円上の位置を計算
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * dynamicRadius;
            Vector3 targetPosition = center + offset;

            // RPCで他のクライアントの位置を移動
            photonView.RPC("RPCMoveAvatar", RpcTarget.All, groupPlayers[i].GetComponent<PhotonView>().ViewID, targetPosition);

            // プレイヤーを円の中心に向ける
            Vector3 directionToCenter = center - targetPosition;
            groupPlayers[i].rotation = Quaternion.LookRotation(new Vector3(directionToCenter.x, 0, directionToCenter.z));
        }

        Debug.Log("【デバッグ】グループメンバーを円形に配置しました。");
    }

    [PunRPC]
    void RPCMoveAvatar(int viewID, Vector3 targetPosition)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            targetView.transform.position = targetPosition;
        }
    }

    private Transform FindAvatarTransformByPlayer(Player player)
    {
        foreach (PhotonView view in FindObjectsOfType<PhotonView>())
        {
            if (view.Owner != null && view.Owner == player)
            {
                return view.transform;
            }
        }
        return null;
    }
}
