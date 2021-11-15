using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Properties;
using Brio.Docs.Integration.Dtos;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    internal class MetaCommentHelper
    {
        private static readonly string GUID_REGEX_PATTERN = "[{(]?[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?";

        private readonly Lazy<ISerializer> serializer = new (
            () => new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build());

        private readonly Lazy<IDeserializer> deserializer = new (
            () => new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build());

        public ICollection<BimElementExternalDto> GetBimElements(IEnumerable<Comment> comments)
            => TryGet(
                comments,
                MrsConstants.BIM_ELEMENTS_META_COMMENT_TAG,
                out ICollection<BimElementExternalDto> result,
                ArraySegment<BimElementExternalDto>.Empty)
                ? result
                : null;

        public IEnumerable<Comment> CreateComments(ICollection<BimElementExternalDto> bimElements, bool isFirst)
        {
            var info = isFirst
                ? Comments.BimElementsAddedInfo
                : Comments.BimElementsChangedInfo;

            return CreateComments(bimElements, MrsConstants.BIM_ELEMENTS_META_COMMENT_TAG, info);
        }

        private static string SkipLine(string x, int count)
            => string.Join('\n', x.Split('\n').Skip(count));

        private bool TryGet<T>(IEnumerable<Comment> comments, string tag, out T result, T empty = default)
            where T : class
        {
            var enumerable = comments as Comment[] ?? comments.ToArray();
            var beComment = enumerable.OrderByDescending(x => x.Attributes.CreatedAt)
               .Select(x => x.Attributes.Body)
               .FirstOrDefault(x => x.Contains(MrsConstants.META_COMMENT_TAG) && x.Contains(tag));

            if (beComment != null)
            {
                var regex = new Regex($"{tag}{GUID_REGEX_PATTERN}");
                var match = regex.Match(beComment);

                if (match != Match.Empty)
                {
                    var commentsThread = enumerable.OrderBy(x => x.Attributes.CreatedAt)
                       .Select(x => x.Attributes.Body)
                       .Where(x => x.Contains(match.Value));
                    beComment = string.Join(string.Empty, commentsThread.Select(x => SkipLine(x, 1)));
                }

                try
                {
                    result = deserializer.Value.Deserialize<T>(beComment);
                    return true;
                }
                catch
                {
                    result = null;
                    return false;
                }
            }

            result = empty;
            return true;
        }

        private IEnumerable<Comment> CreateComments<T>(T data,  string tag, string info)
        {
            var yaml = serializer.Value.Serialize(data).Replace(Environment.NewLine, "\n");

            int length = MrsConstants.MAX_COMMENT_LENGTH;
            var guid = Guid.NewGuid();

            int steps = (int)Math.Ceiling((double)yaml.Length / length);
            info = $"#{info}";
            var firstLine = $"{MrsConstants.META_COMMENT_TAG} {tag}";

            if (steps > 1)
                firstLine += $"{{{guid}}}";

            for (int i = 0; i < steps; i++)
            {
                var currentLength = length;

                if (i == steps - 1 && yaml.Length < (i + 1) * length)
                    currentLength = yaml.Length % length;

                var body = yaml.Substring(i * length, currentLength);

                body = i == 0
                    ? string.Join('\n', firstLine, info, body)
                    : string.Join('\n', firstLine, body);

                var comment = new Comment
                {
                    Attributes = new Comment.CommentAttributes
                    {
                        Body = body,
                    },
                };

                yield return comment;
            }
        }
    }
}
