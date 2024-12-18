using System.Collections;
using UnityEngine;

public class VoiceRingEffect3 : MonoBehaviour
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

    private bool isGeneratingRings = false; // リング生成中フラグ

    void Update()
    {
        // スペースキー長押しで連続生成
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isGeneratingRings = true;
            StartCoroutine(GenerateRingsContinuously());
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isGeneratingRings = false;
        }
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
