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
        public IEnumerable<SearchResult<Article>> GetArticles(string search, bool excludeBody = false, bool boostTitle = false, bool searchAsYouType = false)
        {
            var query = new SearchQuery(search);
            var articleSource = query.AddSource<Article>();

            if (excludeBody)
                articleSource.Exclude(x => x.Body);

            if (searchAsYouType)
            {
                articleSource.Exclude(x => x.Title);

                if (boostTitle)
                    articleSource.Configure(x => x.TitleAsYouType, boost: 10);
            }
            else
            {
                articleSource.Exclude(x => x.TitleAsYouType);

                if (boostTitle)
                    articleSource.Configure(x => x.Title, boost: 10);
            }

            var searchResults = client.Search(query);
            return searchResults.GetDocumentsOfType<Article>();
        }
    }
}
