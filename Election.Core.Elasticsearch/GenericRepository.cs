using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using ElectionApp.Core;
using Nest;
using Newtonsoft.Json;

namespace Election.Core.Elasticsearch
{
    public abstract class GenericRepository<T, TId> : IGenericRepository<T> where T : EntityBase<TId>
    {
        protected readonly ElasticClient SessionClient;
        protected readonly string IndexName;
        
        protected GenericRepository(ElasticClient elasticClient, string indexName)
        {
            SessionClient = elasticClient;
            IndexName = indexName;
        }
        
        protected void HandleResult(IResponse response)
        {
            if (!response.IsValid)
            {
                throw new Exception(response.OriginalException.Message);
            }
        }

        public async Task<string> SaveAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = await SessionClient.IndexAsync(entity, idx => idx.Index(IndexName));
            if (!result.IsValid)
            {
                throw new Exception(result.OriginalException.Message);
            }

            return entity.Id?.ToString();
        }
        
        public async Task<IEnumerable<T>> GetManyAsync(List<string> id)
        {
            var result = await SessionClient.SearchAsync<T>(s =>
                s.Query(q => q.Ids(i => i
                        .Values(id)))
                    .From(0)
                    .Size(id.Count)
                    .Index(IndexName));

            HandleResult(result);

            return result.Documents;
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = await SessionClient.UpdateAsync(
                new DocumentPath<T>(entity.Id.ToString()), u =>
                    u.Doc(entity).Index(IndexName));
            HandleResult(result);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var result = await SessionClient.DeleteAsync<T>(id, idx => idx
                .Index(IndexName));
            if (!result.IsValid)
            {
                if (result.Result != Result.NotFound)
                {
                    //throw new Exception(result.OriginalException.Message);
                    //Logger
                }
            }

            return result.Result == Result.Deleted || result.Result == Result.NotFound;
        }

        public async Task DeleteBulkAsync(IEnumerable<T> entityList)
        {
            if (entityList == null) throw new ArgumentNullException(nameof(entityList));
            var result = await SessionClient.DeleteManyAsync(entityList, IndexName);
            HandleResult(result);
        }

        public async Task DeleteByQuery(QueryContainer query)
        {
            var request = new DeleteByQueryRequest(IndexName) { Query = query };
            var result = await SessionClient.DeleteByQueryAsync(request);
            HandleResult(result);
        }

        public async Task DeleteBulkAsync(List<string> idList)
        {
            if (idList == null) throw new ArgumentNullException(nameof(idList));
            var result = await SessionClient.BulkAsync(new BulkRequest
            {
                Operations = idList.Select(x => new BulkDeleteOperation<T>(x) { Index = IndexName })
                    .Cast<IBulkOperation>().ToList(),
                Refresh = Refresh.WaitFor
            });

            HandleResult(result);

            //Bulk item delete already deleted returns valid response 
            if (result.Items.Any(x => !x.IsValid))
            {
                throw new Exception(
                    JsonConvert.SerializeObject(result.Items.Where(x => !x.IsValid)
                        .Select(x => x.Result)));
            }
        }

        public async Task<IEnumerable<T>> AllAsync()
        {
            var result = (await SessionClient.SearchAsync<T>(search => search.MatchAll()
                .Index(IndexName)));
            HandleResult(result);

            return result.Documents;
        }

        public async Task<(List<T>, string)> ScrollAsync(string scrollId, string scrollTimeout = "2m")
        {
            var loopingResponse = await SessionClient.ScrollAsync<T>(scrollTimeout, scrollId);

            if (!loopingResponse.IsValid)
            {
                throw new Exception(loopingResponse.OriginalException.Message);
            }

            return (loopingResponse.Documents.ToList(), loopingResponse.ScrollId);
        }
    }
}