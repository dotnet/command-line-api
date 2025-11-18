// https://github.com/dotnet/runtime/blob/419e949d258ecee4c40a460fb09c66d974229623/src/libraries/System.Private.CoreLib/src/System/Index.cs
// https://github.com/dotnet/runtime/blob/419e949d258ecee4c40a460fb09c66d974229623/src/libraries/System.Private.CoreLib/src/System/Range.cs

#if NETSTANDARD2_0
#nullable enable

using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>Represents a type that can be used to index a collection either from the beginning or the end.</summary>
    /// <remarks>
    /// <see cref="Index" /> is used by the C# compiler to support the <c>>^</c> or ["index from end" operator](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/member-access-operators#index-from-end-operator-):
    /// <code language="csharp">
    /// int[] someArray = new int[5] { 1, 2, 3, 4, 5 };
    /// int lastElement = someArray[^1]; // lastElement = 5
    /// </code>
    /// </remarks>
    internal readonly struct Index : IEquatable<Index>
    {
        private readonly int _value;

        /// <summary>Initializes a new <see cref="Index" /> with a specified index position and a value that indicates if the index is from the beginning or the end of a collection.</summary>
        /// <param name="value">The index value. It has to be greater then or equal to zero.</param>
        /// <param name="fromEnd"><see langword = "true" /> to index from the end of the collection, or <see langword = "false" /> to index from the beginning of the collection.</param>
        /// <remarks>
        /// If the <see cref="Index" /> is constructed from the end, an index value of 1 points to the last element, and an index value of 0 points beyond the last element.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Index(int value, bool fromEnd = false)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");
            }

            if (fromEnd)
                _value = ~value;
            else
                _value = value;
        }

        // The following private constructors mainly created for perf reason to avoid the checks
        private Index(int value)
        {
            _value = value;
        }

        /// <summary>Gets an <see cref="Index" /> that points to the first element of a collection.</summary>
        /// <value>An instance that points to the first element of a collection.</value>
        public static Index Start => new Index(0);

        /// <summary>Gets an <see cref="Index" /> that points beyond the last element.</summary>
        /// <value>An index that points beyond the last element.</value>
        public static Index End => new Index(~0);

        /// <summary>Creates an <see cref="Index" /> from the specified index at the start of a collection.</summary>
        /// <param name="value">The index position from the start of a collection.</param>
        /// <returns>The index value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromStart(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");
            }

            return new Index(value);
        }

        /// <summary>Creates an <see cref="Index" /> from the end of a collection at a specified index position.</summary>
        /// <param name="value">The index value from the end of a collection.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromEnd(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");
            }

            return new Index(~value);
        }

        /// <summary>Gets the index value.</summary>
        /// <value>The index value.</value>
        public int Value
        {
            get
            {
                if (_value < 0)
                {
                    return ~_value;
                }
                else
                {
                    return _value;
                }
            }
        }

        /// <summary>Gets a value that indicates whether the index is from the start or the end.</summary>
        /// <value><see langword = "true" /> if the Index is from the end; otherwise, <see langword = "false" />.</value>
        public bool IsFromEnd => _value < 0;

        /// <summary>Calculates the offset from the start of the collection using the specified collection length.</summary>
        /// <param name="length">The length of the collection that the Index will be used with. Must be a positive value.</param>
        /// <returns>The offset.</returns>
        /// <remarks>
        /// For performance reasons, this method does not validate if <c>length</c> or the returned value are negative. It also doesn't validate if the returned value is greater than <c>length</c>.
        /// Collections aren't expected to have a negative length/count. If this method's returned offset is negative and is then used to index a collection, the runtime will throw <see cref="ArgumentOutOfRangeException" />, which will have the same effect as validation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int length)
        {
            var offset = _value;
            if (IsFromEnd)
            {
                // offset = length - (~value)
                // offset = length + (~(~value) + 1)
                // offset = length + value + 1

                offset += length + 1;
            }
            return offset;
        }

        /// <summary>Indicates whether the current Index object is equal to a specified object.</summary>
        /// <param name="value">An object to compare with this instance.</param>
        /// <returns><see langword="true" /> if <paramref name="value" /> is of type <see cref="Index" /> and is equal to the current instance; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? value) => value is Index && _value == ((Index)value)._value;

        /// <summary>Returns a value that indicates whether the current object is equal to another <see cref="Index" /> object.</summary>
        /// <param name="other">The object to compare with this instance.</param>
        /// <returns><see langword="true" /> if the current Index object is equal to <paramref name="other" />; otherwise, <see langword="false" />.</returns>
        public bool Equals(Index other) => _value == other._value;

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => _value;

        /// <summary>Converts an integer number to an <see cref="Index" />.</summary>
        /// <param name="value">The integer to convert.</param>
        /// <returns>An index representing the integer.</returns>
        public static implicit operator Index(int value) => FromStart(value);

        /// <summary>Returns the string representation of the current <see cref="Index" /> instance.</summary>
        /// <returns>The string representation of the <see cref="Index" />.</returns>
        public override string ToString()
        {
            if (IsFromEnd)
                return "^" + ((uint)Value).ToString();

            return ((uint)Value).ToString();
        }
    }

    /// <summary>Represents a range that has start and end indexes.</summary>
    /// <remarks>
    /// <see cref="Range" /> is used by the C# compiler to support the range syntax:
    /// <code language="csharp">
    /// int[] someArray = new int[5] { 1, 2, 3, 4, 5 };
    /// int[] subArray1 = someArray[0..2]; // { 1, 2 }
    /// int[] subArray2 = someArray[1..^0]; // { 2, 3, 4, 5 }
    /// </code>
    /// </remarks>
    internal readonly struct Range : IEquatable<Range>
    {
        /// <summary>Gets the inclusive start index of the <see cref="Range" />.</summary>
        /// <value>The inclusive start index of the range.</value>
        public Index Start { get; }

        /// <summary>Gets an <see cref="Index" /> that represents the exclusive end index of the range.</summary>
        /// <value>The end index of the range.</value>
        public Index End { get; }

        /// <summary>Instantiates a new <see cref="Range" /> instance with the specified starting and ending indexes.</summary>
        /// <param name="start">The inclusive start index of the range.</param>
        /// <param name="end">The exclusive end index of the range.</param>
        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }

        /// <summary>Returns a value that indicates whether the current instance is equal to a specified object.</summary>
        /// <param name="value">An object to compare with this <see cref="Range" /> object.</param>
        /// <returns><see langword="true" /> if <paramref name="value" /> is of type <see cref="Range" /> and is equal to the current instance; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? value) =>
            value is Range r &&
            r.Start.Equals(Start) &&
            r.End.Equals(End);

        /// <summary>Returns a value that indicates whether the current instance is equal to another <see cref="Range" /> object.</summary>
        /// <param name="other">A <see cref="Range" /> object to compare with this <see cref="Range" /> object.</param>
        /// <see langword="true" /> if the current instance is equal to <paramref name="other" />; otherwise, <see langword="false" />.
        public bool Equals(Range other) => other.Start.Equals(Start) && other.End.Equals(End);

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Start.GetHashCode() * 31 + End.GetHashCode();
        }

        /// <summary>Returns the string representation of the current <see cref="Range" /> object.</summary>
        /// <returns>The string representation of the range.</returns>
        public override string ToString()
        {
            return Start + ".." + End;
        }

        /// <summary>Returns a new <see cref="Range" /> instance starting from a specified start index to the end of the collection.</summary>
        /// <param name="start">The position of the first element from which the Range will be created.</param>
        /// <returns>A range from <paramref name="start" /> to the end of the collection.</returns>
        public static Range StartAt(Index start) => new Range(start, Index.End);

        /// <summary>Creates a <see cref="Range" /> object starting from the first element in the collection to a specified end index.</summary>
        /// <param name="end">The position of the last element up to which the <see cref="Range" /> object will be created.</param>
        /// <returns>A range that starts from the first element to <paramref name="end" />.</returns>
        public static Range EndAt(Index end) => new Range(Index.Start, end);

        /// <summary>Gets a <see cref="Range" /> object that starts from the first element to the end.</summary>
        /// <value>A range from the start to the end.</value>
        public static Range All => new Range(Index.Start, Index.End);

        /// <summary>Calculates the start offset and length of the range object using a collection length.</summary>
        /// <param name="length">A positive integer that represents the length of the collection that the range will be used with.</param>
        /// <returns>The start offset and length of the range.</returns>
        /// <remarks>
        /// For performance reasons, this method doesn't validate <paramref name="length" /> to ensure that it is not negative. It does ensure that <paramref name="length" /> is within the current <see cref="Range" /> instance.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int Offset, int Length) GetOffsetAndLength(int length)
        {
            int start;
            var startIndex = Start;
            if (startIndex.IsFromEnd)
                start = length - startIndex.Value;
            else
                start = startIndex.Value;

            int end;
            var endIndex = End;
            if (endIndex.IsFromEnd)
                end = length - endIndex.Value;
            else
                end = endIndex.Value;

            if ((uint)end > (uint)length || (uint)start > (uint)end)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return (start, end - start);
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class RuntimeHelpers
    {
        /// <summary>
        /// Slices the specified array using the specified range.
        /// </summary>
        public static T[] GetSubArray<T>(T[] array, Range range)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            (int offset, int length) = range.GetOffsetAndLength(array.Length);

            if (default(T) != null || typeof(T[]) == array.GetType())
            {
                // We know the type of the array to be exactly T[].

                if (length == 0)
                {
                    return Array.Empty<T>();
                }

                var dest = new T[length];
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
            else
            {
                // The array is actually a U[] where U:T.
                var dest = (T[])Array.CreateInstance(array.GetType().GetElementType(), length);
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
        }
    }
}
#endif
