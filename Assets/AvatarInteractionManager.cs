using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Realtime;

public class AvatarInteractionManager : MonoBehaviourPun
{
    private GameObject currentTarget; // 現在押されているアバター
    private Renderer targetRenderer;
    private Material originalMaterial;
    public Material outlineAuraMaterial; // アウトライン＆オーラ用のマテリアル

    private PhotonVoiceView voiceView; // Photon Voiceでの音声制御

    void Update()
    {
        HandleAvatarSelection();
    }

    private void HandleAvatarSelection()
    {
        if (Input.GetMouseButtonDown(0)) // 左クリックで押し始める
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject.GetComponent<PhotonView>())
                {
                    SetTargetAvatar(hit.collider.gameObject);
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) // 左クリックを離したとき
        {
            ClearTargetAvatar();
        }
    }

    private void SetTargetAvatar(GameObject target)
    {
        ClearTargetAvatar(); // 既存のターゲットをクリア

        currentTarget = target;
        targetRenderer = currentTarget.GetComponent<Renderer>();

        if (targetRenderer != null)
        {
            // 元のマテリアルを保存してアウトラインマテリアルを適用
            originalMaterial = targetRenderer.material;
            targetRenderer.material = outlineAuraMaterial;

            Debug.Log("【デバッグ】ターゲットのアウトラインとオーラを有効化しました。");
        }

        // 音声の制御：ターゲットのみ音声を聞けるようにする
        voiceView = currentTarget.GetComponent<PhotonVoiceView>();
        if (voiceView != null)
        {
            voiceView.SpeakerInUse.gameObject.SetActive(true);
            Debug.Log("【デバッグ】音声制御を設定しました。");
        }
    }

    private void ClearTargetAvatar()
    {
        if (currentTarget != null && targetRenderer != null)
        {
            // マテリアルを元に戻す
            targetRenderer.material = originalMaterial;
            Debug.Log("【デバッグ】ターゲットのアウトラインとオーラを解除しました。");
        }

        // 音声制御の解除
        if (voiceView != null)
        {
            voiceView.SpeakerInUse.gameObject.SetActive(false);
        }

        currentTarget = null;
        targetRenderer = null;
        voiceView = null;
    }
}
