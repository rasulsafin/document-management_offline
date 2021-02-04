namespace Forge
{
    public class Project : CloudItem<,>
    {
        public Folder rootFolder;
        public string issuesContainerId;
        public IssuesContainer issues;
        public UsersContainer users;

        public Project(Folder root = null)
        {
            rootFolder = root;
            if (rootFolder != null)
                rootFolder.project = this;

            issues = new IssuesContainer(this);
            users = new UsersContainer(this);
        }
    }
}