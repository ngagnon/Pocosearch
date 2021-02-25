using System;

namespace Pocosearch.Example.Models
{
    public class Article
    {
        [DocumentId]
        public int Id { get; set; }

        [FullText]
        public string Title { get; set; }

        [FullText(SearchAsYouType = true)]
        public string TitleAsYouType => Title; /* in your application you likely wouldn't need a separate field like this */

        [FullText]
        public string Body { get; set; }

        public Guid Author { get; set; }
        public DateTime PublishedOn { get; set; }
    }
}