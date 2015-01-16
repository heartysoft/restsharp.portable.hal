using System;
using System.IO;

namespace RestSharp.Portable.CSharpTests
{
    public static class TestConfig
    {
        public const string RootUrl = "http://localhost:62582/";
        public static string CacheFile = 
            // ReSharper disable once AssignNullToNotNullAttribute
            Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "restsharpcache.txt");
    }
}