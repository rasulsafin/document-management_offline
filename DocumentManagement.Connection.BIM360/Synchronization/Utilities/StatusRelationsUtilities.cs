using System;
using System.Linq;
using System.Reflection;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities
{
    internal static class StatusRelationsUtilities
    {
        public static bool IsMet(this RelationCondition condition, Issue issue)
        {
            if (condition.ObjectType is not(ComparisonObjectType.Bim360 or ComparisonObjectType.Undefined))
                throw new ArgumentException("Need objective for this condition", nameof(condition));

            if (!condition.Values.All(x => IsValueCorrect(condition, x)))
                throw new ArgumentOutOfRangeException(nameof(condition), "Condition contains incorrect values");

            var issueValue = GetIssueValue(condition, issue);
            ReplaceValues(condition);
            var conditionFunc = GetConditionFunc(condition);
            return condition.Values.All(value => conditionFunc(issueValue, value));
        }

        private static IComparable GetIssueValue(RelationCondition condition, Issue issue)
        {
            var property = typeof(Issue.IssueAttributes).GetProperties(BindingFlags.Public)
               .Single(x => DataMemberUtilities.GetDataMemberName(x) == condition.PropertyName);
            var issueValue = (IComparable)property.GetValue(issue.Attributes);
            return issueValue;
        }

        private static void ReplaceValues(RelationCondition condition)
        {
            if (condition.ValueType == RelationComparisonValueType.DateTime)
                ReplaceDateTimeValues(condition);
        }

        private static void ReplaceDateTimeValues(RelationCondition condition)
        {
            for (var i = 0; i < condition.Values.Length; i++)
            {
                if (Equals(condition.Values[i], DateTimeValues.Now.GetEnumMemberValue()))
                    condition.Values[i] = DateTime.UtcNow;
            }
        }

        private static Func<IComparable, object, bool> GetConditionFunc(RelationCondition condition)
            => condition.ComparisonType switch
            {
                RelationComparisonType.Undefined => (comparable, o) => comparable.Equals(o),
                RelationComparisonType.Equal => (comparable, o) => comparable.Equals(o),
                RelationComparisonType.NotEqual => (comparable, o) => !comparable.Equals(o),
                RelationComparisonType.Greater => (comparable, o) => comparable.CompareTo(o) > 0,
                RelationComparisonType.Less => (comparable, o) => comparable.CompareTo(o) < 0,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(condition.ComparisonType),
                    "Not supported comparison type")
            };

        private static bool IsValueCorrect(RelationCondition condition, object value)
            => condition.ValueType switch
            {
                RelationComparisonValueType.Undefined => true,
                RelationComparisonValueType.Int => value is int,
                RelationComparisonValueType.Float => value is float,
                RelationComparisonValueType.DateTime => value is DateTime || IsReplacingValue(value, DateTimeValues.Undefined),
                RelationComparisonValueType.String => value is string,
                _ => throw new ArgumentOutOfRangeException(nameof(condition), "Condition contains incorrect value type")
            };

        private static bool IsReplacingValue<T>(object value, T ignore)
            where T : struct, Enum
        {
            if (value is not string)
                return false;

            return Enum.GetValues<T>()
               .Where(enumValue => enumValue.Equals(ignore))
               .Select(enumValue => enumValue.GetEnumMemberValue())
               .Any(name => name.Equals(value));
        }
    }
}
