using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChatGPTClient : MonoBehaviour
{
    public string apiURL = @"https://unitydemoeastus.openai.azure.com/openai/deployments/unitydemoturbo35/chat/completions?api-version=2023-03-15-preview";

    public string apiKey = "f64f174bd5614385ad5e35bf0b846d5d";

    public IEnumerator Ask(List<string[]> prompts, Action<ChatGPTResponse> callBack)
    {
        var url = apiURL; 

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            ChatGPTMessage[] messages = new ChatGPTMessage[prompts.Count];
            int index = 0;
            foreach (string[] prompt in prompts)
            {
                messages[index] = new ChatGPTMessage
                {
                    role = prompt[0],
                    content = prompt[1]
                };
                index++;
            }

            var requestParams = JsonConvert.SerializeObject(new ChatGPTRequest
            {
                Messages = messages
            });

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestParams);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;
            request.disposeCertificateHandlerOnDispose = true;

            request.SetRequestHeader("Content-Type", "application/json");

            // required to authenticate against OpenAI
            request.SetRequestHeader("api-key", apiKey);

            var requestStartDateTime = DateTime.Now;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(request.error);
            }
            else
            {
                string responseInfo = request.downloadHandler.text;
                Debug.Log(request.downloadHandler.text);
                var response = JsonConvert.DeserializeObject<ChatGPTResponse>(responseInfo);

                response.ResponseTotalTime = (DateTime.Now - requestStartDateTime).TotalMilliseconds;

                callBack(response);
            }
        }
    }
}