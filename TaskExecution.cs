using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkLikes
{
    internal class TaskExecution
    {
        public async Task<bool> Start(List<Task> tasks, int parallelCount, int delay)
        {
            if (tasks.Count < parallelCount)
                parallelCount = tasks.Count;
            int countReady = 0;

            for (int i = 0; ;)
            {
                for (int j = 0; j < parallelCount; j++)
                {
                    try
                    {
                        tasks[i + j].Start();
                    }
                    catch { }
                }

                await Task.WhenAll(tasks.Skip(i).Take(parallelCount));

                await Task.Delay(delay);

                countReady += parallelCount;
                if (countReady + parallelCount < tasks.Count)
                {
                    i += parallelCount;
                }
                else
                {
                    if (countReady < tasks.Count)
                    {
                        for (int j = countReady; j < tasks.Count; j++)
                        {
                            tasks[j].Start();
                        }

                        await Task.WhenAll(tasks.Skip(countReady).Take(tasks.Count - countReady));

                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return true;
        }
    }
}
