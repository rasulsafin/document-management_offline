using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.UnitTests.Dummy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using YamlDotNet.Serialization;

namespace Brio.Docs.Connections.Bim360.UnitTests
{
    [TestClass]
    public class MetaCommentHelperTests
    {
        private static readonly string BIG_TEXT_FILE = "Resources/Lorem Ipsum.txt";

        private MetaCommentHelper metaCommentHelper;
        private Mock<ISerializer> stubSerializer;
        private Mock<IDeserializer> stubDeserializer;

        [TestInitialize]
        public void Setup()
        {
            stubSerializer = new Mock<ISerializer>();
            stubDeserializer = new Mock<IDeserializer>();
            metaCommentHelper = new MetaCommentHelper(
                stubSerializer.Object,
                stubDeserializer.Object);
        }

        [TestMethod]
        [DataRow(0, "#tag1")]
        [DataRow(1, "#tag2")]
        [DataRow(5, "#tag3")]
        [DataRow(10, "%tag4")]
        public void TryGet_NoCommentsWithMrsTag_ReturnsFalse(int commentsCount, string tag)
        {
            // Arrange.
            var comments = Enumerable.Repeat(DummyModels.Comment, commentsCount);

            // Act.
            var result = metaCommentHelper.TryGet(comments, tag, out string _);

            // Assert.
            Assert.IsFalse(result);
        }

        [TestMethod]
        [DataRow(0, "#tag1")]
        [DataRow(1, "&tag2")]
        [DataRow(5, "#tag3")]
        [DataRow(10, "#tag4")]
        public void TryGet_NoCommentsWithTag_ReturnsFalse(int commentsCount, string tag)
        {
            // Arrange.
            var comments = CreateComments(commentsCount).ToArray();
            foreach (var comment in comments)
                comment.Attributes.Body += MrsConstants.META_COMMENT_TAG;

            // Act.
            var result = metaCommentHelper.TryGet(comments, tag, out string _);

            // Assert.
            Assert.IsFalse(result);
        }

        [TestMethod]
        [DataRow(1, 0, "#t1")]
        [DataRow(2, 0, "#tg2")]
        [DataRow(5, 3, "#tag3")]
        [DataRow(10, 9, "!tag4")]
        public void TryGet_OneCommentWithTags_DeserializesCommentWithTagAndReturnsTrue(
            int commentsCount,
            int indexWithTag,
            string tag)
        {
            // Arrange.
            var comments = CreateComments(commentsCount).ToArray();
            comments[indexWithTag].Attributes.Body += MrsConstants.META_COMMENT_TAG + tag;
            stubDeserializer.Setup(x => x.Deserialize<int>(It.IsAny<string>()))
               .Returns<string>(input => input == comments[indexWithTag].Attributes.Body ? indexWithTag : -1);

            // Act.
            var result = metaCommentHelper.TryGet(comments, tag, out int index);

            // Assert.
            Assert.IsTrue(result);
            Assert.AreNotEqual(-1, index);
            Assert.AreEqual(indexWithTag, index);
        }

        [TestMethod]
        [DataRow(1, 0, "#t1")]
        [DataRow(2, 0, "#tg2")]
        [DataRow(5, 3, "@tag3")]
        [DataRow(10, 9, "#tag4")]
        public void TryGet_AllCommentsHasTags_DeserializesLastCommentAndReturnsTrue(
            int commentsCount,
            int indexOfLatest,
            string tag)
        {
            // Arrange.
            var comments = CreateComments(commentsCount).ToArray();
            var dateTime = new DateTime(2021, 11, 25);
            var latestTime = new DateTime(2022, 11, 25);

            for (var i = 0; i < comments.Length; i++)
            {
                comments[i].Attributes.Body += MrsConstants.META_COMMENT_TAG + tag + Guid.NewGuid();
                comments[i].Attributes.CreatedAt = i == indexOfLatest ? latestTime : dateTime;
            }

            stubDeserializer.Setup(x => x.Deserialize<int>(It.IsAny<string>()))
               .Returns<string>(input => input == comments[indexOfLatest].Attributes.Body ? indexOfLatest : -1);

            // Act.
            var result = metaCommentHelper.TryGet(comments, tag, out int index);

            // Assert.
            Assert.IsTrue(result);
            Assert.AreNotEqual(-1, index);
            Assert.AreEqual(indexOfLatest, index);
        }

