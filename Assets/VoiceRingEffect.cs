using UnityEngine;

public class VoiceRingEffect : MonoBehaviour
{
    [Header("リングエフェクト設定")]
    public GameObject ringPrefab; // Blenderで作成したFBXモデルをプレハブ化して割り当て
    public float ringExpansionSpeed = 5f; // リングが拡大する速度
    public float ringTravelSpeed = 3f; // リングが飛ぶ速度
    public float ringLifetime = 2f; // リングが消えるまでの時間
    public float ringStartScale = 0.1f; // リングの初期サイズ
    public float ringMaxScale = 3f; // リングの最大サイズ

    [Header("リングの発生設定")]
    public Transform ringSpawnPoint; // リングが生成される位置（アバターの口元など）
    public Transform avatarTransform; // アバターのTransform（向きを基準にする）

    // 音声を発したタイミングで呼び出す
    public void EmitVoiceRing()
    {
        if (ringPrefab == null || ringSpawnPoint == null || avatarTransform == null)
        {
            Debug.LogError("リングのプレハブ、生成位置、またはアバターのTransformが未設定です。");
            return;
        }

        // リングを生成
        GameObject ring = Instantiate(ringPrefab, ringSpawnPoint.position, Quaternion.identity);

        // 初期サイズを設定
        ring.transform.localScale = Vector3.one * ringStartScale;

        // リングの移動と拡大をコルーチンで実行
        StartCoroutine(ExpandAndMoveRing(ring));
    }

    private System.Collections.IEnumerator ExpandAndMoveRing(GameObject ring)
    {
        float currentTime = 0f;
        Vector3 travelDirection = avatarTransform.forward; // アバターの正面方向

        while (currentTime < ringLifetime)
        {
            // 時間経過に応じてサイズを拡大
            float scale = Mathf.Lerp(ringStartScale, ringMaxScale, currentTime / ringLifetime);
            ring.transform.localScale = Vector3.one * scale;

            // リングを前方に移動
            ring.transform.position += travelDirection * ringTravelSpeed * Time.deltaTime;

            // 時間経過
            currentTime += Time.deltaTime;

            yield return null; // 次のフレームまで待機
        }

        // 一定時間後にリングを削除
        Destroy(ring);
    }
}
