using System.Windows;

namespace TSD_Project;

public class TrackedInfo
{
    public int CurrentDay = 0;
    public int UsageTime = 0;
    public int LimitTime = 0;
    public bool HasLimitTime = false;
    
    
    public List<WorkScheduleDay> WorkSchedule = new List<WorkScheduleDay>();
    public DateTime InitialTime = new DateTime();
}

public class WorkScheduleDay()
{
    public DayOfWeek DayToWork = DayOfWeek.Monday;
    public int StartHourSeconds = 0;
    public int FinishHourSeconds = 0;
    
}
public class KeyboardInfo
{
    public string KeyString = "";
    public int KeyValue = 0;
    public int TimeSeconds = 0;
    
}

public class MouseInfo
{
    public string MouseString = "";
    public int MouseClicks = 0;
    public string MousePosition = "";
    public int TimeSeconds = 0;

}

public class TimeDeal
{
    public static int GetDateSeconds()
    {
        int timeSeconds = 0;
        timeSeconds += DateTime.UtcNow.Hour * 60 * 60;
        timeSeconds += DateTime.UtcNow.Minute * 60;
        timeSeconds += DateTime.UtcNow.Second;
        return timeSeconds;
    }

    public static int GetCustomDate(DateTime time)
    {
        int timeSeconds = 0;
        timeSeconds += time.Hour * 60 * 60;
        timeSeconds += time.Minute * 60;
        timeSeconds += time.Second;
        return timeSeconds;
    }
    
}