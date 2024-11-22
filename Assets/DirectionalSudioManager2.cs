using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using System.Collections; // IEnumerator 用

public class DirectionalAudioManager2 : MonoBehaviourPunCallbacks
{
    public Transform myAvatar; // 自分のアバターの Transform
    public float angleThreshold = 30f; // 正面方向の角度閾値（±30度）
    public float defaultVolume = 0.75f; // 正面以外の音量
    public float maxVolume = 1.0f; // 正面方向の音量

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
            // 自分のアバターが生成されたら取得
            foreach (PhotonView view in FindObjectsOfType<PhotonView>())
            {
                if (view.IsMine)
                {
                    myAvatar = view.transform;
                    Debug.Log("My Avatar found and set.");
                    break;
                }
            }
            yield return null; // 次のフレームまで待機
        }
    }

    void Update()
    {
        if (myAvatar == null) return; // 自分のアバターが設定されていなければスキップ

        foreach (var entry in avatarDictionary)
        {
            AdjustVolume(entry.Value); // アバターの角度差に基づいて音量を調整
        }
    }

    private void AdjustVolume(Transform target)
    {
        if (target == null || myAvatar == null) return; // 無効なターゲットの場合スキップ

        // 自分のアバターからターゲットへの方向ベクトルを計算
        Vector3 directionToTarget = (target.position - myAvatar.position).normalized;

        // 自分の正面方向とターゲット方向の角度を計算
        float angle = Vector3.Angle(myAvatar.forward, directionToTarget);

        // AudioSource を取得して音量を設定
        AudioSource audioSource = target.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.volume = angle <= angleThreshold ? maxVolume : defaultVolume;
        }
    }

    public override void OnJoinedRoom()
    {
        // 他プレイヤーのアバターを初期化
        UpdateAvatarList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 新しいプレイヤーが入ったときにリストを更新
        UpdateAvatarList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // プレイヤーが退出したときにリストを更新
        Debug.Log($"Player {otherPlayer.NickName} left. Removing their avatar...");
        RemoveAvatarFromList(otherPlayer);
    }

    private void UpdateAvatarList()
    {
        avatarDictionary.Clear(); // 既存のリストをクリア

        foreach (PhotonView view in FindObjectsOfType<PhotonView>())
        {
            if (view.Owner != null && view.Owner != PhotonNetwork.LocalPlayer)
            {
                avatarDictionary[view.ViewID] = view.transform; // 他プレイヤーのアバターをリストに追加
            }
        }

        Debug.Log($"Updated avatar list. Total avatars: {avatarDictionary.Count}");
    }

    private void RemoveAvatarFromList(Player otherPlayer)
    {
        // 他プレイヤーの ViewID を探して削除
        List<int> idsToRemove = new List<int>();

        foreach (var kvp in avatarDictionary)
        {
            PhotonView view = PhotonView.Find(kvp.Key); // ViewIDからPhotonViewを取得
            if (view != null && view.Owner == otherPlayer)
            {
                idsToRemove.Add(kvp.Key); // 削除対象のIDをリストに追加
            }
        }

        // リストから該当するIDを削除
        foreach (int id in idsToRemove)
        {
            avatarDictionary.Remove(id);
            Debug.Log($"Removed avatar with ViewID: {id}");
        }
    }
}
