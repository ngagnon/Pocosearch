Pocosearch is an easy-to-use Elasticsearch client for .NET.

@TODO: description
It makes it very easy to add full-text search capabilities to ASP.NET apps by abstracting away most of the nitty gritty details of Elasticsearch.

It lets you add full-text search capabilities to your web app without much knowledge of Elasticsearch.

# Getting Started

## 1. Install the NuGet package

@TODO: Show nuget & dotnet commands, with link to NuGet.org

## 2. Create a POCO

Create a POCO ([Plain Old CLR Object](https://en.wikipedia.org/wiki/Plain_old_CLR_object)) for each type of document you'll want to store in Elasticsearch.

At a minimum, each POCO should have an ID property, which will be used to uniquely identify each document. This property is marked with a `[DocumentId]` attribute.

You'll then want to mark all the properties that should be searchable with the `[FullText]` attribute.

For example:

```csharp
public class Article
{
    [DocumentId]
    public int Id { get; set; }

    [FullText]
    public string Title { get; set; }

    [FullText]
    public string Content { get; set; }

    public DateTime PublishedOn { get; set; }
}
```

Check out [defining POCOs](docs/defining-pocos.md) to learn more.

## 3. Connect to Elasticsearch

Create an instance of the `PocosearchClient`, passing in a `ConnectionConfiguration`:

```csharp
// Learn more about connection settings:
/// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/elasticsearch-net-getting-started.html#_connecting
var uri = new Uri("http://localhost:9200");
var pool = new SingleNodeConnectionPool(uri);
var config = new ConnectionConfiguration(pool);

var pocosearch = new PocosearchClient(config);
```

For optimal performance, it's best to use a single PocosearchClient instance throughout the lifetime of your application.

## 4. Add Documents to the Index

To add a single document to Elastisearch, call `AddOrUpdate`:

```csharp
var article = new Article
{
    Id = 1,
    Title = "Hello, world",
    Content = "...",
    PublishedOn = DateTime.Now
};

pocosearch.AddOrUpdate(article); // or AddOrUpdateAsync();
```

To add a bunch of documents at once, it's best to use `BulkAddOrUpdate` instead:

```csharp
List<Article> someArticles = ...;
pocosearch.BulkAddOrUpdate(someArticles); // or BulkAddOrUpdateAsync();
```

Pocosearch will automatically setup the Elasticsearch index for this document type when you first call `AddOrUpdate`. Alternatively, you can call `SetupIndex` in your app startup to prepare the index at your own convenience.

**N.B. newly added documents are not immediately searchable!** It may take a second or two for Elasticsearch to process them. If you want to make them searchable immediately, call the `Refresh<TDocument>()` method.

## 5. Search!

```csharp
var query = new SearchQuery("brown fox");
query.AddSource<Article>();

var results = pocosearch.Search(query); // or SearchAsync();
var articles = results.GetDocumentsOfType<Article>();

// each search result is an object with:
//     Score = search score,
//     Document = the corresponding article
```

Check out [building search queries](doc/search-queries.md) to learn more.

# Documentation

- [Defining POCOs](docs/defining-pocos.md)
    - Supported data types
    - Renaming indexes & fields
    - Ignoring field
    - Search-as-you-type

- [Building search queries](docs/search-queries.md)
    - Limiting the number of search results
    - Searching from multiple sources
    - Filtering search results
    - Excluding a field from the search
    - Boosting a field's score
    - Combining multiple queries
