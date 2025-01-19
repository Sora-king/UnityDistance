using UnityEngine;
using Photon.Pun; // PUNを使う場合

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
}
