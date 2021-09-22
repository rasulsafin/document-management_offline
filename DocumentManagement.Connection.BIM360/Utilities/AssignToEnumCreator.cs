using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connection.Bim360.Forge;
using Brio.Docs.Connection.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connection.Bim360.Forge.Services;
using Brio.Docs.Connection.Bim360.Forge.Utils;
using Brio.Docs.Connection.Bim360.Synchronization.Utilities;
using Brio.Docs.Connection.Bim360.Utilities.Snapshot;

namespace Brio.Docs.Connection.Bim360.Utilities
{
    internal class AssignToEnumCreator : IEnumCreator<string, AssignToVariant, string>
    {
        private static readonly string ENUM_EXTERNAL_ID =
            $"{DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.AssignedTo)},{DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.AssignedToType)}";

        private static readonly string DISPLAY_NAME = MrsConstants.ASSIGN_TO_FIELD_NAME;

        private readonly AccountAdminService accountAdminService;

        public AssignToEnumCreator(AccountAdminService accountAdminService)
            => this.accountAdminService = accountAdminService;

        public string EnumExternalID => ENUM_EXTERNAL_ID;

        public string EnumDisplayName => DISPLAY_NAME;

        public bool CanBeNull => true;

        public string NullID => $"{EnumExternalID}{DynamicFieldUtilities.NULL_VALUE_ID}";

        public IEnumerable<string> GetOrderedIDs(IEnumerable<AssignToVariant> variants)
            => variants.OrderBy(x => x.Type).ThenBy(x => x.Entity).Select(x => x.Entity);

        public string GetVariantDisplayName(AssignToVariant variant)
            => variant.Title;

        public async Task<IEnumerable<AssignToVariant>> GetVariantsFromRemote(ProjectSnapshot projectSnapshot)
        {
            var users = (await accountAdminService.GetProjectUsersAsync(projectSnapshot.ID)).Where(
                    user => (user.Services
                       .FirstOrDefault(service => service.ServiceName == Constants.DOCUMENT_MANAGEMENT_SERVICE_NAME)
                      ?.Access ?? Constants.SERVICE_NONE_ACCESS) != Constants.SERVICE_NONE_ACCESS)
               .ToArray();

            var result = users.Select(
                    user => new AssignToVariant(user.AutodeskID, AssignToType.User, user.Name, projectSnapshot))
               .ToList();

            var hub = projectSnapshot.HubSnapshot.Entity;

            var roles = await accountAdminService.GetRolesAsync(hub, projectSnapshot.ID);
            result.AddRange(
                roles.Where(role => users.Any(y => y.RoleIds.Contains(role.ID)))
                   .Select(
                        role => new AssignToVariant(
                            role.MemberGroupID,
                            AssignToType.Role,
                            role.Name,
                            projectSnapshot)));

            var companies = await accountAdminService.GetCompaniesAsync(hub, projectSnapshot.ID);
            result.AddRange(
                companies.Select(
                    company => new AssignToVariant(
                        company.MemberGroupID,
                        AssignToType.Company,
                        company.Name,
                        projectSnapshot)));
            return result;
        }

        public IEnumerable<AssignToVariant> GetSnapshots(ProjectSnapshot project)
            => project.AssignToVariants.Values;

        public IEnumerable<string> DeserializeID(string externalID)
            => DynamicFieldUtilities.DeserializeID<string>(externalID);
    }
}
