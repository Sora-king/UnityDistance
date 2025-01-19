using UnityEngine;
using Photon.Pun; // PUNを使う場合
using System.Linq;
using System.Collections.Generic;


public class AnnouncementButton : MonoBehaviourPun
{
    bool is3D = true;
    PhotonView photonView;
    [SerializeField] private Directiontest directiontest;

    private void Start()
    {
        photonView = GetLocalPlayerPhotonView();
    }
    public void OnClick()
    {
        Debug.Log("アナウンスボタンがクリックされました");

        is3D = !is3D;
        photonView.RPC(nameof(directiontest.SetSpatialBlend), RpcTarget.All, is3D);

        UpdateLocalPlayerInCustomProperties(is3D);

    }

    private PhotonView GetLocalPlayerPhotonView()
    {
        PhotonView[] allPhotonViews = FindObjectsOfType<PhotonView>();

        foreach (PhotonView view in allPhotonViews)
        {
            if (view.Owner == PhotonNetwork.LocalPlayer)
            {
                return view; // ローカルプレイヤーに関連付けられた PhotonView を返す
            }
        }

        Debug.LogWarning("LocalPlayer に関連付けられた PhotonView が見つかりませんでした。");
        return null;
    }
    public void UpdateLocalPlayerInCustomProperties(bool is3D)
    {
        // カスタムプロパティのキー
        string key = "announceplayer";

        // LocalPlayer の ActorNumber を取得
        int localPlayerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // カスタムプロパティから現在のリストを取得
        int[] playerActorNumbers = new int[0];
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
        {
            playerActorNumbers = (int[])PhotonNetwork.CurrentRoom.CustomProperties[key];
        }

        // 現在のリストを List<int> に変換
        List<int> actorNumberList = playerActorNumbers.ToList();

        if (is3D)
        {
            // LocalPlayer がリストに存在しない場合は追加
            if (!actorNumberList.Contains(localPlayerActorNumber))
            {
                actorNumberList.Add(localPlayerActorNumber);
                Debug.Log($"LocalPlayer {localPlayerActorNumber} をリストに追加しました。");
            }
        }
        else
        {
            // LocalPlayer がリストに存在する場合は削除
            if (actorNumberList.Contains(localPlayerActorNumber))
            {
                actorNumberList.Remove(localPlayerActorNumber);
                Debug.Log($"LocalPlayer {localPlayerActorNumber} をリストから削除しました。");
            }
        }

        // リストを配列に戻してカスタムプロパティに保存
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { key, actorNumberList.ToArray() }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);

        Debug.Log("カスタムプロパティを更新しました。");
    }

}
