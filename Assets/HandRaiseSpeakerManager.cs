using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Voice.Unity;

public class HandRaiseSpeakerManager : MonoBehaviourPunCallbacks
{
    private const string HAND_RAISE_KEY = "HandRaiseList"; // 手を挙げたリストのルームカスタムプロパティのキー
    private float speakingDuration = 0f; // 現在の発話時間
    private const float SPEAKING_THRESHOLD = 0.5f; // 発話時間の閾値（秒）
    private Recorder localRecorder;
    private int myId = PhotonNetwork.LocalPlayer.ActorNumber;


    void Start()
    {
        localRecorder = FindObjectOfType<Recorder>();

        if (localRecorder == null)
        {
            Debug.LogError("【エラー】Recorderが見つかりませんでした。");
        }
        else
        {
            Debug.Log("【デバッグ】Recorderが正常に取得されました。");
        }
    }

    
    
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
                myId = PhotonNetwork.LocalPlayer.ActorNumber;

                if (firstSpeaker != null && firstSpeakerId == myId)
                {
                    //Debug.Log($"先頭IDと一致します");

                    CheckLocalPlayerSpeaking();
                }
                else
                {
                    Debug.Log($"【デバッグ】自分のID（ActorNumber）: {myId}");
                    Debug.LogWarning($"【警告】プレイヤーID {firstSpeakerId} が見つかりません。");
                }
            }
        }
        else
        {
            //Debug.Log("【デバッグ】手を挙げたリストが存在しません。");
        }
    }

    private void CheckLocalPlayerSpeaking()
    {
        if (localRecorder == null)
        {
            Debug.LogWarning("【警告】Recorderが取得されていません。");
            return;
        }

        if (localRecorder.IsCurrentlyTransmitting)
        {
            speakingDuration += Time.deltaTime;

            Debug.Log($"【デバッグ】自分の発話時間: {speakingDuration:F2} 秒");

            if (speakingDuration >= SPEAKING_THRESHOLD)
            {
                RemoveFirstSpeakerFromList();
                speakingDuration = 0f;
                return;
            }
        }
        else
        {
            speakingDuration = 0f;
        }
    }

/*
    private void CheckSpeaker(Player speaker)
    {
        if (speaker.TagObject is GameObject avatar)
        {
            Recorder recorder = avatar.GetComponent<Recorder>();
            DebugRecorderInfo(recorder);

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
    */

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
        Debug.Log($"  PhotonViewID: {recorder.GetComponent<PhotonView>()?.ViewID}");
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
