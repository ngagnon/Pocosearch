
using System;
using Pocosearch.Internals;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class DocumentIdProviderFacts    
    {
        private readonly DocumentIdProvider provider;

        public DocumentIdProviderFacts()
        {
            provider = new DocumentIdProvider();
        }

        [Fact]
        public void GetDocumentId_FindsDocumentIdAttribute()
        {
            var obj = new ValidObject
            {
                Id = 42,
                Name = "Foobar"
            };

            var id = provider.GetDocumentId(obj);
            id.ShouldBe("42");
        }

        [Fact]
        public void GetDocumentId_ThrowsExceptionWhenMultipleAttributes()
        {
            var obj = new MultipleIdObject();

            Should.Throw<InvalidOperationException>(() =>
            {
                provider.GetDocumentId(obj);
            });
        }
        
        [Fact]
        public void GetDocumentId_ThrowsExceptionWhenNoAttributeFound()
        {
            var obj = new NoIdObject();

            Should.Throw<InvalidOperationException>(() =>
            {
                provider.GetDocumentId(obj);
            });
        }


        [Fact]
        public void GetDocumentId_ThrowsExceptionWhenUnexpectedType()
        {
            var obj = new FloatIdObject { Id = 42.1f };

            Should.Throw<InvalidOperationException>(() =>
            {
                provider.GetDocumentId(obj);
            });
        }

        public class ValidObject
        {
            public string Name { get; set; }

            [DocumentId]
            public int Id { get; set; }
        }

        public class NoIdObject
        {
            public string Name { get; set; }
        }

        public class MultipleIdObject
        {
            [DocumentId]
            public int Id { get; set; }

            [DocumentId]
            public Guid Guid { get; set; }
        }

        public class FloatIdObject
        {
            [DocumentId]
            public float Id { get; set; }
        }
    }
}