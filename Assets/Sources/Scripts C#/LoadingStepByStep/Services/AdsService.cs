using System.Threading.Tasks;

namespace LoadingStepByStep.Services
{
    public class AdsService
    {
        public Task InitializeAsync() 
        {
            return Task.Delay(2000);
        }
    }
}
