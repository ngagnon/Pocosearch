using System.Linq;
using System.Threading.Tasks;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class AsyncFacts : TestBase
    {
        public AsyncFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<Car>();
        }

        [Fact]
        public async Task TestAddAsync()
        {
            var car = new Car
            {
                Id = 1,
                Make = "Beep",
                Model = "Corona GT",
                Year = 2020
            };

            await pocosearch.SetupIndexAsync<Car>();
            await pocosearch.AddOrUpdateAsync(car);
            await pocosearch.RefreshAsync<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = await pocosearch.SearchAsync(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();

            cars.Count.ShouldBe(1);
            cars.First().Document.ShouldBe(car);
        }

        [Fact]
        public async Task TestBulkAddAsync()
        {
            await pocosearch.SetupIndexAsync<Car>();

            var toAdd = Enumerable.Range(1, 5).Select(i => new Car
            {
                Id = i,
                Make = (i % 2 == 0) ? "Beep" : "Zoom",
                Model = $"Corona GT{i}",
                Year = 2020
            });

            await pocosearch.BulkAddOrUpdateAsync(toAdd);
            await pocosearch.RefreshAsync<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = await pocosearch.SearchAsync(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();

            cars.Count.ShouldBe(5);
            cars.Select(x => x.Document).ShouldBe(toAdd.ToArray());

            query.SearchString = "beep";
            results = await pocosearch.SearchAsync(query);
            cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(2);
        }

        [Fact]
        public async Task TestRemoveAsync()
        {
            var toAdd = Enumerable.Range(1, 5).Select(i => new Car
            {
                Id = i,
                Make = (i % 2 == 0) ? "Beep" : "Zoom",
                Model = $"Corona GT{i}",
                Year = 2020
            });

            await pocosearch.SetupIndexAsync<Car>();
            await pocosearch.BulkAddOrUpdateAsync(toAdd);
            await pocosearch.RefreshAsync<Car>();

            var query = new SearchQuery("corona");
            query.AddSource<Car>();

            var results = await pocosearch.SearchAsync(query);
            var cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(5);

            await pocosearch.RemoveAsync<Car>(1);
            await pocosearch.RemoveAsync<Car>(2);
            await pocosearch.RefreshAsync<Car>();

            results = await pocosearch.SearchAsync(query);
            cars = results.GetDocumentsOfType<Car>().ToList();
            cars.Count.ShouldBe(3);
            cars.Select(x => x.Document).ShouldBe(toAdd.Skip(2).ToArray());
        }

        [SearchIndex("car_async_facts")]
        public class Car
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Make { get; set; }

            [FullText]
            public string Model { get; set; }

            public int Year { get; set; }

            public override bool Equals(object obj)
            {
                return obj is Car car &&
                       Id == car.Id &&
                       Make == car.Make &&
                       Model == car.Model &&
                       Year == car.Year;
            }
        }
    }
}