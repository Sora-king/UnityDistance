using UnityEngine;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections;

public class InterestGroupChatManager : MonoBehaviourPunCallbacks
{
    public Recorder recorder; // Recorder を Inspector で設定
    public PunVoiceClient punVoiceClient; // PunVoiceClient を Inspector で設定

    private const string GroupsPropertyKey = "Groups"; // グループ情報を格納するカスタムプロパティのキー
    private string currentGroup = null; // 現在のグループ名

    void Start()
    {
        // PunVoiceClient を動的に取得
        punVoiceClient = FindObjectOfType<PunVoiceClient>();

        if (punVoiceClient == null)
        {
            Debug.LogError("PunVoiceClient がシーン内に見つかりません！");
            return;
        }

        // Recorder が初期化されるまで待機
        StartCoroutine(WaitForRecorderCoroutine());
    }

    /// <summary>
    /// Recorder が初期化されるまで待機するコルーチン
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitForRecorderCoroutine()
    {
        Debug.Log("Recorder を取得中...");

        while (recorder == null)
        {
            recorder = punVoiceClient.PrimaryRecorder;

            if (recorder == null)
            {
                Debug.LogWarning("Recorder がまだ初期化されていません。再試行します...");
                yield return new WaitForSeconds(0.5f);
            }
        }

        Debug.Log("Recorder を取得しました！");
    }

    public void SetInterestGroup(bool is3D)
    {
        if (string.IsNullOrEmpty(currentGroup)) return;

        if (recorder == null)
        {
            Debug.LogError("Recorder が設定されていません。");
            return;
        }

        if (!is3D)
        {
            recorder.InterestGroup = 0; // 全員送信
            Debug.Log("InterestGroup を全員送信 (0) に設定しました。");
        }
        else
        {
            byte interestGroup = GetInterestGroupFromSuffix(currentGroup);
            recorder.InterestGroup = interestGroup; // 指定グループに送信
            Debug.Log($"InterestGroup をグループ {interestGroup} に設定しました。");
        }
    }

    /// <summary>
    /// グループ設定を更新（送信と受信の InterestGroup を設定）
    /// </summary>
    public void UpdateGroupSettings()
    {
        currentGroup = GetMyGroup();

        if (!string.IsNullOrEmpty(currentGroup))
        {
            // グループ名から InterestGroup を計算
            byte interestGroup = GetInterestGroupFromSuffix(currentGroup);

            // 音声送信グループを設定
            recorder.InterestGroup = interestGroup;
            Debug.Log($"Recorder の InterestGroup を {interestGroup} に設定しました。（グループ名: {currentGroup}）");

            // 音声受信グループを設定
            punVoiceClient.Client.OpChangeGroups(null, new byte[] { interestGroup }); // 他のグループを解除して、このグループのみを有効化
            Debug.Log($"PunVoiceClient の受信対象グループを {interestGroup} に設定しました。");
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

        var allGroups = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[GroupsPropertyKey];
        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        foreach (DictionaryEntry groupEntry in allGroups)
        {
            string groupName = groupEntry.Key.ToString();
            var groupInfo = (ExitGames.Client.Photon.Hashtable)groupEntry.Value;

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
    /// グループ名の接尾辞アルファベットから InterestGroup を計算
    /// </summary>
    /// <param name="groupName">グループ名（例: "GroupA"）</param>
    /// <returns>InterestGroup（例: 'A' => 65）</returns>
    private byte GetInterestGroupFromSuffix(string groupName)
    {
        char suffix = groupName[groupName.Length - 1]; // グループ名の最後の文字を取得
        return (byte)suffix; // ASCIIコードを byte に変換
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GroupsPropertyKey))
        {
            UpdateGroupSettings(); // プロパティ更新時にグループ設定を再適用
        }
    }
}