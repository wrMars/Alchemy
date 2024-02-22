/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using System;
using UnityEngine;

namespace Alchemy
{
    public class AL_TimeMgr:MonoBehaviour
    {
        public static AL_TimeMgr Instance;
        private void Awake()
        {
            Instance = this;
            if (!_syncReady) CorrectTime();
        }
        
        //================需要接入的=====================
        private static Func<bool> _useLocalTime;//项目接入使用时候注意：如果是跟GM白名单相关的，最好是确保调用时候白名单数据已经有了，不然就要在数据到来后CheckCorrectTime
        private static Action<Action<DateTime>, Action> _getServerTime;

        public static void Init(Func<bool> checkUseLocalFunc, Action<Action<DateTime>, Action> getServerTimeFunc)
        {
            _useLocalTime = checkUseLocalFunc;
            _getServerTime = getServerTimeFunc;
            AL_FileLog.LogError("时间控制工具初始化");
            if (Instance && !Instance._syncReady) Instance.CorrectTime();
        }
        
        //===================================================
        
        /// <summary>
        /// 【毫秒级】获取时间（北京时间）
        /// </summary>
        /// <param name="timestamp">10位时间戳</param>
        public static DateTime GetDateTimeMilliseconds_add8(long timestamp)
        {
            return GetDateTimeMilliseconds(timestamp).AddHours(8);
        }
        
        /// <summary>
        /// 【毫秒级】获取时间
        /// </summary>
        /// <param name="timestamp">10位时间戳</param>
        public static DateTime GetDateTimeMilliseconds(long timestamp)
        {
            long time_tricks = _originalDT.Ticks + timestamp * 10000;//日期刻度
            return new DateTime(time_tricks);//转化为DateTime
        }

        private const long      ONE_DAY          = 864000000000;
        public const int        ONE_MINUTE_SECONDS  = 60;
        public const int        ONE_HOUR_SECONDS    = 3600;
        public const int        ONE_DAY_SECONDS     = 86400;
        
        private static DateTime _originalDT = new DateTime(1970, 1, 1, 0, 0, 0);
        private float _noteRealTime = 0f;
        private double _curStandardOffsetSec = 0;
        private double _syncStandardOffsetSec = 0;//从基准时间到同步时候的秒差
        private double _nextDayStandardOffsetSec = 0;
        private bool _syncReady = false;
        
        //同步/校准时间
        private void SetSyncTime(DateTime nowDt)
        {
            _inCorrecting = true;
            _noteRealTime = Time.realtimeSinceStartup;
            _syncStandardOffsetSec = (nowDt - _originalDT).TotalSeconds;
            _curStandardOffsetSec = _syncStandardOffsetSec;
            UpdateNextDayStandardOffsetSec();
            
            _curHour = nowDt.Hour;
            _curMin = nowDt.Minute;
            _curSec = nowDt.Second;
            _syncReady = true;
            _inCorrecting = false;
            AL_EventMgr.Action(new AlSyncTime());
        }

        private bool _hadUseGm;

        private void UpdateNextDayStandardOffsetSec()
        {
            DateTime pDateTime = _originalDT.AddSeconds(CurrentTime);
            var today = new DateTime(pDateTime.Year, pDateTime.Month, pDateTime.Day, 0, 0, 0);
            _nextDayStandardOffsetSec = (today.AddDays(1) - _originalDT).TotalSeconds;
        }
        
        public double RemainNextDay
        {
            get
            {
                if (!_syncReady) AL_FileLog.LogError("时间控制工具还未同步时间，当前获取的可能是错误的");
                return _nextDayStandardOffsetSec - CurrentTime;
            }
        }
        
        /// <summary>
        /// 从基准时间到当前时间的秒差
        /// </summary>
        public double CurrentTime
        {
            get
            {
                if (!_syncReady) AL_FileLog.LogError("时间控制工具还未同步时间，当前获取的可能是错误的");
                return _curStandardOffsetSec + (Time.realtimeSinceStartup - _noteRealTime);
            }
        }

        public void GmAddSec(int add)
        {
            _hadUseGm = true;
            _curStandardOffsetSec += add;
            CheckCrossDay();
            AlignCacheTime();//调整后需要重新校准
        }

        public void GmSubSec(int sub)
        {
            _hadUseGm = true;
            _curStandardOffsetSec -= sub;
            CheckCrossDay();
            AlignCacheTime();//调整后需要重新校准
        }

        public void GmResetTime()
        {
            CorrectTime(true);
        }
        
        public DateTime TodayDateTime()
        {
            var nowTime = _originalDT.AddSeconds(CurrentTime);
            var today = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day);

            return today;
        }

        private DateTime DateAllTime(double addSec)
        {
            return _originalDT.AddSeconds(addSec);
        }

        public DateTime TodayDateAllTime()
        {
            return _originalDT.AddSeconds(CurrentTime);
        }

        public DateTime TodayStartTime()
        {
            var nowTime = _originalDT.AddSeconds(CurrentTime);
            var today = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 0, 0, 0);

