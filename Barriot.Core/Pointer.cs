using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barriot
{
    /// <summary>
    ///     Represents a pointer to an existing object.
    /// </summary>
    public readonly struct Pointer
    {
        private readonly object _value;
        public static readonly Dictionary<Guid, object> _dict = new();

        /// <summary>
        ///     Gets the underlying value this pointer is assigned to.
        /// </summary>
        public object Value
        {
            get
                => _value;
        }

        /// <summary>
        ///     Creates a new pointer assigning the <typeparamref name="T"/> value passed.
        /// </summary>
        /// <param name="value"></param>
        public Pointer(object value)
        {
            _value = value;
        }

        public T GetValue<T>()
            => (T)_value;

        /// <summary>
        ///     Compares the left and right value using the underlying value of <typeparamref name="T"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Pointer left, Pointer right)
            => left.Equals(right);

        /// <summary>
        ///     Negatively compares the left and right value using the underlying value of <typeparamref name="T"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Pointer left, Pointer right)
            => !(left == right);

        /// <summary>
        ///     Checks the equality of the underlying <typeparamref name="T"/> to another value.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals([NotNull] object? obj)
            => obj!.Equals(_value);

        /// <summary>
        ///     Gets the hash code of the underlying <typeparamref name="T"/>.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => _value.GetHashCode();

        /// <summary>
        ///     Creates a new <see cref="Pointer{T}"/> with the value of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns>The <see cref="Guid"/> the value of <typeparamref name="T"/> is referenced with.</returns>
        public static Guid Create(object value)
        {
            var guid = Guid.NewGuid();
            _dict.Add(guid, value);
            return guid;
        }

        /// <summary>
        ///     Attempts to parse a <see cref="Pointer{T}"/> with the value of <typeparamref name="T"/> matching the provided <see cref="Guid"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns>True if the value was succesfully retrieved.</returns>
        public static bool TryParse(Guid id, out Pointer? value, bool removeReference = true)
        {
            value = null;
            if (_dict.TryGetValue(id, out var obj))
            {
                if (removeReference)
                    _dict.Remove(id);

                value = new(obj);
                return true;
            }
            return false;
        }
    }
}
