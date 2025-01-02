using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using TMPro;
using Photon.Voice.Unity;

public class HandRaiseManager45 : MonoBehaviourPunCallbacks
{
    private const string HAND_RAISE_KEY = "HandRaiseList"; // 手を挙げたリストのルームカスタムプロパティのキー
    private const string NAME_KEY_PREFIX = "name_"; // プレイヤー名のキー
    private float speakingDuration = 0f; // 現在の発話時間
    private const float SPEAKING_THRESHOLD = 1f; // 発話時間の閾値（秒）

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

    void Update()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        // 手を挙げたリストを取得
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(HAND_RAISE_KEY, out object handRaiseObj))
        {
            int[] handRaiseList = (int[])handRaiseObj;

            if (handRaiseList.Length > 0)
            {
                // 自分がリストの先頭にいるかを確認
                if (handRaiseList[0] == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    CheckLocalPlayerSpeaking();
                }
            }
        }
    }

    private void OnHandRaiseButtonClicked()
    {
        RaiseHand();
    }

    private void RaiseHand()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(HAND_RAISE_KEY))
        {
            PhotonNetwork.CurrentRoom.CustomProperties[HAND_RAISE_KEY] = new int[0];
        }

        int[] handRaiseList = (int[])PhotonNetwork.CurrentRoom.CustomProperties[HAND_RAISE_KEY];

        if (!System.Array.Exists(handRaiseList, id => id == PhotonNetwork.LocalPlayer.ActorNumber))
        {
            int[] updatedList = new int[handRaiseList.Length + 1];
            handRaiseList.CopyTo(updatedList, 0);
            updatedList[handRaiseList.Length] = PhotonNetwork.LocalPlayer.ActorNumber;

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { HAND_RAISE_KEY, updatedList } });
        }
    }

    private void CheckLocalPlayerSpeaking()
    {
        // 自分のID（ActorNumber）を取得
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;

        // プレイヤーIDに対応するRecorderを取得
        Recorder recorder = GetRecorderByPlayerId(localPlayerId);
        DebugRecorderInfo(recorder);

        if (recorder != null && recorder.IsCurrentlyTransmitting)
        {
            // 自分が発話中の場合、発話時間を加算
            speakingDuration += Time.deltaTime;

            // デバッグ：発話時間を表示
            Debug.Log($"【デバッグ】自分の発話時間: {speakingDuration:F2} 秒");

            if (speakingDuration >= SPEAKING_THRESHOLD)
            {
                Debug.Log("【デバッグ】発話時間の閾値を超えました。リストから削除します。");
                RemoveFirstSpeakerFromList();
                speakingDuration = 0f; // リセット
            }
        }
        else
        {
            // 発話していない場合は発話時間をリセット
            speakingDuration = 0f;
        }
    }

    private Recorder GetRecorderByPlayerId(int playerId)
    {
        // シーン内のすべてのオブジェクトを検索
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // `Recorder`コンポーネントを持つオブジェクトを探す
            Recorder recorder = obj.GetComponent<Recorder>();

            if (recorder != null)
            {
                // `PhotonView`を取得してプレイヤーIDをチェック
                PhotonView photonView = obj.GetComponent<PhotonView>();
                if (photonView != null && photonView.OwnerActorNr == playerId)
                {
                    return recorder; // 一致するRecorderを返す
                }
            }
        }

        // 見つからない場合はnullを返す
        return null;
    }

    private void DebugRecorderInfo(Recorder recorder)
    {
        if (recorder == null)
        {
            Debug.LogWarning("【デバッグ】Recorderが見つかりませんでした。");
            return;
        }

        // Recorderの基本情報をログに出力
        Debug.Log($"【デバッグ】Recorder情報:");
        Debug.Log($"  IsCurrentlyTransmitting: {recorder.IsCurrentlyTransmitting}");
        Debug.Log($"  IsRecording: {recorder.IsCurrentlyTransmitting}");
        Debug.Log($"  PhotonViewID: {recorder.GetComponent<PhotonView>()?.ViewID}");
    }


    private void RemoveFirstSpeakerFromList()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(HAND_RAISE_KEY, out object handRaiseObj))
        {
            int[] handRaiseList = (int[])handRaiseObj;

            if (handRaiseList.Length > 0)
            {
                int[] updatedList = new int[handRaiseList.Length - 1];
                System.Array.Copy(handRaiseList, 1, updatedList, 0, updatedList.Length);

                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { HAND_RAISE_KEY, updatedList } });

                // UI更新
                UpdateHandRaiseDisplay(updatedList);
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
                handRaiseDisplay.text += $"{playerName}\n";
            }
            else
            {
                handRaiseDisplay.text += $"Unknown Player ({actorNumber})\n";
            }
        }

        AdjustDisplayBackgroundSize();
    }

    private void AdjustDisplayBackgroundSize()
    {
        float preferredHeight = handRaiseDisplay.preferredHeight;
        displayBackground.sizeDelta = new Vector2(displayBackground.sizeDelta.x, preferredHeight + 10f);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(HAND_RAISE_KEY))
        {
            int[] handRaiseList = (int[])propertiesThatChanged[HAND_RAISE_KEY];
            UpdateHandRaiseDisplay(handRaiseList);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(HAND_RAISE_KEY, out object handRaiseObj))
        {
            int[] handRaiseList = (int[])handRaiseObj;
            int[] updatedList = System.Array.FindAll(handRaiseList, id => id != otherPlayer.ActorNumber);

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { HAND_RAISE_KEY, updatedList } });
            UpdateHandRaiseDisplay(updatedList);
        }
    }
}
