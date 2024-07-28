using System.Windows;
using FireSharp;
using FireSharp .Config;
using FireSharp.Interfaces;
using FireSharp.Response;


namespace TSD_Project;

public class ServerTalk
{
    private IFirebaseConfig _config = new FirebaseConfig()
    {
        AuthSecret = "20XHfSp7Ccl5Zv40kPXmIm0Qci63deeSmG2ccS5W",
        BasePath = "https://tsd-project-85b0e-default-rtdb.firebaseio.com/"
    };

    public IFirebaseClient _client;
    
    //Tasks Database
    private IFirebaseConfig _configTasks = new FirebaseConfig()
    {
        AuthSecret = "5DrvBEykOcsKoO4q589TTekGZkovCQx3TW9IiqLg",    
        BasePath ="https://tsd-project-taskinfo-default-rtdb.firebaseio.com/"
    };

    public IFirebaseClient _clientTasks;

    
    private IFirebaseConfig _configToken = new FirebaseConfig()
    {
        AuthSecret = "lhrhwSofri5yplaZqWobMVS0P3nQhnVhdEDjr7BA",    
        BasePath ="https://tsd-project-tokens-default-rtdb.firebaseio.com/"
    };

    public IFirebaseClient _clientToken;

    public void LoadDataBaseTokens()
    {
        _clientToken = new FirebaseClient(_configToken);
        if (_clientToken != null)
        {
            Console.WriteLine("Connection with DataBase Tokens Sucessfully!");
        }
    }
    public void LoadDataBaseTasks()
    {
        
        _clientTasks = new FirebaseClient(_configTasks);
        if (_clientTasks != null)
        {
            Console.WriteLine("Connection with DataBase Tasks Sucessfully!");
        }
    }
    public void LoadDataBase()
    {
        _client = new FirebaseClient(_config);
        if (_client != null)
        {
            Console.WriteLine("Connection with DataBase Sucessfully!");
        }
    }

    public class DataTasks()
    {
        public bool Completed { get; set; } = false;
        public string TaskAuthor { get; set; } = "";
        public string TaskDescription { get; set; } = "";
        public string TaskName { get; set; } = "";
        public string TaskTime { get; set; } = "";
    }
    public class Data()
    {
        public string UserHash { get; set; } = "";
        public string KeyStrings { get; set; } = "";
        public int KeyNumbers { get; set; } = 0;
        public string TimeSecondsKeyboard { get; set; } = "";
        public string MousePos { get; set; } = "";
        public int MouseClicks { get; set; } = 0;
        public string TimeSecondsMouse { get; set; } = "";
        public int UsageTime { get; set; } = 0;
    }

    
}