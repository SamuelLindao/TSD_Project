using System;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using Gma.System.MouseKeyHook;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Threading;
using FireSharp;
using FireSharp .Config;
using FireSharp.Extensions;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.Security.Cryptography;
using System.Text;

namespace TSD_Project
{
    public partial class App : Application
    {
        private IKeyboardMouseEvents _globalHook;
        private TrackedInfo TrackedInfo { get; set; } = new TrackedInfo();
        private List<KeyboardInfo> _trackedKeyboard = new List<KeyboardInfo>();
        private List<MouseInfo> _trackedMouse = new List<MouseInfo>();
        private Dictionary<string, ServerTalk.DataTasks> _myTasks = new Dictionary<string, ServerTalk.DataTasks>();
        private bool TrackerStarted { get; set; } = false;

        public string UserToken = "SamuelRx";
        public string UserProject = "InitialProject";
        private DispatcherTimer _timer = new DispatcherTimer();
        private int _inativeSeconds = 0;

        public float GetMonthlyProgress()
        {
            return (float)(TrackedInfo.UsageTime * TrackedInfo.WorkSchedule.Count())/(float)(TrackedInfo.LimitTime * TrackedInfo.WorkSchedule.Count());
        }
        public int GetUsageTime()
        {
            return TrackedInfo.UsageTime;
        }

        public bool InteractTrackButton()
        {

            TrackerStarted = !TrackerStarted;
            if (TrackedInfo.InitialTime.Day != DateTime.UtcNow.Day &&
                TrackedInfo.InitialTime.Month != DateTime.UtcNow.Month)
            {
                Console.WriteLine("Oi");
                TrackedInfo.InitialTime = DateTime.UtcNow;
            }

            if (!TrackerStarted)
                SendAllInfo();
            return TrackerStarted;
        }

        public TrackedInfo GetTrackedInfo()
        {
            return TrackedInfo;
        }

        public readonly ServerTalk Talk = new ServerTalk();

        private void OnStartup(object sender, StartupEventArgs e)
        {
            Console.WriteLine("Waiting for Start!");
            Talk.LoadDataBase();
            Talk.LoadDataBaseTasks();
            Talk.LoadDataBaseTokens();
        }

        public void StartApp()
        {
            InitializeGlobalKeyboardHook();
            GetTasks();
            SaveLocal();
            TrackedInfo = new TrackedInfo();     //Precisa ser pega no banco de dados a informação de tracker.
            
            //_trackedInfo.InitialTime = DateTime.UtcNow;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += VerifyInactive;
            _timer.Start();
        }
        private void VerifyInactive(object sender, EventArgs e)
        {
            if (!TrackerStarted && TrackedInfo.InitialTime.Year == DateTime.Now.Year)
                _inativeSeconds++;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            if (_globalHook != null)
            {
                _globalHook.KeyDown -= GlobalHook_KeyDown;
                _globalHook.MouseDown -= GlobalHook_MouseDown;
                _globalHook.Dispose();
            }
        }

        private void InitializeGlobalKeyboardHook()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
            _globalHook.MouseDown += GlobalHook_MouseDown;

        }

