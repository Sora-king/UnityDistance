using System.Collections;
using UnityEngine;

public class VoiceRingEffect2 : MonoBehaviour
{
    [Header("リングエフェクト設定")]
    public GameObject ringPrefab; // Blenderで作成したリングプレハブ
    public float ringSpawnInterval = 0.2f; // リング生成間隔
    public float ringExpansionSpeed = 3f; // リングの拡大速度
    public float ringTravelSpeed = 5f; // リングの移動速度
    public float ringLifetime = 1.5f; // リングが消えるまでの時間
    public float ringStartScale = 0.1f; // 初期サイズ
    public float ringMaxScale = 2.0f; // 最大サイズ

    [Header("生成ポイント")]
    public Transform ringSpawnPoint; // リングの生成位置
    public Transform avatarTransform; // アバターのTransform

    private bool isGeneratingRings = false; // 連続生成フラグ

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

        // リングを生成し、アバターの正面方向を向ける
        Quaternion rotation = Quaternion.LookRotation(avatarTransform.forward, Vector3.up);
        GameObject ring = Instantiate(ringPrefab, ringSpawnPoint.position, rotation);

        // 初期サイズ設定
        ring.transform.localScale = Vector3.one * ringStartScale;

        // リングの拡大＆移動を開始
        StartCoroutine(ExpandAndMoveRing(ring));
    }

    /// <summary>
    /// リングを拡大しながら前方へ移動させるコルーチン
    /// </summary>
    private IEnumerator ExpandAndMoveRing(GameObject ring)
    {
        float currentTime = 0f; // 経過時間
        Vector3 travelDirection = avatarTransform.forward; // アバターの正面方向

        while (currentTime < ringLifetime)
        {
            // リングの拡大
            float scale = Mathf.Lerp(ringStartScale, ringMaxScale, currentTime / ringLifetime);
            ring.transform.localScale = Vector3.one * scale;

            // リングの移動
            ring.transform.position += travelDirection * ringTravelSpeed * Time.deltaTime;

            // 時間更新
            currentTime += Time.deltaTime;
            yield return null; // 次のフレームまで待機
        }

        // リングを削除
        Destroy(ring);
    }
}
