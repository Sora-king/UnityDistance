using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // Photon の Hashtable を使用
using System.Collections.Generic;
using System.Linq;
using System; // System.Serializable をサポートするため



public class NodeGroupManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField conclusionInputField; // 結論入力用のUI
    public Button submitButton; // 結論を送信するボタン
   //private List<Node> leafNodes; // 葉ノードのリスト
    private Dictionary<string, int> activeNodesByGroup; // 各グループに現在割り当てられているノード
    //private Queue<Node> remainingNodes; // 空きノードのキュー

    private const string ACTIVE_NODES_KEY = "ActiveNodesByGroup";

    private Node root_Node;

    string myGroupKey = null;

    int leafcount = -1;

    [System.Serializable]
    private class DictionaryWrapper
    {
        public List<string> keys;
        public List<int> values; // 値を int に変更

        public DictionaryWrapper(Dictionary<string, int> dictionary)
        {
            keys = new List<string>(dictionary.Keys);
            values = new List<int>(dictionary.Values);
        }

        public Dictionary<string, int> ToDictionary()
        {
            var result = new Dictionary<string, int>();
            for (int i = 0; i < keys.Count; i++)
            {
                result[keys[i]] = values[i];
            }
            return result;
        }
    }

    /*private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitConclusion);
    }*/

    public void NodeGroupeStart(Node rootNode)
    {
        root_Node = rootNode;
        // 葉ノードを取得
        //leafNodes = FindLeafNodes(rootNode);
        StartCoroutine(WaitForConclusionsSet());

        // 空きノードのキューを初期化
        //remainingNodes = new Queue<Node>(leafNodes);

        // グループごとの割り当て状況を初期化
        activeNodesByGroup = new Dictionary<string, int>();

        // カスタムプロパティから情報を取得
        /*if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ACTIVE_NODES_KEY))
        {
            string jsonData = (string)PhotonNetwork.CurrentRoom.CustomProperties[ACTIVE_NODES_KEY];
            activeNodesByGroup = JsonUtility.FromJson<DictionaryWrapper>(jsonData).ToDictionary();
        }
        else
        {*/

            //SaveActiveNodesToCustomProperties();
        /*}*/

        // ボタンのクリックイベントを設定
        //submitButton.onClick.AddListener(OnSubmitConclusion);
    }

    // 再帰的に葉ノードを探索
    /*private List<Node> FindLeafNodes(Node root)
    {
        List<Node> leafNodes = new List<Node>();

        void Traverse(Node node)
        {
            if (node.subTopics == null || node.subTopics.Length == 0)
            {
                leafNodes.Add(node);
            }
            else
            {
                foreach (var subTopic in node.subTopics)
                {
                    Traverse(subTopic);
                }
            }
        }

        Traverse(root);
        return leafNodes;
    }*/

    private IEnumerator WaitForConclusionsSet()
    {
        // groups がカスタムプロパティに見つかるまで待機
        while (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("conclusions"))
        {
            Debug.Log("カスタムプロパティ 'conclusions' を待機中...");
            yield return new WaitForSeconds(0.5f); // 0.5秒ごとに確認
        }
        if(leafcount == -1){
            string[] conclusions = (string[])PhotonNetwork.CurrentRoom.CustomProperties["conclusions"];
            leafcount = conclusions.Count();
        }

        StartCoroutine(WaitForGroupsAndAssignNodes());
    }

    private IEnumerator WaitForGroupsAndAssignNodes()
    {
        // groups がカスタムプロパティに見つかるまで待機
        while (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Groups"))
        {
            Debug.Log("カスタムプロパティ 'Groups' を待機中...");
            yield return new WaitForSeconds(0.5f); // 0.5秒ごとに確認
        }

        // groups が見つかった場合、AssignInitialNodesToGroups を実行
        AssignInitialNodesToGroups();
    }

    // 最初に各グループに1つずつ割り当て
    private void AssignInitialNodesToGroups()
    {
        // カスタムプロパティから "groups" を取得してグループ数を確認
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Groups"))
        {
            Debug.LogError("カスタムプロパティ 'groups' が見つかりません。");
            return;
        }

        // グループリストを取得
        string[] groups = GetGroupNames()?.ToArray();
        int groupCount = groups.Length; // グループ数を取得

        // インデックスを管理する変数
        int currentIndex = 0;

        foreach (string groupKey in groups)
        {
            // ノードが足りない場合は終了
            if (currentIndex >= leafcount) break;

            // キューをリストに変換してインデックスでアクセス
            //Node assignedNode = new List<Node>(remainingNodes)[currentIndex];

            // グループにノードを割り当て
            activeNodesByGroup[groupKey] = currentIndex;

            //"議論中"にそれぞれのconclusionsを書き換える"
            {
                string[] conclusions = (string[])PhotonNetwork.CurrentRoom.CustomProperties["conclusions"];
                conclusions[currentIndex] = $"{groupKey}-議論中";
                // カスタムプロパティを更新
                ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
                {
                    { "conclusions", conclusions }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(properties);

                Debug.Log($"Updated conclusion for node {currentIndex}");
            }

            // ノードに初期状態の結論を設定
            //assignedNode.conclusion = $"{groupKey}-議論中";

            Debug.Log($"初期割り当て: グループ {groupKey} にノードを割り当て:");

            currentIndex++; // 次のインデックスに進む
        }

        // 割り当て結果をカスタムプロパティに保存
        SaveActiveNodesToCustomProperties();
    }

    private List<string> GetGroupNames()
    {
        // カスタムプロパティから "Groups" を取得
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Groups"))
        {
            Debug.LogError("カスタムプロパティ 'Groups' が見つかりません。");
            return null;
        }

        object groupsObject = PhotonNetwork.CurrentRoom.CustomProperties["Groups"];
        List<string> groupNames = new List<string>();

        // Groups が Photon の Hashtable の場合
        if (groupsObject is ExitGames.Client.Photon.Hashtable groupsHashtable)
        {
            foreach (var key in groupsHashtable.Keys)
            {
                if (key is string groupName)
                {
                    groupNames.Add(groupName);
                }
                else
                {
                    Debug.LogWarning($"Groups のキーが string 型ではありません: {key}");
                }
            }
        }
        // Groups が string[] の場合
        else if (groupsObject is string[] groupsArray)
        {
            groupNames.AddRange(groupsArray);
        }
        else
        {
            Debug.LogError("カスタムプロパティ 'Groups' の型がサポートされていません。");
            return null;
        }

        Debug.Log($"取得したグループ名: {string.Join(", ", groupNames)}");
        return groupNames;
    }



    // カスタムプロパティに保存
    private void SaveActiveNodesToCustomProperties()
    {
        string jsonData = JsonUtility.ToJson(new DictionaryWrapper(activeNodesByGroup));
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        properties[ACTIVE_NODES_KEY] = jsonData;
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    // グループの割り当てを再調整
    public bool ReassignGroup(string groupKey)
    {
        // 最新の情報を取得
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ACTIVE_NODES_KEY))
        {
            string jsonData = (string)PhotonNetwork.CurrentRoom.CustomProperties[ACTIVE_NODES_KEY];
            activeNodesByGroup = JsonUtility.FromJson<DictionaryWrapper>(jsonData).ToDictionary();
        }

        if (activeNodesByGroup.ContainsKey(groupKey))
        {
            // 現在の各グループに割り当てられている葉ノードのインデックスを収集
            /*List<int> assignedIndices = new List<int>();
            foreach (var entry in activeNodesByGroup)
            {
                Node assignedNode = entry.Value;
                int index = leafNodes.IndexOf(assignedNode);
                if (index >= 0)
                {
                    assignedIndices.Add(index);
                }
            }*/

            // 割り当て済みのインデックスの最大値を取得
            int maxAssignedIndex = activeNodesByGroup.Values.Max();

            // 新しく割り当てるノードのインデックスを計算
            int nextIndex = maxAssignedIndex + 1;

            if (nextIndex < leafcount)
            {
                // 新しいノードを割り当て
                //Node nextNode = leafNodes[nextIndex];
                activeNodesByGroup[groupKey] = nextIndex;

                //"議論中"にnextIndexのconclusionsを書き換える"
                {
                    string[] conclusions = (string[])PhotonNetwork.CurrentRoom.CustomProperties["conclusions"];
                    conclusions[nextIndex] = $"{groupKey}-議論中";
                    // カスタムプロパティを更新
                    ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
                    {
                        { "conclusions", conclusions }
                    };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(properties);

                    Debug.Log($"Updated conclusion for node");
                }
                List<int> valuesList = activeNodesByGroup.Values.ToList();
                Debug.Log($"グループ {groupKey} に新しいノードを割り当て: {string.Join(", ", valuesList)}");

                // カスタムプロパティを更新
                SaveActiveNodesToCustomProperties();
            }
            else
            {
                activeNodesByGroup[groupKey] = -1;
                Debug.Log($"グループ {groupKey} に割り当て可能なノードがありません。");
            }

            if (activeNodesByGroup.Values.All(value => value == -1))
            {
                Debug.Log("activeNodesByGroup のすべての値が -1 です。");
                // 必要な処理をここに記述
                return true;
            }
            else
            {
                Debug.Log("activeNodesByGroup に -1 以外の値があります。");
            }
        }
        else
        {
            Debug.LogError($"グループ {groupKey} の割り当てが見つかりません。");
        }
        return false;
    }


    // ボタンが押されたときの処理
    public void OnSubmitConclusion()
    {
        // 自分のグループキーを取得
        if(myGroupKey == null){
            myGroupKey = GetMyGroup();
        }

        if(leafcount == -1){
            string[] conclusions = (string[])PhotonNetwork.CurrentRoom.CustomProperties["conclusions"];
            leafcount = conclusions.Count();
        }

        // 最新の情報を取得
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ACTIVE_NODES_KEY))
        {
            string jsonData = (string)PhotonNetwork.CurrentRoom.CustomProperties[ACTIVE_NODES_KEY];
            activeNodesByGroup = JsonUtility.FromJson<DictionaryWrapper>(jsonData).ToDictionary();
        }

        // 自分の担当ノードを取得
        if (activeNodesByGroup.ContainsKey(myGroupKey))
        {
            //Node currentNode = activeNodesByGroup[myGroupKey];

            // 現在のノードのインデックスを取得
            int currentIndex = activeNodesByGroup[myGroupKey];
            if (currentIndex == -1)
            {
                Debug.LogError("担当ノードが葉ノードリストに見つかりません。");
                return;
            }
            Debug.Log($"currentIndex: {currentIndex}");
            

            // カスタムプロパティから "conclusions" を取得
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("conclusions"))
            {
                string[] conclusions = (string[])PhotonNetwork.CurrentRoom.CustomProperties["conclusions"];

                // 結論を設定
                if (currentIndex >= 0 && currentIndex < conclusions.Length)
                {
                    string newConclusion = conclusionInputField.text;
                    if (!string.IsNullOrEmpty(newConclusion))
                    {
                        conclusions[currentIndex] = newConclusion; // 指定されたインデックスに結論を設定

                        // カスタムプロパティを更新
                        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
                        {
                            { "conclusions", conclusions }
                        };
                        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);

                        Debug.Log($"Updated conclusion for node {currentIndex}: {newConclusion}");
                    }
                    else
                    {
                        Debug.LogError("結論が空です。入力してください。");
                    }
                }
                else
                {
                    Debug.LogError("ノードインデックスが範囲外です。");
                }
            }
            else
            {
                Debug.LogError("カスタムプロパティ 'conclusions' が見つかりません。");
            }
            //activeNodesByGroup のすべての値が -1 ならtrueを返す
            if (ReassignGroup(myGroupKey))
            {
                if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("conclusions"))
                {
                    string[] conclusions = (string[])PhotonNetwork.CurrentRoom.CustomProperties["conclusions"];
                    // 次の処理を記述
                    Debug.Log("現在の担当ノードが最後のノードに達しました。次の処理を実行します。");

                    // conclusionsがあるか？
                    if (conclusions == null)
                    {
                        Debug.LogError("結論リストがnullです。処理を中断します。");
                        return;
                    }
                    
                    /*for (int i = 0; i < leafNodes.Count; i++)
                    {
                        leafNodes[i].conclusion = conclusions[i]; // 葉ノードの結論にリストの値を代入
                        Debug.Log($"葉ノード {i} の結論を更新します: {conclusions[i]}");
                    }*/

                    JsonHandler JsonHandler = FindObjectOfType<JsonHandler>();
                    if (JsonHandler != null)
                    {
                        JsonHandler.BackGPT(conclusions);

                    }
                    else
                    {
                        Debug.LogError("JsonHandlerが見つかりませんでした。");
                    }
                    
                    
                    
                    /*string jsonText = JsonUtility.ToJson(root_Node, true);

                    Debug.Log($"ChatGPTへ送信: {jsonText}");

                    ChatGPTCommunicator ChatGPTCommunicator = FindObjectOfType<ChatGPTCommunicator>();
                    if (ChatGPTCommunicator != null)
                    {
                        ChatGPTCommunicator.SendMessageToChatGPT(jsonText, false);

                    }
                    else
                    {
                        Debug.LogError("ChatGPTCommunicatorが見つかりませんでした。");
                    }*/

                }
            }
            else
            {
                // 入力フィールドをリセット
                conclusionInputField.text = "";        
                
                //ReassignGroup(myGroupKey);
            }
        }
        else
        {
            Debug.LogError($"{myGroupKey} に割り当てられた担当が見つかりません。");
        }

    }

    public string GetMyGroup()
    {
        // カスタムプロパティからグループ情報を取得
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Groups"))
        {
            Debug.LogError("Groups カスタムプロパティが見つかりません！");
            return null;
        }

        ExitGames.Client.Photon.Hashtable allGroups = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties["Groups"];

        // 自分のActorNumberを取得
        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // 自分のグループを検索
        foreach (System.Collections.DictionaryEntry groupEntry in allGroups)
        {
            string groupName = groupEntry.Key.ToString();
            ExitGames.Client.Photon.Hashtable groupInfo = (ExitGames.Client.Photon.Hashtable)groupEntry.Value;

            if (groupInfo.ContainsKey("Members"))
            {
                int[] members = (int[])groupInfo["Members"];

                if (System.Array.Exists(members, member => member == myActorNumber))
                {
                    Debug.Log($"自分のグループは {groupName} です。");
                    return groupName;
                }
            }
        }

        Debug.LogWarning("自分のグループが見つかりませんでした。");
        return null;
    }


    /*[System.Serializable]
    private class DictionaryWrapper
    {
        public List<string> keys;
        public List<Node> values;

        public DictionaryWrapper(Dictionary<string, Node> dictionary)
        {
            keys = new List<string>(dictionary.Keys);
            values = new List<Node>(dictionary.Values);
        }

        public Dictionary<string, Node> ToDictionary()
        {
            var result = new Dictionary<string, Node>();
            for (int i = 0; i < keys.Count; i++)
            {
                result[keys[i]] = values[i];
            }
            return result;
        }
    }*/
}
