using System;

public class OldUtils
{
	public Class1()
	{

        /// <summary>
        /// Creates a pinned array that contains a pointer for unsafe access to the elements in memory.
        /// Use in a "using" block. 
        /// </summary>
        public static PinnedArray<T> Pin<T>(this T[] xs)
        {
            return new PinnedArray<T>(xs);
        }

        /// <summary>
        /// Creates a pinned array that contains a pointer for unsafe access to the elements in memory.
        /// Use in a "using" block. 
        /// </summary>
        public static PinnedArray Pin(this Array xs)
        {
            return new PinnedArray(xs);
        }
	}
}
