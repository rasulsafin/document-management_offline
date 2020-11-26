using MRS.DocumentManagement.Interface.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MRS.DocumentManagement.Tests
{
    internal class ObjectiveComparer : AbstractModelComparer<Objective>
    {
        private readonly UserComparer userComparer;
        private readonly IEqualityComparer<BimElement> bimComparer;
        private readonly IEqualityComparer<DynamicField> fieldComparer;

        public ObjectiveComparer(bool ignoreIDs = false) : base(ignoreIDs)
        {
            userComparer = new UserComparer(false);
            bimComparer = new DelegateComparer<BimElement>((x, y) => x.ItemID == y.ItemID && x.GlobalID == y.GlobalID);
            fieldComparer = new DelegateComparer<DynamicField>((x, y) => x.Key == y.Key && x.Type == y.Type && x.Value == y.Value);
        }

        public override bool NotNullEquals([DisallowNull] Objective x, [DisallowNull] Objective y)
        {
            var idMatched = IgnoreIDs ? true : x.ID == y.ID;

            bool bimMatched;
            if (x.BimElements != null && y.BimElements != null)
                bimMatched = x.BimElements.SequenceEqual(y.BimElements, bimComparer);
            else if (x.BimElements == null && y.BimElements == null)
                bimMatched = true;
            else
                return false;

            bool fieldsMatched;
            if (x.DynamicFields != null && y.DynamicFields != null)
                fieldsMatched = x.DynamicFields.SequenceEqual(y.DynamicFields, fieldComparer);
            else if (x.DynamicFields == null && y.DynamicFields == null)
                fieldsMatched = true;
            else
                return false;

            return idMatched
                && bimMatched
                && fieldsMatched
                && x.ProjectID == y.ProjectID
                && x.ParentObjectiveID == y.ParentObjectiveID
                && userComparer.Equals(x.Author, y.Author)
                && x.CreationDate == y.CreationDate
                && x.DueDate == y.DueDate
                && x.Title == y.Title
                && x.Description == y.Description
                && x.Status == y.Status
                && x.TaskType.ID == y.TaskType.ID
                && x.TaskType.Name == y.TaskType.Name;
        }
    }
}
