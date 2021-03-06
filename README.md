Pocosearch is an easy-to-use Elasticsearch client for .NET.

It was designed specifically for web apps, where much of Elasticsearch's power and flexibility is often unnecessary.

Its straightforward API allows you to easily add full-text search capability to any part of your app without much hassle.

# Getting Started

## 1. Install the NuGet package

Via the .NET CLI:

```
dotnet add package Pocosearch
```

Or the NuGet console:

```
Install-Package Pocosearch 
```

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
// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/elasticsearch-net-getting-started.html#_connecting
var uri = new Uri("http://localhost:9200");
var pool = new SingleNodeConnectionPool(uri);
var config = new ConnectionConfiguration(pool);

var pocosearch = new PocosearchClient(config);
```

For optimal performance, it's best to use a single PocosearchClient instance throughout the lifetime of your application.

## 4. Add Some Documents to the Index

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

**N.B. newly added documents are not immediately searchable!** It may take a second or two for Elasticsearch to process them. If you want to make them searchable immediately, call the `Refresh` method.

## 5. Search!

```csharp
var query = new SearchQuery("brown fox");
query.AddSource<Article>();

var results = pocosearch.Search(query); // or SearchAsync();
var articles = results.GetDocumentsOfType<Article>();

// Each search result is an object with:
// {
//     Score = search score,
//     Document = the corresponding article
// }
```

Check out [building search queries](docs/search-queries.md) to learn more.

# Documentation

- [Defining POCOs](docs/defining-pocos.md)
    - [Supported data types](docs/defining-pocos.md#supported-data-types)
    - [Renaming indexes & fields](docs/defining-pocos.md#renaming-indexes-and-fields)
    - [Ignoring a field](docs/defining-pocos.md#ignoring-a-field)
    - [Enabling search-as-you-type](docs/defining-pocos.md#enabling-search-as-you-type)

- [Building search queries](docs/search-queries.md)
    - [Limiting the number of search results](docs/search-queries.md#limiting-the-number-of-search-results)
    - [Searching from multiple sources](docs/search-queries.md#searching-from-multiple-sources)
    - [Filtering search results](docs/search-queries.md#filtering-search-results)
    - [Excluding a field from the search](docs/search-queries.md#excluding-a-field-from-the-search)
    - [Boosting a field's score](docs/search-queries.md#boosting-a-fields-score)
    - [Combining multiple queries](docs/search-queries.md#combining-multiple-queries)
