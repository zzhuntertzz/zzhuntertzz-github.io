using System;

#if USING_FIREBASE
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Installations;
#endif

public class FirebaseController : SinglePrivaton<FirebaseController>
{
    public static Action OnFirebaseInitialized = delegate { };
    public static bool IsFirebaseReadied { get; private set; } = false;

#if USING_FIREBASE
    private FirebaseInstallations _installations;
    private DependencyStatus _dependencyStatus;
    private FirebaseApp app;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            _dependencyStatus = task.Result;
            Debug.Log("CheckAndFixDependenciesAsync " + _dependencyStatus);
            if (_dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                IsFirebaseReadied = true;
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + _dependencyStatus);
                Init();
            }
        });
    }
#endif

    public void Initialize() { }

#if USING_FIREBASE
    public void SetUserProperty(string property, string value)
    {
        FirebaseAnalytics.SetUserProperty(property, value);
    }

    public void SetUserLevel(int level)
    {
        if (_dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Firebase is not readied to log event " + Time.time);
            return;
        }

        FirebaseAnalytics.SetUserProperty("level", level.ToString());
    }

    public void SetUserLastScreen(string screen)
    {
        if (_dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Firebase is not readied to log event " + Time.time);
            return;
        }

        FirebaseAnalytics.SetUserProperty("last_screen", screen);
    }

    private void InitializeFirebase()
    {
        InitFireBaseAnalytics();
        _installations = FirebaseInstallations.DefaultInstance;
        GetIdAsync();

        OnFirebaseInitialized?.Invoke();
    }

    private void InitFireBaseAnalytics()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
    }

    private Task GetIdAsync()
    {
        return _installations.GetIdAsync().ContinueWithOnMainThread(task =>
        {
            if (LogTaskCompletion(task, nameof(GetIdAsync)))
            {
                Debug.LogFormat(String.Format("Installations {0}", task.Result));
            }
        });
    }

    private bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            Debug.LogFormat(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            Debug.LogFormat(operation + " encountered an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string errorCode = "";
                FirebaseException firebaseException = exception as FirebaseException;
                if (firebaseException != null)
                {
                    errorCode = String.Format("Error code={0}: ",
                        firebaseException.ErrorCode.ToString(),
                        firebaseException.Message);
                }
                Debug.LogFormat(errorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            Debug.LogFormat(operation + " completed");
            complete = true;
        }
        return complete;
    }
#endif
}