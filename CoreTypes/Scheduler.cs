using System.Collections.Generic;

namespace CoreTypes
{
    public class Scheduler
    {
        public List<ICommand> GetCommands() =>
            new ();
    }
}
