using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Tests
{
    internal class ObjectiveComparer : AbstractModelComparer<ObjectiveDto>
    {
        private readonly UserComparer userComparer;
        private readonly IEqualityComparer<BimElementDto> bimComparer;
        private readonly IEqualityComparer<DynamicFieldDto> fieldComparer;

        public ObjectiveComparer(bool ignoreIDs = false) : base(ignoreIDs)
        {
            userComparer = new UserComparer(false);
            bimComparer = new DelegateComparer<BimElementDto>((x, y) => x.ParentName == y.ParentName && x.GlobalID == y.GlobalID);
            fieldComparer = new DelegateComparer<DynamicFieldDto>((x, y) => x.Key == y.Key && x.Type == y.Type && x.Value == y.Value);
        }

        public override bool NotNullEquals([DisallowNull] ObjectiveDto x, [DisallowNull] ObjectiveDto y)
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
                && x.AuthorID == y.AuthorID
                && x.CreationDate == y.CreationDate
                && x.DueDate == y.DueDate
                && x.Title == y.Title
                && x.Description == y.Description
                && x.Status == y.Status
                && x.ObjectiveTypeID == y.ObjectiveTypeID;
        }
    }
}
