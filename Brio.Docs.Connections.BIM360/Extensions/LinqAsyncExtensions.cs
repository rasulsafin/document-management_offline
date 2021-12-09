using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Brio.Docs.Connections.Bim360.Extensions
{
    // System.Linq.Async cannot be used in this solution.
    // Reason: https://stackoverflow.com/a/60347953/16047481
    // Reference: https://github.com/dotnet/reactive/tree/main/Ix.NET/Source/System.Linq.Async/System/Linq
    public static class LinqAsyncExtensions
    {
        /// <summary>
        ///     Determines whether an async-enumerable sequence contains any elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to check for non-emptiness.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>
        ///     An async-enumerable sequence containing a single element determining whether the source sequence contains any
        ///     elements.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is null.</exception>
        /// <remarks>
        ///     The return type of this operator differs from the corresponding operator on IEnumerable in order to retain
        ///     asynchronous behavior.
        /// </remarks>
        public static async ValueTask<bool> AnyAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            await foreach (TSource unused in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                return true;

            return false;
        }

        /// <summary>
        ///     Returns the first element of an async-enumerable sequence, or a default value if no such element exists.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">Source async-enumerable sequence.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>
        ///     ValueTask containing the first element in the async-enumerable sequence, or a default value if no such element
        ///     exists.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is null.</exception>
        public static async ValueTask<TSource> FirstOrDefaultAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                return element;

            return default;
        }

        /// <summary>
        ///     Returns the first element of an async-enumerable sequence that satisfies the condition in the predicate, or a
        ///     default value if no such element exists.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">Source async-enumerable sequence.</param>
        /// <param name="predicate">A predicate function to evaluate for elements in the source sequence.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>
        ///     ValueTask containing the first element in the async-enumerable sequence that satisfies the condition in the
        ///     predicate, or a default value if no such element exists.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        public static async ValueTask<TSource> FirstOrDefaultAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            await foreach (TSource element in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (predicate(element))
                    return element;
            }

            return default;
        }

        /// <summary>
        /// Projects each element of an async-enumerable sequence into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by running the selector function for each element in the source sequence.</typeparam>
        /// <param name="source">A sequence of elements to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each source element.</param>
        /// <returns>An async-enumerable sequence whose elements are the result of invoking the transform function on each element of source.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            await foreach (var element in source)
                yield return selector(element);
        }

        /// <summary>
        /// Converts an enumerable sequence to an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">Enumerable sequence to convert to an async-enumerable sequence.</param>
        /// <returns>The async-enumerable sequence whose elements are pulled from the given enumerable sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
#pragma warning disable 1998
        public static async IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IEnumerable<TSource> source)
#pragma warning restore 1998
        {
            foreach (var element in source)
                yield return element;
        }

        /// <summary>
        ///     Creates a dictionary from an async-enumerable sequence according to a specified key selector function, and a
        ///     comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>
        ///     An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the
        ///     corresponding source sequence's element.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or <paramref name="keySelector" /> or
        ///     <paramref name="comparer" /> is null.
        /// </exception>
        /// <remarks>
        ///     The return type of this operator differs from the corresponding operator on IEnumerable in order to retain
        ///     asynchronous behavior.
        /// </remarks>
        public static async ValueTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            Dictionary<TKey, TSource> d = new (comparer);

            await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var key = keySelector(item);
                d.Add(key, item);
            }

            return d;
        }

        /// <summary>
        /// Creates a dictionary from an async-enumerable sequence according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
            where TKey : notnull
            => ToDictionaryAsync(source, keySelector, null, cancellationToken);

        /// <summary>
        /// Creates a dictionary from an async-enumerable sequence according to a specified key selector function, and an element selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <typeparam name="TElement">The type of the dictionary value computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="elementSelector"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
            where TKey : notnull
            => ToDictionaryAsync(source, keySelector, elementSelector, null, cancellationToken);

        /// <summary>
        /// Creates a dictionary from an async-enumerable sequence according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <typeparam name="TElement">The type of the dictionary value computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="elementSelector"/> or <paramref name="comparer"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static async ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            var d = new Dictionary<TKey, TElement>(comparer);

            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var key = keySelector(item);
                var value = elementSelector(item);
                d.Add(key, value);
            }

            return d;
        }

        /// <summary>
        ///     Creates a list from an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source async-enumerable sequence to get a list of elements for.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>
        ///     An async-enumerable sequence containing a single element with a list containing all the elements of the source
        ///     sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is null.</exception>
        /// <remarks>
        ///     The return type of this operator differs from the corresponding operator on IEnumerable in order to retain
        ///     asynchronous behavior.
        /// </remarks>
        public static async ValueTask<List<TSource>> ToListAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            List<TSource> list = new ();

            await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                list.Add(item);

            return list;
        }

        /// <summary>
        /// Filters the elements of an async-enumerable sequence based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence whose elements to filter.</param>
        /// <param name="predicate">A function to test each source element for a condition.</param>
        /// <returns>An async-enumerable sequence that contains elements from the input sequence that satisfy the condition.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static async IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            await foreach (var element in source)
            {
                if (predicate(element))
                    yield return element;
            }
        }
    }
}
