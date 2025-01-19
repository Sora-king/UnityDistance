using UnityEngine;
using TMPro; // TextMeshProを使用する場合に必要
using UnityEngine.UI; // UIのBackgroundを操作するために必要
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class UIUpdater : MonoBehaviourPunCallbacks
{
    [SerializeField] private Image background; // 背景用のImageコンポーネント
    [SerializeField] private TextMeshProUGUI text; // TextMeshProのUI用テキスト

    private string mygroup;

    // 色と名前を引数として受け取り、背景色とテキストを変更するメソッド
    public void UpdateUI(Color newColor, string newName)
    {
        if (background != null)
        {
            background.color = newColor; // 背景色を設定
        }
        else
        {
            Debug.LogWarning("Background Image is not assigned!");
        }

        if (text != null)
        {
            text.text = newName; // テキストを設定
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI is not assigned!");
        }
    }

    public string GetMyGroup()
    {
        // カスタムプロパティからグループ情報を取得
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Groups"))
        {
            Debug.LogError("Groups カスタムプロパティが見つかりません！");
            return null;
        }

        ExitGames.Client.Photon.Hashtable allGroups = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties["Groups"];

        // 自分のActorNumberを取得
        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // 自分のグループを検索
        foreach (System.Collections.DictionaryEntry groupEntry in allGroups)
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

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        // カスタムプロパティに "Groups" が含まれている場合
        if (propertiesThatChanged.ContainsKey("Groups"))
        {
            mygroup = GetMyGroup();
            // "Groups" に格納されたすべてのグループ情報を取得
            ExitGames.Client.Photon.Hashtable allGroups = (ExitGames.Client.Photon.Hashtable)propertiesThatChanged["Groups"];

            // 自分のグループ (mygroup) が存在するか確認
            if (allGroups.ContainsKey(mygroup))
            {
                // mygroup の情報を取得
                ExitGames.Client.Photon.Hashtable myGroupInfo = (ExitGames.Client.Photon.Hashtable)allGroups[mygroup];

                // Color 情報が存在するか確認
                if (myGroupInfo.ContainsKey("Color"))
                {
                    // Color を float[] として取得
                    float[] colorArray = (float[])myGroupInfo["Color"];

                    // Unity の Color 型に変換
                    Color groupColor = new Color(colorArray[0], colorArray[1], colorArray[2]);

                    // UI を更新
                    UpdateUI(groupColor, mygroup);
                }
                else
                {
                    Debug.LogWarning($"Color not found in group {mygroup}.");
                }
            }
            else
            {
                Debug.LogWarning($"Group {mygroup} not found in allGroups.");
            }
        }
    }
}
