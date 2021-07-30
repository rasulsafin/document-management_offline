using System;
using System.Linq;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    public readonly struct Filter
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

        public override string ToString()
            => string.Format(
                Constants.FILTER_QUERY_PARAMETER,
                key,
                comparisonType.GetAttribute<ComparisonTypeAttribute>().Command,
                values.Aggregate((sum, s) => $"{sum},{s}"));

        [AttributeUsage(AttributeTargets.Field)]
        private class ComparisonTypeAttribute : Attribute
        {
            public ComparisonTypeAttribute(string command)
                => Command = command;

            public string Command { get; }
        }
    }
}