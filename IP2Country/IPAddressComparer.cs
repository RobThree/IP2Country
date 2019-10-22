using System;
using System.Collections.Generic;
using System.Net;

namespace IP2Country
{
    /// <summary>
    /// Compares IP addresses to determine numerically which is greater than the other.
    /// </summary>
    public class IPAddressComparer : Comparer<IPAddress>
    {

        private static readonly Lazy<IPAddressComparer> _Default = new Lazy<IPAddressComparer>();

        /// <summary>
        /// The default singleton instance of the comparer.
        /// </summary>
        public static new IPAddressComparer Default => _Default.Value;

        /// <summary>
        /// When overridden in a derived class, performs a comparison of two objects of 
        /// the same type and returns a value indicating whether one object is less than, 
        /// equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public override int Compare(IPAddress x, IPAddress y)
        {

            if (ReferenceEquals(x, y))
                return 0;   // same instance

            if (x is null)
                return -1;  // nulls are always less than non-null

            if (y is null)
                return 1;   // non-null is always greater than null

            if (x.AddressFamily != y.AddressFamily)
                throw new ArgumentException("IP addresses must be of the same address family.");

            var xBytes = x.GetAddressBytes();
            var yBytes = y.GetAddressBytes();

            if (xBytes.Length != yBytes.Length)
                throw new ArgumentException("IP addresses must be of the same length.");

                // compare byte by byte
            for (var i = 0; i < xBytes.Length; i++)
            {
                if (xBytes[i] != yBytes[i])
                    return xBytes[i] < yBytes[i] ? -1 : 1;
            }
            return 0;   // equal
        }
    }
}