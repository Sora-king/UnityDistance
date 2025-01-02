using System.Collections;
using UnityEngine;
using Photon.Pun;

public class VoiceRingEffect4 : MonoBehaviourPun
{
    [Header("リングエフェクト設定")]
    public GameObject ringPrefab; // リングプレハブ
    public float ringSpawnInterval = 0.2f; // リング生成間隔
    public float ringExpansionSpeed = 3f; // リングの拡大速度
    public float ringTravelSpeed = 5f; // リングの移動速度
    public float ringLifetime = 1.5f; // リングが消えるまでの時間
    public float ringStartScale = 0.1f; // 初期サイズ
    public float ringMaxScale = 2.0f; // 最大サイズ

    [Header("生成ポイント")]
    public Transform ringSpawnPoint; // リング生成位置
    public Transform avatarTransform; // アバターのTransform

    [Header("ネットワーク設定")]
    public int avatarID; // このアバターのID

    private bool isGeneratingRings = false; // リング生成中フラグ

    void OnMouseDown()
    {
            // RPCを対象のアバターだけに送信
            Debug.Log($"[RPC送信] StartGeneratingRingsを {photonView.Owner.NickName} (ID: {photonView.Owner.ActorNumber}) に送信");
            //PhotonView local_photonview = GetPhotonViewByPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
            photonView.RPC("StartGeneratingRings", RpcTarget.Others, photonView.Owner.ActorNumber);

    }

    void OnMouseUp()
    {
            // RPCを対象のアバターだけに送信
            Debug.Log($"[RPC送信] StopGeneratingRingsを {photonView.Owner.NickName} (ID: {photonView.Owner.ActorNumber}) に送信");
            photonView.RPC("StopGeneratingRings", RpcTarget.Others, photonView.Owner.ActorNumber);
    }

/*
    // 指定したプレイヤーのPhotonViewを取得するメソッド
    private PhotonView GetPhotonViewByPlayer(int targetPlayerId)
    {
        // すべてのPhotonViewを検索
        PhotonView[] allPhotonViews = FindObjectsOfType<PhotonView>();

        foreach (PhotonView view in allPhotonViews)
        {
            // PhotonViewのオーナーが指定したプレイヤーIDと一致するか確認
            if (view.Owner != null && view.Owner.ActorNumber == targetPlayerId)
            {
                return view; // 一致したPhotonViewを返す
            }
        }

        Debug.LogWarning($"プレイヤーID {targetPlayerId} に対応するPhotonViewが見つかりませんでした。");
        return null;
    */

/*
    [PunRPC]
    public void FirstRPC(int targetId, string rpc_name, PhotonMessageInfo info)
    {
        
        // 自分のアバターのみ処理を継続
        if (!photonView.IsMine)
        {
            Debug.Log($"[FirstRPC受信] 自分のアバターではないためスキップ (自分: {PhotonNetwork.LocalPlayer.ActorNumber}, ターゲットID: {targetId})");
            return;
        }

        if (photonView.Owner.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Debug.Log($"[StartGeneratingRings受信] 処理をスキップ (ターゲットID: {photonView.Owner.ActorNumber}, 自分のID: {PhotonNetwork.LocalPlayer.ActorNumber})");
            return;
        }

        // 自分が操作しているアバターの場合、2つ目のRPCを送信
        Debug.Log($"リング生成開始 (Photonview.ownerid: {photonView.Owner.ActorNumber}");
        photonView.RPC(rpc_name, photonView.Owner, targetId);
    }
*/

