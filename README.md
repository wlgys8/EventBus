# EventBus

A simple event subscribe & post library for Unity

# Quick Start

## Install


By unity package manager:

    "com.ms.eventbus":"https://github.com/wlgys8/EventBus.git",

## Usage


### 1. Non-parameterized Event
```csharp

var bus = new EventBus();

System.Action callback = ()=>{

}
//subscribe the event
bus.On(callback);

//callback will be invoked one time and remove self.
bus.Once(()=>{
});

//post a event
bus.Post();

//unsubscribe callback
bus.Off(callback);

```

### 2. Parameterized Event

```csharp

var bus = new EventBus();

//subscribe int event
bus.On<int>((int number)=>{

})

//only callback with int could receive the post.
bus.Post<int>(1024);

```

### 3. EventBus with EventType

```csharp

//eventbus with string as EventType
var bus = new EventBus<string>();

bus.On("open",()=>{

});

bus.On("close",()=>{

});

bus.On<float>("tick",(time)=>{

});

bus.On("any other event name",()=>{

});


//only the callbacks subscribed on "open" could receive the post
bus.Post("open");

bus.Post<float>("tick",Time.time);

```
