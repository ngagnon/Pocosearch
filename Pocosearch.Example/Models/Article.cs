using System;

namespace Pocosearch.Example.Models
{
    public class Article
    {
        [DocumentId]
        public int Id { get; set; }

        [FullText]
        public string Title { get; set; }

        [FullText]
        public string Body { get; set; }

        public Guid Author { get; set; }
        public DateTime PublishedOn { get; set; }
    }
}