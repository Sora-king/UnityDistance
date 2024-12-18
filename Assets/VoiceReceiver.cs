using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using ExitGames.Client.Photon; // Hashtableのための名前空間
using System.Collections.Generic;

public class VoiceReceiver : MonoBehaviour
{
    private Dictionary<int, int> senderTargetMap = new Dictionary<int, int>(); // 送信者とターゲットのマッピング

    void Start()
    {
        // イベントリスナーの登録
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 200) // ターゲット指定イベント
        {
            // CustomDataをHashtableとして受け取る
            Hashtable data = photonEvent.CustomData as Hashtable;
            if (data != null && data.ContainsKey("senderId") && data.ContainsKey("targetId"))
            {
                int senderId = (int)data["senderId"];
                int targetId = (int)data["targetId"];

                senderTargetMap[senderId] = targetId; // ローカルでマッピング更新
                UpdateVoiceFilters();
            }
        }
        else if (photonEvent.Code == 201) // ターゲット解除イベント
        {
            // CustomDataは送信者ID（int）
            if (photonEvent.CustomData is int senderId)
            {
                if (senderTargetMap.ContainsKey(senderId))
                {
                    senderTargetMap.Remove(senderId);
                    UpdateVoiceFilters();
                }
            }
        }
    }

    private void UpdateVoiceFilters()
    {
        foreach (var speaker in FindObjectsOfType<Speaker>())
        {
            PhotonView photonView = speaker.GetComponent<PhotonView>();
            if (photonView != null)
            {
                int senderId = photonView.Owner.ActorNumber; // 送信者IDを取得

                if (senderTargetMap.ContainsKey(senderId) &&
                    senderTargetMap[senderId] != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // ターゲットでない場合はミュート
                    speaker.enabled = false;
                }
                else
                {
                    // ターゲットまたは制限なしの場合は再生
                    speaker.enabled = true;
                }
            }
        }
    }

    void OnDestroy()
    {
        // イベントリスナーの解除
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
}
