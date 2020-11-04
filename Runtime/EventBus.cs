using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MS.CommonUtils;

namespace MS.EventBus{

    public enum CallbackType{
        Once,
        Permanent
    }


    public class EventBus
    {
        private struct EventReceiver{

            public CallbackType type;
            public Action action;
            public readonly int uniqueId;
            public EventReceiver(CallbackType type,Action action,int uniqueId){
                this.type = type;
                this.action = action;
                this.uniqueId = uniqueId;
            }
        }

        private List<EventReceiver> _noneContextualReceivers = new List<EventReceiver>();

        private Dictionary<System.Type,object> _contextualEventBuses;
        private int _globalReceiverId = 0;
       
        public void On(Action action){
            _noneContextualReceivers.Add(new EventReceiver(CallbackType.Permanent,action,_globalReceiverId++));
        }


        /// <summary>
        /// remove the first action that registered by "On" or "Once"
        /// </summary>
        public bool Off(Action action){
            var index = 0;
            foreach(var receiver in _noneContextualReceivers){
                if(receiver.action == action){
                    _noneContextualReceivers.RemoveAt(index);
                    return true;
                }
                index ++;
            }
            return false;
        }

        public void Once(Action action){
            _noneContextualReceivers.Add(new EventReceiver(CallbackType.Once,action,_globalReceiverId++));
        }
        
        public void OffAll(){
            _noneContextualReceivers.Clear();
        }

        private void RemoveReceivers(HashSet<int> receiverIdSet){
            for(var i = _noneContextualReceivers.Count - 1; i >=0; i--){
                var rv = _noneContextualReceivers[i];
                if(receiverIdSet.Contains(rv.uniqueId)){
                    _noneContextualReceivers.RemoveAt(i);
                }
            }
        }
       

       /// <summary>
       /// 
       /// </summary>
       /// <returns>how much receivers received the event</returns>
        public int Post(){
            var receivers = ListPool<EventReceiver>.Request();
            receivers.AddRange(_noneContextualReceivers);
            var receviersToBeRemove = SetPool<int>.Request();
            try{
                var postIndex = 0;
                while(postIndex < receivers.Count){
                    var receiver = receivers[postIndex];
                    receiver.action();
                    postIndex ++;
                    if(receiver.type == CallbackType.Once){
                        receviersToBeRemove.Add(receiver.uniqueId);
                    }
                }
                return receivers.Count;
            }finally{
                ListPool<EventReceiver>.Release(receivers);
                this.RemoveReceivers(receviersToBeRemove);
                SetPool<int>.Release(receviersToBeRemove);
            }
        }


        private ContextualEventBus<TContext> TryGetContextualEventBus<TContext>(){
            if(_contextualEventBuses == null){
                return null;
            }
            var tp = typeof(TContext);
            object bus = null;
            if(_contextualEventBuses.TryGetValue(tp,out bus)){
                return (ContextualEventBus<TContext>)bus;
            }
            return null;
        }

        private ContextualEventBus<TContext> EnsureContextualEventBus<TContext>(){
            if(_contextualEventBuses == null){
                _contextualEventBuses = new Dictionary<Type, object>();
            }
            var tp = typeof(TContext);
            object bus = null;
            if(_contextualEventBuses.TryGetValue(tp,out bus)){
                return (ContextualEventBus<TContext>)bus;
            }else{
                bus = new ContextualEventBus<TContext>();
                _contextualEventBuses.Add(tp,bus);
                return (ContextualEventBus<TContext>)bus;
            }
        }


        public void On<TContext>(Action<TContext> action){
            EnsureContextualEventBus<TContext>().On(action);
        }

        public bool Off<TContext>(Action<TContext> action){
            return EnsureContextualEventBus<TContext>().Off(action);
        }

        public void Once<TContext>(Action<TContext> action){
            EnsureContextualEventBus<TContext>().Once(action);
        }

        public void OffAll<TContext>(){
            EnsureContextualEventBus<TContext>().OffAll();
        }

        public int Post<TContext>(TContext ctx){
            var bus = TryGetContextualEventBus<TContext>();
            if(bus == null){
                return 0 ;
            }
            return bus.Post(ctx);
        }
    }


    public class EventBus<TEventType>
    {
        private struct EventReceiver{

            public CallbackType type;
            public Action action;
            public readonly int uniqueId;

            public EventReceiver(CallbackType type,Action action,int uniqueId){
                this.type = type;
                this.action = action;
                this.uniqueId = uniqueId;
            }

        }


        private Dictionary<TEventType,EventBus> _eventbuses = new Dictionary<TEventType, EventBus>();

        private EventBus EnsureBus(TEventType evt){
            EventBus bus;
            if(!_eventbuses.TryGetValue(evt,out bus)){
                bus = new EventBus();
                _eventbuses.Add(evt,bus);
            }
            return bus;
        }

        private EventBus TryGet(TEventType evt){
            EventBus bus;
            _eventbuses.TryGetValue(evt,out bus);
            return bus;
        }
       
        public void On(TEventType evt, Action action){
            EnsureBus(evt).On(action);
        }



        /// <summary>
        /// remove the first action that registered by "On" or "Once"
        /// </summary>
        public bool Off(TEventType evt, Action action){
            var bus = TryGet(evt);
            if(bus == null){
                return false;
            }
            return bus.Off(action);
        }
       
        public int Post(TEventType evt){
            var bus = TryGet(evt);
            if(bus == null){
                return 0;
            }
            return bus.Post();
        }


        public void On<TContext>(TEventType evt, Action<TContext> action){
            EnsureBus(evt).On<TContext>(action);
        }
        /// <summary>
        /// remove the first action that registered by "On" or "Once"
        /// </summary>
        public bool Off<TContext>(TEventType evt, Action<TContext> action){
            var bus = TryGet(evt);
            if(bus == null){
                return false;
            }
            return bus.Off<TContext>(action);
        }

        public void Once<TContext>(TEventType evt,Action<TContext> action){
            EnsureBus(evt).On<TContext>(action);
        }

        public int Post<TContext>(TEventType evt,TContext ctx){
            var bus = TryGet(evt);
            if(bus == null){
                return 0 ;
            }
            return bus.Post<TContext>(ctx);
        }

    }

}
