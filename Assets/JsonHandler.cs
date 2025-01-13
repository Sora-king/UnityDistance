using UnityEngine;
using TMPro; // TextMeshProの名前空間

using System;

[Serializable]
public class Node
{
    public string question; // 問題
    public string conclusion; // 結論（null または "(結論未記入)"）
    public Node[] subTopics; // サブ議題（nullの可能性あり）
}


public class JsonHandler : MonoBehaviour
{
    [TextArea]
    public string jsonInput; // Unityエディターから入力するJSONデータ

    public TextMeshProUGUI textDisplay; // TextMeshProのTextエリアを指定

    private Node rootNode;

    void Start()
    {
        if (string.IsNullOrEmpty(jsonInput))
        {
        Debug.Log("No input provided. Exiting function.");
        return;
        }
        // JSONデータをC#オブジェクトに変換
        rootNode = JsonUtility.FromJson<Node>(jsonInput);

        // 番号付きでデータを表示（インデント付き）
        string formattedText = DisplayNodeWithNumbersAndIndent(rootNode, "1", 0);

        // TextMeshProエリアに表示
        textDisplay.text = formattedText;
    }

    string DisplayNodeWithNumbersAndIndent(Node node, string currentNumber, int indentLevel)
    {
        // インデントを生成
        string indent = new string(' ', indentLevel * 2);

        // 番号と問題をテキストに追加
        string result = $"{indent}{currentNumber}. {node.question}\n";

        // 結論を追加（nullの場合はスキップ）
        if (!string.IsNullOrEmpty(node.conclusion))
        {
            result += $"{indent}  {currentNumber} Conclusion: {node.conclusion}\n";
        }

        // サブ議題が存在する場合、再帰的に処理
        if (node.subTopics != null && node.subTopics.Length > 0)
        {
            for (int i = 0; i < node.subTopics.Length; i++)
            {
                string nextNumber = $"{currentNumber}-{i + 1}"; // 次の番号を生成
                result += DisplayNodeWithNumbersAndIndent(node.subTopics[i], nextNumber, indentLevel + 1);
            }
        }

        return result;
    }
}

