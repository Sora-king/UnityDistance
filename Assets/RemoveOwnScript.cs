using Photon.Pun;
using UnityEngine;

public class RemoveOwnScript : MonoBehaviour
{
    private PhotonView photonView;

    void Start()
    {
        // PhotonViewコンポーネントを取得
        photonView = GetComponent<PhotonView>();

        // 自分のオブジェクトか判定
        if (photonView.IsMine)
        {
            Debug.Log("自分のオブジェクトです。スクリプトを削除します。");
            
            // 削除したいスクリプトを取得
            var targetScript = GetComponent<AvatarVoiceTarget>();
            
            // スクリプトが存在すれば削除
            if (targetScript != null)
            {
                Destroy(targetScript);
                Debug.Log("スクリプトが削除されました。");
            }
            else
            {
                Debug.Log("削除するスクリプトが見つかりません。");
            }
        }
        else
        {
            Debug.Log("他人のオブジェクトなのでスクリプトを削除しません。");
        }
    }
}
