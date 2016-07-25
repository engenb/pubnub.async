using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Subscribe;
using PubNub.Async.Services.Access;

namespace PubNub.Async.Services.Subscribe
{
    public class SubscriptionMonitor : ISubscriptionMonitor
    {
        protected static readonly SemaphoreSlim Mutex = new SemaphoreSlim(1);

        private CancellationTokenSource CancellationSource { get; set; }

        private Func<IPubNubEnvironment, Channel, IAccessManager> Access { get; }
        private ISubscriptionRegistry Subscriptions { get; }

        private IDictionary<string, long> SubscribeTimeTokens { get; }

        private IList<Task> MonitorTasks { get; }

        public SubscriptionMonitor(
            Func<IPubNubEnvironment, Channel, IAccessManager> access,
            ISubscriptionRegistry subscriptions)
        {
            Access = access;
            Subscriptions = subscriptions;

            SubscribeTimeTokens = new ConcurrentDictionary<string, long>();
            MonitorTasks = new List<Task>();
        }

        public void Register(IPubNubEnvironment environment, long subscribeTimeToken)
        {
            SubscribeTimeTokens[environment.AuthenticationKey] = subscribeTimeToken;
        }

        public async Task Start(IPubNubEnvironment environment)
        {
            await Mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                if (CancellationSource == null)
                {
                    CancellationSource = new CancellationTokenSource();
                }

                if (!CancellationSource.IsCancellationRequested)
                {
                    var subs = Subscriptions.Get(environment.SubscribeKey);
                    var authSubGroups = subs.GroupBy(x => x.Environment.AuthenticationKey);
                    foreach (var authSubs in authSubGroups)
                    {
                        MonitorTasks.Add(Task.Run(async () =>
                        {
                            while (!CancellationSource.IsCancellationRequested)
                            {
                                await EstablishAccess(authSubs);
                                await ReceiveMessages(environment, authSubs.Key, authSubs);
                            }
                        }, CancellationSource.Token));
                    }
                }
            }
            finally
            {
                Mutex.Release();
            }
        }

        public async Task Stop(IPubNubEnvironment environment)
        {
            await Mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                CancellationSource?.Cancel();
                await Task.WhenAll(MonitorTasks);
            }
            finally
            {
                MonitorTasks.Clear();
                CancellationSource = null;
                Mutex.Release();
            }
        }

        private async Task<IEnumerable<Subscription>> EstablishAccess(IEnumerable<Subscription> subscriptions)
        {
            var subLookup = subscriptions.ToDictionary(x => x.Channel.Name);

            var results = await Task.WhenAll(subscriptions
                .Where(x => x.Channel.Secured)
                .Select(x => Access(x.Environment, x.Channel).Establish(AccessType.Read))
                .ToArray());

            foreach (var result in results)
            {
                if (!result.Success)
                {
                    subLookup.Remove(result.Channel);
                }
            }

            return subLookup.Values;
        }

        private async Task ReceiveMessages(
            IPubNubEnvironment environment,
            string authenticationKey,
            IEnumerable<Subscription> subscriptions)
        {
            var channelsCsv = string.Join(",", subscriptions.Select(x => x.Channel.Name).ToArray());

            var requestUrl = environment.Host
                .AppendPathSegments("v2", "subscribe")
                .AppendPathSegment(environment.SubscribeKey)
                .AppendPathSegment(channelsCsv)
                .AppendPathSegment("0")
                .SetQueryParam("uuid", environment.SessionUuid)
                .SetQueryParam("auth", authenticationKey);

            if (SubscribeTimeTokens.ContainsKey(authenticationKey))
            {
                requestUrl.AppendPathSegment(SubscribeTimeTokens[authenticationKey]);
            }

            try
            {
                var httpResponse = await requestUrl
                    .GetAsync(CancellationSource.Token)
                    .ProcessResponse();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var response = await Task.FromResult(httpResponse)
                        .ReceiveJson<PubNubSubscribeResponse>();

                    SubscribeTimeTokens[authenticationKey] = response.SubscribeTime.TimeToken;

                    Task.Run(() =>
                    {
                        foreach (var message in response.Messages)
                        {
                            Subscriptions.MessageReceived(message);
                        }
                    });
                }
                else if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    //TODO
                }
                else
                {
                    //TODO
                }
            }
            catch (FlurlHttpException ex)
            {
                if (!(ex.InnerException is TaskCanceledException))
                {
                    throw;
                }
            }
        }
    }
}
