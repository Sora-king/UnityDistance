using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

public class AvatarVoiceTarget : MonoBehaviour
{
    private Recorder recorder;
    private int targetPlayerId = -1;

    void Start()
    {
        // PunVoiceClient から PrimaryRecorder を取得
        PunVoiceClient punVoiceClient = PunVoiceClient.Instance;

        if (punVoiceClient != null)
        {
            if (punVoiceClient.PrimaryRecorder != null)
            {
                recorder = punVoiceClient.PrimaryRecorder;
            }
            else
            {
                Debug.LogError("PrimaryRecorder が設定されていません。PunVoiceClient を確認してください。");
            }
        }
        else
        {
            Debug.LogError("PunVoiceClient が見つかりません。");
        }
    }

    void OnMouseDown()
    {
        PhotonView photonView = GetComponent<PhotonView>();

        if (photonView != null && recorder != null)
        {
            // ターゲットのプレイヤーIDを取得
            targetPlayerId = photonView.Owner.ActorNumber;
            SetVoiceTarget(targetPlayerId);
            Debug.Log($"ターゲットプレイヤー {targetPlayerId} に音声を送信します。");
        }
    }

    void OnMouseUp()
    {
        ClearVoiceTarget();
        Debug.Log("音声送信先を全員に戻しました。");
    }

    private void SetVoiceTarget(int playerId)
    {
        if (recorder != null)
        {
            if (recorder.TargetPlayers == null || recorder.TargetPlayers.Length == 0)
            {
                // 送信先のプレイヤーIDを設定
                recorder.TargetPlayers = new int[] { playerId };
            }
            else
            {
                Debug.LogWarning("ターゲットはすでに設定されています。");
            }
        }
    }

    private void ClearVoiceTarget()
    {
        if (recorder != null)
        {
            // 送信先をリセットし、全員に送信
            recorder.TargetPlayers = null;
        }
    }
}
