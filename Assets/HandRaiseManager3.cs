using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using TMPro; // TextMeshProを使用

public class HandRaiseManager3 : MonoBehaviourPunCallbacks
{
    private const string HAND_RAISE_KEY = "HandRaiseList"; // ルームカスタムプロパティのキー
    private const string NAME_KEY_PREFIX = "name_"; // プレイヤー名のキー
    [SerializeField] private Button handRaiseButton; // 手を挙げるボタン
    [SerializeField] private TMP_Text handRaiseDisplay; // 手を挙げた人の名前を表示するテキスト
    [SerializeField] private RectTransform displayBackground; // 表示背景

    void Start()
    {
        // 必須要素が設定されているか確認
        if (handRaiseButton == null || handRaiseDisplay == null || displayBackground == null)
        {
            Debug.LogError("【エラー】必要なUI要素がアサインされていません。");
            return;
        }

        // ボタンのクリックリスナーを登録
        handRaiseButton.onClick.AddListener(OnHandRaiseButtonClicked);

        // 初期化
        handRaiseDisplay.text = string.Empty;
        AdjustDisplayBackgroundSize();
    }

    private void OnHandRaiseButtonClicked()
    {
        Debug.Log("【デバッグ】手を挙げるボタンが押されました。");
        RaiseHand();
    }

    private void RaiseHand()
    {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (!roomProperties.ContainsKey(HAND_RAISE_KEY))
        {
            roomProperties[HAND_RAISE_KEY] = new int[0];
        }

        int[] handRaiseList = (int[])roomProperties[HAND_RAISE_KEY];

        if (!System.Array.Exists(handRaiseList, id => id == PhotonNetwork.LocalPlayer.ActorNumber))
        {
            int[] updatedList = new int[handRaiseList.Length + 1];
            handRaiseList.CopyTo(updatedList, 0);
            updatedList[handRaiseList.Length] = PhotonNetwork.LocalPlayer.ActorNumber;

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { HAND_RAISE_KEY, updatedList } });
            Debug.Log($"【デバッグ】手を挙げたプレイヤーをリストに追加しました: {PhotonNetwork.LocalPlayer.ActorNumber}");
        }
        else
        {
            Debug.LogWarning("【デバッグ】既に手を挙げています。");
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(HAND_RAISE_KEY))
        {
            int[] handRaiseList = (int[])propertiesThatChanged[HAND_RAISE_KEY];
            if (handRaiseList != null)
            {
                UpdateHandRaiseDisplay(handRaiseList);
                Debug.Log($"【デバッグ】手を挙げているプレイヤー一覧: {string.Join(", ", handRaiseList)}");
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"【デバッグ】プレイヤーが退出しました: {otherPlayer.ActorNumber}");

        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (roomProperties.ContainsKey(HAND_RAISE_KEY))
        {
            int[] handRaiseList = (int[])roomProperties[HAND_RAISE_KEY];

            // 退出したプレイヤーをリストから削除
            int[] updatedList = System.Array.FindAll(handRaiseList, id => id != otherPlayer.ActorNumber);

            // 変更を反映
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { HAND_RAISE_KEY, updatedList } });

            Debug.Log($"【デバッグ】退出したプレイヤーをリストから削除しました: {otherPlayer.ActorNumber}");
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(NAME_KEY_PREFIX + targetPlayer.ActorNumber))
        {
            Debug.Log($"【デバッグ】プレイヤー {targetPlayer.ActorNumber} の名前が更新されました。再描画を実行します。");

            // 名前変更に応じて再描画
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(HAND_RAISE_KEY))
            {
                int[] handRaiseList = (int[])PhotonNetwork.CurrentRoom.CustomProperties[HAND_RAISE_KEY];
                UpdateHandRaiseDisplay(handRaiseList);
            }
        }
    }

    private void UpdateHandRaiseDisplay(int[] handRaiseList)
    {
        handRaiseDisplay.text = string.Empty;

        foreach (int actorNumber in handRaiseList)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player != null && player.CustomProperties.TryGetValue(NAME_KEY_PREFIX + actorNumber, out object playerName))
            {
                handRaiseDisplay.text += $"{playerName}\n"; // 名前を改行で追加
            }
            else
            {
                handRaiseDisplay.text += $"Unknown Player ({actorNumber})\n"; // 名前が未設定の場合
            }
        }

        AdjustDisplayBackgroundSize();
    }

    private void AdjustDisplayBackgroundSize()
    {
        float preferredHeight = handRaiseDisplay.preferredHeight;
        displayBackground.sizeDelta = new Vector2(displayBackground.sizeDelta.x, preferredHeight + 10f); // マージンを追加
    }
}
