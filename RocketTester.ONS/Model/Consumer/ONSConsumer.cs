﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RocketTester.ONS.Enum;
using RocketTester.ONS.Model;
using RocketTester.ONS.Util;
using ons;
using System.Diagnostics;

namespace RocketTester.ONS.Model
{
    public class ONSConsumer : IONSConsumer
    {
        /// <summary>
        /// 自定义属性TagList
        /// </summary>
        public List<string> TagList { get; set; }

        /// <summary>
        /// 自定义属性Topic
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// 自定义属性ConsumerId
        /// </summary>
        public string ConsumerId { get; private set; }

        /// <summary>
        /// 自定义属性Type
        /// </summary>
        public string Type { get; private set; }

        PushConsumer _consumer;

        public ONSConsumer(string topic, string consumerId, PushConsumer consumer)
        {
            Type = ONSMessageType.BASE.ToString();
            Topic = topic;
            ConsumerId = consumerId;
            _consumer = consumer;
            TagList = new List<string>();
        }

        public void start()
        {
            _consumer.start();
        }

        public void shutdown()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _consumer.shutdown();
            stopwatch.Stop();
            LogHelper.Log("ONSConsumer spent " + stopwatch.ElapsedMilliseconds + " on shutdown.");
        }

        public void subscribe(string topic, string tags)
        {
            ONSMessageListener listener = new ONSMessageListener();
            _consumer.subscribe(topic, tags, listener);
        }
    }
}
