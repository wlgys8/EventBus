using System.Collections.Generic;

namespace MS.EventBus{
    using CommonUtils;
    using System;

    internal class ContextualEventBus<TContext>{
        private struct EventReceiver{
            public CallbackType type;
            public Action<TContext> action;
            public readonly int uniqueId;
            public EventReceiver(CallbackType type,Action<TContext> action,int uniqueId){
                this.type = type;
                this.action = action;
                this.uniqueId = uniqueId;
            }
        }

        private List<EventReceiver> _receivers = new List<EventReceiver>();
        private int _globalReceiverId = 0;
        public void On(Action<TContext> action){
            _receivers.Add(new EventReceiver(CallbackType.Permanent,action,_globalReceiverId++));
        }

        /// <summary>
        /// remove the first action that registered by "On" or "Once"
        /// </summary>
        public bool Off(Action<TContext> action){
            var index = 0;
            foreach(var receiver in _receivers){
                if(receiver.action == action){
                    _receivers.RemoveAt(index);
                    return true;
                }
                index ++;
            }
            return false;
        }

        public void Once(Action<TContext> action){
            _receivers.Add(new EventReceiver(CallbackType.Once,action,_globalReceiverId++));
        }

        public void OffAll(){
            _receivers.Clear();
        }

        private void RemoveReceivers(HashSet<int> receiverIdSet){
            for(var i = _receivers.Count - 1; i >=0; i--){
                var rv = _receivers[i];
                if(receiverIdSet.Contains(rv.uniqueId)){
                    _receivers.RemoveAt(i);
                }
            }
        }
       

       /// <summary>
       /// 
       /// </summary>
       /// <returns>how much receivers received the event</returns>
        public int Post(TContext ctx){
            var receivers = ListPool<EventReceiver>.Request();
            receivers.AddRange(_receivers);
            var receviersToBeRemove = SetPool<int>.Request();
            try{
                var postIndex = 0;
                while(postIndex < receivers.Count){
                    var receiver = receivers[postIndex];
                    receiver.action(ctx);
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
    }
}
