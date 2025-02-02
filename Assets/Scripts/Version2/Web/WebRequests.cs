using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Net;


[System.Serializable]
public class LeaderboardEntry
{
    public string name;
    public int rank;
    public int score;
    public int wave;
}

[System.Serializable]
public class LeaderboardList
{
    public List<LeaderboardEntry> leaderboard;
}
[System.Serializable]
public class LeaderboardWrapper
{
    public LeaderboardEntry[] leaderboard;
}

[System.Serializable]
public class Response
{
    public string error;
    public string success;
}

public class WebRequests : MonoBehaviour
{
    public Text errorText;

    void Start()
    {

        // StartCoroutine(GetLeaderboard("http://172.30.81.176/SpaceInvadersBackend/GetLeaderboard.php"));
        //StartCoroutine(AddToLeaderboard("http://172.30.81.176/SpaceInvadersBackend/AddToLeaderboard.php", "test1", 1, 1000, 40));
    }


    public IEnumerator FilterPlayerName(string playerName)
    {

        string uri = "https://44e3-132-205-229-9.ngrok-free.app/SpaceInvadersBackend/UsernameChecker.php";
        WWWForm form = new WWWForm();
        form.AddField("playerName", playerName);
        Debug.Log($"Sending Data -> Name: {playerName}");
        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                Debug.LogError($"Response Text: {webRequest.downloadHandler.text}");
            }
            else
            {
                string response = webRequest.downloadHandler.text;
                Debug.Log("Raw Server Response: " + response);

                Response jsonresponse = JsonUtility.FromJson<Response>(response);

                if (jsonresponse == null)
                {
                    Debug.LogError("JSON parsing failed: JsonUtility.FromJson returned null.");
                }
                else if (!string.IsNullOrEmpty(jsonresponse.error))
                {
                    Debug.Log("Duplicate Error: " + jsonresponse.error);
                    DisplayError(jsonresponse.error);
                }
                else if (!string.IsNullOrEmpty(jsonresponse.success))
                {
                    Debug.Log("Success: " + jsonresponse.success);
                }

            }

        }
    }

    public IEnumerator GetLeaderboard(Text[] LeaderboardScoreList, Text[] LeaderboardWaveList, Text[] LeaderboardNameList)
    {
        {
            string uri = "https://44e3-132-205-229-9.ngrok-free.app/SpaceInvadersBackend/GetLeaderboard.php";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        string json = webRequest.downloadHandler.text;
                        Debug.Log("Received JSON: " + json);
                        LeaderboardWrapper wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);

                        if (wrapper != null && wrapper.leaderboard != null)
                        {
                            LeaderboardEntry[] leaderboardEntries = wrapper.leaderboard;

                            int count = Mathf.Min(leaderboardEntries.Length, LeaderboardScoreList.Length);

                            for (int i = 0; i < count; i++)
                            {
                                LeaderboardScoreList[i].text = leaderboardEntries[i].score.ToString();
                                LeaderboardWaveList[i].text = leaderboardEntries[i].wave.ToString();
                                LeaderboardNameList[i].text = leaderboardEntries[i].name;

                                Debug.Log($"Updated UI: {LeaderboardNameList[i].text}, Score: {LeaderboardScoreList[i].text}, Wave: {LeaderboardWaveList[i].text}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Failed to parse leaderboard JSON.");
                        }

                        break;
                }
            }
        }
    }

    public IEnumerator AddToLeaderboard(string playerName, int playerWave, int playerScore)
    {
        Debug.Log($"AddToLeaderboard started: {playerName}, {playerWave}, {playerScore}");

        string uri = "https://44e3-132-205-229-9.ngrok-free.app/SpaceInvadersBackend/AddToLeaderboard.php";
        WWWForm form = new WWWForm();
        form.AddField("playerName", playerName);
        form.AddField("playerWave", playerWave);
        form.AddField("playerScore", playerScore);
        Debug.Log($"Sending Data -> Name: {playerName}, Wave: {playerWave}, Score: {playerScore}");


        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                Debug.LogError($"Response Text: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.Log("Server Response: " + webRequest.downloadHandler.text);
            }
        }
    }
    public void DisplayError(string errorMessage)
    {
        Debug.Log("errorMessage: " + errorMessage + " errorText.text: " + errorText.text);

        if (errorText != null) // Check if errorText is assigned
        {
            Debug.Log("errorMessage: " + errorMessage + " errorText.text: " + errorText.text);

            if (errorText.text != null)
            {
                errorText.text = errorMessage; // Display the error message in the Text component
            }
            else
            {
                Debug.LogError("Error: errorText.text is null!");
            }
        }
        else
        {
            Debug.LogError("Error: errorText is not assigned!");
        }
    }

}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"leaderboard\": " + json + "}";
        LeaderboardList<T> wrapper = JsonUtility.FromJson<LeaderboardList<T>>(newJson);
        return wrapper.leaderboard;
    }

    [Serializable]
    private class LeaderboardList<T>
    {
        public T[] leaderboard;
    }
}
