using System;
using Newtonsoft.Json.Linq;

namespace PubNub.Async.Services.Subscribe
{
    public class MessageReceivedEventArgs<TMessage> : EventArgs
    {
        public MessageReceivedEventArgs(
            string subscribeKey,
            string channel,
            string senderSessionUuid,
            long sent,
            JToken msgJson,
            JToken decryptedMsgJson,
            TMessage message)
        {
            SubscribeKey = subscribeKey;
            Channel = channel;
            SenderSessionUuid = senderSessionUuid;
            Sent = sent;
            MessageJson = msgJson;
            DecryptedMessageJson = decryptedMsgJson;
            Message = message;
        }

        public string SubscribeKey { get; }
        public string Channel { get; }

        public string SenderSessionUuid { get; }
        public long Sent { get; }

        public JToken MessageJson { get; }
        
        public JToken DecryptedMessageJson { get; }

        public TMessage Message { get; }
    }
}
