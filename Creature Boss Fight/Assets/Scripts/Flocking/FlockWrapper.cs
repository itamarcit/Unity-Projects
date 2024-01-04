namespace Flocking
{
	public class FlockWrapper
	{
		public int index;
		public FlockManager flockManager;

		public FlockWrapper(int index, FlockManager flockManager)
		{
			this.index = index;
			this.flockManager = flockManager;
		}
	}
}
