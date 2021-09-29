using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Exceptions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Interfaces.StatusRelations;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities
{
    internal static class StatusRelationsUtilities
    {
        public static IEnumerable<Status> GetSuitableStatuses(
            this ObjectiveExternalDto objective,
            StatusesRelations config,
            Issue existing = null)
        {
            for (var i = 0; i < config.Set.Length; i++)
            {
                var relationRule = config.Set[i];

                if (relationRule.Source != objective.Status)
                    continue;

                bool isAllMet;

                try
                {
                    isAllMet = IsAllMet(relationRule, existing, objective);
                }
                catch (ConfigIncorrectException exception)
                {
                    throw new ConfigIncorrectException(
                        "Set contains incorrect rule",
                        DataMemberUtilities.GetPath<StatusesRelations>(x => x.Set),
                        i,
                        exception);
                }

                if (isAllMet)
                    yield return relationRule.Destination;
            }
        }

        public static IEnumerable<ObjectiveStatus> GetSuitableStatuses(this Issue issue, StatusesRelations config)
        {
            for (var i = 0; i < config.Get.Length; i++)
            {
                var relationRule = config.Get[i];

                if (relationRule.Source != issue.Attributes.Status)
                    continue;

                bool isAllMet;

                try
                {
                    isAllMet = IsAllMet(relationRule, issue);
                }
                catch (ConfigIncorrectException exception)
                {
                    throw new ConfigIncorrectException(
                        "Get contains incorrect rule",
                        DataMemberUtilities.GetPath<StatusesRelations>(x => x.Get),
                        i,
                        exception);
                }

                if (isAllMet)
                    yield return relationRule.Destination;
            }
        }

        private static bool IsMet(this RelationCondition condition, Issue issue)
        {
            if (issue == null)
                return false;

            if (condition.ObjectType is not(ComparisonObjectType.Bim360 or null))
                throw new ArgumentException("Need objective for this condition", nameof(condition));

            var issueValue = GetObjectValue(condition.PropertyName, issue);
            return condition.IsMet(issueValue);
        }

        private static bool IsMet(this RelationCondition condition, ObjectiveExternalDto objective)
        {
            if (objective == null)
                return false;

            if (condition.ObjectType is not(ComparisonObjectType.BrioMrs or null))
                throw new ArgumentException("Need objective for this condition", nameof(condition));

            var objectiveValue = GetObjectValue(condition.PropertyName, objective);
            return condition.IsMet(objectiveValue);
        }

        private static bool IsMet(this RelationCondition condition, IComparable comparedValue)
        {
            CheckValues(condition);
            ReplaceValues(condition);
            var conditionFunc = GetConditionFunc(condition.ComparisonType);
            return condition.Values.All(value => conditionFunc(comparedValue, value));
        }

        private static void CheckValues(RelationCondition condition)
        {
            if (!condition.Values.All(x => IsValueCorrect(condition.ValueType, x)))
            {
                throw new ConfigIncorrectException(
                    "Condition contains incorrect value",
                    DataMemberUtilities.GetPath<RelationCondition>(x => x.Values),
                    condition.Values.IndexOfFirst(x => !IsValueCorrect(condition.ValueType, x)));
            }
        }

        private static IComparable GetObjectValue<T>(string propertyName, T o)
        {
            var property = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
               .FirstOrDefault(x => DataMemberUtilities.GetDataMemberName(x) == propertyName);

            if (property == null)
            {
                throw new ConfigIncorrectException(
                    "Condition contains incorrect property name",
                    DataMemberUtilities.GetPath<RelationCondition>(x => x.PropertyName));
            }

            var value = property.GetValue(o);
            if (value is Enum enumValue)
                value = enumValue.GetEnumMemberValue();
            return (IComparable)value;
        }

        private static void ReplaceValues(RelationCondition condition)
        {
            if (condition.ValueType == RelationComparisonValueType.DateTime)
                ReplaceDateTimeValues(condition.Values);
        }

        private static void ReplaceDateTimeValues(object[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (Equals(values[i], DateTimeValues.Now.GetEnumMemberValue()))
                    values[i] = DateTime.UtcNow;
            }
        }

        private static Func<IComparable, object, bool> GetConditionFunc(RelationComparisonType? comparisonType)
        {
            bool ThrowNullException()
                => throw new ConfigIncorrectException(
                    "Property is null. Add condition to check this",
                    DataMemberUtilities.GetPath<RelationCondition>(x => x.PropertyName));

            return comparisonType switch
            {
                null => Equals,
                RelationComparisonType.Undefined => Equals,
                RelationComparisonType.Equal => Equals,
                RelationComparisonType.NotEqual => (comparable, o) => !Equals(comparable, o),
                RelationComparisonType.Greater => (comparable, o)
                    => comparable != null ? comparable.CompareTo(o) > 0 : ThrowNullException(),
                RelationComparisonType.Less => (comparable, o)
                    => comparable != null ? comparable.CompareTo(o) < 0 : ThrowNullException(),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(comparisonType),
                    "Not supported comparison type")
            };
        }

        private static bool IsValueCorrect(RelationComparisonValueType? valueType, object value)
            => valueType switch
            {
                null => true,
                RelationComparisonValueType.Undefined => true,
                RelationComparisonValueType.Int => value is int or null,
                RelationComparisonValueType.Float => value is float or double or null,
                RelationComparisonValueType.DateTime => value is DateTime or null ||
                    IsReplacingValue(value, DateTimeValues.Undefined),
                RelationComparisonValueType.String => value is string or null,
                _ => throw new ArgumentOutOfRangeException(nameof(valueType), "Condition contains incorrect value type")
            };

        private static bool IsReplacingValue<T>(object value, T ignore)
            where T : struct, Enum
        {
            if (value is not string)
                return false;

            return Enum.GetValues<T>()
               .Where(enumValue => !enumValue.Equals(ignore))
               .Select(enumValue => enumValue.GetEnumMemberValue())
               .Any(name => name.Equals(value));
        }

        private static bool IsAllMet(IRelationRule rule, Issue issue)
        {
            CheckRule(rule);

            return rule.Conditions == null ||
                rule.Conditions.Length == 0 ||
                rule.Conditions.All(x => x.IsMet(issue));
        }

        private static bool IsAllMet(
            IRelationRule rule,
            Issue issue,
            ObjectiveExternalDto objective)
        {
            CheckRule(rule);

            return rule.Conditions == null || rule.Conditions.Length == 0 ||
                rule.Conditions.All(
                    x => x.ObjectType == ComparisonObjectType.Bim360 ? x.IsMet(issue) : x.IsMet(objective));
        }

        private static void CheckRule(IRelationRule rule)
        {
            if (rule.Conditions != null &&
                rule.Conditions.Any(x => x.ComparisonType == RelationComparisonType.Undefined))
            {
                throw new ConfigIncorrectException(
                    "Conditions contains incorrect comparison type",
                    DataMemberUtilities.GetPath<RelationCondition>(x => x.ComparisonType),
                    rule.Conditions.IndexOfFirst(x => x.ComparisonType == RelationComparisonType.Undefined));
            }

            if (rule.Conditions != null &&
                rule.Conditions.Any(x => x.ObjectType == ComparisonObjectType.Undefined))
            {
                throw new ConfigIncorrectException(
                    "Conditions contains incorrect object type",
                    DataMemberUtilities.GetPath<RelationCondition>(x => x.ObjectType),
                    rule.Conditions.IndexOfFirst(x => x.ObjectType == ComparisonObjectType.Undefined));
            }
        }
    }
}
