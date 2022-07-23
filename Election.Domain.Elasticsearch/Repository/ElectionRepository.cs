using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Election.Core.Elasticsearch;
using Election.Domain.Model;
using Nest;

namespace Election.Domain.Elasticsearch.Repository
{
    public class ElectionRepository : GenericRepository<ElectionInfo, Guid>, IElectionRepository
    {
        public ElectionRepository(ElasticClient elasticClient, string aliasName) : base(elasticClient, aliasName)
        {
        }
        
        protected void HandleResult(IResponse response)
        {
            if (!response.IsValid)
            {
                throw new Exception(response.OriginalException.Message);
            }
        }
        
        public Task<ElectionInfo> GetElection(string electionName)
        {
            if (string.IsNullOrEmpty(electionName)) throw new ArgumentNullException(nameof(electionName));
            var result = SessionClient.SearchAsync<ElectionInfo>(s => s
                .Take(1)
                .Query(x => x
                    .Term(m => m
                        .Field(f => f.PropertyName.Suffix("keyword"))
                        .Value(electionName)))
                .Index(IndexName)).GetAwaiter().GetResult();

            HandleResult(result);
            return Task.FromResult(result.Documents.FirstOrDefault());
            
        }

        public async Task<List<ElectionInfo>> GetAll()
        {
            var electionList = new List<ElectionInfo>();

            var searchDescriptor = new SearchDescriptor<ElectionInfo>()
                .Index(IndexName)
                .Take(1000)
                .Query(q => q.MatchAll())
                .Scroll("2m");
             
            var result = await SessionClient.SearchAsync<ElectionInfo>(searchDescriptor);
            if (result.Documents != null && result.Documents.Any())
            {
                electionList.AddRange(result.Documents);
            }
            var scrollId = result.ScrollId;
            while (!string.IsNullOrEmpty(scrollId))
            {
                List<ElectionInfo> users;
                (users, scrollId) = await ScrollAsync(scrollId);
                if (users != null && users.Any())
                {
                    electionList.AddRange(users);
                }
                else
                    break;
            }
            return electionList;
        }
        
        
    }
}