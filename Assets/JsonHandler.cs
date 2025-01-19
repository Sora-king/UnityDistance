using UnityEngine;
using TMPro; // TextMeshProの名前空間
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine.UI;


using System;

[Serializable]
public class Node
{
    public string question; // 問題
    public string conclusion; // 結論（null または "(結論未記入)"）
    public Node[] subTopics; // サブ議題（nullの可能性あり）
}


public class JsonHandler : MonoBehaviourPunCallbacks
{
   //[TextArea]
    //public string jsonInput; // Unityエディターから入力するJSONデータ

    public TextMeshProUGUI textDisplay; // TextMeshProのTextエリアを指定
    public GameObject scrollView;

    private Node rootNode;

    private bool mastarflag = false;
    private const string CONCLUSIONS_KEY = "conclusions";
    private int leafNodeCount; // 葉ノードの数


    public void jsonhandlerstart(string jsonInput, bool mode)
    {
        mastarflag = true;

        if (string.IsNullOrEmpty(jsonInput))
        {
        Debug.Log("No input provided. Exiting function.");
        return;
        }

        // カスタムプロパティに設定
        //Hashtable customProperties = new Hashtable();
        //customProperties["jsonData"] = jsonInput;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "jsonData", jsonInput } });
        Debug.Log("Node data shared: ");

        if(!mode){
            return;
        }

        Debug.Log($"Input JSON: {jsonInput}");
        // JSONデータをC#オブジェクトに変換
        rootNode = JsonUtility.FromJson<Node>(jsonInput);

           string jsonText = JsonUtility.ToJson(rootNode, true);

           Debug.Log($"ChatGPTへ送信: {jsonText}");

        // 葉ノードの数を計算
        List<string> leafNodes = CreateLeafNodeList(rootNode);
        leafNodeCount = leafNodes.Count;

        // 葉ノードの数だけ空文字のリストを初期化
        string[] conclusions = new string[leafNodeCount]; // 空文字で初期化
        for (int i = 0; i < leafNodeCount; i++)
        {
            conclusions[i] = "結論未記入"; // 各要素を"結論未記入"で初期化
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { CONCLUSIONS_KEY, conclusions }
        });

        Debug.Log("なぜかバグ０００");
        NodeGroupManager NodeGroupManager = FindObjectOfType<NodeGroupManager>();
                if (NodeGroupManager != null)
                {
                    Debug.Log("なぜかバグ");
                    NodeGroupManager.NodeGroupeStart(rootNode);
                }
                else
                {
                    Debug.LogError("NodeGroupManagerが見つかりませんでした。");
                }
        
        // 番号付きでデータを表示（インデント付き）
        //string formattedText = DisplayNodeWithNumbersAndIndent(rootNode, "1", 0);

        // TextMeshProエリアに表示
        //textDisplay.text = formattedText;
    }

    private List<string> CreateLeafNodeList(Node rootNode)
    {
        List<string> leafNodes = new List<string>();

        void Traverse(Node node)
        {
            if (node.subTopics == null || node.subTopics.Length == 0)
            {
                leafNodes.Add(node.question);
            }
            else
            {
                foreach (var subTopic in node.subTopics)
                {
                    Traverse(subTopic);
                }
            }
        }

        Traverse(rootNode);
        return leafNodes;
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
        /*else
        {
            // サブ議題が存在しない場合は葉ノードとみなして結論を追加
            if (!string.IsNullOrEmpty(node.conclusion))
            {
                result += $"{indent}  {currentNumber} Conclusion: {node.conclusion}\n";
                Debug.Log($"Leaf Node Conclusion Added: {node.conclusion} for Node: {node.question}");
            }
        }*/


        return result;
    }

    public override void OnRoomPropertiesUpdate(Hashtable changedProps)
    {
        // 更新されたプロパティに "NodeData" が含まれているか確認
        if (changedProps.ContainsKey("jsonData"))
        {
            
            string jsonData = changedProps["jsonData"] as string;
            rootNode = JsonUtility.FromJson<Node>(jsonData);
            if (!string.IsNullOrEmpty(jsonData))
            {
                // JSON データを C# オブジェクトに変換
                rootNode = JsonUtility.FromJson<Node>(jsonData);

                // 番号付きでデータを表示（インデント付き）
                string formattedText = DisplayNodeWithNumbersAndIndent(rootNode, "1", 0);

                scrollView.SetActive(true);

                // TextMeshProエリアに表示
                textDisplay.text = formattedText;

                // 必要に応じて更新処理を追加
               // HandleUpdatedNodeData(rootNode);
            }
        }

        // 更新されたプロパティに "conclusions" が含まれているか確認
        if(changedProps.ContainsKey("conclusions"))
        {
            string[] conclusions = (string[])PhotonNetwork.CurrentRoom.CustomProperties["conclusions"];
            UpdateLeafNodeConclusions(rootNode, conclusions);
        }


    }

    
    
    private void UpdateLeafNodeConclusions(Node root, string[] conclusions)
    {
        // 既存の葉ノードを取得
        List<Node> leafNodes = GetLeafNodes(root);

        // 葉ノード数と結論リスト数の一致を確認
        if (leafNodes.Count != conclusions.Length)
        {
            Debug.LogError($"葉ノードの数 ({leafNodes.Count}) と結論リストの数 ({conclusions.Length}) が一致しません。");
            return;
        }

        // 結論を更新
        for (int i = 0; i < leafNodes.Count; i++)
        {
            leafNodes[i].conclusion = conclusions[i];
            Debug.Log($"葉ノード {i} の結論を更新: {conclusions[i]}");
        }

        // 更新後の JSON をデバッグ出力
        string formattedText = DisplayNodeWithNumbersAndIndent(root, "1", 0);

                // TextMeshProエリアに表示
        textDisplay.text = formattedText;
    }

    private List<Node> GetLeafNodes(Node root)
    {
        List<Node> leafNodes = new List<Node>();

        // 再帰的に葉ノードを探索
        void Traverse(Node node)
        {
            if (node.subTopics == null || node.subTopics.Length == 0)
            {
                // 葉ノードの場合、リストに追加
                leafNodes.Add(node);
            }
            else
            {
                // サブトピックが存在する場合、再帰的に処理
                foreach (var subTopic in node.subTopics)
                {
                    Traverse(subTopic);
                }
            }
        }

        Traverse(root);
        return leafNodes;
    }

    public void BackGPT(string[] conclusions)
    {
        UpdateLeafNodeConclusions(rootNode, conclusions);

        string jsonText = JsonUtility.ToJson(rootNode, true);

        Debug.Log($"ChatGPTへ送信: {jsonText}");

        ChatGPTCommunicator ChatGPTCommunicator = FindObjectOfType<ChatGPTCommunicator>();
        if (ChatGPTCommunicator != null)
        {
            ChatGPTCommunicator.SendMessageToChatGPT(jsonText, false);

        }
        else
        {
            Debug.LogError("ChatGPTCommunicatorが見つかりませんでした。");
        }
    }

}

