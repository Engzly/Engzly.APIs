using System.Linq.Expressions;

namespace Engzly.Domain.Specifications
{
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected BaseSpecification() { }

        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>>? Criteria { get; }

        public List<Expression<Func<T, object>>> Includes { get; }
            = new();

        protected void AddInclude(Expression<Func<T, object>> include)
            => Includes.Add(include);
    }
}
