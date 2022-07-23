using System.Collections.Generic;
using System.Threading.Tasks;
using Election.Domain.Model;

namespace Election.Service
{
    public interface IApplicationService
    {
        Task<ElectionInfo> GetElectionInfo(string propertyName);

        Task<List<ElectionInfo>> GetAllElections();
        Task CreateElectionInfo(ElectionInfo toElectionInfo);
    }
}