# Defining POCOs

## Supported Data Types

Your POCOs can have properties of the following types:

- `string`
- `int`
- `long`
- `float`
- `double`
- `bool`
- `Guid`
- `DateTime`

For document IDs specifically, Pocosearch will only accept:

- `string`
- `int`
- `long`
- `Guid`

## Ignoring a Field

```csharp
public class Article
{
    [DocumentId]
    public int Id { get; set; }

    [FullText]
    public string Content { get; set; }

    [Ignore] // Will not be stored in Elasticsearch 
    public string Summary { get; set; }
}
```

## Renaming Indexes and Fields

```csharp
// Articles will be stored in the blog_posts index in Elasticsearch
[SearchIndex("blog_posts")] 
public class Article
{
    [DocumentId]
    public int Id { get; set; }

    // Property will be called body in the Elasticsearch index
    [FullText(Name = "body")]
    public string Content { get; set; }

    // Property will be called brief in the Elasticsearch index
    [Value(Name = "brief")]
    public string Summary { get; set; }
}
```

## Enabling Search-as-you-type

```csharp
public class Article
{
    [DocumentId]
    public int Id { get; set; }

    // Queries against this field will allow partial matches
    // e.g. "brown f" will match "The brown fox likes to read."
    [FullText(SearchAsYouType = true)]
    public string Content { get; set; }
}
```

