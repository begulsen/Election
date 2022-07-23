using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Election.Domain.Elasticsearch.Repository;
using Election.Domain.Model;

namespace Election.Service
{
    public class ApplicationService : IApplicationService
    {
        private readonly IElectionRepository _electionRepository;

        public ApplicationService(IElectionRepository electionRepository)
        {
            _electionRepository = electionRepository;
        }
        
        public async Task<ElectionInfo> GetElectionInfo(string propertyName)
        {
            return await _electionRepository.GetElection(propertyName);
        }
        
        public async Task<List<ElectionInfo>> GetAllElections()
        {
            return await _electionRepository.GetAll();
        }

        public async Task CreateElectionInfo(ElectionInfo electionInfo)
        {
            await _electionRepository.SaveAsync(electionInfo);
        }

    }
}