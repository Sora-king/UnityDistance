using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceTest : MonoBehaviour
{
    private VoiceRingEffect voiceRingEffect; // VoiceRingEffectの参照を取得

    void Start()
    {
        // シーン内の VoiceRingEffect を探して参照を設定
        voiceRingEffect = FindObjectOfType<VoiceRingEffect>();

        // voiceRingEffectが見つからない場合はエラーログを出力
        if (voiceRingEffect == null)
        {
            Debug.LogError("【エラー】VoiceRingEffect が見つかりません。シーン内にアタッチされていますか？");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // スペースキーを押したとき
        {
            if (voiceRingEffect != null) // voiceRingEffectが null でないことを確認
            {
                voiceRingEffect.EmitVoiceRing(); // リング生成メソッドを呼び出す
            }
            else
            {
                Debug.LogWarning("【警告】VoiceRingEffect が参照されていないため、リングを生成できません。");
            }
        }
    }
}