        private void GlobalHook_MouseDown(object send, System.Windows.Forms.MouseEventArgs e)
        {
            if (IsTimeToWork() && TrackerStarted)
            {
                MouseInfo info = new MouseInfo();
                info.MouseString = e.Button.ToString();
                info.MouseClicks = e.Clicks;
                info.MousePosition = e.Location.ToString();
                info.TimeSeconds = TimeDeal.GetDateSeconds();
                _trackedMouse.Add(info);
            }
        }

        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (IsTimeToWork() && TrackerStarted)
            {
                KeyboardInfo info = new KeyboardInfo();
                info.KeyString = e.KeyData.ToString();
                info.KeyValue = e.KeyValue;
                info.TimeSeconds = TimeDeal.GetDateSeconds();
                _trackedKeyboard.Add(info);
            }
        }

        private async void SendAllInfo()
        {
            MainWindow mainWindow = this.MainWindow as MainWindow;
            if (TrackedInfo.UsageTime == 0 ) return;
            if (!mainWindow.IsTokenValid()) return;
            string concatenetedStrings = "";
            string times = "";
            for (int i = 0; i < _trackedKeyboard.Count; i++)
            {
                concatenetedStrings += "_" + _trackedKeyboard[i].KeyString;
                times += "_" + _trackedKeyboard[i].TimeSeconds;
            }

            string mouseTimes = "";
            string mousePosA = "";
            for (int i = 0; i < _trackedMouse.Count; i++)
            {
                mouseTimes += "_" + _trackedMouse[i].TimeSeconds;
                mousePosA += "_" + _trackedMouse[i].MousePosition + "_" + _trackedMouse[i].MouseString;
            }

            var dataBoard = new ServerTalk.Data()
            {
                UserHash = UserToken,
                KeyStrings = concatenetedStrings,
                KeyNumbers = _trackedKeyboard.Count,
                TimeSecondsKeyboard = times,
                MouseClicks = _trackedMouse.Count,
                MousePos = mousePosA,
                TimeSecondsMouse = mouseTimes,
                UsageTime = TrackedInfo.UsageTime
            };
            Random rnd = new Random();

            string id = Guid.NewGuid().ToString();
            SetResponse response =
                await Talk._client.SetTaskAsync(
                    "TrackedInfo_" + UserToken + "/" + "_" + TrackedInfo.UsageTime + "_" + id, dataBoard);
            ServerTalk.Data result = response.ResultAs<ServerTalk.Data>();

            var app = (App)System.Windows.Application.Current;
            TrackedInfo tracked = app.GetTrackedInfo();
            TimeSpan time = TimeSpan.FromSeconds(TrackedInfo.UsageTime);
            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                (int)time.TotalHours,
                time.Minutes,
                time.Seconds);

            if (TrackerStarted && mainWindow != null)
                mainWindow.AddTrackedTime(formattedTime, _trackedKeyboard.Count.ToString(),
                    _trackedMouse.Count.ToString());

            _trackedKeyboard.Clear();
            _trackedMouse.Clear();
            Console.WriteLine("Sent!" + result.UserHash);
        }

        private DateTime _currentHour = DateTime.UtcNow;
        private int _offsetTime = 0;
        private int _realTime = 0;
        private const int TimeToTrack = 5;

        public void Update_TrackInfo()
        {    
            if (TrackerStarted && TrackedInfo.InitialTime.Year != DateTime.Now.Year) return;

            _currentHour = DateTime.UtcNow;
            
            TrackedInfo.UsageTime = (TimeDeal.GetDateSeconds() - TimeDeal.GetCustomDate(TrackedInfo.InitialTime)) -
                                     _inativeSeconds;
            _realTime = TrackedInfo.UsageTime - _offsetTime;
            TrackedInfo.CurrentDay = _currentHour.Day - TrackedInfo.InitialTime.Day;
            if (_realTime > TimeToTrack)
            {
                _offsetTime += _realTime;
                _realTime = 0;
                SendAllInfo();
            }

            Console.WriteLine(TrackedInfo.UsageTime + " : " + _realTime + " : " + _offsetTime);
        }

        public bool IsTimeToWork()
        {
            if (!TrackedInfo.HasLimitTime)
            {
                return true;
            }
            else
            {
                return TrackedInfo.WorkSchedule.Any(x => x.DayToWork == _currentHour.DayOfWeek) && TrackedInfo.UsageTime < TrackedInfo.LimitTime;
            }
        }

        public void SendCompletedTasks()
        {
            Console.WriteLine("All Task Send");
        }

        private async void GetTasks()
        {
            try
            {
                FirebaseResponse response = await Task.Run(() => Talk._clientTasks.Get("Tasks_" + UserToken));
                Dictionary<string, ServerTalk.DataTasks> tasks =
                    response.ResultAs<Dictionary<string, ServerTalk.DataTasks>>();
                _myTasks = tasks;
                MainWindow mainWindow = this.MainWindow as MainWindow;
                foreach (var kvp in _myTasks)
                {
                    TasksClass newTask = new TasksClass()
                    {
                        TaskAuthor = kvp.Value.TaskAuthor,
                        TaskDescription = kvp.Value.TaskDescription,
                        TaskName = kvp.Value.TaskName,
                        Completed = kvp.Value.Completed,
                        TaskTime = kvp.Value.TaskTime
                    };

                    Console.WriteLine("Processando tarefa: " + newTask.TaskName);

                    if (!newTask.Completed)
                    {
                        mainWindow.AddItem(newTask.TaskName, newTask.TaskDescription, newTask.TaskAuthor,
                            newTask.TaskTime);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Don't have tasks!");
            }
        }
        private void SaveLocal()
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(executablePath, "primaryToken.txt");
            
            string content = UserToken;

                content = SecurityCheck.EncryptString(content, "senha_secreta");
            Console.WriteLine(content);
            File.WriteAllText(filePath, content);
        }
        
    }
  
}   