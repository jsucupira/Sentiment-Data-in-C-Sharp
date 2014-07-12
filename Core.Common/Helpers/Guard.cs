using System;

namespace Core.Common.Helpers
{
    public sealed class Guard
    {
        public static void IsTrue(bool condition)
        {
            if (condition == false)
                throw new ArgumentException("The condition supplied is false");
        }

        public static void IsTrue(bool condition, String message)
        {
            if (!condition)
                throw new ArgumentException(message);
        }

        public static void IsFalse(bool condition)
        {
            if (condition)
                throw new ArgumentException("The condition supplied is true");
        }
        
        public static void IsFalse(bool condition, String message)
        {
            if (condition)
                throw new ArgumentException(message);
        }

        public static void IsNotNull(Object obj, string message)
        {
            if (obj == null)
                throw new ArgumentNullException(message);
        }

        public static void IsNotNull(Object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("The argument provided cannot be null.");
        }
        
        public static void IsNull(Object obj, string message)
        {
            if (obj != null)
                throw new ArgumentNullException(message);
        }

        public static void IsNull(Object obj)
        {
            if (obj != null)
                throw new ArgumentNullException("The argument provided cannot be null.");
        }
    }
}