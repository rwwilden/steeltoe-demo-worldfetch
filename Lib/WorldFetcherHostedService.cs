namespace worldfetch.Lib
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;

    public class WorldFetcherHostedService : HostedService
    {
        private readonly IOptions<DataOptions> dataOptionsAccessor;

        public WorldFetcherHostedService(IOptions<DataOptions> dataOptionsAccessor)
        {
            this.dataOptionsAccessor = dataOptionsAccessor;
        }

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            var dataOptions = dataOptionsAccessor.Value;
            var dataBaseUrl = dataOptions.DataBaseUrl;

            throw new System.NotImplementedException();
        }
    }
}