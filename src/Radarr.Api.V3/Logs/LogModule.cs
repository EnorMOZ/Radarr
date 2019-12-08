using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using Radarr.Http;

namespace Radarr.Api.V3.Logs
{
    public class LogModule : RadarrRestModule<LogResource>
    {
        private readonly ILogService _logService;

        public LogModule(ILogService logService)
        {
            _logService = logService;
            GetResourcePaged = GetLogs;
        }

        private PagingResource<LogResource> GetLogs(PagingResource<LogResource> pagingResource)
        {
            var pageSpec = pagingResource.MapToPagingSpec<LogResource, Log>();

            if (pageSpec.SortKey == "time")
            {
                pageSpec.SortKey = "id";
            }

            var levelFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "level");

            if (levelFilter != null)
            {
                switch (levelFilter.Value)
                {
                    case "fatal":
                        pageSpec.FilterExpressions.Add(new WhereEqualPagingFilter<Log>(x => x.Level, "Fatal"));
                        break;
                    case "error":
                        pageSpec.FilterExpressions.Add(new WhereInPagingFilter<Log>(x => x.Level, new [] { "Fatal", "Error" }));
                        break;
                    case "warn":
                        pageSpec.FilterExpressions.Add(new WhereInPagingFilter<Log>(x => x.Level, new [] { "Fatal", "Error", "Warn" }));
                        break;
                    case "info":
                        pageSpec.FilterExpressions.Add(new WhereInPagingFilter<Log>(x => x.Level, new [] { "Fatal", "Error", "Warn", "Info" }));
                        break;
                    case "debug":
                        pageSpec.FilterExpressions.Add(new WhereInPagingFilter<Log>(x => x.Level, new [] { "Fatal", "Error", "Warn", "Info", "Debug" }));
                        break;
                    case "trace":
                        pageSpec.FilterExpressions.Add(new WhereInPagingFilter<Log>(x => x.Level, new [] { "Fatal", "Error", "Warn", "Info", "Debug", "Trace" }));
                        break;
                }
            }

            var response = ApplyToPage(_logService.Paged, pageSpec, LogResourceMapper.ToResource);

            if (pageSpec.SortKey == "id")
            {
                response.SortKey = "time";
            }

            return response;
        }
    }
}
