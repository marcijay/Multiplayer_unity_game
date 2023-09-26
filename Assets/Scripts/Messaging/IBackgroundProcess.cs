using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Messaging
{
    public interface IBackgroundProcess
    {
        public Task StartAsync(CancellationToken cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken);
    }
}