using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Voice.Unity;

public class HandRaiseSpeakerManager : MonoBehaviourPunCallbacks
{
    private const string HAND_RAISE_KEY = "HandRaiseList"; // 手を挙げたリストのルームカスタムプロパティのキー
    private float speakingDuration = 0f; // 現在の発話時間
    private const float SPEAKING_THRESHOLD = 1f; // 発話時間の閾値（秒）

    void Update()
    {
        // ルームが存在するか確認
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogWarning("【警告】ルームが存在しません。");
            return;
        }

        // 手を挙げたリストを取得
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(HAND_RAISE_KEY, out object handRaiseObj))
        {
            int[] handRaiseList = (int[])handRaiseObj;

            if (handRaiseList.Length > 0)
            {
                // 最先頭のプレイヤーIDを取得
                int firstSpeakerId = handRaiseList[0];
                Player firstSpeaker = PhotonNetwork.CurrentRoom.GetPlayer(firstSpeakerId);

                if (firstSpeaker != null)
                {
                    CheckSpeaker(firstSpeaker);
                }
                else
                {
                    Debug.LogWarning($"【警告】プレイヤーID {firstSpeakerId} が見つかりません。");
                }
            }
        }
        else
        {
            Debug.Log("【デバッグ】手を挙げたリストが存在しません。");
        }
    }

    private void CheckSpeaker(Player speaker)
    {
        if (speaker.TagObject is GameObject avatar)
        {
            Recorder recorder = avatar.GetComponent<Recorder>();

            if (recorder != null && recorder.IsCurrentlyTransmitting)
            {
                // マイク入力がある場合、発話時間を加算
                speakingDuration += Time.deltaTime;

                if (speakingDuration >= SPEAKING_THRESHOLD)
                {
                     Debug.Log("おおおおおおおおお");
                    RemoveFirstSpeakerFromList();
                    speakingDuration = 0f; // 発話時間をリセット
                }
            }
            else
            {
                // マイク入力がない場合、発話時間をリセット
                speakingDuration = 0f;
            }
        }
    }

    private void RemoveFirstSpeakerFromList()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(HAND_RAISE_KEY, out object handRaiseObj))
        {
            int[] handRaiseList = (int[])handRaiseObj;

            if (handRaiseList.Length > 0)
            {
                // 最先頭のプレイヤーをリストから削除
                int[] updatedList = new int[handRaiseList.Length - 1];
                System.Array.Copy(handRaiseList, 1, updatedList, 0, updatedList.Length);

                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { HAND_RAISE_KEY, updatedList } });

                Debug.Log($"【デバッグ】リストから削除されました: ID={handRaiseList[0]}");
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(HAND_RAISE_KEY))
        {
            int[] handRaiseList = (int[])propertiesThatChanged[HAND_RAISE_KEY];
            Debug.Log($"【デバッグ】更新された手を挙げているプレイヤー一覧: {string.Join(", ", handRaiseList)}");
        }
    }
}
