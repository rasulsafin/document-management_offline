using System.Collections.Generic;
using System.Linq;
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
            builder.IsCollection();
            var folderElement1 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri2");
            builder.IsCollection();
            builder.WithContentType("SomeContentType2");
            var folderElement2 = builder.Build();

            var folders = new List<WebDavResource>
                {
                    folderElement1,
                    folderElement2,
                };

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri1");
            builder.WithContentType("SomeContentType1");
            builder.IsNotCollection();
            var fileElement1 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri2");
            builder.WithContentType("SomeContentType2");
            builder.IsNotCollection();
            var fileElement2 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri3");
            builder.WithContentType("SomeContentType3");
            builder.IsNotCollection();
            var fileElement3 = builder.Build();

            var files = new List<WebDavResource>
                {
                    fileElement1,
                    fileElement2,
                    fileElement3,
                };

            var collection = new List<WebDavResource>();
            collection.AddRange(folders);
            collection.AddRange(files);

            var result = BrioCloudElement.GetElements(collection, string.Empty);

            int foldersCount = result.Where(r => r.IsDirectory).Count();
            int filesCount = result.Where(r => !r.IsDirectory).Count();

            Assert.AreEqual(folders.Count, foldersCount);
            Assert.AreEqual(files.Count, filesCount);
        }
    }
}
