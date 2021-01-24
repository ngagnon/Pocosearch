using System;
using System.Collections.Generic;
using System.Data;
using FultonSearch.Entities;

namespace FultonSearch.DAL
{
    public interface IStorageBackend : IDisposable
    {
        void ScanIndex(int indexId, string getDocumentsSql, Action<OriginalDocument> addOrUpdate);
        SearchData GatherSearchData(int indexId, List<string> tokens, IDbCommand subset);
        void AddOrUpdateDocument(int indexId, int docId, List<string> tokens, byte[] checksum);
        bool TryRemoveDocument(int docId, int indexId);
    }
}
