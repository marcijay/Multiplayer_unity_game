using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Amazon.SQS;
using Messaging.Dto;
using Messaging.Event;
using UnityEngine;


namespace Messaging.Event
{
    public class InfluenceEvent
    {
        public EventHeader header;
        public Dictionary<string, string> payload;
    }
}