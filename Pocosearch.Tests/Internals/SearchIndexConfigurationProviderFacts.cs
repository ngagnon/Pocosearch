
using Pocosearch.Internals;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class SearchIndexConfigurationProviderFacts
    {
        private readonly SearchIndexConfigurationProvider provider;

        public SearchIndexConfigurationProviderFacts()
        {
            provider = new SearchIndexConfigurationProvider();
        }

        [Fact]
        public void GetSearchIndex_FindsSearchIndexAttribute()
        {
            var attribute = provider.GetSearchIndex(typeof(Car));
            attribute.Name.ShouldBe("vehicles");
        }

        [Fact]
        public void GetSearchIndex_GeneratesAttributeWhenMissing()
        {
            var attribute = provider.GetSearchIndex(typeof(SomeSortOfThing));
            attribute.Name.ShouldBe("somesortofthing");
        }

        [SearchIndex("vehicles")]
        public class Car
        {}

        public class SomeSortOfThing
        {}
    }
}