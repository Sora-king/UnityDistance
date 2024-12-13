using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using TMPro; // TextMeshProを使用

public class HandRaiseManager : MonoBehaviourPunCallbacks
{
    private const string HAND_RAISE_KEY = "HandRaiseList"; // ルームカスタムプロパティのキー
    [SerializeField] private Button handRaiseButton; // 手を挙げるボタン
    [SerializeField] private TMP_Text handRaiseDisplay; // 手を挙げた人のIDを表示するテキスト
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

    private void UpdateHandRaiseDisplay(int[] handRaiseList)
    {
        handRaiseDisplay.text = string.Join("\n", handRaiseList); // IDを改行で表示
        AdjustDisplayBackgroundSize();
    }

    private void AdjustDisplayBackgroundSize()
    {
        float preferredHeight = handRaiseDisplay.preferredHeight;
        displayBackground.sizeDelta = new Vector2(displayBackground.sizeDelta.x, preferredHeight + 10f); // マージンを追加
    }
}
