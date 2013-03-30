using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;
namespace CookieMonster.CookieMonster_Objects
{
    class Timer : IDisposable
    {
        static protected Game engine { get { return engineReference.getEngine(); } }
        static Timers_Manager timeMgr;
        /// <summary>
        /// Zero Timer is special timer that will always return current time as 0
        /// this is very handy and needed sometimes 
        /// (fe. your actually Timer just been disabled
        /// but you're setting some values based on that timmer current_time/total_time) then you can use zeroTimer
        /// </summary>
        static public Timer zeroTimer = new Timer(eUnits.MSEC, 1);
        public enum eUnits { MSEC, FPS }
        private eUnits type;
        private Int64 initTime, initTimeVar;
        private Int64 time; // in units of #type# when time hits 0 Timer will be destructed
                            // if its decreasing timer.
        public Int64 currentTime { get { if (this == zeroTimer) return 0; else return time; } set { time = value; } }
        public Int64 totalTime   { get { return initTime; } }
        /// <summary>
        /// Part of timer that is done (for multipiling and stuff) 
        /// </summary>
        /// <returns>part done in range 0.0-1.0</returns>
        public float partDone { get { return (float)currentTime / totalTime; } }
        public float partDoneReverse { get { return 1f-partDone; } }
        private int iterator;

        private bool recreateOnDestroy;
        private bool autostartOnRecreate;
        private bool _enabled;
        public bool isIngameTimer{get; private set;}//if flag is set to true, stop counting when game is pause
        public bool enabled{
            get { return _enabled;}
        }

        #region Timer_Constructors
        /// <summary>
        /// Construct Timer
        /// </summary>
        /// <param name="t"></param>
        /// <param name="timeAvg"></param>
        public Timer(eUnits t, int timeAvg)
        {
            initTime = (Int64)timeAvg;
            type = t; time = initTime;
            setIterator();
        }
        /// <summary>
        /// Construct Timer
        /// </summary>  
        /// <param name="t"></param>
        /// <param name="timeAvg"></param>
        public Timer(eUnits t, Int64 timeAvg)
        {
            initTime = timeAvg;
            type = t; time = initTime;
            setIterator();
        }
        /// <summary>
        /// Construct timer with random variable value by
        /// formula time = timeAvg +/- timeVar
        /// </summary>
        /// <param name="t"></param>
        /// <param name="timeAvg"></param>
        /// <param name="timeVar"></param>
        public Timer(eUnits t, int timeAvg, int timeVar) : this(t,timeAvg)
        {
            initTimeVar = (Int64)timeVar;

            int rnd = new Random().Next(timeVar * 2) - timeVar;
            time = timeAvg + rnd;
        }
        /// <summary>
        /// Construct timer with variable time and option recreation of same timer at destruction
        /// </summary>
        /// <param name="t"></param>
        /// <param name="timeAvg"></param>
        /// <param name="timeVar"></param>
        /// <param name="recreate">Recreate timer at destruction?</param>
        /// <param name="recAutostart">Auto start timer created on destruction?</param>
        
        public Timer(eUnits t, int timeAvg, int timeVar,bool recreate,bool recAutostart) : this(t,timeAvg,timeVar)
        {
            recreateOnDestroy = recreate;
            autostartOnRecreate = recAutostart;
        }
        /// <summary>
        /// Construct timer with random variable value by
        /// formula time = timeAvg +/- timeVar
        /// </summary>
        /// <param name="t"></param>
        /// <param name="timeAvg"></param>
        /// <param name="timeVar"></param>
        public Timer(eUnits t, Int64 timeAvg, Int64 timeVar) : this(t,timeAvg)
        {
            initTimeVar = timeVar;

            Int64 rnd = new Random().Next((int)timeVar * 2) - timeVar;
            time = timeAvg + rnd;
            setIterator();
        }
        #endregion

