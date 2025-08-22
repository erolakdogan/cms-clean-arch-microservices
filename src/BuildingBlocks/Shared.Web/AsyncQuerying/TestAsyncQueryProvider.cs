using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Shared.Web.AsyncQuerying
{
    internal sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression)
            => (IQueryable)Activator.CreateInstance(
                typeof(TestAsyncEnumerable<>).MakeGenericType(
                    expression.Type.GetGenericArguments().First()), expression)!;

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression) => _inner.Execute(expression)!;
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            => new TestAsyncEnumerable<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            // Task<TResult> sarmalaması
            var expectedResultType = typeof(TResult).GetGenericArguments().First();
            var execResult = typeof(IQueryProvider)
                .GetMethods()
                .Single(m => m.Name == nameof(IQueryProvider.Execute)
                             && m.IsGenericMethodDefinition
                             && m.GetParameters().Length == 1)
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new[] { expression });

            return (TResult)typeof(Task)
                .GetMethods()
                .Single(m => m.Name == nameof(Task.FromResult) && m.IsGenericMethod)
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { execResult })!;
        }
    }

    internal sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    }

    public static class AsyncQueryableExtensions
    {
        public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
            => new TestAsyncEnumerable<T>(source);

        public static IQueryable<T> AsAsyncQueryable<T>(this IQueryable<T> source)
            => new TestAsyncEnumerable<T>(source.Expression);
    }
}
