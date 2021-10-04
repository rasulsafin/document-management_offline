using System;
using Brio.Docs.Connections.Bim360.Synchronization.Models.StatusRelations;

namespace Brio.Docs.Connections.Bim360.Synchronization.Interfaces.StatusRelations
{
    /// <summary>
    /// Represents a rule for converting one enumeration value to another enumeration value.
    /// </summary>
    internal interface IRelationRule
    {
        /// <summary>
        /// The source enumeration value to be converted.
        /// </summary>
        Enum Source { get; }

        /// <summary>
        /// The target value of the enumeration, the result of the conversion.
        /// </summary>
        Enum Destination { get; }

        /// <summary>
        /// Rule conditions, all of which must be met to convert to a target value.
        /// </summary>
        RelationCondition[] Conditions { get; }
    }
}
