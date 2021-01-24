# Short-term

- Rename to Solid Search
- Review the API
    - Fuzzy queries
    - Suggest queries? (or at least a way to implement them)
    - Support for LIKE query terms (e.g. abc%)?
    - Allow for custom tokenizer for the query
    - Maybe provide an expression builder to make it easier to build custom predicates
- Test it out
    - Optimization via parallelism?
- Add support for other RDMBSes, probably with a different backend for each DB engine,
  or maybe an SQL dialect translator
- Documentation

# Long-term

- Should querying have its own tokenizer?
