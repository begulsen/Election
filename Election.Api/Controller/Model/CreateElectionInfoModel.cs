using System.Collections.Generic;

namespace Election.Controller.Model
{
    public class CreateElectionInfoModel
    {
        public string PropertyName { get; set; }
        public string ColorHexCode { get; set; }
        public List<string> PropertyInfoList { get; set; }
    }
}