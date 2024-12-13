using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class LocalSpeechRecognition : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer; // キーワード認識用
    public Transform[] avatarTransforms; // アバターのTransformを格納する配列
    public string[] avatarNames; // アバターに対応する名前

    void Start()
    {
        // 名前リストをキーワードとして設定
        if (avatarNames.Length > 0)
        {
            keywordRecognizer = new KeywordRecognizer(avatarNames);
            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();
            Debug.Log("音声認識を開始しました。");
        }
        else
        {
            Debug.LogWarning("アバター名が設定されていません。");
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"認識されたフレーズ: {args.text}");
        MatchNameToAvatar(args.text);
    }

    private void MatchNameToAvatar(string recognizedName)
    {
        // 名前と一致するアバターを探す
        for (int i = 0; i < avatarNames.Length; i++)
        {
            if (recognizedName == avatarNames[i])
            {
                Debug.Log($"名前一致: {recognizedName}");
                HighlightAvatar(avatarTransforms[i]);
                return;
            }
        }

        Debug.Log("一致する名前が見つかりませんでした。");
    }

    private void HighlightAvatar(Transform avatarTransform)
    {
        // アバターをハイライトする例（色を変更）
        Renderer renderer = avatarTransform.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow; // ハイライトカラー
        }
    }

    void OnDestroy()
    {
        // 音声認識を停止
        if (keywordRecognizer != null)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}
