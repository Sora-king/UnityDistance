using UnityEngine;
using Photon.Pun;

public class SetTrigger : MonoBehaviour
{
    private PhotonView photonView;
    void Start()
    {
        // BoxColliderを取得
        Collider boxCollider = GetComponent<Collider>();
        photonView = GetComponent<PhotonView>();

        if (photonView.Owner.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            // isTriggerをtrueに設定
            boxCollider.isTrigger = true;
            Debug.Log("isTriggerをtrueに設定しました！");
        }
        else
        {
            Debug.LogWarning("BoxColliderがこのオブジェクトにアタッチされていません！");
        }
    }
}