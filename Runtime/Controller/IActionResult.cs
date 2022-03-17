using System.Threading.Tasks;

namespace UnityMVC
{
    public interface IActionResult
    {
        Task ExecuteResultAsync();
    }
}
