# Building Search Queries

## Limiting the Number of Search Results

```csharp
var query = new SearchQuery
{
    SearchString = "brown fox",

    // Will return at most 25 search results. Default is 10.
    Limit = 25 
};
```

## Searching from Multiple Sources

```csharp
var query = new SearchQuery("blue");
query.AddSource<User>();
query.AddSource<Article>();
query.AddSource<Comment>();
query.AddSource<Tag>();

var results = pocosearch.Search(query);

// To iterate over all results, regardless of document type
foreach (var hit in results)
{
    Console.WriteLine(hit.DocumentType);
}

// To extract results of a specific type
var comments = results.GetDocumentsOfType<Comment>();

foreach (var hit in comments)
{
    Console.WriteLine(hit.Document.Content);
}
```

## Filtering Search Results

Given the following POCO:

```csharp
public class Article
{
    [DocumentId]
    public int Id { get; set; }

    [FullText]
    public string Content { get; set; }

    public DateTime PublishedOn { get; set; }
}
```

If you wanted to return articles that were published in the past 3 months only:

```csharp
var query = new SearchQuery("brown fox");
query.AddSource<Article>()
     .Filter(x => x.PublishedOn >= DateTime.Now.AddMonths(-3));
```

The `Filter` method supports more complex expressions too:

```csharp
source.Filter(x => 
    x.PublishedOn >= DateTime.Now.AddMonths(-3) 
    && (x.Source == "newspaper" || x.Source == "internet")
);
```

## Excluding a Field from a Search

Given the following POCO:

```csharp
public class Article
{
    [DocumentId]
    public int Id { get; set; }

    [FullText]
    public string Title { get; set; }

    [FullText]
    public string Content { get; set; }
}
```

If you wanted to search by title only:

```csharp
var query = new SearchQuery("brown fox");
query.AddSource<Article>()
     .Exclude(x => x.Content);
```

## Boosting a Field's Score

Given the same POCO, if you wanted to search both the title and content of your articles, but giving more weight to documents where the title matched the query:

```csharp
var query = new SearchQuery("brown fox");
query.AddSource<Article>()
     .Configure(x => x.Title, boost: 10.0);
```

## Combining Multiple Queries

```csharp
var query1 = new SearchQuery("big red truck");
query1.AddSource<Vehicle>();

var query2 = new SearchQuery("brown fox");
query2.AddSource<Animal>();

var resultSet = pocosearch.MultiSearch(query1, query2).ToList(); // Or MultiSearchAsync();

var query1Results = resultSet[0];
query1Results.GetDocumentsOfTypes<Vehicle>();
// ...

var query2Results = resultSet[1];
query2Results.GetDocumentsOfTypes<Animal>();
// ...
```
