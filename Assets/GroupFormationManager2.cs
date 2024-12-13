using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using Photon.Voice.Unity;


public class GroupFormationManager2 : MonoBehaviourPunCallbacks
{
    private const string GROUP_KEY = "Groups";
    public float clusteringDistance = 10f;

    private int groupCounter = 0; // グループ名のカウンター
    private List<string> groupNames = new List<string>(); // グループ名リスト

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            FormAndShareGroups();
        }
    }

    void FormAndShareGroups()
    {
        List<int> groupIds = new List<int>();
        Hashtable allGroups = new Hashtable();

        Player masterPlayer = PhotonNetwork.MasterClient;
        Transform masterAvatarTransform = FindAvatarTransformByPlayer(masterPlayer);

        if (masterAvatarTransform != null)
        {
            Vector3 masterPosition = masterAvatarTransform.position;

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
                            groupIds.Add(player.ActorNumber);
                        }
                    }
                }
            }

            groupIds.Add(masterPlayer.ActorNumber);

            // グループ名の生成
            string groupName = $"グループ{(char)('A' + groupCounter)}";
            groupCounter++;
            groupNames.Add(groupName);

            allGroups[groupName] = groupIds.ToArray();
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { GROUP_KEY, allGroups } });

            Debug.Log($"【デバッグ】グループ情報を保存しました: {groupName}");
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GROUP_KEY))
        {
            Hashtable allGroups = (Hashtable)propertiesThatChanged[GROUP_KEY];
            foreach (DictionaryEntry entry in allGroups)
            {
                string groupName = (string)entry.Key;
                int[] group = (int[])entry.Value;
                ArrangeInCircle(group, groupName);
            }
        }
    }

    void ArrangeInCircle(int[] group, string groupName)
    {
        List<Transform> groupPlayers = new List<Transform>();

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
                    Debug.Log($"【デバッグ】{player.NickName}のアバターを配置対象に追加しました: {groupName}");
                }
                else
                {
                    Debug.LogWarning($"【デバッグ】{player.NickName}のアバターが見つかりませんでした。");
                }
            }
        }

        Vector3 center = Vector3.zero;
        Transform masterAvatarTransform = FindAvatarTransformByPlayer(PhotonNetwork.MasterClient);
        if (masterAvatarTransform != null)
        {
            center = masterAvatarTransform.position;
        }

        float dynamicRadius = Mathf.Max(5f, groupPlayers.Count * 1.5f);
        float angleStep = 360f / groupPlayers.Count;

        GroupVisualizer visualizer = FindObjectOfType<GroupVisualizer>();

        for (int i = 0; i < groupPlayers.Count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * dynamicRadius;
            Vector3 targetPosition = center + offset;

            photonView.RPC("RPCMoveAvatar", RpcTarget.All, groupPlayers[i].GetComponent<PhotonView>().ViewID, targetPosition);

            Vector3 directionToCenter = center - targetPosition;
            groupPlayers[i].rotation = Quaternion.LookRotation(new Vector3(directionToCenter.x, 0, directionToCenter.z));
        }

        // グループを可視化
        Color groupColor = Random.ColorHSV();
        visualizer.VisualizeGroup(center, dynamicRadius, groupName, groupColor);

        Debug.Log($"【デバッグ】{groupName}を円形に配置しました。");
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
