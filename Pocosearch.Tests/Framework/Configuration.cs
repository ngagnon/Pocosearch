using System;
using Microsoft.Extensions.Configuration;

namespace Pocosearch.Tests.Framework
{
    public static class Configuration
    {
        private static readonly Lazy<IConfiguration> instance = new Lazy<IConfiguration>(Read);

        public static IConfiguration Instance => instance.Value;

        private static IConfiguration Read()
        {
            var config = new ConfigurationBuilder()
                .AddYamlFile("appsettings.yml")
                .Build();

            return config;
        }

        public static T Get<T>()
        {
            if (typeof(T) == typeof(ElasticsearchSettings))
                return Instance.GetSection("elasticsearch").Get<T>();

            throw new InvalidOperationException();
        }
    }

    public class ElasticsearchSettings
    {
        public bool Managed { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}