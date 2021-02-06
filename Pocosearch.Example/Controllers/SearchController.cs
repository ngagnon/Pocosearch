using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pocosearch.Example.Models;

namespace Pocosearch.Example.Controllers
{
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly PocosearchClient client;

        public SearchController(PocosearchClient client)
        {
            this.client = client;
        }

        [HttpPost, Route("articles/seed")]
        public void SeedArticles()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Pocosearch.Example.articles.csv";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Article>();
                client.BulkAddOrUpdate(records);
            }
        }

        [HttpGet, Route("articles")]
        public IEnumerable<SearchResult<Article>> GetArticles(string search)
        {
            var query = new SearchQuery
            {
                SearchString = search,
                Sources = new List<Source>
                {
                    new Source<Article>()
                }
            };

            var searchResults = client.Search(query);
            return searchResults.GetDocumentsOfType<Article>();
        }
    }
}
