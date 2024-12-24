using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Plugins.Base.Managers
{
    public class TimerManager
    {
        private static TimerManager instance;

        public static TimerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TimerManager();
                }
                return instance;
            }
        }

        private readonly Dictionary<string, Schedule> schedules;

        private TimerManager()
        {
            schedules = new Dictionary<string, Schedule>();
        }

        public void AddSchedule(Action action, TimeSpan timeSpan)
        {
            string name = Guid.NewGuid().ToString().Replace("-", "");
            AddSchedule(name, action, timeSpan);
        }

        public void AddSchedule(string name, Action action, TimeSpan timeSpan)
        {
            if (schedules.TryGetValue(name, out Schedule schedule)
                && schedule.Interval == (int)timeSpan.TotalMilliseconds)
            {
                schedule.Actions.Add(action);
            }
            schedules.Add(name, new Schedule(name, timeSpan, action));
        }

        public void RemoveSchedule(string name)
        {
            if (schedules.TryGetValue(name, out Schedule schedule))
            {
                schedules.Remove(name);
                schedule.Timer.Dispose();
            }
        }

        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        public string GetNowDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); ;
        }
    }

    class Schedule
    {
        public Schedule(string name, TimeSpan timeSpan, Action action)
        {
            Name = name;
            Interval = (int)timeSpan.TotalMilliseconds;
            Actions = new List<Action>()
        {
            action
        };
            Timer = new Timer(ExecuteActions, null, 0, Interval);
        }

        public string Name { get; set; }

        public Timer Timer { get; private set; }

        public int Interval { get; private set; }

        public List<Action> Actions { get; private set; }

        private void ExecuteActions(object _)
        {
            Actions.ForEach(i =>
            {
                try
                {
                    i();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }
    }
}
