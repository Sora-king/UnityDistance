using Photon.Pun;
using System.Collections.Generic;
using ExitGames.Client.Photon; // PhotonのHashtableを使うための名前空間

public static class TargetManager
{
    // ターゲットのロックを試行
    public static bool LockTarget(int senderId, int targetId)
    {
        var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        // 既にロックされている場合は拒否
        if (roomProperties.ContainsKey(targetId.ToString()))
        {
            return false;
        }

        // ターゲットをロック
        roomProperties[targetId.ToString()] = senderId;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        return true;
    }

    // ターゲットのロックを解除
    public static void ReleaseTarget(int targetId)
    {
        var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        // ターゲットロックを解除
        if (roomProperties.ContainsKey(targetId.ToString()))
        {
            roomProperties.Remove(targetId.ToString());
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
    }

    // 現在のターゲット状態を取得
    public static Dictionary<string, object> GetTargetStatus()
    {
        Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        // HashtableをDictionary<string, object>に変換
        Dictionary<string, object> targetStatus = new Dictionary<string, object>();

        foreach (System.Collections.DictionaryEntry entry in properties)
        {
            targetStatus.Add(entry.Key.ToString(), entry.Value);
        }

        return targetStatus;
    }
}
