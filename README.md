# pubnub.async
An async/await, testable, extensible, fluent api, easy-to-use, cross-platform PubNub client.

[![GitHub license](https://img.shields.io/github/license/en-gen/crankshaft.svg)](https://raw.githubusercontent.com/engenb/pubnub.async.pcl/history/LICENSE)

| Branch | Nuget | Build | Test Coverage | Static Analysis |
| ------ | ----- | ----- | ------------- | --------------- |
| master | [![NuGet](https://img.shields.io/nuget/v/pubnub.async.svg)](https://www.nuget.org/packages/PubNub.Async) | [![Build status](https://ci.appveyor.com/api/projects/status/djkl9c797sqw94tv/branch/master?svg=true)](https://ci.appveyor.com/project/engenb/pubnub-async-pcl/branch/master) | [![Coverage Status](https://coveralls.io/repos/github/engenb/pubnub.async.pcl/badge.svg?branch=master)](https://coveralls.io/github/engenb/pubnub.async.pcl?branch=master) | [![Coverity](https://scan.coverity.com/projects/8733/badge.svg)](https://scan.coverity.com/projects/engenb-pubnub-async-pcl) |
| development | [![NuGet](https://img.shields.io/nuget/vpre/pubnub.async.svg)](https://www.nuget.org/packages/PubNub.Async) | [![Build status](https://ci.appveyor.com/api/projects/status/djkl9c797sqw94tv/branch/development?svg=true)](https://ci.appveyor.com/project/engenb/pubnub-async-pcl/branch/development) | [![Coverage Status](https://coveralls.io/repos/github/engenb/pubnub.async.pcl/badge.svg?branch=development)](https://coveralls.io/github/engenb/pubnub.async.pcl?branch=development) | [![Coverity](https://scan.coverity.com/projects/8733/badge.svg)](https://scan.coverity.com/projects/engenb-pubnub-async-pcl) |

## Disclaimer
:exclamation: This is an unofficial .net client for the cloud messaging service [PubNub](www.pubnub.com).  If you like to keep things official, I'd suggest heading over to their [:octocat:](https://github.com/pubnub) repository.

## Intoducing...
Still with me?  I'm on a mission to improve upon the stock offering of the PubNub service client.  Here are my goals:

1. **Async/Await**: The stock PubNub client forces you into callback hell, with separate success/error/etc callbacks based on the response scenario.  I think the client should be responsible for abstracting this for the developer by returning a consistent response model that describes success, failure, or otherwise.
2. **Dependency Injection**: PubNub's client is implemented largely in one code file containing several classes and utility functions.  pubnub.async employs development best practices like separation of concerns.  I've provided a nuget package to get you up and running with Autofac as my container of choice.  If you'd like to see other containers supported, please [open an issue](https://github.com/engenb/pubnub.async.pcl/issues) or submit a [pull request](https://github.com/engenb/pubnub.async.pcl/pulls).
3. **Testability**: the stock client's high amount of coupling without the use of interfaces makes it very difficult to mock during testing.  Hand in hand with goal #2 above, we all know IoC helps out a lot with testability.  I am also working on a test framework to make testing with the client even easier in your own application tests.
4. **Fluent API**: It seems, these days, you've gotta have a fluent API!  More details on this later.
5. **Extensibility**: Through a nifty application of extension methods, it's easy to bolt on additional features with additional nuget packages.  My first planned candidate for this is mobile push notifications as I see this feature living outside the client's "core" feature set.
6. **Easy to Use**: This client will do as much of the work for you as possible.
 * It'll batch history requests into groups of 200 messages (PubNub's max) for you and aggregate the results.
 * If a channel is secured with PubNub Access Manager, pubnub.async will request and manage all grants for you if you've provided your secret key.
 * Application-level delegates for lifecyle events like message recieved on any channel, success, or error.
  
 ## Fluenticity
Yes, that's a made up word.  Below are some examples of what it means.
```csharp
// Everything revolves around a channel
var history = await "channelA".History<ApplicationSpecificMessageModel>();
var ancientHistory = await new Channel("channelB").History<ApplicationSpecificMessageModel>();

// you can specifically configure a single shot channel...
var response = await "particular-config-channel"
	.SecuredWith(AuthKey) //enables & handles PAM
	.EncryptedWith(CipherKey) //enables & handles encryption
	.Publish(HelloWorld); // publish a message

// or configure a reusable client
var client = "multi-use"
	.SecuredWith(AuthKey)
	.EncryptedWith(CipherKey);
var response = await client.Publish(YourMessageModel);
var history = await client.History<YourMessageModel>();
```
```csharp
// you may, additionally, configure an application-level configuration, ideally at application startup...
PubNub.Configure(c =>
{
	c.SessionUuid = "system";
	c.PublishKey = Settings.Default.PublishKey;
	c.SubscribeKey = Settings.Default.SubscribeKey;
	c.SecretKey = Settings.Default.SecretKey;
	c.CipherKey = "42";
});

// yet still override the global config as needed
var specialClient = "special-channel"
	.ConfigurePubNub(c =>
	{
		c.SessionUuid = "userId";
		c.CipherKey = "special";
		c.MinutesToTimeout = 20;
	});
```

## Testability
I'm a fan of [Moq](https://github.com/moq/moq), and I hope you are too.  This is still a work in progress, but the groundword for a basic testing framework is in place.  The end result should look something like this:
```csharp
using(var pubNubTest = new PubNubTest())
{
  pubNubTest.MockAccess
    .Setup(x => x.Establish(AccessType.ReadWrite))
    .Returns(expectedGrantResponse);
  
  var result = await "test-channel"
    .Secured()
    .Grant(AccessType.ReadWrite);
  
  Assert.Equal(expectedGrantResponse, result);
}
```
