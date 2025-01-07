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
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
    //[SerializeField] private string apiKey; // 正しいAPIキーを入力
    private string apiKey;

    public delegate void OnReplyReceived(string reply);
    public event OnReplyReceived ReplyReceived;

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
    }

    public void SendMessageToChatGPT(string userMessage)
    {
        StartCoroutine(SendChatGPTRequest(userMessage, (reply) =>
        {
            if (!string.IsNullOrEmpty(reply))
            {
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
        var requestData = new ChatGPTRequest
        {
            model = "gpt-3.5-turbo",
            messages = new List<ChatGPTRequest.Message>
            {
                new ChatGPTRequest.Message
                { role = "system", content = 
                @"下記のjson文法に従って出力してください。
与えられた議題を適切に階層化してjson形式で出力してください。
********
{
  'question': '議題または問題を記述します',
  'conclusion': '結論がある場合に記述します。サブ議題がある場合はnullを指定します。',
  'subTopics': [
    {
      'question': 'サブ議題または問題を記述します',
      'conclusion': '結論には'結論未記入'を記述します。サブ議題がある場合はnullを指定します。', 
      'subTopics': 'さらに詳細なサブ議題がある場合に記述します。2つ以上存在する場合のみ有効。なければnullを指定します。'
    }
  ]
}
********"
                },
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
