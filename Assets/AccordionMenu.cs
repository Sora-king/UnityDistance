using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;              // PhotonNetworkの名前空間
using Photon.Realtime;         // Photon Realtimeの名前空間
using System.Collections.Generic;
using ExitGames.Client.Photon;
using TMPro; // TextMeshProを使用

public class AccordionMenu : MonoBehaviourPunCallbacks
{
    public GameObject menuPanel;          // アコーディオンメニューのパネル
    public Button menuButton;             // メニューボタン
    public GameObject buttonPrefab;       // ボタンのプレハブ

    public GameObject scrollView;

    private bool isMenuOpen = false;

    // ボタンのラベル
    private string[] groupNotCreatedLabels = new string[] {
        "名前入力ボタン", "アナウンスボタン", "テーマボタン"
    };

    private string[] groupCreatedLabels = new string[] {
        "名前入力ボタン", "アナウンスボタン", "チャットボタン", "結論ボタン"
    };

    void Start()
    {
        // メニューボタンにクリックイベントを設定
        menuButton.onClick.AddListener(ToggleMenu);
        scrollView.SetActive(false);
    }

    // メニューを開閉する
    void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        // メニューを閉じる際にもボタンを削除
        if (!isMenuOpen)
        {
            ClearMenuButtons();
        }

        menuPanel.SetActive(isMenuOpen); // メニューの表示・非表示

        if (isMenuOpen)
        {
            PopulateButtons(); // ボタンの生成
        }
    }

    // ボタンを動的に生成してメニューに配置する
    void PopulateButtons()
    {
        // メニューの中身をクリア
        ClearMenuButtons();

        // グループの作成状況に応じてボタンを設定
        bool isGroupCreated = IsGroupCreated();
        string[] buttonLabels = isGroupCreated ? groupCreatedLabels : groupNotCreatedLabels;

        // デバッグ出力: グループ作成状況
        Debug.Log($"グループ生成状況: {(isGroupCreated ? "生成済み" : "未生成")}");

        // ボタンを動的に生成
        foreach (string label in buttonLabels)
        {
            GameObject newButton = Instantiate(buttonPrefab, menuPanel.transform);
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = label; // ボタンのラベル設定

            // 各ボタンに対応するスクリプトをアタッチ
            Button button = newButton.GetComponent<Button>();

            // 名前入力ボタンの処理
            if (label == "名前入力ボタン")
            {
                var nameInputScript = newButton.AddComponent<NameInputButton>();
                button.onClick.AddListener(nameInputScript.OnClick); // メソッドをonClickに追加
            }
            // アナウンスボタンの処理
            else if (label == "アナウンスボタン")
            {
                var announcementScript = newButton.AddComponent<AnnouncementButton>();
                button.onClick.AddListener(announcementScript.OnClick); // メソッドをonClickに追加
            }
            // テーマボタンの処理
            else if (label == "テーマボタン")
            {
                var themeScript = newButton.AddComponent<ThemeButton>();
                button.onClick.AddListener(themeScript.OnClick);
            }
            // チャットボタンの処理
            else if (label == "チャットボタン")
            {
                var chatScript = newButton.AddComponent<ChatButton>();
                button.onClick.AddListener(chatScript.OnClick); // メソッドをonClickに追加
            }
            // 結論ボタンの処理
            else if (label == "結論ボタン")
            {
                var conclusionScript = newButton.AddComponent<ConclusionButton>();
                button.onClick.AddListener(conclusionScript.OnClick); // メソッドをonClickに追加
            }
        }
    }

    // メニューの中身を削除する
    void ClearMenuButtons()
    {
        foreach (Transform child in menuPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // グループの作成状況を判定する
    private bool IsGroupCreated()
    {
        // 例: Photonのカスタムプロパティを使用してグループ作成状況を確認
        Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        return currentProperties.ContainsKey("Groups") && currentProperties["Groups"] != null;
    }


    // カスタムプロパティが更新されたときに呼ばれる
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Groups"))
        {
            Debug.Log("カスタムプロパティが更新されました: 再生成を実行します");
            
            // メニューが開いている場合のみボタンを再生成
            if (isMenuOpen)
            {
                PopulateButtons();
            }
        }
    }
}
