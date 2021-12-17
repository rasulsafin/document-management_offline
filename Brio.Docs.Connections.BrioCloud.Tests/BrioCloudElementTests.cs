using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebDav;

namespace Brio.Docs.Connections.BrioCloud.Tests
{
    [TestClass]
    public class BrioCloudElementTests
    {
        [TestMethod]
        public void GetElements_MixedElements_ValidCount()
        {
            var builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri1");
            builder.WithContentType("SomeContentType1");
            var validElement1 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri2");
            builder.WithContentType("SomeContentType2");
            var validElement2 = builder.Build();

            var validCollection = new List<WebDavResource>
                {
                    validElement1,
                    validElement2,
                };

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri1");
            var invalidElement1 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri2");
            var invalidElement2 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri3");
            var invalidElement3 = builder.Build();

            var invalidCollection = new List<WebDavResource>
                {
                    invalidElement1,
                    invalidElement2,
                    invalidElement3,
                };

            var collection = new List<WebDavResource>();
            collection.AddRange(validCollection);
            collection.AddRange(invalidCollection);

            var result = BrioCloudElement.GetElements(collection);

            Assert.AreEqual(validCollection.Count, result.Count);
        }
    }
}
