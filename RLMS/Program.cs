using System;
using Squared.Task;

namespace RLMS {
    public static class Program {
        public static readonly TaskScheduler TaskScheduler = new TaskScheduler(JobQueue.WindowsMessageBased);

        static void Main(string[] args) {
            using (TaskScheduler) 
            using (Game game = new Game())
                game.Run();
        }
    }
}

