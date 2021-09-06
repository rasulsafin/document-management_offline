using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
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
                    x => (x.Services.FirstOrDefault(s => s.ServiceName == "documentManagement")?.Access ?? "none") !=
                        "none")
               .ToArray();

            var result = users.Select(
                    user => new AssignToVariant(user.AutodeskID, AssignToType.User, user.Name, projectSnapshot))
               .ToList();

            var accountID = projectSnapshot.HubSnapshot.Entity.ID.Remove(0, 2);

            var roles = await accountAdminService.GetRolesAsync(
                accountID,
                projectSnapshot.ID);
            result.AddRange(
                roles.Where(x => users.Any(y => y.RoleIds.Contains(x.ID)))
                   .Select(x => new AssignToVariant(x.MemberGroupID, AssignToType.Role, x.Name, projectSnapshot)));

            var companies = await accountAdminService.GetCompaniesAsync(accountID, projectSnapshot.ID);
            result.AddRange(
                companies.Select(
                    x => new AssignToVariant(x.MemberGroupID, AssignToType.Company, x.Name, projectSnapshot)));
            return result;
        }

        public IEnumerable<AssignToVariant> GetSnapshots(ProjectSnapshot project)
            => project.AssignToVariants.Values;

        public IEnumerable<string> DeserializeID(string externalID)
            => DynamicFieldUtilities.DeserializeID<string>(externalID);
    }
}
