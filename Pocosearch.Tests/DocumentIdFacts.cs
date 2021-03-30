using System;
using System.Linq;
using System.Threading;
using Pocosearch.Tests;
using Pocosearch.Tests.Framework;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class DocumentIdFacts : TestBase
    {
        public DocumentIdFacts(ElasticsearchClusterFixture fixture) : base(fixture)
        {
            pocosearch.DeleteIndex<DocumentId_Int>();
            pocosearch.DeleteIndex<DocumentId_Long>();
            pocosearch.DeleteIndex<DocumentId_Guid>();
            pocosearch.DeleteIndex<DocumentId_String>();
            pocosearch.DeleteIndex<DocumentId_Float>();
            pocosearch.DeleteIndex<DocumentId_DateTime>();
        }

        [Fact]
        public void ValidDocumentIds()
        {
            var intDoc = new DocumentId_Int { Id = 1, Description = "Hello, world!" };
            pocosearch.SetupIndex<DocumentId_Int>();
            pocosearch.AddOrUpdate(intDoc);
            pocosearch.Refresh<DocumentId_Int>();

            var longDoc = new DocumentId_Long { Id = 1, Description = "Hello, world!" };
            pocosearch.SetupIndex<DocumentId_Long>();
            pocosearch.AddOrUpdate(longDoc);
            pocosearch.Refresh<DocumentId_Long>();

            var guidDoc = new DocumentId_Guid { Id = Guid.Parse("ce9cb87e-1d2f-4e2f-ba76-a5cbe929c33a"), Description = "Hello, world!" };
            pocosearch.SetupIndex<DocumentId_Guid>();
            pocosearch.AddOrUpdate(guidDoc);
            pocosearch.Refresh<DocumentId_Guid>();

            var stringDoc = new DocumentId_String { Id = "abcd1234", Description = "Hello, world!" };
            pocosearch.SetupIndex<DocumentId_String>();
            pocosearch.AddOrUpdate(stringDoc);
            pocosearch.Refresh<DocumentId_String>();

            var query = new SearchQuery("hello");
            query.AddSource<DocumentId_Int>();
            query.AddSource<DocumentId_Long>();
            query.AddSource<DocumentId_Guid>();
            query.AddSource<DocumentId_String>();

            var results = pocosearch.Search(query);

            results.GetDocumentsOfType<DocumentId_Int>().Count().ShouldBe(1);
            results.GetDocumentsOfType<DocumentId_Int>().First().Document.ShouldBe(intDoc);

            results.GetDocumentsOfType<DocumentId_Long>().Count().ShouldBe(1);
            results.GetDocumentsOfType<DocumentId_Long>().First().Document.ShouldBe(longDoc);

            results.GetDocumentsOfType<DocumentId_Guid>().Count().ShouldBe(1);
            results.GetDocumentsOfType<DocumentId_Guid>().First().Document.ShouldBe(guidDoc);

            results.GetDocumentsOfType<DocumentId_String>().Count().ShouldBe(1);
            results.GetDocumentsOfType<DocumentId_String>().First().Document.ShouldBe(stringDoc);
        }

        [Fact]
        public void InvalidDocumentIds()
        {
            Should.Throw<Exception>(() => pocosearch.SetupIndex<DocumentId_Float>());
            Should.Throw<Exception>(() => pocosearch.SetupIndex<DocumentId_DateTime>());
        }

        [SearchIndex("int_document_id_facts")]
        public class DocumentId_Int 
        {
            [DocumentId]
            public int Id { get; set; }

            [FullText]
            public string Description { get; set; }

            public override bool Equals(object obj)
            {
                return obj is DocumentId_Int doc &&
                    Id == doc.Id && Description == doc.Description;
            }
        }

        [SearchIndex("long_document_id_facts")]
        public class DocumentId_Long
        {
            [DocumentId]
            public long Id { get; set; }

            [FullText]
            public string Description { get; set; }

            public override bool Equals(object obj)
            {
                return obj is DocumentId_Long doc &&
                    Id == doc.Id && Description == doc.Description;
            }
        }

        [SearchIndex("guid_document_id_facts")]
        public class DocumentId_Guid
        {
            [DocumentId]
            public Guid Id { get; set; }

            [FullText]
            public string Description { get; set; }

            public override bool Equals(object obj)
            {
                return obj is DocumentId_Guid doc &&
                    Id == doc.Id && Description == doc.Description;
            }
        }

        [SearchIndex("string_document_id_facts")]
        public class DocumentId_String
        {
            [DocumentId]
            public string Id { get; set; }

            [FullText]
            public string Description { get; set; }

            public override bool Equals(object obj)
            {
                return obj is DocumentId_String doc &&
                    Id == doc.Id && Description == doc.Description;
            }
        }

        [SearchIndex("float_document_id_facts")]
        public class DocumentId_Float
        {
            [DocumentId]
            public float Id { get; set; }

            [FullText]
            public string Description { get; set; }
        }

        [SearchIndex("datetime_document_id_facts")]
        public class DocumentId_DateTime
        {
            [DocumentId]
            public DateTime Id { get; set; }

            [FullText]
            public string Description { get; set; }
        }
    }
}