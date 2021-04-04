using System;

namespace Pocosearch
{
    ///
    /// Supported document ID types:
    /// Guid, int, long, string
    ///
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DocumentIdAttribute : Attribute
    {
    }
}