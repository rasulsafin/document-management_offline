using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using YamlDotNet.Serialization;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    internal class MetaCommentHelper
    {
        private static readonly string GUID_REGEX_PATTERN =
            "[{(][0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]";

        private readonly ISerializer serializer;
        private readonly IDeserializer deserializer;

        public MetaCommentHelper(ISerializer serializer, IDeserializer deserializer)
        {
            this.serializer = serializer;
            this.deserializer = deserializer;
        }

        public bool TryGet<T>(IEnumerable<Comment> comments, string tag, out T result)
        {
            var array = comments as Comment[] ?? comments.ToArray();
            var withTags = array.OrderByDescending(x => x.Attributes.CreatedAt)
               .Select(x => x.Attributes.Body)
               .Where(x => x.Contains(MrsConstants.META_COMMENT_TAG) && x.Contains(tag));

            foreach (var tagged in withTags)
            {
                var regex = new Regex($"{Regex.Escape(tag)}{GUID_REGEX_PATTERN}");
                var match = regex.Match(tagged);
                var yaml = tagged;

                if (match != Match.Empty)
                {
                    var commentsThread = array.OrderBy(x => x.Attributes.CreatedAt)
                       .Select(x => x.Attributes.Body)
                       .Where(x => x.Contains(match.Value));
                    yaml = string.Join(string.Empty, commentsThread.Select(x => SkipLine(x, 1)));
                }

                try
                {
                    result = deserializer.Deserialize<T>(yaml);
                    return true;
                }
                catch
                {
                }
            }

            result = default;
            return false;
        }

        public IEnumerable<Comment> CreateComments<T>(T data, string tag, string info)
        {
            var yaml = ReplaceNewLines(serializer.Serialize(data));

            int length = MrsConstants.MAX_COMMENT_LENGTH;
            var guid = Guid.NewGuid();

            int steps = (int)Math.Ceiling((double)yaml.Length / length);
            info = ReplaceNewLines(info);
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
                    ? string.Join('\n', info, string.Empty, firstLine, body)
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

        private static string SkipLine(string x, int count)
            => string.Join('\n', x.Split('\n').Skip(count));

        private static string ReplaceNewLines(string text)
            => text.Replace("\r\n", "\n");
    }
}