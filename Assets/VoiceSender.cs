using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // SendOptionsのための名前空間

public class VoiceSender : MonoBehaviour
{
    private int targetClientId = -1; // 現在のターゲットID

    void OnMouseDown()
    {
        int clickedClientId = GetComponent<PhotonView>().Owner.ActorNumber;

        // ターゲットをロック
        if (TargetManager.LockTarget(PhotonNetwork.LocalPlayer.ActorNumber, clickedClientId))
        {
            targetClientId = clickedClientId;

            // データをHashtableで作成
            Hashtable eventData = new Hashtable
            {
                { "senderId", PhotonNetwork.LocalPlayer.ActorNumber },
                { "targetId", targetClientId }
            };

            // ターゲット指定の通知を全員に送信
            PhotonNetwork.RaiseEvent(
                200, // カスタムイベントコード
                eventData,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );

            Debug.Log($"ターゲット {targetClientId} をロックしました。");
        }
        else
        {
            Debug.Log($"ターゲット {clickedClientId} は既に他の送信者がロック中です。");
        }
    }

    void OnMouseUp()
    {
        if (targetClientId != -1)
        {
            // ターゲットロックを解除
            TargetManager.ReleaseTarget(targetClientId);

            // ロック解除通知を全員に送信
            PhotonNetwork.RaiseEvent(
                201, // カスタムイベントコード（ロック解除）
                targetClientId,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );

            Debug.Log($"ターゲット {targetClientId} のロックを解除しました。");
            targetClientId = -1;
        }
    }
}
