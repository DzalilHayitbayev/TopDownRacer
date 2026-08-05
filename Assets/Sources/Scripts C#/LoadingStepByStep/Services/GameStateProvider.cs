using System.Threading.Tasks;

namespace LoadingStepByStep.Services
{
    public class GameStateProvider
    {
        public Task LoadStateAsync() 
        {
            return Task.Delay(2500);
        }

    }
}
