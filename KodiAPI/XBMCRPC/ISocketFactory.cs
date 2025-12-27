using System.Threading.Tasks;

namespace XBMCRPC
{
    public interface ISocketFactory
    {
        ISocket GetSocket();
        Task<string[]> ResolveHostname(string hostname);
    }
}