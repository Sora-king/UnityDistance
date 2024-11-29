using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using Photon.Voice.Unity;

public class Directiontest : MonoBehaviourPunCallbacks
{
    public Transform myAvatar; // 自分のアバターの Transform
    public float angleThreshold = 30f; // 正面方向の角度閾値（±30度）
    public float defaultVolume = 0.1f; // 正面以外の音量
    public float maxVolume = 1.0f; // 正面方向の音量

    void Start()
    {
        // 自分のアバターを探して設定
        StartCoroutine(WaitForMyAvatar());
    }

    private IEnumerator WaitForMyAvatar()
    {
        while (myAvatar == null)
        {
            foreach (PhotonView view in FindObjectsOfType<PhotonView>())
            {
                if (view.IsMine)
                {
                    myAvatar = view.transform;
                    Debug.Log("【デバッグ】自分のアバターを設定しました。");
                    break;
                }
            }
            yield return null; // 次のフレームまで待機
        }
    }

    void Update()
    {
        if (myAvatar == null)
        {
            Debug.Log("【デバッグ】自分のアバターがまだ設定されていません。");
            return;
        }

        // 他のプレイヤーの音量を毎フレーム調整
        AdjustVolumesForAllPlayers();
    }

    private void AdjustVolumesForAllPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == PhotonNetwork.LocalPlayer) continue; // 自分自身はスキップ

            // プレイヤーに紐付くアバターを取得
            GameObject targetAvatar = FindAvatarByPlayer(player);
            if (targetAvatar != null)
            {
                AdjustVolume(targetAvatar.transform);
            }
        }
    }

    private GameObject FindAvatarByPlayer(Player player)
    {
        foreach (PhotonView view in FindObjectsOfType<PhotonView>())
        {
            if (view.Owner != null && view.Owner == player)
            {
                return view.gameObject; // 該当するアバターを返す
            }
        }
        return null; // アバターが見つからなかった場合
    }

    private void AdjustVolume(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("【デバッグ】ターゲットがnullのため音量調整をスキップしました。");
            return;
        }

        if (myAvatar == null)
        {
            Debug.LogWarning("【デバッグ】自分のアバターがnullのため音量調整をスキップしました。");
            return;
        }

        // 自分の位置とターゲットの位置を取得
        Vector3 directionToTarget = target.position - myAvatar.position;

        // 水平面での方向ベクトルを計算（高さ成分を無視）
        directionToTarget.y = 0; // y成分を無視することで水平方向のみを考慮
        directionToTarget.Normalize(); // 正規化

        // 自分のアバターの正面方向（水平成分のみ）
        Vector3 forwardDirection = myAvatar.forward;
        forwardDirection.y = 0; // y成分を無視
        forwardDirection.Normalize();

        // 水平面での角度を計算
        float angle = Vector3.Angle(forwardDirection, directionToTarget);

        // Speaker に関連付けられた AudioSource を取得
        Speaker speaker = target.GetComponent<Speaker>();
        if (speaker != null)
        {
            AudioSource audioSource = speaker.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.volume = angle <= angleThreshold ? maxVolume : defaultVolume;
                Debug.Log($"【デバッグ】AudioSourceの音量を変更しました: {audioSource.volume}");
            }
            else
            {
                Debug.LogError("【デバッグ】AudioSourceがSpeakerにアタッチされていません。");
            }
        }
        else
        {
            Debug.LogError("【デバッグ】Speakerコンポーネントがターゲットにアタッチされていません。");
        }
    }
}
