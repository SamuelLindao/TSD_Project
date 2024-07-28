using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using FireSharp.Response;
using Newtonsoft.Json.Serialization;
using Notification.Wpf;
using Notification.Wpf.Base;
using RestSharp.Extensions;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace TSD_Project;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

public partial class MainWindow : Window
{
    private List<int> _objToSend = new List<int>();
    private DispatcherTimer _timer; 
    private NotifyIcon _notifyIcon;
    private const string AppName = "TSD_Project v0.0.1";
    private bool _isStarted = false;
    private bool _not = false;
    private bool _tokenValid = false;
    private Uri _iconUri =  new Uri("C:\\Users\\lacer\\RiderProjects\\TSD_Project\\TSD_Project\\brasao_uesc-01.ico");
    private BitmapImage IconBitMap = new BitmapImage();
    App _app = (App)System.Windows.Application.Current;

    enum ProgressTypeEnum
    {
        daily,
        weekly,
        monthly
    };

    private ProgressTypeEnum _progressType = ProgressTypeEnum.daily;
    public MainWindow()
    {
        InitializeComponent();
        IconBitMap = new BitmapImage(_iconUri);
        SendButton.Visibility = Visibility.Hidden;
        Title = AppName;
        GridOptionsActive(GridTasks);
        
        _notifyIcon = new NotifyIcon
        {
            Icon = new System.Drawing.Icon("C:\\Users\\lacer\\RiderProjects\\TSD_Project\\TSD_Project\\brasao_uesc-01.ico"), 
            Visible = true,
            Text = AppName
        };
        VerifyLocalToken();
        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1); 
        _timer.Tick += UpdateText;
        _timer.Start();
    }

    private void HamburgerButton_Unchecked(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void UpdateText(object sender, EventArgs e)
    {
        if (!_isStarted) return;
        _app.Update_TrackInfo();
        TrackedInfo tracked = _app.GetTrackedInfo();
        TimeSpan time = TimeSpan.FromSeconds(_app.GetUsageTime());
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", 
            (int)time.TotalHours, 
            time.Minutes, 
            time.Seconds);
        TimerText.Content = formattedTime;
        switch (_progressType)
        {
            case ProgressTypeEnum.daily:
                TimeProgress.Value = (float)_app.GetUsageTime() / 250 * 100;
                break;
            case ProgressTypeEnum.weekly:
                break;
            case ProgressTypeEnum.monthly:
                break;
        }
    }
    private void NotifyIcon_DoubleClick(object sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        _notifyIcon.Visible = false;
    }

    protected override void OnStateChanged(EventArgs e) 
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            if (!_not)
            {
                var notificationManager = new NotificationManager();
                if (!_isStarted) 
                    notificationManager.Show(new NotificationContent 
                        {
                            Title = "AppName",
                            Message = "Tracker Minimized",
                            Type = NotificationType.Information,
                            Icon = IconBitMap,
                            Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#121417"))
                        },
                        onClick: () => { this.Show(); });
                else
                {
                    TimeSpan time = TimeSpan.FromSeconds(_app.GetUsageTime());
                    string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        (int)time.TotalHours,
                        time.Minutes,
                        time.Seconds);
                    TimerText.Content = formattedTime;
                    notificationManager.Show(new NotificationContent
                    {
                        Title = AppName,
                        Message  = "Tracker Minimized : " + formattedTime,
                        Type = NotificationType.Information,
                        Icon = IconBitMap,
                        Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#121417"))
                    } , onClick: () => { this.Show(); });
                
                    _not = true;
                }
            }
            _notifyIcon.Visible = true;
        }
        base.OnStateChanged(e);
    }
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var notificationManager = new NotificationManager();
        
        notificationManager.Show(new NotificationContent
        {
            Title = AppName,
            Message  = "Tracker Closed",
            Type = NotificationType.Information,
            Icon = IconBitMap,
            Background = Brushes.White,
            Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#121417")),
        });
        _notifyIcon.Visible = false;
    }

    protected override void OnClosed(EventArgs e)
    {
        _notifyIcon.Dispose();
        base.OnClosed(e);
    }

    private void TrackStart(object sender, RoutedEventArgs e)
    {
        Start();
    }

    private void Start()
    {
        var notificationManager = new NotificationManager();
        
        _isStarted = _app.InteractTrackButton();
        if (_isStarted)
        {
            notificationManager.Show(AppName, "Tracker Started" , NotificationType.Information,
                onClick: () => { this.Show(); });
            TrackerButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#15878E"));
            TrackerButton.Content = "Stop Track";
        }
        else
        {
            notificationManager.Show(AppName, "Tracker Stopped" , NotificationType.Information,
                onClick: () => { this.Show(); });

            TrackerButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#32E158"));
            TrackerButton.Content = "Track";
        }   
    }

    public void AddItem(string TaskName, string TaskDescription, string TaskAuthor, string TaskDate)
    {
        var notificationManager = new NotificationManager();
        notificationManager.Show("New Task: " + TaskName, TaskDescription, NotificationType.Notification, onClick: () => {this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            InfoTask.Visibility = InfoTask.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            this.TaskTitle.Text = TaskName;
            this.TaskDescription.Text = TaskDescription;
            this.TaskAuthor.Text = TaskAuthor;
        });
        
        ListViewItem taskSlot = new ListViewItem();
        taskSlot.HorizontalAlignment = HorizontalAlignment.Stretch;
        taskSlot.Name = "TaskSlot";
        taskSlot.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#171717"));
        taskSlot.Height = 50;
        Grid grid = new Grid();
        ColumnDefinition col1 = new ColumnDefinition();
        col1.Width = new GridLength(1, GridUnitType.Star);
        ColumnDefinition col2 = new ColumnDefinition();
        col2.Width = new GridLength(1, GridUnitType.Auto);   
        ColumnDefinition col3 = new ColumnDefinition();
        col2.Width = new GridLength(1, GridUnitType.Auto);    

        grid.ColumnDefinitions.Add(col1);
        grid.ColumnDefinitions.Add(col2);
        grid.ColumnDefinitions.Add(col3);

        TextBlock textBlock = new TextBlock();
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        textBlock.Text = TaskName ;
        textBlock.Width = 210;
        Grid.SetColumn(textBlock, 1);
        
        TextBlock dateText = new TextBlock();
        dateText.VerticalAlignment = VerticalAlignment.Center;
        dateText.HorizontalAlignment = HorizontalAlignment.Right;
        dateText.Width = 110;
        dateText.Text = TaskDate;
        Grid.SetColumn(dateText, 2);
        
        ToggleButton toggleButton = new ToggleButton();
        toggleButton.Height = 30;
        toggleButton.Width = 30;
        toggleButton.Margin = new Thickness(0, 0, 10, 0);
        Grid.SetColumn(toggleButton, 0);
        
        grid.Children.Add(textBlock);
        grid.Children.Add(toggleButton);
        grid.Children.Add(dateText);

        taskSlot.Content = grid;
        int index = _objToSend.Count;
        
        toggleButton.Checked+= (sender, e) =>
        {
            index = _objToSend.Count;
            _objToSend.Add(_objToSend.Count);
            SendButton.Visibility = _objToSend.Count > 0? Visibility.Visible : Visibility.Hidden;
        };
        toggleButton.Unchecked += (sender, e) =>
        {
            _objToSend.RemoveAt(_objToSend[index]);
            SendButton.Visibility = _objToSend.Count > 0? Visibility.Visible : Visibility.Hidden;
        };
        SendButton.Click += (sender, e) =>
        {
            _objToSend.Clear();
            if(toggleButton.IsChecked == true)
                ListTasks.Items.Remove(taskSlot);
            NoTasks.Visibility = ListTasks.Items.Count == 0 ? Visibility.Hidden : Visibility.Visible;
            SendButton.Visibility = _objToSend.Count > 0? Visibility.Visible : Visibility.Hidden;
            SendTaskInfo(TaskName, TaskDescription, TaskAuthor, TaskDate);
            
        };
        taskSlot.MouseDoubleClick += (sender, e) =>
        {
            InfoTask.Visibility = InfoTask.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            this.TaskTitle.Text = TaskName;
            this.TaskDescription.Text = TaskDescription;
            this.TaskAuthor.Text = "Author: " + TaskAuthor;
        };
        ListTasks.Items.Add(taskSlot);
        NoTasks.Visibility = ListTasks.Items.Count == 0 ? Visibility.Hidden : Visibility.Visible;

    }
    public async void SendTaskInfo(string TaskName, string TaskDescription, string TaskAuthor, string TaskDate)
    {
        var data = new ServerTalk.DataTasks
        {
            TaskName = TaskName,
            TaskDescription = TaskDescription,
            TaskAuthor = TaskAuthor,
            TaskTime = TaskDate,
            Completed = true
        };
        SetResponse response = await _app.Talk._clientTasks.SetTaskAsync("Tasks_" +_app.UserToken + "/" + TaskName, data);
        ServerTalk.Data result = response.ResultAs<ServerTalk.Data>();
        NoTasks.Visibility = ListTasks.Items.Count == 0 ? Visibility.Hidden : Visibility.Visible;
    }
    public void AddTrackedTime(string TimeTracked, string KeyboardTracked, string MouseTracked)
    {
        Console.WriteLine("Add Item Ativado!");
        NoTracked.Visibility = Visibility.Hidden;
        ListViewItem taskSlot = new ListViewItem();
        taskSlot.HorizontalAlignment = HorizontalAlignment.Stretch;
        taskSlot.Name = "TrackedSlot";
        taskSlot.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#171717"));
        taskSlot.Height = 50;
        taskSlot.IsHitTestVisible = false;
        
        Grid grid = new Grid();
        ColumnDefinition col1 = new ColumnDefinition();
        col1.Width = new GridLength(320, GridUnitType.Pixel);   
        ColumnDefinition col2 = new ColumnDefinition();
        col2.Width = new GridLength(20, GridUnitType.Pixel);   
        ColumnDefinition col3 = new ColumnDefinition();
        col3.Width = new GridLength(20, GridUnitType.Pixel);   

        grid.ColumnDefinitions.Add(col1);
        grid.ColumnDefinitions.Add(col2);
        grid.ColumnDefinitions.Add(col3);
        
        TextBlock textBlock = new TextBlock();
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        textBlock.Text = TimeTracked ;
        textBlock.FontSize = 16;
        textBlock.Margin = new Thickness(5);
        Grid.SetColumn(textBlock, 0);
        
        TextBlock keyboardTracked = new TextBlock();
        keyboardTracked.VerticalAlignment = VerticalAlignment.Center;
        keyboardTracked.HorizontalAlignment = HorizontalAlignment.Center;
        keyboardTracked.Width = 20;
        keyboardTracked.Text = KeyboardTracked;
        Grid.SetColumn(keyboardTracked, 1);
        
        TextBlock mouseTracked = new TextBlock();
        mouseTracked.VerticalAlignment = VerticalAlignment.Center;
        mouseTracked.HorizontalAlignment = HorizontalAlignment.Center;
        mouseTracked.Width = 20;
        mouseTracked.Text = MouseTracked;
        Grid.SetColumn(mouseTracked, 2);
        
        grid.Children.Add(textBlock);
        grid.Children.Add(mouseTracked);
        grid.Children.Add(keyboardTracked);

        taskSlot.Content = grid;
        TimeUsage.Items.Insert(0,taskSlot);
        
    }
    private void ClosePopup(object sender, RoutedEventArgs e)
    {
        InfoTask.Visibility = Visibility.Hidden;
    }
    
    public bool IsTokenValid()
    {
        return _tokenValid;
    }
    
    private void SetToken(object sender, RoutedEventArgs e)
    {
       VerifyToken(TokenBox.Text);
    }
    
    private async void VerifyToken(string tokenToVerify)
    {
        if(tokenToVerify == String.Empty) return;
        
        try
        {
            ButtonToVerify.IsHitTestVisible = false;
            FirebaseResponse response = await Task.Run(() => _app.Talk._clientToken.GetTaskAsync(tokenToVerify ));
            string responseBody = response.Body.Trim('"');         
            
            Console.WriteLine(responseBody + " Essa aqui é resposta meu rei"); 
            responseBody = response.Body?.ToString() ?? string.Empty;
            bool isResponseBodyEmpty = string.IsNullOrEmpty(responseBody);
            
            if(!isResponseBodyEmpty && responseBody != "null")
            {
                _tokenValid = true;
                _app.UserProject = responseBody.Trim('"');
                MyProject.Text = _app.UserProject;
                _app.UserToken = tokenToVerify;
                ScreenToken.Visibility = Visibility.Hidden;
                _app.StartApp();
            }
            else
            {
                TokenFrame.Visibility = Visibility.Visible;
            }
            ButtonToVerify.IsHitTestVisible = true;

        }
        catch(Exception ex)
        {
            TokenFrame.Visibility = Visibility.Visible;
            Console.WriteLine($"Erro ao verificar o token '{tokenToVerify}': {ex.Message}");
        }
    }
    public void VerifyLocalToken()
    {
        try
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(executablePath, "primaryToken.txt");
            
            Console.WriteLine(filePath);
            if (File.Exists(filePath))
            {
                string readContent = File.ReadAllText(filePath);
                readContent = SecurityCheck.DecryptString(readContent, "senha_secreta");
                Console.WriteLine(readContent);
                VerifyToken(readContent);
            }
            else
            {
                TokenFrame.Visibility = Visibility.Visible;
                Console.WriteLine("File doesn't exist");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void Window_MouseLeft(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void BtnMinimize_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnSettings_OnClick(object sender, RoutedEventArgs e) //Se lembra de modificar o icone
    {
        if (SettingsButton.Visibility != Visibility.Collapsed)
        {
            SettingsButton.Visibility = Visibility.Collapsed;
            BackSettingsButton.Visibility = Visibility.Visible;
            SettingsGrid.Visibility = Visibility.Visible;
        }
        else
        {
            SettingsButton.Visibility = Visibility.Visible;
            BackSettingsButton.Visibility = Visibility.Collapsed;
            SettingsGrid.Visibility = Visibility.Hidden;

        }

    }

    private Grid[] _options;
    private System.Windows.Controls.Button[] _buttons;
    public void GridOptionsActive(Grid option)
    {
        _buttons = new System.Windows.Controls.Button[] { TaskOption, TrackedOption, MeetsOption };
        _options = new Grid[] { GridTasks, GridTracked, GridMeets};
        for(int i =0; i < _options.Length;i++)
        {
            _buttons[i].BorderBrush = _options[i] == option ? Brushes.White : Brushes.DimGray; 
            _options[i].Visibility = _options[i] == option ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    private void TaskOption_OnClick(object sender, RoutedEventArgs e)
    { 
        GridOptionsActive(GridTasks);
    }

    private void TrackedOption_OnClick(object sender, RoutedEventArgs e)
    {
        GridOptionsActive(GridTracked);
    }

    private void MeetsOption_OnClick(object sender, RoutedEventArgs e)
    {
        GridOptionsActive(GridMeets);
    }
}