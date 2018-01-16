namespace worldfetch.Lib
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class WorldFetcherHostedService : HostedService
    {
        // https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        private static HttpClient HttpClient = new HttpClient();

        private readonly IOptions<FetcherOptions> dataOptionsAccessor;
        private readonly ILogger<WorldFetcherHostedService> logger;

        public WorldFetcherHostedService(
            IOptions<FetcherOptions> dataOptionsAccessor, ILogger<WorldFetcherHostedService> logger)
        {
            this.dataOptionsAccessor = dataOptionsAccessor;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var dataOptions = dataOptionsAccessor.Value;
            var baseDataUri = dataOptions.BaseDataUri;
            var fetchInterval = dataOptions.FetchInterval;

            while (!ct.IsCancellationRequested)
            {
                // Compose data uris.
                var dataUriBuilder = new UriBuilder(baseDataUri);
                var offsets = new[] { 0, 200 };
                var dataUris = offsets
                    .Select(offset =>
                    {
                        dataUriBuilder.Query = $"output=json&rows=200&offset={offset}";
                        return dataUriBuilder.Uri;
                    })
                    .ToList();
                logger.LogDebug("Fetching from {n} data uri's: {dataUris}", dataUris.Count, string.Join(" | ", dataUris));

                // Run paged fetches in parallel.
                var fetchTasks = dataUris
                    .Select(async dataUri =>
                    {
                        var data = await HttpClient.GetStringAsync(dataUri);
                        return JArray.Parse(data);
                    });
                var dataJArrays = await Task.WhenAll(fetchTasks);
                var dataJArray = new JArray(dataJArrays.SelectMany(array => array));

                // Wait one minute before next pull.
                await Task.Delay(fetchInterval, ct);
            }
        }
    }
}