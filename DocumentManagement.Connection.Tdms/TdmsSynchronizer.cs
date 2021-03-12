using System;
using System.Collections.Generic;
using System.Linq;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class TdmsSynchronizer
    {
        protected readonly TDMSApplication tdms;

        public TdmsSynchronizer(TDMSApplication tdms)
        {
            this.tdms = tdms;
        }

        protected List<string> FindByDef(string objectTypeId, DateTime date)
        {
            var list = new List<string>();

            var queryCom = tdms.CreateQuery();
            queryCom.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, objectTypeId);

            var updatedAfter = queryCom.Objects.OfType<TDMSObject>().Where(o => DateTime.Parse(o.ModifyTime.ToString()) > date);

            foreach (TDMSObject obj in updatedAfter)
                list.Add(obj.GUID);

            return list;
        }
    }
}