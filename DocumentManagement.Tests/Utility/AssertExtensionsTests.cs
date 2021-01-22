//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;

//namespace MRS.DocumentManagement.Tests.Utility
//{
//    [TestClass]
//    public class AssertExtensionsTests
//    {
//        [TestMethod]
//        public void Are_equivalent_simple()
//        {
//            var c1 = new List<int> { 1, 2, 3 };
//            var c2 = new List<int> { 3, 2, 1 };
//            CollectionAssert.That.AreEquivalent(c1, c2, EqualityComparer<int>.Default);
//        }

//        [TestMethod]
//        public void Are_equivalent_same_collection()
//        {
//            var c1 = new List<int> { 1, 2, 3 };
//            CollectionAssert.That.AreEquivalent(c1, c1, EqualityComparer<int>.Default);
//        }

//        internal class Dummy 
//        { 
//            public int Value { get; }
//            public Dummy(int v) { Value = v; }
//        }

//        internal class DummyComparer : IEqualityComparer<Dummy>
//        {
//            public bool Equals([AllowNull] Dummy x, [AllowNull] Dummy y)
//            {
//                if ((x == null) || (y == null))
//                {
//                    return (x == null) && (y == null);
//                }
//                return x.Value == y.Value;
//            }

//            public int GetHashCode([DisallowNull] Dummy obj) => obj.GetHashCode();
//        }

//        [TestMethod]
//        public void Are_equivalent_null_collections()
//        {
//            CollectionAssert.That.AreEquivalent(null, null, EqualityComparer<Dummy>.Default);
//        }

//        [TestMethod]
//        public void Are_equivalent_collections_custom_comparer()
//        {
//            var c1 = new List<Dummy> { new Dummy(1), new Dummy(2), new Dummy(3) };
//            var c2 = new List<Dummy> { new Dummy(3), new Dummy(1), new Dummy(2) };
//            CollectionAssert.That.AreEquivalent(c1, c2, new DummyComparer());
//        }

//        [TestMethod]
//        public void Are_equivalent_collections_custom_comparer_with_nulls()
//        {
//            var c1 = new List<Dummy> { new Dummy(1), new Dummy(2), null, new Dummy(3) };
//            var c2 = new List<Dummy> { null, new Dummy(3), new Dummy(1), new Dummy(2) };
//            CollectionAssert.That.AreEquivalent(c1, c2, new DummyComparer());
//        }

//        [TestMethod]
//        [ExpectedException(typeof(AssertFailedException))]
//        public void Are_equivalent_fails_when_different_length()
//        {
//            var c1 = new List<int> { 1, 2, 3 };
//            var c2 = new List<int> { 1, 2 };
//            CollectionAssert.That.AreEquivalent(c1, c2, EqualityComparer<int>.Default);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(AssertFailedException))]
//        public void Are_equivalent_fails_when_null_collection()
//        {
//            var c1 = new List<Dummy> { new Dummy(1), new Dummy(2), null, new Dummy(3) };
//            CollectionAssert.That.AreEquivalent(c1, null, new DummyComparer());
//        }

//        [TestMethod]
//        [ExpectedException(typeof(AssertFailedException))]
//        public void Are_equivalent_fails_when_null_mismatch()
//        {
//            var c1 = new List<Dummy> { new Dummy(1), new Dummy(2), null, new Dummy(3) };
//            var c2 = new List<Dummy> { new Dummy(1), new Dummy(2), new Dummy(3), new Dummy(4) };
//            CollectionAssert.That.AreEquivalent(c1, c2, new DummyComparer());
//        }
//    }
//}
