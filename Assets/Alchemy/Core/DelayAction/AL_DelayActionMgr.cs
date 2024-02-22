using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public class AL_DelayAction
    {
        public static int CurID = 0;
        public bool Once = true;
        public bool UseCoroutine;
        public int ID;
        public Action Fun;

        public AL_DelayAction()
        {
            ID = CurID;
            CurID++;
        }

        public void Handler(Action<AL_DelayAction> callback = null)
        {
            if (UseCoroutine)
            {
                Al_Main.instance.StartCoroutine(() =>
                {
                    Fun?.Invoke();
                    callback?.Invoke(this);
                }, 0);
            }
            else
            {
                Fun?.Invoke();
                callback?.Invoke(this);
            }
        }
    }
    
    public static class AL_DelayActionMgr
    {
        private static Dictionary<string, List<AL_DelayAction>> _funDic = new Dictionary<string, List<AL_DelayAction>>();
        
        /// <summary>
        /// 使用时注意，此处不做存在校验，上层自己避免重复注册问题
        /// </summary>
        /// <param name="logo"></param>
        /// <param name="fun"></param>
        /// <param name="param"></param>
        public static void Register(string logo, Action fun, AL_DelayAction param = null)
        {
            if (!_funDic.ContainsKey(logo))
            {
                _funDic[logo] = new List<AL_DelayAction>();
            }
            param??=new AL_DelayAction();
            param.Fun = fun;
            _funDic[logo].Add(param);
        }

        public static void Wakeup(string logo)
        {
            if (_funDic.TryGetValue(logo, out var paramArr))
            {
                var needRemove = new List<AL_DelayAction>();
                for (int i = 0; i < paramArr.Count; i++)
                {
                    var param = paramArr[i];
                    if (param.Once)
                    {
                        param.Handler((act) =>
                        {
                            needRemove.Add(act);
                            if (act.UseCoroutine && i == paramArr.Count)
                            {
                                Remove(paramArr, needRemove);
                            }
                        });
                    }
                    else
                    {
                        param.Handler();
                    }
                }
                Remove(paramArr, needRemove);
            }
        }

        private static void Remove(List<AL_DelayAction> original, List<AL_DelayAction> removeArr)
        {
            if (removeArr == null || removeArr.Count == 0) return;
            foreach (var remove in removeArr)
            {
                try
                {
                    original.Remove(remove);
                }
                catch (Exception e)
                {
                }
                // var one = original.First(a => a.ID == remove.ID);
                // if (one != null) original.Remove(one);
            }
        }
    }

    public static class AL_DelayActionConst
    {
        public static readonly string init_obj_and_building_in_map = "init_obj_and_building_in_map";
    }
}