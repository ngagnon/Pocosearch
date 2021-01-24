Fulton is a full-text search library for .NET which uses a conventional SQL database to persist
its search index.

Its main purpose is to add basic full-text search capabilities to your ASP.NET Web site, using
what you already have -- an SQL database.

It aims to address some of the shortcomings with existing full-text search solutions:

- Full-text indexes included with relational databases tend to be limited and awkward
- Lucene.NET becomes impractical as soon as you run two or more IIS servers
- Elasticsearch adds another moving part to your infrastructure, increasing the complexity 
  of your production & development environments.

