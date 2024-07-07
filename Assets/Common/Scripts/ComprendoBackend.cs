using UnityEngine;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Reflection;

public class ComprendoBackend : ScriptableObject {

    private readonly bool isActive = true;

#if UNITY_WEBGL && !UNITY_EDITOR
    private readonly string basePath = "/mondoelli-server"; // URL relativo al server
#else
    private readonly string basePath = "http://localhost:8080";
#endif

    private readonly string loginPath = "/auth/login";
    private readonly string checkToken = "/utenti/current"; //GET
    private readonly string getUserPrefsPath = "/status";
    private readonly string setUserPrefsPath = "/status";


    public int id;
    public string username;
    public string token;

    [Serializable]
    public class LoginBody {
        public string username;
        public string password;
    }

    [Serializable]
    public class LoginResponse
    {
        public int id;
        public string username;
        public string token;
    }
    
    [Serializable]
    public class TokenCheckResponse
    {
        public int id;
        public string username;
    }
    [Serializable]
    public class UserPrefs
    {
        public bool tutorialArrowDoubleUp = false;
        public bool tutorialArrowTripleUp = false;
        public bool tutorialArrowX2 = false;
        public bool tutorialArrowX3 = false;
        public bool tutorialArrowX4 = false;
        public bool showScrignoIntro = false;
        public string mediaLiteracyExercisesStarted = string.Empty;
    }

    public void SetTokenHeader() {
        RestClient.DefaultRequestHeaders["Authorization"] = "Bearer " + token;
    }

    private void SetHeader() {
        // Tutte le richieste al server devono avere lo stesso header
        RestClient.DefaultRequestHeaders["Content-Type"] = "application/json";
    }

    public void CheckToken(string tokenToCheck, Action<TokenCheckResponse> onComplete = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            onComplete?.Invoke(new TokenCheckResponse());
            return;
        }

        if (!isActive) return;
        
        Debug.Log("COMPRENDO BACKEND: Check token");

        var req = new RequestHelper
        {
            Uri = basePath+checkToken,
            Method = "GET",
            Timeout = 20,
            Headers = new Dictionary<string, string> {{"Authorization", "Bearer " + tokenToCheck}}
        };
        
        RestClient.Get<TokenCheckResponse>(req).Then(response =>
        {
            Debug.Log("COMPRENDO BACKEND: Check token request success: " + JsonUtility.ToJson(response, true));
            onComplete?.Invoke(response);
        }).Catch(err =>
        {
            var error = err as RequestException;
            Debug.LogError("COMPRENDO BACKEND check token failed "+error?.Response);
            onComplete?.Invoke(null);
        });
    }

    public void Login(string user, string pass, Action<LoginResponse> onSuccess, Action<Exception> onFailure) {
        if (isActive) {
            Debug.Log("COMPRENDO BACKEND: Login request, user: " + user);

            SetHeader();
            // Richiesta di login 
            username = user;
            RestClient.Post<LoginResponse>(basePath + loginPath, new LoginBody {
                username = user,
                password = pass,
            })
            .Then(res => {
                Debug.Log("COMPRENDO BACKEND: Login request success: " + JsonUtility.ToJson(res, true));
                token = res.token;
                id = res.id;
                username = res.username;
                SetTokenHeader();
                onSuccess(res);
            })
            .Catch(err => onFailure(err));
        } else {
            onSuccess?.Invoke(new LoginResponse());
        }
    }

    public void GetUserPrefs(Action<UserPrefs> onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("COMPRENDO BACKEND: User prefs request");
            SetTokenHeader();
            RestClient.Get<UserPrefs>(basePath + getUserPrefsPath).Then(response =>
            {
                Debug.Log("COMPRENDO BACKEND: Get user prefs request success: " + JsonUtility.ToJson(response, true));
                onSuccess?.Invoke(response);
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.LogError("COMPRENDO BACKEND: Get user prefs request failed "+error?.Response);
                onFailure?.Invoke(err);
            });
        }
        else
        {
            onSuccess?.Invoke(new UserPrefs());
        }
    }

    public void SetUserPrefs(UserPrefs prefs, Action onSuccess, Action<Exception> onFailure)
    {
        if (isActive)
        {
            Debug.Log("COMPRENDO BACKEND: Set user prefs request");
            SetTokenHeader();
            SetHeader();
            RestClient.Post<UserPrefs>(basePath + setUserPrefsPath, prefs).Then(res =>
            {
                Debug.Log("COMPRENDO BACKEND: Set user prefs request success: " + JsonUtility.ToJson(res, true));
                onSuccess?.Invoke();
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.LogError("COMPRENDO BACKEND: Set user prefs request failed "+error?.Response);
                onFailure.Invoke(err);
            });
        }
        else
        {
            Debug.LogError("COMPRENDO BACKEND: Set user prefs request failed: Comprendo backend inactive");
            onFailure?.Invoke(new Exception("Comprendo backend inactive"));
        }
    }
}
