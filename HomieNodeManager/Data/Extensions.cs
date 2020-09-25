using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomieNodeManager.Data
{
    public static class Extensions
    {

        //========================================================
        /// <summary>
        /// Find the indexes of a given pattern in a byte[] buffer
        /// </summary>
        /// <remarks>
        /// Using this for now, as it accomplishes the beginnings of what needs to happen
        ///   this would likely be better done on a stream instead of reading the entire
        ///   file into memory before parsing it
        /// https://stackoverflow.com/a/332667
        /// </remarks>
        /// <param name="buffer"></param>
        /// <param name="pattern"></param>
        /// <param name="startIndex"></param>
        /// <returns>List of index positions where the substring pattern appears</returns>
        //========================================================
        public static List<int> IndexOfSequence(this byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];

                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);

                if (segment.SequenceEqual<byte>(pattern))
                    positions.Add(i);

                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return positions;
        }



        ////  ------------------------------------------------------------------
        ///// <summary>                                                         
        ///// Gather a sub array from a specified array
        ///// </summary>                                                        
        //// -------------------------------------------------------------------
        //public static T[] SubArray<T>(this T[] data, int index, int length)
        //{
        //    T[] result = new T[length];
        //    Array.Copy(data, index, result, 0, length);
        //    return result;
        //}



    }
}
