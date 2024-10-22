using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    [Header("Login")]
    public TMP_InputField LoginEmail;
    public TMP_InputField LoginPassword;

    [Header("Signup")]
    public TMP_InputField SignupEmail;
    public TMP_InputField SignupPassword;
    public TMP_InputField SignupConfirmPassword;

    [Header("Popups")]
    public GameObject LoginPopup;
    public GameObject SignupPopup;
    private FirebaseAuth auth;

    [Header("Texts")]
    public TMP_Text MessageText;
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        InitializeFirebase();
    }

    // Initialize Firebase
    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            Debug.Log("Firebase is ready");
        });
    }
    public IEnumerator ShowMessage(string _msg, Color _clr)
    {
        MessageText.text = _msg;
        MessageText.color = _clr;
        yield return new WaitForSeconds(2.5f);
        MessageText.text = "";
    }
    // Sign up method
    public void SignUp()
    {
        string email = SignupEmail.text;
        string password = SignupPassword.text;
        string confirmPassword = SignupConfirmPassword.text;
        if(password != confirmPassword)
        {
            //Debug Passwords don't match
            StartCoroutine(ShowMessage("Passwords doesn't match!!!", Color.red));
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignUp was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                StartCoroutine(ShowMessage("Something went wrong", Color.red));
                Debug.LogError("SignUp encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            //Show message to ask for user to login
            StartCoroutine(ShowMessage("Congratulations!!!\n Your account has been created successfully.\nLogin Now.", Color.green));
            SignupPopup.SetActive(false);
            LoginPopup.SetActive(true);
            Debug.LogFormat("User created successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
    }

    // Sign in method
    public void SignIn()
    {
        string email = LoginEmail.text;
        string password = LoginPassword.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignIn was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignIn encountered an error: " + task.Exception);
                StartCoroutine(ShowMessage("Incorrect Email/Password", Color.red));
                return;
            }

            AuthResult result = task.Result;
            StartCoroutine(ShowMessage("Logged in successfully.", Color.green));
            OnSignInSuccess(result.User.UserId);
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
    }
    public void ResetPassword()
    {
        string email = LoginEmail.text;
        if(string.IsNullOrEmpty(email))
        {
            //Show message
            StartCoroutine(ShowMessage("Email field is empty.", Color.red));
            return;
        }

        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("Password reset was canceled.");
                StartCoroutine(ShowMessage("Password reset was canceled.", Color.red));
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Password reset encountered an error: " + task.Exception);
                StartCoroutine(ShowMessage("Something went wrong.", Color.red));
                return;
            }

            StartCoroutine(ShowMessage("Password reset email sent successfully.", Color.green));
            Debug.Log("Password reset email sent successfully.");
        });
    }
    void OnSignInSuccess(string userid)
    {
        PlayerPrefs.SetString("FirebaseUserId", userid);
        SceneManager.LoadScene(1);
    }
}
