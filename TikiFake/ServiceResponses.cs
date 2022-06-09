﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TikiFake
{
    public class ServiceResponses<T>
    {
        public object Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; }
    }
}
