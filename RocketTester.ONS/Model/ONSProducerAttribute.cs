﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RocketTester.ONS.Enum;

namespace RocketTester.ONS.Model
{
    public class ONSProducerAttribute : Attribute
    {
        public string Topic { get; set; }
        public ONSMessageTag Tag { get; set; }

        public ONSProducerAttribute(string topic, ONSMessageTag tag)
        {
            Topic = topic;
            Tag = tag;
        }
    }
}