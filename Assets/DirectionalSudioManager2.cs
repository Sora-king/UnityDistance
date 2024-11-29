using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using System.Collections; // IEnumerator 用
using Photon.Voice.Unity;

public class DirectionalAudioManager2 : MonoBehaviourPunCallbacks
{
    public Transform myAvatar; // 自分のアバターの Transform
    public float angleThreshold = 30f; // 正面方向の角度閾値（±30度）
    public float defaultVolume = 0.1f; // 正面以外の音量
    public float maxVolume = 1.0f; // 正面方向の音量

    private bool isPressed = false;

    private Dictionary<int, Transform> avatarDictionary = new Dictionary<int, Transform>(); // ViewIDでアバターを管理

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

        foreach (var entry in avatarDictionary)
        {
            ////Debug.Log($"【デバッグ】音量調整対象のアバター: {entry.Value.name}");
                // ボタンを押した瞬間（1回だけ処理）
                if (Input.GetKeyDown(KeyCode.Y) && !isPressed)
                {
                    Debug.Log("Yボタンが押されました（1回だけ実行）");
                    isPressed = true; // フラグを立てて再入力を防ぐ
                    UpdateAvatarList();
                }

                // ボタンを離した瞬間にフラグをリセット
                if (Input.GetKeyUp(KeyCode.Y))
                {
                    isPressed = false; // フラグをリセット
                }

            AdjustVolume(entry.Value);
        }
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

        // 自分のアバターからターゲットへの方向ベクトルを計算
        Vector3 directionToTarget = (target.position - myAvatar.position).normalized;
        float angle = Vector3.Angle(myAvatar.forward, directionToTarget);

        ////Debug.Log($"【デバッグ】アバター間の角度: {angle} 度");

        // Speaker に関連付けられた AudioSource を取得
        Speaker speaker = target.GetComponent<Speaker>();
        if (speaker != null)
        {
            AudioSource audioSource = speaker.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.volume = angle <= angleThreshold ? maxVolume : defaultVolume;
                ////Debug.Log($"【デバッグ】AudioSourceの音量を変更しました: {audioSource.volume}");
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

    public override void OnJoinedRoom()
    {
        UpdateAvatarList();
        Debug.Log("【デバッグ】ルームに参加しました。アバターリストを更新します。");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateAvatarList();
        Debug.Log($"【デバッグ】新しいプレイヤーがルームに参加しました: {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemoveAvatarFromList(otherPlayer);
        Debug.Log($"【デバッグ】プレイヤーがルームを退出しました: {otherPlayer.NickName}");
    }

    private void UpdateAvatarList()
    {
        avatarDictionary.Clear();

        foreach (PhotonView view in FindObjectsOfType<PhotonView>())
        {
            if (view.Owner != null)
            {
                avatarDictionary[view.ViewID] = view.transform;
                Debug.Log($"【デバッグ】アバターをリストに追加しました: ViewID={view.ViewID}, Name={view.name}");
            }
        }

        Debug.Log($"【デバッグ】アバターリストを更新しました。現在のアバター数: {avatarDictionary.Count}");
    }

    private void RemoveAvatarFromList(Player otherPlayer)
    {
        List<int> idsToRemove = new List<int>();

        foreach (var kvp in avatarDictionary)
        {
            PhotonView view = PhotonView.Find(kvp.Key);
            if (view != null && view.Owner == otherPlayer)
            {
                idsToRemove.Add(kvp.Key);
            }
        }

        foreach (int id in idsToRemove)
        {
            avatarDictionary.Remove(id);
            Debug.Log($"【デバッグ】アバターリストから削除しました: ViewID={id}");
        }
    }
}