using Discussly.Server.Data.Entities.Comments;
using Discussly.Server.Infrastructure.Services.Elastic.Interfaces;
using Discussly.Server.SharedKernel.DTO;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.Core.Search;

namespace Discussly.Server.Infrastructure.Services.Elastic
{
    public class ElasticSearchService(ElasticsearchClient elasticClient) : IElasticSearchService
    {
        private const int batchSize = 1000;
        private const string commentIndex = "comment_index";

        public async Task<Guid[]> GetRootCommentsAsync(
            int pageIndex,
            int pageSize,
            string? searchText,
            CommentSortDirectionDto? sortDirection,
            CommentSortByDto? sortBy,
            CancellationToken cancellationToken)
        {
            var from = (pageIndex - 1) * pageSize;

            var response = await elasticClient.SearchAsync<Comment>(s => s
                .Index(commentIndex)
                .Query(q => q
                    .Bool(b =>
                    {
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            b.Must(m => m
                                .Wildcard(wc => wc
                                    .Field(f => f.Text.Suffix("keyword"))
                                    .Value($"*{searchText}*")
                                )
                            );
                        }
                    })
                )
                .From(from)
                .Size(pageSize)
                .Sort(so =>
                {
                    var order = sortDirection == CommentSortDirectionDto.ASC ? SortOrder.Asc : SortOrder.Desc;

                    switch (sortBy)
                    {
                        case CommentSortByDto.UserName:
                            so.Field("user.userName.keyword"!, fs => fs.Order(order));
                            break;

                        case CommentSortByDto.Email:
                            so.Field("user.email.keyword"!, fs => fs.Order(order));
                            break;

                        case CommentSortByDto.CreatedDate:
                        default:
                            so.Field("createdAt"!, fs => fs.Order(order));
                            break;
                    }
                })
            .Source(new SourceConfig(new SourceFilter { Includes = new Field("id") })), cancellationToken);

            if (!response.IsValidResponse)
                return [];

            return [.. response.Documents.Select(d => d.Id)];
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
        {
            var countResponse = await elasticClient.CountAsync<Comment>(cancellationToken);

            return (int)countResponse.Count;
        }

        public async Task AddCommentsAsync(Comment[] comments, CancellationToken cancellationToken)
        {
            for (var i = 0; i < comments.Length; i += batchSize)
            {
                var batch = comments.Skip(i).Take(batchSize);

                var bulkRequest = new BulkRequest(commentIndex)
                {
                    Operations = new BulkOperationsCollection(
                        batch.Select(comment =>
                        {
                            var routing = $"{comment.ParentCommentId}" ?? $"{comment.Id}";
                            return new BulkIndexOperation<Comment>(comment)
                            {
                                Id = $"{comment.Id}",
                                Routing = routing
                            };
                        })
                    )
                };

                await elasticClient.BulkAsync(bulkRequest, cancellationToken);
            }
        }

        public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            var bulkRequest = new BulkRequest(commentIndex)
            {
                Operations = new BulkOperationsCollection(
                [
                    new BulkIndexOperation<Comment>(comment)
                    {
                        Id = $"{comment.Id}",
                        Routing = $"{comment.ParentCommentId}" ?? $"{comment.Id}"
                    }
                ])
            };

            await elasticClient.BulkAsync(bulkRequest, cancellationToken);
            await elasticClient.Indices.RefreshAsync(commentIndex, cancellationToken);
        }
    }
}