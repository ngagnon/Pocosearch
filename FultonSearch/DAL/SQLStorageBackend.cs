using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using Dapper;
using FultonSearch.Entities;

namespace FultonSearch.DAL
{
    public class SQLStorageBackend : IStorageBackend
    {
        private readonly IDbConnection conn;

        public SQLStorageBackend(IDbConnection conn)
        {
            this.conn = conn;
        }

        public void Dispose()
        {
            conn.Dispose();
        }

        public void ScanIndex(int indexId, string getDocumentsSql, Action<OriginalDocument> addOrUpdate)
        {
            using (var tx = new TransactionScope())
            {
                var docSql = $@"DROP TABLE IF EXISTS #documents;
                                CREATE TABLE #documents (ID int NOT NULL, ContentHash binary(32) NOT NULL);
                                INSERT INTO #documents 
                                SELECT src.ID, HASHBYTES('SHA2_256', src.Content) AS ContentHash
                                FROM ({getDocumentsSql}) src";

                conn.Execute(docSql);

                var sql = $@"
                            -- Gather documents to remove, and remove them
                            DECLARE @docsToRemove TABLE (
                                ID int NOT NULL
                            );

                            INSERT INTO @docsToRemove
                            SELECT {Table.FultonDocuments}.ID
                            FROM {Table.FultonDocuments}
                            LEFT JOIN #documents ON #documents.ID = {Table.FultonDocuments}.ID
                            WHERE #documents.ID IS NULL
                            AND IndexID = @indexId

                            DELETE FROM {Table.FultonTermFrequencies}
                            WHERE DocumentId IN (SELECT ID FROM @docsToRemove)
                            AND IndexID = @indexId

                            DELETE FROM {Table.FultonDocuments}
                            WHERE ID IN (SELECT ID FROM @docsToRemove)
                            AND IndexID = @indexId

                            -- Gather new documents & changed documents
                            SELECT * FROM ({getDocumentsSql}) src
                            WHERE ID IN (
                                SELECT #documents.ID
                                FROM #documents
                                LEFT JOIN {Table.FultonDocuments} ON {Table.FultonDocuments}.IndexID = @indexId 
                                    AND #documents.ID = {Table.FultonDocuments}.ID
                                WHERE {Table.FultonDocuments}.ID IS NULL

                                UNION

                                SELECT #documents.ID
                                FROM #documents
                                INNER JOIN {Table.FultonDocuments} ON {Table.FultonDocuments}.IndexID = @indexId 
                                    AND #documents.ID = {Table.FultonDocuments}.ID
                                WHERE #documents.ContentHash != {Table.FultonDocuments}.Checksum
                            )
                            ";

                var documents = conn.Query<OriginalDocument>(sql, new { indexId });

                foreach (var document in documents)
                    addOrUpdate(document);

                tx.Complete();
            }
        }

        public void AddOrUpdateDocument(int indexId, int docId, List<string> tokens, byte[] checksum)
        { 
            var tfSql = $@"INSERT INTO {Table.FultonTermFrequencies} (IndexId, Token, DocumentId, Frequency)
                           VALUES (@indexId, @token, @documentId, @frequency)";

            var documentSql = $@"INSERT INTO {Table.FultonDocuments} (IndexId, Id, Norm, Checksum)
                                 VALUES (@indexId, @documentId, @norm, @checksum)";

            using (var tx = new TransactionScope())
            {
                var norm = 1 / Math.Sqrt(tokens.Count);

                RemoveDocument(docId, indexId);

                conn.Execute(documentSql, new { indexId, docId, norm, checksum });

                foreach (var token in tokens.GroupBy(x => x))
                {
                    var tfParam = new { token = token.Key, docId, frequency = Math.Sqrt(token.Count()) };
                    conn.Execute(tfSql, tfParam);
                }

                tx.Complete();
            }
        }

        public bool TryRemoveDocument(int docId, int indexId)
        {
            using (var tx = new TransactionScope())
            {
                var rows = RemoveDocument(docId, indexId);
                tx.Complete();
                return rows > 0;
            }
        }

        public int RemoveDocument(int docId, int indexId)
        { 
            var sql = $@"DELETE FROM {Table.FultonTermFrequencies}
                         WHERE DocumentId = @docId
                         AND IndexID = @indexId

                         DELETE FROM {Table.FultonDocuments}
                         WHERE Id = @docId
                         AND IndexID @indexId";


            return conn.Execute(sql, new { docId, indexId });
        }

        public SearchData GatherSearchData(int indexId, List<string> tokens, IDbCommand subset)
        {
            var data = new SearchData();
            var txOptions = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.Snapshot
            };

            using (var tx = new TransactionScope(TransactionScopeOption.Required, txOptions))
            {
                var subsetJoin = string.Empty;

                if (subset != null)
                {
                    var subsetSql = @"DROP TABLE IF EXISTS #subset;
                                      CREATE TABLE #subset (ID int NOT NULL);
                                      INSERT INTO #subset " + subset.CommandText;

                    subset.CommandText = subsetSql;
                    subset.Connection = conn;

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    subset.ExecuteNonQuery();

                    subsetJoin = $"INNER JOIN #subset ON #subset.ID = {Table.FultonTermFrequencies}.DocumentID";
                }

                var sql = $@"
                            -- Get number of documents in the index
                            SELECT COUNT(*) as NumDocs
                            FROM {Table.FultonDocuments}
                            WHERE IndexId = @indexId;

                            -- Gather the term frequencies
                            DECLARE @termFrequencies TABLE (
                                Token varchar(255) NOT NULL,
                                Frequency float NOT NULL,
                                DocumentID int NOT NULL
                            );

                            INSERT INTO @termFrequencies
                            SELECT Token, Frequency, DocumentID FROM {Table.FultonTermFrequencies}
                            {subsetJoin}
                            WHERE Token IN @tokens
                            AND IndexID = @indexId

                            SELECT * FROM @termFrequencies;

                            -- Gather the document norms
                            DELCARE @relevantDocuments (ID int NOT NULL);
                            INSERT INTO @relevantDocuments
                            SELECT DISTINCT DocumentID AS ID FROM @termFrequencies;

                            SELECT Id, Norm FROM {Table.FultonDocuments}
                            WHERE Id IN (SELECT ID FROM @relevantDocuments)
                            AND IndexID = @indexId;
                            ";

                using (var multi = conn.QueryMultiple(sql, new { indexId, tokens }))
                {
                    data.NumDocs = multi.Read<int>().Single();
                    data.Results = multi.Read<TermFrequency>().ToList();
                    data.ResultsByDoc = data.Results.GroupBy(x => x.DocumentID).ToList();
                    data.Norms = multi.Read<Document>().ToDictionary(x => x.Id, x => x.Norm);
                }

                tx.Complete();
            }

            return data;
        }
    }
}
