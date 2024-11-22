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
            Debug.Log("チェックポイント2");
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

    /*public void UpdateAvatarList()
    {
        otherPlayers.Clear();

        var playerListCopy = PhotonNetwork.PlayerListOthers.ToList();

        foreach (Player player in playerListCopy)
        {
            Debug.Log($"チェックポイント1: Checking TagObject for Player {player.ActorNumber}");

            // プレイヤーの TagObject が設定されているか確認
            if (player.TagObject is GameObject avatar)
            {
                otherPlayers.Add(avatar.transform);
                Debug.Log($"Player {player.ActorNumber}'s avatar added to otherPlayers.");
            }
            else
            {
                Debug.LogWarning($"チェックポイント2: Player {player.ActorNumber} has no TagObject.");
                StartCoroutine(WaitForPlayerTagObject(player)); // TagObject が設定されるまで待機
            }
        }

        Debug.Log($"Updated avatar list. Other player count: {otherPlayers.Count}");
    }*/

    public void UpdateAvatarList()
    {
        otherPlayers.Clear();

        var playerListCopy = PhotonNetwork.PlayerListOthers.ToList();

        foreach (Player player in playerListCopy)
        {
            if (player.TagObject != null)
            {
                Debug.Log($"Player {player.ActorNumber} has TagObject: {player.TagObject}");
            }
            else
            {
                Debug.LogWarning($"Player {player.ActorNumber} has no TagObject.");
            }

            if (player.TagObject is GameObject avatar)
            {
                otherPlayers.Add(avatar.transform);
                Debug.Log($"Player {player.ActorNumber}'s avatar added to otherPlayers.");
            }
        }

        Debug.Log($"Updated avatar list. Other player count: {otherPlayers.Count}");
    }

    private IEnumerator WaitForPlayerTagObject(Player player)
    {
        while (player.TagObject == null)
        {
            Debug.Log($"Waiting for TagObject of Player {player.ActorNumber}");
            yield return null; // 次のフレームまで待機
        }

        if (player.TagObject is GameObject avatar)
        {
            Debug.Log("チェックポイント3");
            otherPlayers.Add(avatar.transform);
            Debug.Log($"Player {player.ActorNumber}'s TagObject has been set and added to otherPlayers.");
        }
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
