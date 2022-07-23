using System;
using System.Collections.Generic;
using ElectionApp.Core;

namespace Election.Domain.Model
{
    public class ElectionInfo: Entity<Guid>
    {
        public string PropertyName { get; set; }
        public string ColorHexCode { get; set; }
        public List<string> PropertyInfoList { get; set; }
    }
}