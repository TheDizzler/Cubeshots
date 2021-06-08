namespace AtomosZ.Cubeshots.Libraries
{
	public class CircularBuffer<T>
	{
		public bool isFull;
		public T[] buffer;
		private int nextIndex = 0;


		public CircularBuffer(int size)
		{
			buffer = new T[size];
		}

		/// <summary>
		/// Returns true if buffer is full.
		/// False if has empty slots.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Enqueue(T value)
		{
			buffer[nextIndex++] = value;

			if (nextIndex >= buffer.Length)
			{
				nextIndex = 0;
				isFull = true;
			}

			return isFull;
		}

		public T GetLastEnqueuedValue()
		{
			if (nextIndex == 0)
				return buffer[buffer.Length - 1];
			return buffer[nextIndex - 1];
		}

		public void Clear()
		{
			buffer = new T[buffer.Length];
			isFull = false;
		}
	}
}