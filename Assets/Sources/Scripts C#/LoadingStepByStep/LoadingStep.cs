using System;
using System.Threading.Tasks;

namespace LoadingStepByStep
{
    public class LoadingStep 
    {
        public string Description { get;}

        public Func<Task> ActionAsync { get; }

        public LoadingStep(string description, Func<Task> actionAsync)
        {
            Description = description;
            ActionAsync = actionAsync;
        }
    }
}