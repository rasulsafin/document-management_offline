using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Forge.Interfaces
{
    /// <summary>
    /// The service for working with both document-related (pusphin) issues and project-related issues.
    /// </summary>
    public interface IIssuesService
    {
        /// <summary>
        /// Retrieves information about all the BIM 360 issues in a project, including details about their associated comments and attachments.
        /// To get more detailed information about a single issue, for example, details about whether attachments have been added to an issue since a specific date, see <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-:id-GET/">GET issues/:id</a>.
        /// <br/>
        /// You can retrieve both document-related (pusphin) issues and project-related issues.
        /// For details about pushpin issues, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/pushpins/create-pushpin/">Create Pushpin Issues in Your App</a> and <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/pushpins/retrieve-pushpin/">Render Pushpin Issues in Your App</a> tutorials.
        /// <br/>
        /// To verify whether an issue is document-related (pusphin) or project-related, check the <b>target_urn</b> attribute in the response.
        /// If the issue is project-related the <b>target_urn</b> will be assigned a <b>null</b> value, if the issue is document-related, it will be assigned an ID.
        /// BIM 360 issues are managed either in the <a href="https://docs.b360.autodesk.com/">BIM 360 Document Management</a> module or the <a href="https://field.b360.autodesk.com/">BIM 360 Field Management</a> module.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <param name="parameters">The parameters to configure the response payload.</param>
        /// <returns>The information about all the BIM 360 issues in a project.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-GET/">`<b>GET</b> issues` on forge.autodesk.com</a></footer>
        IAsyncEnumerable<Issue> GetIssuesAsync(string containerID, IEnumerable<IQueryParameter> parameters = null);

        /// <summary>
        /// Adds a BIM 360 Document Management file to a BIM 360 issue.
        /// <br/>
        /// BIM 360 issues are managed either in the <a href="https://docs.b360.autodesk.com/">BIM 360 Document Management</a> module or the <a href="https://field.b360.autodesk.com/">BIM 360 Field Management</a> module.
        /// <br/>
        /// See the <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/tutorials/attach-BIM-360-files/">Attach BIM 360 Document Management Files to Issues</a> tutorial for more details.
        /// You can attach any type of file that you upload to BIM 360 Document Management.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <param name="attachment">The object to attach to the issue.</param>
        /// <returns>The attach result.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-attachments-POST/">`<b>POST</b> attachments` on forge.autodesk.com</a></footer>
        Task<Attachment> PostIssuesAttachmentsAsync(string containerID, Attachment attachment);

        /// <summary>
        /// Retrieves a list of supported issue types (e.g., quality and safety) and issue subtypes (e.g., work to complete and pre-punch list) that you can allocate to an issue.
        /// By default, the response only includes issues types. To also include issue subtypes in the response, use the <b>include=subtypes</b> query string parameter.
        /// <br/>
        /// To verify which attributes the user can update, call <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-:id-GET/">GET issues/:id</a> and check the <b>permitted_attributes</b> and <b>permitted_statuses</b> lists.
        /// <br/>
        /// Note that issue type and subtype IDs are unique for each project.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <returns>The list of supported issue types (e.g., quality and safety) and issue subtypes (e.g., work to complete and pre-punch list) that you can allocate to an issue.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/ng-issue-types-GET/">`<b>GET</b> ng-issue-types` on forge.autodesk.com</a></footer>
        Task<List<IssueType>> GetIssueTypesAsync(string containerID);

        /// <summary>
        /// Adds a BIM 360 issue to a project.
        /// You can create both document-related (pushpin) issues, and project-related issues.
        /// For details about how to create pushpin issues, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/pushpins/create-pushpin/">Create Pushpin Issues in Your App</a> tutorial.
        /// <br/>
        /// BIM 360 issues are managed either in the <a href="https://docs.b360.autodesk.com/">BIM 360 Document Management</a> module or the <a href="https://field.b360.autodesk.com/">BIM 360 Field Management</a> module.
        /// <br/>
        /// The following users can create issues:
        /// <ul><li>Project admins</li>
        /// <li>Project members who are assigned either <i>create</i>, <i>view and create</i>, or <i>full control</i> Management permissions. For more details, open the Project Admin module and see the Services tab.</li></ul>
        /// <br/>
        /// To verify whether a user can create issues for a specific project, call <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/users-me-GET">GET users/me</a> and verify that the <b>quality_issues</b> section includes the <b>new</b> object.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <param name="issue">The issue to create.</param>
        /// <returns>The created issue.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-POST/">`<b>POST</b> issues` on forge.autodesk.com</a></footer>
        Task<Issue> PostIssueAsync(string containerID, Issue issue);

        /// <summary>
        /// Updates a BIM 360 issue.
        /// <br/>
        /// BIM 360 issues are managed either in the <a href="https://docs.b360.autodesk.com/">BIM 360 Document Management</a> module or the <a href="https://field.b360.autodesk.com/">BIM 360 Field Management</a> module.
        /// <br/>
        /// The following users can create issues:
        /// <ul><li>Project admins</li>
        /// <li>Project members who are assigned either <i>create</i>, <i>view and create</i>, or <i>full control</i> Management permissions. For more details, open the Project Admin module and see the Services tab.</li></ul>
        /// <br/>
        /// To verify whether a user can create issues for a specific project, call <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/users-me-GET">GET users/me</a> and verify that the <b>quality_issues</b> section includes the <b>new</b> object.
        /// <br/>
        /// To verify which attributes the user can update, call <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-:id-GET/">GET issues/:id</a> and check the <b>permitted_attributes</b> and <b>permitted_statuses</b> lists.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <param name="issue">The issue to update</param>
        /// <returns>The updated issue</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-:id-PATCH/">`<b>PATCH</b> issues/:id` on forge.autodesk.com</a></footer>
        Task<Issue> PatchIssueAsync(string containerID, Issue issue);

        /// <summary>
        /// Retrieves detailed information about a single BIM 360 issue. For example, details about whether attachments have been added to an issue since a specific time. For general information about all the issues in a project, see <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-GET">GET issues</a>.
        /// <br/>
        /// You can retrieve both document-related (pusphin) issues and project-related issues.
        /// For details about pushpin issues, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/pushpins/create-pushpin/">Create Pushpin Issues in Your App</a> and <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/pushpins/retrieve-pushpin/">Render Pushpin Issues in Your App</a> tutorials.
        /// <br/>
        /// To verify whether an issue is document-related (pusphin) or project-related, check the <b>target_urn</b> attribute in the response.
        /// If the issue is project-related the <b>target_urn</b> will be assigned a <b>null</b> value, if the issue is document-related, it will be assigned an ID.
        /// BIM 360 issues are managed either in the <a href="https://docs.b360.autodesk.com/">BIM 360 Document Management</a> module or the <a href="https://field.b360.autodesk.com/">BIM 360 Field Management</a> module.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <param name="issueID">The ID of the issue.</param>
        /// <returns>The detailed information about a single BIM 360 issue.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-:id-GET/">`<b>GET</b> issues/:id` on forge.autodesk.com</a></footer>
        Task<Issue> GetIssueAsync(string containerID, string issueID);

        /// <summary>
        /// Retrieves information about all the attachments associated with a specific BIM 360 issue in a project.
        /// <br/>
        /// BIM 360 issues are managed either in the <a href="https://docs.b360.autodesk.com/">BIM 360 Document Management</a> module or the <a href="https://field.b360.autodesk.com/">BIM 360 Field Management</a> module.
        /// <br/>
        /// To retrieve information about the attachments associated with <b>all</b> the issues in a project, call <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-GET">GET issues</a> with the <b>include=attachments</b> query string parameter.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <param name="issueID">The ID of the issue.</param>
        /// <param name="parameters">The parameters to configure the response payload.</param>
        /// <returns>The information about all the attachments associated with a specific BIM 360 issue in a project.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-attachments-GET/">`<b>GET</b> issues/:id/attachments` on forge.autodesk.com</a></footer>
        IAsyncEnumerable<Attachment> GetAttachmentsAsync(string containerID, string issueID, IEnumerable<IQueryParameter> parameters = null);

        /// <summary>
        /// Retrieves a list of supported root causes that you can allocate to an issue. For example, communication and coordination.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <returns>The list of supported root causes that you can allocate to an issue. For example, communication and coordination.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/root-causes-GET/">`<b>GET</b> root-causes` on forge.autodesk.com</a></footer>
        Task<List<RootCause>> GetRootCausesAsync(string containerID);

        /// <summary>
        /// Retrieves the profile information about an end user, including the user’s name and workflow role, as well as details about whether the user has permissions to create issues.
        /// However it does not currently provide full information about a users permissions.
        /// For example, whether a user has <i>basic</i>, <i>view all</i>, <i>create</i>, <i>view and create</i>, or <i>full control</i> permissions.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <returns>The profile information about an end user, including the user’s name and workflow role, as well as details about whether the user has permissions to create issues.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/users-me-GET/">`<b>GET</b> users/me` on forge.autodesk.com</a></footer>
        Task<UserInfo> GetMeAsync(string containerID);

        /// <summary>
        /// Retrieves all the comments associated with a BIM 360 issue.
        /// <br/>
        /// BIM 360 issues are managed either in the <a href="https://docs.b360.autodesk.com/">BIM 360 Document Management</a> module or the <a href="https://field.b360.autodesk.com/">BIM 360 Field Management</a> module.
        /// <br/>
        /// To retrieve the comments associated with all issues in a project, call <a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-GET">GET issues</a> with the <b>include=comments</b> query string parameter.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <param name="issueID">The ID of the issue.</param>
        /// <param name="parameters">The parameters to configure the response payload.</param>
        /// <returns>The all the comments associated with a BIM 360 issue</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-:id-comments-GET/">`<b>GET</b> issues/:id/comments` on forge.autodesk.com</a></footer>
        IAsyncEnumerable<Comment> GetCommentsAsync(string containerID, string issueID, IEnumerable<IQueryParameter> parameters = null);

        /// <summary>
        /// Adds a comment to a BIM 360 issue.
        /// <br/>
        /// BIM 360 issues are managed either in the <a href="https://docs.b360.autodesk.com/">BIM 360 Document Management</a> module or the <a href="https://field.b360.autodesk.com/">BIM 360 Field Management</a> module.
        /// </summary>
        /// <param name="containerID">
        /// Each project is assigned a container that stores all the issues for the project.
        /// To find the ID, see the <a href="https://forge.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-container-id/">Retrieve a Container ID</a> tutorial.
        /// </param>
        /// <param name="comment">The comment to add.</param>
        /// <returns>The created comment.</returns>
        /// <footer><a href="https://forge.autodesk.com/en/docs/bim360/v1/reference/http/field-issues-comments-POST/">`<b>POST</b> comments` on forge.autodesk.com</a></footer>
        Task<Comment> PostIssuesCommentsAsync(string containerID, Comment comment);
    }
}