            return today;
        }

        public DateTime GetMonthlyStart()
        {
            var nowTime = _originalDT.AddSeconds(CurrentTime);
            var monthly = new DateTime(nowTime.Year, nowTime.Month, 1, 0, 0, 0);

            return monthly;
        }
        
        private bool CheckCrossDay()
        {
            if (!_syncReady || _inCorrecting || RemainNextDay > 0) return false;
            if (CurrentTime < ONE_DAY_SECONDS + _nextDayStandardOffsetSec)
            {
                CrossDay(DateAllTime(_nextDayStandardOffsetSec));
            }
            else
            {
                int days = (int)(CurrentTime - _nextDayStandardOffsetSec) / ONE_DAY_SECONDS + 1;
                days = Math.Max(2, days);
                for (int i = 0; i < days; i++)
                {
                    CrossDay(DateAllTime(i * ONE_DAY_SECONDS + _nextDayStandardOffsetSec));
                }
            }
            return true;
        }

        private void CrossDay(DateTime dt)
        {
            AL_FileLog.LogError($"跨天 =》 {dt}");
            AL_EventMgr.Action(new AlCrossDay(){DateTime = dt});
        }
        
        private float _curCount = 0f;
        private void Update()
        {
            _curCount += Time.unscaledDeltaTime;
            if (_curCount >= 1f)
            {
                _curCount %= 1f;
                SecLoop();
            }
        }
        
        private void SecLoop()
        {
            CheckTimeLoop();
            if (!_syncReady) return;
            UpdateSec();
        }

        //每次执行+1,独立于真实时间；忽略卡帧导致的多秒才执行一次的情况；
        private int _loopSec = 0;
        private int _loopMin = 0;
        private int _loopHour = 0;
        private void CheckTimeLoop()
        {
            _loopSec++;
            AL_EventMgr.Action(new AlSecLoop());
            if (_loopSec % 60 == 0)
            {
                _loopMin++;
                AL_EventMgr.Action(new AlMinLoop());
                if (_loopMin % 60 == 0)
                {
                    _loopHour++;
                    AL_EventMgr.Action(new AlHourLoop());
                }
            }
        }
        
        private int _curSec = -1;
        private int _curMin = -1;
        private int _curHour = -1;
        private void UpdateSec()
        {
            var now = TodayDateAllTime();
            _curSec = now.Second;
            if (_curMin > -1 && _curMin != now.Minute) AL_EventMgr.Action(new AlMinuteChange());
            _curMin = now.Minute;
            if (_curHour > -1 && _curHour != now.Hour) AL_EventMgr.Action(new AlHourChange());
            _curHour = now.Hour;
            if (CheckCrossDay())
            {
                if (CheckNeedCorrectTimeWhenCrossDay())
                {
                    CorrectTime();
                }
                else
                {
                    AlignCacheTime();
                }
            }
        }

        private bool CheckNeedCorrectTimeWhenCrossDay()
        {
            //只有使用过GM调整时间并且使用服务器时间的时候，自然跨天才不需要校准时间，否则此时不然GM调整的时间又被覆盖了
            return !(_hadUseGm && _useLocalTime?.Invoke() == false);
        }
        
        private bool _inCorrecting;//正在校正时间
        public void CorrectTime()
        {
            if (_useLocalTime == null || _getServerTime == null)
            {
                _inCorrecting = false;
                // AL_FileLog.LogError("通用时间工具未初始化");
                return;
            }
            _inCorrecting = true;
            CorrectTime(false);
            
        }

        private void AlignCacheTime()
        {
            SetSyncTime(TodayDateAllTime());
            AL_FileLog.LogError("校准本地缓存时间成功");
        }

        public void CheckCorrectTime()
        {
            AL_FileLog.LogError($"CheckCorrectTime=> 时间工具当前是否使用本地时间:{_curUseLocal}  业务判断是否使用本地时间：{_useLocalTime.Invoke()}");
            if (_curUseLocal != _useLocalTime.Invoke())
            {
                CorrectTime(true);
            } 
        }

        private bool _curUseLocal;
        public void CorrectTime(bool force)
        {
            _curUseLocal = _useLocalTime.Invoke();
            if (_curUseLocal)
            {
                if (!_syncReady || force)
                {
                    SetSyncTime(DateTime.Now);
                    AL_FileLog.LogError("强制同步本地时间成功");
                    _hadUseGm = false;
                }
                else
                {
                    AlignCacheTime();
                }
                _inCorrecting = false;
            }
            else
            {
                _getServerTime(nowDt => {
                    SetSyncTime(nowDt);
                    AL_FileLog.LogError("同步服务器时间成功");
                    _inCorrecting = false;
                    _hadUseGm = false;
                }, () =>
                {
                    // SetSyncTime(DateTime.Now);
                    AL_FileLog.LogError("同步服务器时间失败，稍后重新申请同步");
                    _inCorrecting = false;
                    CorrectTimeLater();
                });
            }
        }
        
        public void CorrectTimeLater()
        {
            if (_inCorrecting) return;
            _inCorrecting = true;
            Al_Main.instance.StartCoroutine(CorrectTime, 0, true);
        }
    }
}