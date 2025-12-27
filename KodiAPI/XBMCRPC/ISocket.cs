using System;
using System.IO;
using System.Threading.Tasks;

namespace XBMCRPC
{

    public interface ISocket : IDisposable
    {
        Task ConnectAsync(string hostName, int port);
        Stream GetInputStream();
    }
}