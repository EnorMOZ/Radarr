using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using Radarr.Http;

namespace NzbDrone.Api.Logs
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

            var filter = pagingResource.Filters.FirstOrDefault();

            if (filter != null && filter.Key == "level")
            {
                switch (filter.Value)
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

            return ApplyToPage(_logService.Paged, pageSpec, LogResourceMapper.ToResource);
        }
    }
}
