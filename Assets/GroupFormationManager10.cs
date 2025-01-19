using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class GroupFormationManager10 : MonoBehaviourPunCallbacks
{
    public float baseRadius = 5f; // ベースとなる円の半径
    public float groupSpacing = 15f; // グループ間の間隔
    private const string GROUP_KEY = "Groups";

    //private string mygroup;
   // private bool myflag = false;
    public void FormGroups(List<int> photonIds, int groupSize)
    {
        if (photonIds == null || photonIds.Count == 0 || groupSize <= 0)
        {
            Debug.LogError("無効なパラメータが指定されました。");
            return;
        }

        Hashtable allGroups = new Hashtable();
        List<Color> assignedColors = new List<Color>();
        List<Vector3> groupCenters = new List<Vector3>();

        int totalPlayers = photonIds.Count;
        int groupCount = Mathf.CeilToInt((float)totalPlayers / groupSize); // グループ数
        int remainder = totalPlayers % groupSize; // 余り

        // グループのメンバー数を計算
        List<int> groupSizes = new List<int>();
        for (int i = 0; i < groupCount; i++)
        {
            groupSizes.Add(i < remainder ? groupSize + 1 : groupSize); // 余りを先に割り振る
        }

        // 各グループの処理
        for (int i = 0; i < groupCount; i++)
        {
            List<int> groupMembers = new List<int>();
            for (int j = 0; j < groupSizes[i] && photonIds.Count > 0; j++)
            {
                /*if(photonIds[j] == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    myflag = true;
                    mygroup = "Group" + (char)('A' + i);
                    Debug.Log("My GroupNameを設定しました");
                }*/

                groupMembers.Add(photonIds[j]);
                photonIds.RemoveAt(j);
            }

            // グループの中心位置を計算
            Vector3 center = new Vector3(i * groupSpacing, 0, 0);

            // 色を決定
            Color groupColor = GenerateUniqueColor(assignedColors);
            assignedColors.Add(groupColor);
            /*if(myflag)
            {
                UIUpdater UIUpdater = FindObjectOfType<UIUpdater>();
                if (UIUpdater != null)
                {
                    UIUpdater.UpdateUI(groupColor, mygroup);
                }
                else
                {
                    Debug.LogError("UIUpdaterが見つかりませんでした。");
                }

                myflag = false;

            }*/

            // 半径をメンバー数に基づいて決定
            float radius = baseRadius + groupMembers.Count;

            // グループ情報を作成
            Hashtable groupInfo = new Hashtable
            {
                { "Members", groupMembers.ToArray() },
                { "Center", new float[] { center.x, center.y, center.z } },
                { "Radius", radius },
                { "Color", new float[] { groupColor.r, groupColor.g, groupColor.b } }
            };

            // 全グループ情報に追加
            allGroups.Add("Group" + (char)('A' + i), groupInfo);

            // グループメンバーを配置
            ArrangeGroupMembersInCircle(groupMembers, center, groupColor);
        }

        // グループ情報をルームカスタムプロパティに設定
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { GROUP_KEY, allGroups } });
    }


    private void ArrangeGroupMembersInCircle(List<int> groupMembers, Vector3 center, Color groupColor)
    {
        float angleStep = 360f / groupMembers.Count;
        for (int i = 0; i < groupMembers.Count; i++)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(groupMembers[i]);
            Transform avatarTransform = FindAvatarTransformByPlayer(player);

            if (avatarTransform != null)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 position = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * baseRadius;

                // アバターの位置を設定
                photonView.RPC("RPCMoveAvatar", RpcTarget.All, avatarTransform.GetComponent<PhotonView>().ViewID, position);

                // グループカラーを適用 (仮に色を示す方法を考慮)
                avatarTransform.GetComponent<Renderer>().material.color = groupColor;
            }
        }
    }

    private Color GenerateUniqueColor(List<Color> assignedColors)
    {
        Color newColor;
        do
        {
            newColor = new Color(Random.value, Random.value, Random.value);
        } while (assignedColors.Contains(newColor));
        return newColor;
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
