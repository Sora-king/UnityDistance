using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using ExitGames.Client.Photon;
using Photon.Voice.PUN;
using System.Collections;

public class InterestGroupChatManager : MonoBehaviourPunCallbacks
{
    public Recorder recorder;
    public Speaker speaker;
    public VoiceConnection voiceConnection; // VoiceConnectionをInspectorでアタッチ

    private const string GroupsPropertyKey = "Groups"; // グループ情報を格納するカスタムプロパティのキー
    private string currentGroup = null;               // 現在のグループ名

    /// <summary>
    /// グループ設定を更新（送信と受信を設定）
    /// </summary>
    public void UpdateGroupSettings()
    {
        currentGroup = GetMyGroup();

        if (!string.IsNullOrEmpty(currentGroup))
        {
            // 接尾辞のアルファベットを取得してInterestGroupに設定
            byte interestGroup = GetInterestGroupFromSuffix(currentGroup);

            if (recorder != null)
            {
                recorder.InterestGroup = interestGroup;
                Debug.Log($"RecorderのInterestGroupを {interestGroup} に設定しました。（グループ名: {currentGroup}）");
            }
            else
            {
                Debug.LogError("Recorderが設定されていません！");
            }

            // 受信グループを設定
            if (voiceConnection != null)
            {
                voiceConnection.Client.OpChangeGroups(null, new byte[] { interestGroup });
                Debug.Log($"現在の受信対象グループ: {interestGroup}");
            }
            else
            {
                Debug.LogError("VoiceConnectionが設定されていません！");
            }
        }
        else
        {
            Debug.LogWarning("自分の所属するグループが見つかりませんでした。");
        }
    }

    /// <summary>
    /// 自分が所属するグループを取得
    /// </summary>
    /// <returns>グループ名（例: "GroupA"）</returns>
    private string GetMyGroup()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(GroupsPropertyKey))
        {
            Debug.LogError("Groups カスタムプロパティが見つかりません！");
            return null;
        }

        ExitGames.Client.Photon.Hashtable allGroups = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[GroupsPropertyKey];
        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        foreach (DictionaryEntry groupEntry in allGroups)
        {
            string groupName = groupEntry.Key.ToString();
            ExitGames.Client.Photon.Hashtable groupInfo = (ExitGames.Client.Photon.Hashtable)groupEntry.Value;

            if (groupInfo.ContainsKey("Members"))
            {
                int[] members = (int[])groupInfo["Members"];
                if (System.Array.Exists(members, member => member == myActorNumber))
                {
                    Debug.Log($"自分のグループは {groupName} です。");
                    return groupName;
                }
            }
        }

        Debug.LogWarning("自分のグループが見つかりませんでした。");
        return null;
    }

    /// <summary>
    /// グループ名の接尾辞アルファベットからInterestGroupを計算
    /// </summary>
    /// <param name="groupName">グループ名（例: "GroupA"）</param>
    /// <returns>InterestGroup（例: 'A' => 65）</returns>
    private byte GetInterestGroupFromSuffix(string groupName)
    {
        char suffix = groupName[groupName.Length - 1]; // グループ名の最後の文字を取得
        return (byte)suffix; // ASCIIコードをbyteに変換
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GroupsPropertyKey))
        {
            UpdateGroupSettings();
        }
    }
}
