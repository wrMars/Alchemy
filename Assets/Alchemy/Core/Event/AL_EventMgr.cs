using System;
using System.Collections.Generic;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public static class AL_EventMgr
    {
        private static Dictionary<Type, Al_EventHandlerBase> _handlers = new Dictionary<Type, Al_EventHandlerBase>();
        
        public static void Add<T>(object caller, Action<T> action) where T:struct
        {
	        GetOrCreateHandler<T>()?.Subscribe(caller, action);
        }

        public static bool CheckHad<T>(Action<T> action) where T : struct
        {
	        return GetOrCreateHandler<T>()?.CheckSubscribed(action) ?? false;
        }
        
        private static Al_EventHandler<T> GetOrCreateHandler<T>() where T:struct
        {
            if ( !_handlers.TryGetValue(typeof(T), out var handler) ) {
                handler = new Al_EventHandler<T>();
                _handlers.Add(typeof(T), handler);
            }
            return handler as Al_EventHandler<T>;
        }
        
        public static void Remove<T>(Action<T> action) where T:struct
        {
	        if ( !_handlers.TryGetValue(typeof(T), out var handler) ) {
		        return;
	        }
	        if ( handler is Al_EventHandler<T> tHandler ) {
		        tHandler.Unsubscribe(action);
	        }
        }

        public static void Action<T>(T args) where T:struct
        {
	        GetOrCreateHandler<T>()?.Fire(args);
        }

        public static void Clear<T>() where T : struct
        {
	        GetOrCreateHandler<T>()?.CleanUp();
        }

        public static void ClearAll()
        {
	        foreach (var kv in _handlers)
	        {
		        kv.Value.CleanUp();
	        }
	        _handlers.Clear();
        }
    }
}