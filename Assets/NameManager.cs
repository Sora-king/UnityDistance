using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using TMPro; // TextMeshProを使用

public class NameManager : MonoBehaviourPunCallbacks
{
    private const string NAME_KEY_PREFIX = "name_"; // 名前のキーのプレフィックス
    public TMP_InputField nameInputField; // 名前を入力するUI
    public Button saveButton; // 名前を保存するボタン
    public Button showInputButton; // InputFieldを表示するボタン

    void Start()
    {
        // 初期状態ではInputFieldと保存ボタンを非表示
        nameInputField.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);

        // ボタンのクリックイベントを登録
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        showInputButton.onClick.AddListener(OnShowInputButtonClicked);

        // 自分の名前をカスタムプロパティから読み込んで表示
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(NAME_KEY_PREFIX + PhotonNetwork.LocalPlayer.ActorNumber, out object playerName))
        {
            nameInputField.text = playerName.ToString();
        }
    }

    private void OnShowInputButtonClicked()
    {
        // InputFieldと保存ボタンを表示
        nameInputField.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(true);
    }

    private void OnSaveButtonClicked()
    {
        string newName = nameInputField.text;

        if (!string.IsNullOrEmpty(newName))
        {
            // 自分のカスタムプロパティに名前を設定
            Hashtable nameProperty = new Hashtable
            {
                { NAME_KEY_PREFIX + PhotonNetwork.LocalPlayer.ActorNumber, newName }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(nameProperty);
            Debug.Log($"【デバッグ】名前を更新しました: {newName}");

            // InputFieldと保存ボタンを非表示
            nameInputField.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("【デバッグ】名前が空です。入力してください。");
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // 他プレイヤーの名前が変更された場合の処理
        if (changedProps.ContainsKey(NAME_KEY_PREFIX + targetPlayer.ActorNumber))
        {
            string updatedName = changedProps[NAME_KEY_PREFIX + targetPlayer.ActorNumber].ToString();
            Debug.Log($"【デバッグ】プレイヤー {targetPlayer.ActorNumber} の名前が更新されました: {updatedName}");
        }
    }
}
