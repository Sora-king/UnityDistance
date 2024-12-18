using UnityEngine;
using Photon.Pun;

public class AvatarShaderController : MonoBehaviour
{
    private Material material; // アバターのマテリアル
    private Renderer renderer;

    private float defaultOutlineWidth; // デフォルトのアウトライン幅
    private Color defaultAuraColor;    // デフォルトのオーラ色

    private PhotonView photonView; // 自分自身を判定するためのPhotonView

    void Start()
    {
        renderer = GetComponent<Renderer>();
        
        // Rendererが見つからない場合
        if (renderer == null)
        {
            Debug.LogError("Rendererが見つかりません。");
            return;
        }

        material = renderer.material; // マテリアルの参照を取得
        
        // Materialがnullの場合
        if (material == null)
        {
            Debug.LogError("Materialが設定されていません。");
            return;
        }

        photonView = GetComponent<PhotonView>();

        // デフォルト値を取得
        defaultOutlineWidth = material.GetFloat("_OutlineWidth");
        defaultAuraColor = material.GetColor("_AuraColor");
    }

    void OnMouseEnter()
    {
        if (photonView != null && photonView.IsMine) return;

        if (material != null)
        {
            material.SetFloat("_OutlineWidth", 0.066f);
        }
    }

    void OnMouseExit()
    {
        if (photonView != null && photonView.IsMine) return;

        if (material != null)
        {
            material.SetFloat("_OutlineWidth", defaultOutlineWidth);
        }
    }

    void OnMouseDown()
    {
        if (photonView != null && photonView.IsMine) return;

        if (material != null)
        {
            Color auraColor = defaultAuraColor;
            auraColor.a = 94 / 255f; // 透明度を94に設定
            material.SetColor("_AuraColor", auraColor);
        }
    }

    void OnMouseUp()
    {
        if (photonView != null && photonView.IsMine) return;

        if (material != null)
        {
            Color auraColor = defaultAuraColor;
            auraColor.a = 0; // 透明度を0に戻す
            material.SetColor("_AuraColor", auraColor);
        }
    }
}