        [TestMethod]
        [DataRow(2, 0, 1, "#tg2")]
        [DataRow(5, 3, 1, "$tag3")]
        [DataRow(10, 7, 9,  "@tag4")]
        public void TryGet_ThereAreMultipleMetaComments_DeserializesCommentWithoutFirstLinesAndReturnsTrue(
            int commentsCount,
            int indexOfFirstPart,
            int indexOfSecondPart,
            string tag)
        {
            // Arrange.
            var comments = CreateComments(commentsCount).ToArray();
            var dateTimeOfFirstPart = new DateTime(2021, 11, 25);
            var dateTimeOfSecondPart = new DateTime(2021, 11, 26);
            var first = comments[indexOfFirstPart];
            var second = comments[indexOfSecondPart];
            first.Attributes.CreatedAt = dateTimeOfFirstPart;
            second.Attributes.CreatedAt = dateTimeOfSecondPart;
            var info = $"{MrsConstants.META_COMMENT_TAG}{tag}{{{Guid.NewGuid()}}}";
            first.Attributes.Body = $"{info}\n{indexOfFirstPart}";
            second.Attributes.Body = $"{info}\n {indexOfSecondPart}";

            stubDeserializer.Setup(x => x.Deserialize<(int, int)>(It.IsAny<string>()))
               .Returns<string>(
                    input =>
                    {
                        var split = input.Split(' ');
                        return (int.Parse(split[0]), int.Parse(split[1]));
                    });

            // Act.
            var result = metaCommentHelper.TryGet(comments, tag, out (int, int) indexes);

            // Assert.
            Assert.IsTrue(result);
            Assert.AreEqual(indexOfFirstPart, indexes.Item1);
            Assert.AreEqual(indexOfSecondPart, indexes.Item2);
        }

        [TestMethod]
        [DataRow(0, "serialized", "$t", "Add 0 like 'serialized'")]
        [DataRow(null, "NULL", "$tag", "Add null like 'NULL'")]
        [DataRow("text", "serialized \r\n text", "#tag", "Add 'text' like 'serialized \r\n text'")]
        public void CreateComments_SerializedValueIsSmall_ReturnsOneComment(
            object data,
            string serializedData,
            string tag,
            string info)
        {
            // Arrange.
            stubSerializer.Setup(x => x.Serialize(It.IsAny<object>())).Returns<object>(_ => serializedData);
            var mustBe = $"#{info}\n\n{MrsConstants.META_COMMENT_TAG} {tag}\n{serializedData}";

            // Act.
            var comments = metaCommentHelper.CreateComments(data, tag, info);
            var comment = comments.FirstOrDefault();

            // Assert.
            Assert.IsNotNull(comment);
            Assert.AreEqual(1, comments.Count());
            Assert.IsNotNull(comment.Attributes.Body);
            Assert.AreEqual(mustBe.Replace("\r\n", "\n"), comment.Attributes.Body);
        }

        [TestMethod]
        [DataRow(0, 0, "$t", "Add 0")]
        [DataRow(null, 10, "$tag", "Add null")]
        [DataRow("text", 1000, "#tag", "Add 1000 symbols")]
        [DataRow("text", 1899, "#tag", "Add 1899 symbols")]
        [DataRow("text", 1900, "#tag", "Add 1900 symbols")]
        [DataRow("text", 1901, "#tag", "Add 1901 symbols")]
        [DataRow("text", 10000, "#tag", "Add 10000 symbols")]
        [DataRow("text", 20000, "#tag", "Add 20000 symbols")]
        public void CreateComments_SerializedValueIsBig_ReturnsMultipleComments(
            object data,
            int serializedLength,
            string tag,
            string info)
        {
            // Arrange.
            var serializedData = File.ReadAllText(BIG_TEXT_FILE).Replace("\r\n", "\n")[..serializedLength];
            stubSerializer.Setup(x => x.Serialize(It.IsAny<object>())).Returns<object>(_ => serializedData);
            var mustBeFirstComment = $"#{info}\n\n{MrsConstants.META_COMMENT_TAG} {tag}";
            var mustBeChildComments = $"{MrsConstants.META_COMMENT_TAG} {tag}";
            var count = (int)Math.Ceiling((double)serializedLength / MrsConstants.MAX_COMMENT_LENGTH);

            // Act.
            var comments = metaCommentHelper.CreateComments(data, tag, info);
            var commentsArray = comments.ToArray();

            // Assert.
            Assert.AreEqual(count, commentsArray.Length);

            for (var i = 0; i < commentsArray.Length; i++)
            {
                Assert.IsTrue(
                    i == 0
                        ? commentsArray[i].Attributes.Body.StartsWith(mustBeFirstComment)
                        : commentsArray[i].Attributes.Body.StartsWith(mustBeChildComments));
            }
        }

        private IEnumerable<Comment> CreateComments(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var comment = DummyModels.Comment;
                yield return comment;
            }
        }
    }
}
