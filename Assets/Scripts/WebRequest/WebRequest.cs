using Fusion;
using Network;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class WebRequest : MonoBehaviour
{
    private string baseURL = "http://localhost:5000/api/players";
    private string token;

    [Header("Register / Login UI")]
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_InputField confirmPassword;
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField userID;
    [SerializeField] private GameObject forgotPasswordBtn;
    [SerializeField] private GameObject createNewBtn;
    [SerializeField] private GameObject signInBtn;
    [SerializeField] private GameObject loginBtn;
    [SerializeField] private GameObject changePasswordBtn;
    [SerializeField] private GameObject createAccountBtn;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private TextMeshProUGUI allPlayers;
    [SerializeField] private TextMeshProUGUI playerByID;

    //[SerializeField] private TextMeshProUGUI error;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI killsCount;

    [Header("Object Enable")]
    [SerializeField] private GameObject UIpanel;
    [SerializeField] private GameObject deleteAccountBtn;

    public string playerId;
    [SerializeField] private DeletePanel deletePanel;
    [SerializeField] private NetworkSessionManager networkSesh;


    public void StartRegister()
    {
        StartCoroutine(Register(username.text, email.text, password.text));
    }

    public void StartLogin()
    {
        StartCoroutine(Login(username.text, password.text));
    }

    //public void StartNewPassword()
    //{
    //    StartCoroutine(NewPassword());
    //}

    public void StartUpdateKills(int totalScore)
    {
        StartCoroutine(UpdateScore(totalScore));
    }

    public void StartGetAllUsers()
    {
        StartCoroutine(GetAllUsers());
    }

    public void StartGetPlayerById()
    {
        StartCoroutine(GetUserById(userID.text));
    }

    public void StartDeleteAccount()
    {
        StartCoroutine(DeleteAccount());
    }

    public IEnumerator Register(string usernameTxt, string emailTxt, string passwordTxt)
    {
        var body = new RegisterData(usernameTxt, emailTxt, passwordTxt);
        yield return SendRequest("/register", "POST", body, false);
   
        username.gameObject.SetActive(true);
        password.gameObject.SetActive(true);
        confirmPassword.gameObject.SetActive(false);
        email.gameObject.SetActive(false);
        forgotPasswordBtn.gameObject.SetActive(true);
        createNewBtn.SetActive(true);
        signInBtn.SetActive(false);
        loginBtn.SetActive(true);
        changePasswordBtn.SetActive(false);
        createAccountBtn.SetActive(false);
        message.gameObject.SetActive(false);
        //error.gameObject.SetActive(false);
    }

    IEnumerator Login(string usernameTxt, string passwordTXt)
    {
        var body = new LoginData(usernameTxt, passwordTXt);

        string json = JsonUtility.ToJson(body);

        UnityWebRequest request = new UnityWebRequest(baseURL + "/login", "POST");
        byte[] raw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log("LOGIN RESPONSE: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse res = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);

            if (res.success)
            {
                token = res.token;
                PlayerPrefs.SetString("TOKEN", token);
                Debug.Log("Login Success! Token saved.");

                message.gameObject.SetActive(false);
                //error.gameObject.SetActive(false);
                UIpanel.SetActive(false);

                deletePanel.loggedIn = true;
                StartUpdateKills(0);
                StartCoroutine(GetMe());

                networkSesh.StartGame(GameMode.Client);
            }
            else
            {
                Debug.LogError(res.message);
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    //IEnumerator NewPassword()
    //{
    //    PasswordUpdateRequest requestData = new PasswordUpdateRequest
    //    {
    //        username = username.text,
    //        newPassword = password.text,
    //        confirmPassword = confirmPassword.text
    //    };

    //    string json = JsonUtility.ToJson(requestData);
    //    byte[] jsonToSend = Encoding.UTF8.GetBytes(json);

    //    UnityWebRequest www = UnityWebRequest.Put($"http://localhost:3000/players/new-password", jsonToSend);
    //    www.SetRequestHeader("Content-Type", "application/json");
    //    www.downloadHandler = new DownloadHandlerBuffer();

    //    yield return www.SendWebRequest();

    //    Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
    //    if (!response.success)
    //    {
    //        message.text = "";
    //        message.gameObject.SetActive(true);
    //        message.text = response.message;

    //        error.text = "";
    //        error.gameObject.SetActive(true);
    //        string err = "";
    //        if (response.error != null && response.error.Length > 0)
    //        {
    //            err += "\n" + string.Join("\n", response.error);
    //        }
    //        error.text = err;

    //        yield break;
    //    }

    //    if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
    //    {
    //        message.gameObject.SetActive(false);
    //        error.gameObject.SetActive(true);
    //        error.text = "Server Error: " + www.error;
    //        yield break;
    //    }

    //    username.gameObject.SetActive(true);
    //    password.gameObject.SetActive(true);
    //    confirmPassword.gameObject.SetActive(false);
    //    email.gameObject.SetActive(false);
    //    forgotPasswordBtn.gameObject.SetActive(true);
    //    createNewBtn.SetActive(true);
    //    signInBtn.SetActive(false);
    //    loginBtn.SetActive(true);
    //    changePasswordBtn.SetActive(false);
    //    createAccountBtn.SetActive(false);
    //    message.gameObject.SetActive(false);
    //    error.gameObject.SetActive(false);

    //    Debug.Log(www.downloadHandler.text);
    //}

    IEnumerator UpdateScore(int score)
    {
        ScoreData body = new ScoreData(score);
        string json = JsonUtility.ToJson(body);

        UnityWebRequest request = new UnityWebRequest(baseURL + "/score", "POST");
        byte[] raw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse res = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);

            if (res.success)
            {
                StartCoroutine(GetMe());
            }
            else
            {
                Debug.LogError("Failed to update score: " + res.message);
            }
        }
        else
        {
            Debug.LogError("Network Error: " + request.error);
        }
    }
    IEnumerator DeleteAccount()
    {
        UnityWebRequest request = UnityWebRequest.Delete(baseURL + "/delete/" + playerId);

        request.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return request.SendWebRequest();

        SceneManager.LoadScene("SampleScene");
    }

    public IEnumerator GetMe()
    {
        UnityWebRequest request = UnityWebRequest.Get(baseURL + "/me");
        request.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse res = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
            if (res.success)
            {
                Debug.Log("Player ID: " + res.data._id);
                playerId = res.data._id;
                PlayerPrefs.SetString("PLAYER_ID", playerId);

                killsCount.text = res.data.score.ToString();
                Debug.Log("Player ID and score retrieved via GetMe(): " + playerId);
            }
            else
            {
                Debug.LogError("Failed to get player info: " + res.message);
            }
        }
        else
        {
            Debug.LogError("Network Error: " + request.error);
        }
    }

    public IEnumerator GetAllUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(baseURL + "/all");
        request.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            PlayersResponse res = JsonUtility.FromJson<PlayersResponse>(request.downloadHandler.text);

            if (res.success)
            {
                allPlayers.text = ""; // clear previous
                foreach (var p in res.data)
                {
                    allPlayers.text += $"Username: {p.username}\nScore: {p.score}\n\n";
                }
            }
            else
            {
                Debug.LogError("Failed to get users: " + res.success);
            }
        }
        else
        {
            Debug.LogError("Network Error: " + request.error);
        }
    }

    public IEnumerator GetUserById(string id)
    {
        UnityWebRequest request = UnityWebRequest.Get(baseURL + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse res = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);

            if (res.success)
            {
                playerByID.text = ""; // clear previous
                playerByID.text += $"Username: {res.data.username}\nScore: {res.data.score}\n\n";
            }
            else
            {
                Debug.LogError("Failed to get user: " + res.message);
            }
        }
        else
        {
            Debug.LogError("Network Error: " + request.error);
        }

        Debug.Log("USER INFO: " + request.downloadHandler.text);
    }

    //Request Sender
    IEnumerator SendRequest(string endpoint, string method, object body, bool useAuth)
    {
        string json = JsonUtility.ToJson(body);

        UnityWebRequest request = new UnityWebRequest(baseURL + endpoint, method);
        byte[] raw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        if (useAuth)
        {
            request.SetRequestHeader("Authorization", "Bearer " + GetToken());
        }

        yield return request.SendWebRequest();

        Debug.Log(endpoint + " RESPONSE: " + request.downloadHandler.text);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
    }

    //Token Handler
    private string GetToken()
    {
        if (string.IsNullOrEmpty(token))
        {
            token = PlayerPrefs.GetString("TOKEN");
        }
        return token;
    }

    //Request Bodies
    [System.Serializable]
    class RegisterData
    {
        public string username, email, password;
        public RegisterData(string u, string e, string p)
        {
            username = u;
            email = e;
            password = p;
        }
    }

    [System.Serializable]
    class LoginData
    {
        public string username, password;
        public LoginData(string u, string p)
        {
            username = u;
            password = p;
        }
    }

    [System.Serializable]
    class ScoreData
    {
        public int score;
        public ScoreData(int s)
        {
            score = s;
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string _id;
    public string username;
    public string email;
    public int score;
}

[System.Serializable]
public class AuthResponse
{
    public bool success;
    public string message;
    public string token;
    public PlayerData data;
}

[System.Serializable]
public class PlayersResponse
{
    public bool success;
    public int count;
    public PlayerData[] data;
}

[System.Serializable]
public class ScoreData
{
    public int score;
    public ScoreData(int s) { score = s; }
}

//[System.Serializable]
//public class PasswordUpdateRequest
//{
//    public string username;
//    public string newPassword;
//    public string confirmPassword;
//}

