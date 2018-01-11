using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace worldfetch.Lib
{
    public interface IDataPublisher {

        Task PublishAsync(JArray data);
    }
}