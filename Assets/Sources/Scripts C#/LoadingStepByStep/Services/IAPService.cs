using System.Threading.Tasks;

namespace LoadingStepByStep.Services
{
    public class IAPService
    {
        public Task InitializeAsync()
        {
            return Task.Delay(1000);
        }
    }
}