        /// <summary>
        /// function will check if timer is ingame timer
        /// (if that's true timer need to be stoped when game is paused)
        /// 
        /// </summary>
        private void isGameTimerOrNot()
        {
            if (engine.gameManager == null || !((engine.gameState & Game.game_state.Game) == Game.game_state.Game) || engine.gameManager.gamePaused)
                isIngameTimer = false;
            else
                isIngameTimer = true;
        }
        /// <summary>
        /// Launch the timmer (adding it to timers list in game class)
        /// </summary>
        public void start()
        {
            isGameTimerOrNot();
            if (_enabled == false)
            {
                _enabled = true;
                if (type == eUnits.FPS)
                    timeMgr.addFpsTimer(this);
                else if (type == eUnits.MSEC)
                    timeMgr.addMsecTimer(this);
            }
        }
        /// <summary>
        /// Reset elapsed time and launch the timmer (adding it to timers list in game class)
        /// </summary>
        public void restart()
        {
            this.time = initTime;
            start();
           
        }
        /// <summary>
        /// Stop the timer (releasing it from timers list in game class)
        /// if object was found on List, function returns true;
        /// </summary>
        /// <returns></returns>
        public bool stop()
        {
            _enabled = false;
            if (type == eUnits.FPS)
                return timeMgr.removeFpsTimer(this);
            else if (type == eUnits.MSEC)
                return timeMgr.removeMsecTimer(this);
            return false;
        }
        public void Update(long time_passed)
        {
            time += iterator*time_passed;
            
            // time was runing from certain time eg. 1000ms
            // till reach of 0ms
            if (time <= 0 && iterator < 0)
            {
                if (recreateOnDestroy)
                {
                    long rnd = new Random().Next((int)initTimeVar * 2) - initTimeVar;
                    time = initTime + rnd;
                    if (!autostartOnRecreate)
                    {
                        this.stop();
                    }
                }
                else // remove timer:
                {
                    this.stop();
                    Dispose();
                    GC.SuppressFinalize(this);
                };
            };

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tm"></param>
        public static void setTimerManager(Timers_Manager tm)
        {
            if(timeMgr == null)//on instance only!
              timeMgr = tm;
        }
        public void Dispose()
        {
            if (type == eUnits.MSEC)
                timeMgr.removeMsecTimer(this);
            else if (type == eUnits.FPS)
                timeMgr.removeMsecTimer(this);
        }
        #region bool_operators
        public static bool operator ==(Timer x, long y)
        {
            return (x.time == y);
        }
        public static bool operator <=(Timer x, long y)
        {
            return (x.time <= y);
        }
        public static bool operator >=(Timer x, long y)
        {
            return (x.time >= y);
        }
        public static bool operator !=(Timer x, long y)
        {
            return (x.time != y);
        }
        public static bool operator <(Timer x, long y)
        {
            return (x.time < y);
        }
        public static bool operator >(Timer x, long y)
        {
            return (x.time > y);
        }
        public static bool operator ==(Timer x, Timer y)
        {
            if (((object)x == null) || ((object)y == null)) return ((object)x == (object)y);
            return (x.time == y.time);
        }
        public static bool operator !=(Timer x, Timer y)
        {
            if (((object)x == null) || ((object)y == null)) return ((object)x != (object)y);

            return (x.time != y.time);
        }
        public static bool operator <=(Timer x, Timer y)
        {
            return (x.time <= y.time);
        }
        public static bool operator >=(Timer x, Timer y)
        {
            return (x.time >= y.time);
        }
        public static bool operator <(Timer x, Timer y)
        {
            return (x.time < y.time);
        }
        public static bool operator >(Timer x, Timer y)
        {
            return (x.time > y.time);
        }
#endregion
        private void setIterator()
        {
            if (time > 0) iterator = -1;
            else iterator = 1;
        }
        public override string ToString()
        {
            return currentTime.ToString() + " / " + totalTime.ToString() + " (" + ((type == eUnits.FPS) ? "fps)" : "msec)");
        } 
    }

    /// <summary>
    /// Timers_Manager is a class handling all Timer(s) added to game
    /// </summary>
    class Timers_Manager : engineReference
    {
        private System.Diagnostics.Stopwatch msecMeasure = new System.Diagnostics.Stopwatch();
        private List<Timer> fpsTimers = new List<Timer>();
        private List<Timer> msecTimers = new List<Timer>();

        public Timers_Manager()
        {
            Timer.setTimerManager(this);
        }
        public void addFpsTimer(Timer t)
        {
            fpsTimers.Add(t);
        }
        public void addMsecTimer(Timer t)
        {
            msecTimers.Add(t);
        }
        public bool removeFpsTimer(Timer t)
        {
            return fpsTimers.Remove(t);
        }
        public bool removeMsecTimer(Timer t)
        {
            return msecTimers.Remove(t);
        }
        /// <summary>
        /// Update all timers on list, called by Game.OnUpdateFrame()
        /// </summary>
        public void Update()
        {
            msecMeasure.Stop();
            long msec_elapsed = msecMeasure.ElapsedMilliseconds;

            // update lists:
            for (int i = 0; i < msecTimers.Count; i++)
            {
                if (!msecTimers[i].isIngameTimer || (engine.gameManager!=null && !engine.gameManager.gamePaused))
                    msecTimers[i].Update(msec_elapsed);
            }
            for (int i = 0; i < fpsTimers.Count; i++)
            {
                if (!fpsTimers[i].isIngameTimer || (engine.gameManager != null && !engine.gameManager.gamePaused))
                    fpsTimers[i].Update(1); // 1 fps passed
            }

            msecMeasure.Reset();
            msecMeasure.Start();
        }


    }
}
 