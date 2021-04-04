Pocosearch is an easy-to-use library for Elasticsearch,  kind of like what EasyNetQ does for RabbitMQ

Add full-text search capabilities to your web app!

1. Create POCO
Annotate ID field with [DocumentId]
Annotate full text fields with [FullText]

2. Add to index

AddOrUpdate for single record
BuldAddOrUpdate for multiple

N.B. records aren't immediately searchable, use Refresh to make sure!

3. Search!
Search string
Sources
GetDocumentsOfType

Extras:

- Supported data types (for ID & values)
- Search-as-you-type
- Renaming index & fields
- Ignoring field
- Filtering search results
- Limiting number of search results
- Async methods
- Searching from multiple sources
- Exclude field from search
- Boost field score
- Combining multiple queries
- Setup index without adding to it
- Best practices
    - Use a single PocosearchClient instance!
