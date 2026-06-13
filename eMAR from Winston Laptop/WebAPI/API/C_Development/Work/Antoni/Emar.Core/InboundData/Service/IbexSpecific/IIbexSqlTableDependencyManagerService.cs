using System.Threading;
using System.Threading.Tasks;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public interface IIbexSqlTableDependencyManagerService
    {
        Task DoWork(CancellationToken stoppingToken);
    }
}
