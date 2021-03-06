
using System.Linq;
using Pocosearch.Internals;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class FullTextFieldProviderFacts
    {
        private readonly FullTextFieldProvider provider;

        public FullTextFieldProviderFacts()
        {
            provider = new FullTextFieldProvider();
        }

        [Fact]
        public void GetFullTextFields_FindsFullTextFields()
        {
            var fields = provider.GetFullTextFields(typeof(Car)).ToList();
            fields.Select(x => x.Name).ShouldBe(new string[] { "Make", "Model" });
        }

        [Fact]
        public void GetFullTextFields_ReadsNameParameter()
        {
            var fields = provider.GetFullTextFields(typeof(Employee)).ToList();
            fields.Select(x => x.Name).ShouldBe(new string[] { "Name", "Title" });
        }

        public class Car
        {
            [FullText]
            public string Make { get; set ;}

            [FullText]
            public string Model { get; set; }

            public string VIN { get; set; }
        }

        public class Employee
        {
            [FullText]
            public string Name { get; set; }

            [FullText(Name = "Title")]
            public string Role { get; set; }
        }
    }
}