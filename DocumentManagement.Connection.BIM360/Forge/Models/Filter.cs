using System;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    public class Filter : IQueryParameter
    {
        private readonly string key;
        private readonly string[] values;
        private readonly ComparisonType comparisonType;

        public Filter(string key, string value)
        {
            this.key = key;
            values = new[] { value };
            comparisonType = ComparisonType.DefaultEqual;
        }

        public Filter(string key, ComparisonType comparison, string value)
        {
            this.key = key;
            values = new[] { value };
            comparisonType = comparison;
        }

        public Filter(string key, params string[] values)
        {
            this.key = key;
            this.values = values;
            comparisonType = ComparisonType.DefaultEqual;
        }

        public Filter(string key, ComparisonType comparison, params string[] values)
        {
            this.key = key;
            this.values = values;
            comparisonType = comparison;
        }

        public enum ComparisonType
        {
            [ComparisonType("")]
            DefaultEqual,
            [ComparisonType("-lt")]
            LessThan,
            [ComparisonType("-le")]
            LessThanOrEqualTo,
            [ComparisonType("-eq")]
            EqualTo,
            [ComparisonType("-ge")]
            GreaterThanOrEqualTo,
            [ComparisonType("-gt")]
            GreaterThan,
            [ComparisonType("-starts")]
            StartsWith,
            [ComparisonType("-ends")]
            EndsWith,
            [ComparisonType("-contains")]
            Contains,
        }

        /// <inheritdoc cref="IQueryParameter.ToQueryString" />
        public string ToQueryString()
            => string.Format(
                Constants.FILTER_QUERY_PARAMETER,
                key,
                comparisonType.GetAttribute<ComparisonTypeAttribute>().Command,
                string.Join(',', values));

        [AttributeUsage(AttributeTargets.Field)]
        private class ComparisonTypeAttribute : Attribute
        {
            public ComparisonTypeAttribute(string command)
                => Command = command;

            public string Command { get; }
        }
    }
}
