using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class StatusToStringResolver : IValueResolver<Objective, ObjectiveToReportDto, string>
    {
        public string Resolve(Objective source, ObjectiveToReportDto destination, string destMember, ResolutionContext context)
        {
            switch (source.Status)
            {
                case (int)ObjectiveStatus.Undefined:
                    return "Не определен";
                case (int)ObjectiveStatus.Open:
                    return "Открыт";
                case (int)ObjectiveStatus.InProgress:
                    return "В ходе выполнения";
                case (int)ObjectiveStatus.Ready:
                    return "Готов";
                case (int)ObjectiveStatus.Late:
                    return "Просрочен";
                default:
                    return "-";
            }
        }
    }
}
