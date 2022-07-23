using System;
using Election.Controller.Model;
using Election.Domain.Model;

namespace Election.Helper
{
    public static class MapHelper
    {
        public static ElectionInfo ToElectionInfo(this CreateElectionInfoModel model, Guid id)
        {
            return new ElectionInfo()
            {
                Id = id,
                PropertyName = model.PropertyName,
                ColorHexCode = model.ColorHexCode,
                PropertyInfoList = model.PropertyInfoList,
                CreatedAt = DateTime.Now,
            };
        }
    }
}