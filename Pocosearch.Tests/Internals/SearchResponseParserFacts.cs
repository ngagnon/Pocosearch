
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Pocosearch.Internals;
using Xunit;

namespace Pocosearch.Tests
{
    public class SearchResponseParserFacts
    {
        private readonly SearchResponseParser parser;
        private readonly IFileProvider fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

        public SearchResponseParserFacts()
        {
            var configurationProvider = new SearchIndexConfigurationProvider();
            parser = new SearchResponseParser(configurationProvider);
        }

        [Fact]
        public void Parse_ReturnsListOfSearchResults()
        {
            /* @TODO: test that it uses [SearchIndex] */
            /* @TODO: test with 2 sources */
            var query = new SearchQuery
            {

            };

            string json;

            using (var stream = fileProvider.GetFileInfo("Internals/SearchResponseParserFacts.Data.json").CreateReadStream())
            using (var reader = new StreamReader(stream))
                json = reader.ReadToEnd();

            var results = parser.Parse(json, query).ToList();

            /* @TODO: assertions */
        }
    }
}