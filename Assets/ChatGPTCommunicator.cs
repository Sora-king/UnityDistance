using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

[System.Serializable]
public class Config
{
    public string apiKey;
}

public class ChatGPTCommunicator : MonoBehaviour
{
    [SerializeField] private JsonHandler jsonHandler;
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
    //[SerializeField] private string apiKey; // 正しいAPIキーを入力
    private string apiKey;

    public delegate void OnReplyReceived(string reply);
    public event OnReplyReceived ReplyReceived;

    private string currentSystemContent;

    private string systemContentTemplate = 
@"与えられた議題を細分化し階層化してjson形式で出力してください。必ず細分化してください。末端以外の結論にはnullを記述します。
出力フォーマットは以下のJSON文法と同じ形式で返してください。ただし、コードブロック記法（```json）は不要です。
********
{
  ""question"": ""議題を記述します"",
  ""conclusion"": ""末端の結論には""結論未記入""を記述します。末端以外の結論にはnullを記述します"",
  ""subTopics"": [
    {
      ""question"": ""サブ議題を記述します"",
      ""conclusion"": ""末端の結論には""結論未記入""を記述します。末端以外の結論にはnullを記述します"", 
      ""subTopics"": ""さらに詳細なサブ議題がある場合に記述します。2つ以上存在する場合のみ有効。なければnullを指定します。""
    }
  ]
}
********";

private string systemContentConclusionFilling = 
@"与えられたJSONデータに基づき""question""の具体的な回答になるように""conclusion""を全て埋めてください。
サブ議題の意図を読み取り、関連性を持つ情報を統合し、ChatGPTの知見を用いて最適な結論を推論してください
回答は以下の条件に基づき具体的かつ明確にしてください：
1. 各""conclusion"" は、""question"" を解決または補完するために論理的かつ実用的な内容にする。
2. サブ議題の例示が可能であれば、具体例を挙げて説明する。
3. 必要に応じて、補足説明や理由を簡潔に追加する。
4. 最後に、それらに当てはまる具体的なものがあれば、一例として提示する。

出力フォーマットは以下のJSON文法と同じ形式で返してください。ただし、コードブロック記法（```json）は不要です。
********
{
  ""question"": ""議題を記述します"",
  ""conclusion"": ""サブ議題の結論をまとめた結論を記述します。"",
  ""subTopics"": [
    {
      ""question"": ""サブ議題を記述します"",
      ""conclusion"": 末端以外の結論を帰納的に全て埋めます"", 
      ""subTopics"": ""さらに詳細なサブ議題がある場合に記述します。2つ以上存在する場合のみ有効。なければnullを指定します。""
    }
  ]
}
********";

       

    [System.Serializable]
    public class ChatGPTRequest
    {
        public string model;
        public List<Message> messages;

        [System.Serializable]
        public class Message
        {
            public string role;
            public string content;
        }
    }

    [System.Serializable]
    public class ChatGPTResponse
    {
        public List<Choice> choices;

        [System.Serializable]
        public class Choice
        {
            public Message message;

            [System.Serializable]
            public class Message
            {
                public string role;
                public string content;
            }
        }
    }

    public void Start()
    {
        // 設定ファイルのパスを取得
        string filePath = Path.Combine(Application.dataPath, "apiconfig.json");

        if (File.Exists(filePath))
        {
            // ファイルを読み込む
            string jsonText = File.ReadAllText(filePath);
            Config config = JsonUtility.FromJson<Config>(jsonText);

            // APIキーを取得
            apiKey = config.apiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("設定ファイルにAPIキーが見つかりません。");
            }
        }
        else
        {
            Debug.LogError($"設定ファイルが見つかりません: {filePath}");
        }

        ReplyReceived += (reply) =>
        {
            if (jsonHandler != null)
            {
                jsonHandler.jsonhandlerstart(reply); // ChatGPTの出力をJsonHandlerに渡して実行
                Debug.Log("ChatGPTの出力をJsonHandlerに渡しました。");
            }
            else
            {
                Debug.LogError("JsonHandlerが設定されていません。");
            }
        };
    }

    public void SendMessageToChatGPT(string userMessage, bool mode)
    {
        if (mode)
        {
            currentSystemContent = systemContentTemplate; // 初期の文法説明
        }
        else
        {
            currentSystemContent = systemContentConclusionFilling; // 結論埋めの説明
        }

        StartCoroutine(SendChatGPTRequest(userMessage, (reply) =>
        {
            if (!string.IsNullOrEmpty(reply))
            {
                Debug.Log($"ChatGPTからの返信: {reply}");
                ReplyReceived?.Invoke(reply);
            }
            else
            {
                Debug.LogError("No response received from ChatGPT.");
            }
        }));
    }

    private IEnumerator SendChatGPTRequest(string userInput, System.Action<string> callback)
    {
        Debug.Log(currentSystemContent);
        var requestData = new ChatGPTRequest
        {
            model = "gpt-4o",
            messages = new List<ChatGPTRequest.Message>
            {
                new ChatGPTRequest.Message
                { role = "system", content = currentSystemContent},
                new ChatGPTRequest.Message { role = "user", content = userInput }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);
        UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Request Error: {request.error}");
            callback?.Invoke(null);
        }
        else if ((int)request.responseCode >= 400)
        {
            Debug.LogError($"HTTP Error {request.responseCode}: {request.downloadHandler.text}");
            callback?.Invoke(null);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            ChatGPTResponse responseData = JsonUtility.FromJson<ChatGPTResponse>(responseText);
            if (responseData.choices != null && responseData.choices.Count > 0)
            {
                string reply = responseData.choices[0].message.content.Trim();
                callback?.Invoke(reply);
            }
            else
            {
                callback?.Invoke(null);
            }
        }
    }
}
