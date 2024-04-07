using System;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueAvalonia.Core
{
	public class TaskQueue
	{
		private SemaphoreSlim semaphore;
		public TaskQueue()
		{
			semaphore = new SemaphoreSlim(2,2);
		}

		public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
		{
			await semaphore.WaitAsync();
			try
			{
				
				return await Task.Run(taskGenerator);
			}
			finally
			{
				semaphore.Release();
			}
		}
		public async Task Enqueue(Func<Task> taskGenerator)
		{
			await semaphore.WaitAsync();
			try
			{
				await Task.Run(taskGenerator);
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
}
