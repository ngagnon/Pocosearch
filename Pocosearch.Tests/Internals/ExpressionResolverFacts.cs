using System;
using System.Linq.Expressions;
using Pocosearch.Internals;
using Shouldly;
using Xunit;

namespace Pocosearch.Tests
{
    public class ExpressionResolverFacts
    {
        [Fact]
        public void ResolveProperty_FindsProperty()
        {
            Expression<Func<Car, string>> expression = (x) => x.Model;
            var property = ExpressionResolver.ResolveProperty(expression);
            property.Name.ShouldBe("Model");
        }

        [Fact]
        public void ResolveProperty_ThrowsExceptionIfExpressionInvalid()
        {
            Expression<Func<Car, string>> expression = (x) => "foobar";

            Should.Throw<ArgumentException>(() => 
            {
                ExpressionResolver.ResolveProperty(expression);
            });

            var car = new Car { Make = "Unicorn Vehicles Inc." };
            expression = (x) => car.Make;

            Should.Throw<ArgumentException>(() => 
            {
                ExpressionResolver.ResolveProperty(expression);
            });
        }

        /* @TODO: ResolvePredicate */

        public class Car
        {
            public string Make { get; set; }
            public string Model { get; set; }
            public int Year { get; set; }
        }
    }
}