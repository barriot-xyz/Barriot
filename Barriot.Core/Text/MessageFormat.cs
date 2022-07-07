using System.Diagnostics.CodeAnalysis;

namespace Barriot
{
    /// <summary>
    ///     Represents the emoji format in which the returned Discord string will be started with.
    /// </summary>
    public readonly struct MessageFormat
    {
        private readonly string _format;

        /// <summary>
        ///     Creates a new <see cref="MessageFormat"/> from the input string.
        /// </summary>
        /// <param name="format"></param>
        public MessageFormat(string format)
            => _format = format;

        /// <summary>
        ///     Format a success result.
        /// </summary>
        public static MessageFormat Success
            => new("white_check_mark");

        /// <summary>
        ///     Format a failure result.
        /// </summary>
        public static MessageFormat Failure
            => new("x");

        /// <summary>
        ///     Format a not-allowed result.
        /// </summary>
        public static MessageFormat NotAllowed
            => new("no_entry_sign");

        /// <summary>
        ///     Format a list result.
        /// </summary>
        public static MessageFormat List
            => new("books");

        /// <summary>
        ///     Format a question result.
        /// </summary>
        public static MessageFormat Question
            => new("question");

        /// <summary>
        ///     Format an important result.
        /// </summary>
        public static MessageFormat Important
            => new("exclamation");

        /// <summary>
        ///     Format a warning result.
        /// </summary>
        public static MessageFormat Warning
            => new("warning");

        /// <summary>
        ///     Format a deletion result.
        /// </summary>
        public static MessageFormat Deleting
            => new("wastebasket");

        /// <summary>
        ///     Format a bump result.
        /// </summary>
        public static MessageFormat BumpGiven
            => new("thumbsup");

        /// <summary>
        ///     Returns the internal format of this <see cref="MessageFormat"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => $"{_format}";

        /// <summary>
        ///     Gets the hashcode of underlying <see cref="MessageFormat"/> to a comparable result.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => _format.GetHashCode();

        /// <summary>
        ///     Compares the underlying value of this <see cref="MessageFormat"/> to another.
        /// </summary>
        /// <param name="obj">The other entity to compare to.</param>
        /// <returns></returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is MessageFormat format && format._format == _format)
                return true;
            return false;
        }

        /// <summary>
        ///     Calls the internal equality comparer to the left and right entities.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(MessageFormat left, MessageFormat right)
            => left.Equals(right);

        /// <summary>
        ///     Calls the internal equality comparer to the left and right entities.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(MessageFormat left, MessageFormat right)
            => !(left == right);

        /// <summary>
        ///     Creates a new <see cref="MessageFormat"/> based on the input string.
        /// </summary>
        /// <param name="input"></param>
        public static implicit operator MessageFormat(string input)
            => new(input);
    }
}
