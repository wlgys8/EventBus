using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MS.CommonUtils;

namespace MS.EventBus{
    public class BehaviourEvents
    {

        /// <summary>
        /// Get all MonoBehaviours which implement THandler as Interface
        /// </summary>
        public static void GetHandlers<THandler>(GameObject gameObject,List<THandler> handlers){
            var components = ListPool<MonoBehaviour>.Request();
            try{
                handlers.Clear();
                gameObject.GetComponents<MonoBehaviour>(components);
                foreach(var mono in components){
                    if(mono is THandler handler ){
                        handlers.Add(handler);
                    }
                }
            }finally{
                ListPool<MonoBehaviour>.Release(components);
            }
        }

        /// <summary>
        /// Get all MonoBehaviours which implement handlerType as Interface
        /// </summary>
        public static void GetHandlers(GameObject gameObject,System.Type handlerType, List<object> handlers){
            var components = ListPool<MonoBehaviour>.Request();
            try{
                handlers.Clear();
                gameObject.GetComponents<MonoBehaviour>(components);
                foreach(var mono in components){
                    if(handlerType.IsAssignableFrom(mono.GetType())){
                        handlers.Add(mono);
                    }
                }
            }finally{
                ListPool<MonoBehaviour>.Release(components);
            }
        }

        /// <summary>
        /// get all behaviours which implement THandler, and execute them by handlerExecutor with state.
        /// 
        /// Return true if at least one handler was executed.
        /// otherwise return false
        /// </summary>
        public static bool Execute<THandler>(GameObject gameObject,HandlerExecutor<THandler> handlerExecutor,object state){
            var handlers = ListPool<THandler>.Request();
            try{
                GetHandlers<THandler>(gameObject,handlers);
                if(handlers.Count == 0){
                    return false;
                }
                foreach(var handler in handlers){
                    handlerExecutor(handler,state);
                }
                return true;
            }finally{
                ListPool<THandler>.Release(handlers);
            }
        }

        /// <summary>
        /// get all behaviours which implement THandler, and execute them by handlerExecutor with state.
        /// 
        /// Return true if at least one handler was executed.
        /// otherwise return false
        /// </summary>
        public static bool Execute<THandler,TState>(GameObject gameObject,HandlerExecutor<THandler,TState> handlerExecutor,TState state){
            var handlers = ListPool<THandler>.Request();
            try{
                GetHandlers<THandler>(gameObject,handlers);
                if(handlers.Count == 0){
                    return false;
                }
                foreach(var handler in handlers){
                    handlerExecutor(handler,state);
                }
                return true;
            }finally{
                ListPool<THandler>.Release(handlers);
            }
        }

        public static bool Execute(GameObject gameObject,System.Type handlerType, System.Delegate handlerExecutor,object state){
            var handlers = ListPool<object>.Request();
            try{
                GetHandlers(gameObject,handlerType,handlers);
                if(handlers.Count == 0){
                    return false;
                }
                foreach(var handler in handlers){
                    handlerExecutor.DynamicInvoke(handler,state);
                }
                return true;
            }finally{
                ListPool<object>.Release(handlers);
            }
        }



        public delegate void HandlerExecutor<THandler>(THandler handler,object state);
        public delegate void HandlerExecutor<THandler,TState>(THandler handler,TState state);


    }
}
