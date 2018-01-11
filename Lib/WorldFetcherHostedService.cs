namespace worldfetch.Lib
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class WorldFetcherHostedService : HostedService
    {
        // https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        private static HttpClient HttpClient = new HttpClient();

        private readonly IOptions<FetcherOptions> dataOptionsAccessor;

        public WorldFetcherHostedService(IOptions<FetcherOptions> dataOptionsAccessor)
        {
            this.dataOptionsAccessor = dataOptionsAccessor;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var dataOptions = dataOptionsAccessor.Value;
            var baseDataUri = dataOptions.BaseDataUri;

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
                    });

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
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }
    }
}