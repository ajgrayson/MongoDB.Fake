using System.Linq;
using MongoDB.Driver;

namespace MongoDB.Fake.Tests.Filters.Cases.NotIn
{
    internal class BuilderWhere : NotInTestCaseBase
    {
        public override FilterDefinition<SimpleTestDocument> GetFilter()
        {
            var values = new[] { 0, 1, 2 };
            return FilterBuilder.Where(d => !values.Contains(d.IntField));
        }
    }
}