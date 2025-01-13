using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using System.Collections.Generic;

public class TryGroup : MonoBehaviourPunCallbacks
{
    private const string QUEUE_KEY = "GroupCreationQueue"; // グループ作成順序を管理するキュー
    private const string ALL_GROUPS_FLAG = "Groups";    // グループ作成済みフラグ

    public override void OnCreatedRoom()
    {
        InitializeGroupFlags();
        Debug.Log("ルーム作成時に初期化しました。");
    }

    // 初期化処理
    public void InitializeGroupFlags()
    {
        Hashtable initialProperties = new Hashtable
        {
            { QUEUE_KEY, null },  // キューをリセット（初期状態では存在しない）
            { ALL_GROUPS_FLAG, null } // グループ作成済みフラグをリセット
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(initialProperties);
    }

    public void TryStartGroupFormation(int nummember)
    {
        bool success = false;

        while (!success)
        {
            Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            List<int> queue = GetQueueFromProperties(currentProperties);

            if (!queue.Contains(PhotonNetwork.LocalPlayer.ActorNumber))
            {
                queue.Add(PhotonNetwork.LocalPlayer.ActorNumber);
                success = UpdateQueueInProperties(queue, currentProperties);

                if (!success)
                {
                    Debug.Log("キューの更新に失敗しました。再試行します...");
                    continue;
                }
            }

            if (queue.Count > 0 && queue[0] == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                StartGroupFormation(queue, nummember);
                break;
            }
            else
            {
                Debug.Log($"キューの先頭でないため、グループ作成を中断します。先頭ID: {queue[0]}");
                return;
            }
        }
    }

    private void StartGroupFormation(List<int> queue, int nummember)
    {
        Debug.Log("グループ作成を開始します...");

        List<int> photonIds = new List<int>();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            photonIds.Add(player.ActorNumber);
        }

        GroupFormationManager10 groupFormationManager10 = FindObjectOfType<GroupFormationManager10>();
        if (groupFormationManager10 != null)
        {
            groupFormationManager10.FormGroups(photonIds, nummember);
        }
        else
        {
            Debug.LogError("GroupFormationManagerが見つかりませんでした。");
        }

        ResetQueue();
    }

    private List<int> GetQueueFromProperties(Hashtable properties)
    {
        try
        {
            if (properties[QUEUE_KEY] != null && properties[QUEUE_KEY] is int[] queueArray)
            {
                return new List<int>(queueArray);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"キューの取得に失敗しました: {ex.Message}");
        }
        return new List<int>();
    }

    private bool UpdateQueueInProperties(List<int> queue, Hashtable currentProperties)
    {
        Hashtable newProperties = new Hashtable { { QUEUE_KEY, queue.ToArray() } };
        return PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties, currentProperties);
    }

    private void ResetQueue()
    {
        Hashtable newProperties = new Hashtable { { QUEUE_KEY, null } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties);
        Debug.Log("キューをリセットしました。");
    }

    public void TryResetGroups()
    {
        Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (currentProperties[QUEUE_KEY] != null)
        {
            Debug.Log("グループ作成中のため、解除はできません。");
            return;
        }

        if (currentProperties[ALL_GROUPS_FLAG] != null)
        {
            Debug.Log("グループを解除します...");
            InitializeGroupFlags();
            Debug.Log("グループが解除されました。");
        }
        else
        {
            Debug.Log("グループは存在しません。");
        }
    }
}
