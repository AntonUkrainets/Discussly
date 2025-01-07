using Discussly.Server.Data.Repositories.Interfaces;
using Discussly.Server.Infrastructure.Services.Elastic.Interfaces;

namespace Discussly.Server.Workers
{
    public class ElasticSearchBackgroundWorker(IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var elasticSearchRepository = scope.ServiceProvider.GetRequiredService<IElasticSearchService>();
            var commentRepository = scope.ServiceProvider.GetRequiredService<ICommentRepository>();

            await LoadExistingDataAsync(elasticSearchRepository, commentRepository, stoppingToken);
        }

        private static async Task LoadExistingDataAsync(
            IElasticSearchService elasticSearchService,
            ICommentRepository commentRepository,
            CancellationToken cancellationToken)
        {
            var totalCount = await elasticSearchService.GetTotalCountAsync(cancellationToken);
            if (totalCount > 0)
                return;

            var comments = await commentRepository.GetAllCommentsAsync(cancellationToken);
            if (comments.Length == 0)
                return;

            await elasticSearchService.AddCommentsAsync(comments, cancellationToken);
        }
    }
}