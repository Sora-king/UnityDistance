using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections; // IEnumerator 用
using System.Collections.Generic;
using System.Linq; // ToList を使用

public class DirectionalAudioManager : MonoBehaviourPunCallbacks
{
    public Transform myAvatar; // 自分のアバター
    public float angleThreshold = 30f; // 音量100%にする角度の範囲（±30度）
    public float defaultVolume = 0.75f; // 正面以外の音量
    public float maxVolume = 1.0f; // 正面方向の音量

    private List<Transform> otherPlayers = new List<Transform>(); // 他プレイヤーのアバターリスト

    void Start()
    {
        // コルーチンを開始して自分のアバターが設定されるまで待機
        StartCoroutine(WaitForMyAvatar());
    }

    private IEnumerator WaitForMyAvatar() // 非ジェネリックな IEnumerator を使用
    {
        while (myAvatar == null) // myAvatar が設定されるまで待機
        {
            if (PhotonNetwork.LocalPlayer.TagObject is GameObject myAvatarObject)
            {
                myAvatar = myAvatarObject.transform; // 自分のアバターを設定
                Debug.Log("My avatar has been set via WaitForMyAvatar.");
                break; // 待機を終了
            }
            yield return null; // 次のフレームまで待機
        }
    }

    void Update()
    {
        if (myAvatar == null) return; // 自分のアバターが設定されていなければスキップ

        foreach (var target in otherPlayers)
        {
            AdjustVolume(target); // 他プレイヤーのアバターに対して音量を調整
        }
    }

    void AdjustVolume(Transform target)
    {
        if (target == null || myAvatar == null) return;

        Vector3 directionToTarget = (target.position - myAvatar.position).normalized;
        float angle = Vector3.Angle(myAvatar.forward, directionToTarget);

        AudioSource audioSource = target.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.volume = angle <= angleThreshold ? maxVolume : defaultVolume;
        }
    }

    public void UpdateAvatarList()
    {
        otherPlayers.Clear();

        var playerListCopy = PhotonNetwork.PlayerListOthers.ToList();

        foreach (Player player in playerListCopy)
        {
            if (player.TagObject is GameObject avatar)
            {
                otherPlayers.Add(avatar.transform);
            }
        }

        Debug.Log($"Updated avatar list using ToList. Other player count: {otherPlayers.Count}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateAvatarList(); // 他プレイヤーリストを更新
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateAvatarList(); // 他プレイヤーリストを更新
    }
}
