/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Alchemy
{
    public class Al_EventHandler<T>:Al_EventHandlerBase where T:struct
    {
        List<Action<T>> _actions  = new List<Action<T>>();
        public List<object> Caller { get; } = new List<object>();
        
    	public void Subscribe(object watcher, Action<T> action) {
    		if ( !_actions.Contains(action)) {
    			_actions.Add(action);
    			Caller.Add(watcher);
    		} else {
    			AL_FileLog.LogErrorLater($"{watcher} 试图多次注册事件 {action}  fun:{action.Method.Name}");
    		}
    	}

    	public bool CheckSubscribed(Action<T> action)
    	{
    		return _actions.Contains(action);
    	}

    	public void Unsubscribe(Action<T> action)
    	{ 
    		var index = _actions.IndexOf(action);
    		if (index < 0) {
    			AL_FileLog.LogErrorLater($"试图注销不存在的事件{action}  fun:{action.Method.Name}");
    		}
    		else
    		{
    			SafeUnsubscribe(index);
    		}
    	}

    	void SafeUnsubscribe(int index) {
    		if ( index >= 0 ) {
    			if (_actions.Count > index && Caller.Count > index)
    			{
    				_actions.RemoveAt(index);
    				Caller.RemoveAt(index);
    			}
    			else
    			{
    				AL_FileLog.LogErrorLater($"试图注销越界的index");
    			}
    		}
    	}

    	private Action<T> _tempAction;
    	private object _tempCaller;
    	private List<int> _needDelIndexs = new List<int>();
    	public void Fire(T arg) {
    		_needDelIndexs.Clear();
    		for ( var i = 0; i < _actions.Count; i++ ) {
    			_tempAction = _actions[i];
    			_tempCaller = Caller[i];
    			if ( _tempCaller is MonoBehaviour behaviour ) {
    				if ( !behaviour ) {
    					_needDelIndexs.Add(i);
    					continue;
    				}
    			}
    			try {
    				_tempAction.Invoke(arg);
    			} catch ( Exception e ) {
    				UnityEngine.Debug.LogError(e.Message);
    			}
    		}

    		foreach (var index in _needDelIndexs)
    		{
    			SafeUnsubscribe(index);
    		}
    	}

    	public override void CleanUp()
    	{
    		_actions.Clear();
    		Caller.Clear();
    	}
    }
    
    public class Al_EventHandlerBase
    {
	    public virtual void CleanUp() {}
    }
}