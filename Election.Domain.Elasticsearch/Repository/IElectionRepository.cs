using System.Collections.Generic;
using System.Threading.Tasks;
using Election.Domain.Model;
using ElectionApp.Core;

namespace Election.Domain.Elasticsearch.Repository
{
    public interface IElectionRepository : IGenericRepository<ElectionInfo>
    {
        Task<ElectionInfo> GetElection(string electionName);
        Task<List<ElectionInfo>> GetAll();
    }
}