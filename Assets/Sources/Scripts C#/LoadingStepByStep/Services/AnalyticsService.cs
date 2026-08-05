using System.Threading.Tasks;

namespace LoadingStepByStep.Services
{
    public class AnalyticsService 
    {
        public Task InitializeAsync() 
        {
            return Task.Delay(2500);
        }
    }
}