    /// <summary>
    /// RPC: リング生成開始
    /// </summary>
    [PunRPC]
    public void StartGeneratingRings(int targetID, PhotonMessageInfo info)
    {
        // IDが一致しない場合は終了
        if (targetID != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Debug.Log($"[RPC受信] StartGeneratingRingsを受信しましたが、ターゲットIDが一致しないため処理をスキップしました (自分: {PhotonNetwork.LocalPlayer.ActorNumber}, ターゲットID: {targetID})");
            return;
        }

         // 送信元がオブジェクトの所有者でない場合はスキップ
        if (photonView.Owner.ActorNumber != info.Sender.ActorNumber)
        {
            Debug.LogWarning($"[FirstRPC] 送信元がこのオブジェクトの所有者ではありません (photonView.Owner: {photonView.Owner.ActorNumber}, info.Sender: {info.Sender.ActorNumber})");
            return;
        }

        Debug.Log($"[RPC受信] StartGeneratingRingsを受信 (自分: {PhotonNetwork.LocalPlayer.ActorNumber}, ターゲットID: {targetID})");
        Debug.Log($"リング生成開始 (自分: {PhotonNetwork.LocalPlayer.ActorNumber}, ターゲットID: {targetID})");
        isGeneratingRings = true;
        StartCoroutine(GenerateRingsContinuously());
    }

    /// <summary>
    /// RPC: リング生成停止
    /// </summary>
    [PunRPC]
    public void StopGeneratingRings(PhotonMessageInfo info)
    {
        // IDが一致しない場合は終了
        avatarID = photonView.Owner.ActorNumber;
        if (avatarID != info.Sender.ActorNumber)
        {
            return;
        }

        // 送信元がオブジェクトの所有者でない場合はスキップ
        if (photonView.Owner.ActorNumber != info.Sender.ActorNumber)
        {
            Debug.LogWarning($"[FirstRPC] 送信元がこのオブジェクトの所有者ではありません (photonView.Owner: {photonView.Owner.ActorNumber}, info.Sender: {info.Sender.ActorNumber})");
            return;
        }

        Debug.Log($"リング生成停止 (送信元: {info.Sender.NickName})");
        isGeneratingRings = false;
    }

    /// <summary>
    /// リングを連続で生成するコルーチン
    /// </summary>
    private IEnumerator GenerateRingsContinuously()
    {
        while (isGeneratingRings)
        {
            EmitVoiceRing();
            yield return new WaitForSeconds(ringSpawnInterval);
        }
    }

    /// <summary>
    /// リングを生成し、拡大＆移動処理を開始
    /// </summary>
    private void EmitVoiceRing()
    {
        if (ringPrefab == null || ringSpawnPoint == null || avatarTransform == null)
        {
            Debug.LogError("リングのプレハブ、生成位置、またはアバターのTransformが未設定です。");
            return;
        }

        // 初期位置：SpawnPointから少し上
        Vector3 spawnPosition = ringSpawnPoint.position + new Vector3(0, 0.5f, 0); // 少し上にオフセット
        Quaternion rotation = Quaternion.LookRotation(avatarTransform.forward + new Vector3(0, 0.2f, 0), Vector3.up); // 正面 + 少し上方向

        // リング生成
        GameObject ring = Instantiate(ringPrefab, spawnPosition, rotation);

        // 初期サイズ設定
        ring.transform.localScale = Vector3.one * ringStartScale;

        // 拡大＆移動処理開始
        StartCoroutine(ExpandAndMoveRing(ring));
    }

    /// <summary>
    /// リングを拡大しながら前方へ移動させるコルーチン
    /// </summary>
    private IEnumerator ExpandAndMoveRing(GameObject ring)
    {
        float currentTime = 0f;
        Vector3 travelDirection = (avatarTransform.forward + new Vector3(0, 0.2f, 0)).normalized; // 正面 + 少し上方向

        while (currentTime < ringLifetime)
        {
            // 拡大処理
            float scale = Mathf.Lerp(ringStartScale, ringMaxScale, currentTime / ringLifetime);
            ring.transform.localScale = Vector3.one * scale;

            // 前方 + 少し上方向に移動
            ring.transform.position += travelDirection * ringTravelSpeed * Time.deltaTime;

            // 時間更新
            currentTime += Time.deltaTime;
            yield return null;
        }

        // リング削除
        Destroy(ring);
    }
}
