using System.Threading.Tasks;

namespace Tetrifact.Web
{
    public interface ICron
    {
        void Start();

        void Stop();

        Task Work();
    }
}
