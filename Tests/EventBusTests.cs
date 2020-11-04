using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace MS.EventBus.Tests{
    public class EventBusTests
    {

        [Test]
        public void SimpleEventOn(){
            var bus = new EventBus();
            int callbackInvokeCount = 0;
            bus.On(()=>{
                callbackInvokeCount ++;
            });
            bus.Post();
            Assert.True(callbackInvokeCount == 1);
        }


        [Test]
        public void SimpleEventOff(){
            var bus = new EventBus();
            int callbackInvokeCount = 0;
            System.Action callback = ()=>{
                callbackInvokeCount ++;
            };
            bus.On(callback);
            bus.Post();
            bus.Off(callback);
            bus.Post();
            Assert.True(callbackInvokeCount == 1);
        }

        [Test]
        public void SimpleEventOnce(){
            var bus = new EventBus();
            int callbackInvokeCount = 0;
            bus.Once(()=>{
                callbackInvokeCount ++;
            });
            bus.Post();
            bus.Post();
            bus.Post();
            Assert.True(callbackInvokeCount == 1);
        }


        [Test]
        public void SimpleEventMultiCallbacks(){
            var bus = new EventBus();
            var callback1Invoked = false;
            var callback2Invoked = false;
            System.Action callback1 = ()=>{
                callback1Invoked = true;
            };
            System.Action callback2 = ()=>{
                callback2Invoked = true;
            };
            bus.On(callback1);
            bus.On(callback2);
            bus.Post();
            Assert.True(callback1Invoked && callback2Invoked);
        }


        [Test]
        public void SimpleEventOffInCallback(){
            var bus = new EventBus();
            var callback1InvokeCount = 0;
            var callback2InvokeCount = 0;
            System.Action callback1,callback2 = null;
            callback1 = ()=>{
                callback1InvokeCount ++;
                bus.Off(callback2);
            };
            callback2 = ()=>{
                callback2InvokeCount ++;
            };
            bus.On(callback1);
            bus.On(callback2);

            //post 2 times
            bus.Post();
            bus.Post();
            Assert.True(callback1InvokeCount == 2 && callback2InvokeCount == 1);
        }

        [Test]
        public void EventWithContext(){
            var bus = new EventBus();
            var receivedNumber = 0;
            bus.On<int>((number)=>{
                receivedNumber = number;
            });

            bus.Post(1024);
            Assert.True(receivedNumber == 1024);
        }


        [Test]
        public void EventWithContextFilter(){
            var bus = new EventBus();
            var receivedNumber = 0;
            bus.On<int>((number)=>{
                receivedNumber = number;
            });
            var boolCallbackInvoked = false;
            bus.On<bool>((bValue)=>{
                boolCallbackInvoked = true;
            });

            var noneParameterCallbackInvoked = false;

            bus.On(()=>{
                noneParameterCallbackInvoked = true;
            });
            bus.Post(1024);
            //only callback that with int parameter will be invoked
            Assert.True(receivedNumber == 1024 && !boolCallbackInvoked && !noneParameterCallbackInvoked);
        }





    }
}
