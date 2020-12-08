using System;
using System.IO;
using System.Linq;

namespace MRS.Bim.Tools
{
    public class PathUtility
    {
        private static readonly string APPLICATION_DIRECTORY_NAME = "Brio MRS";
        private static readonly string DATABASE_DIRECTORY_NAME = "Database";
        private static readonly string DEFAULT_PROJECT_DIRECTORY_NAME = ".Default";
        private static readonly string MEDIA_DIRECTORY_NAME = "Media";
        private static readonly string MY_DOCUMENTS = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string[] PICTURES_EXTENSIONS = { ".png", ".jpg" };
        private static readonly string[] VIDEO_EXTENSIONS = { ".mp4" };
        private static readonly string[] IFC_EXTENSIONS = { ".ifc", ".ifczip", ".ifcxml", ".nwd", ".nwf", ".nwc" };

        public static PathUtility Instance => INSTANCE_CONTAINER.Value;
        private static readonly Lazy<PathUtility> INSTANCE_CONTAINER = new Lazy<PathUtility>(() => new PathUtility());

        public event Action PathChanged;

        public string ProjectDirectory
        {
            get
            {
                var directory = Combine(Database, project);
                Directory.CreateDirectory(directory);
                return directory;
            }
        }
        public string MediaDirectory
        {
            get
            {
                var directory = Combine(ProjectDirectory, MEDIA_DIRECTORY_NAME);
                Directory.CreateDirectory(directory);
                return directory;
            }
        }

        public string TempDirectory
        {
            get
            {
                var directory = Combine(@"C:\", "Temp");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }

        public static string ApplicationFolder => Combine(MY_DOCUMENTS, APPLICATION_DIRECTORY_NAME);

        public static string Database => Combine(ApplicationFolder, DATABASE_DIRECTORY_NAME);

        private string project;

        private PathUtility()
        {
            project = DEFAULT_PROJECT_DIRECTORY_NAME;
            //var connection = ConnectionHandler.Instance;
            //connection.ConnectionStateChanged += OnConnectionStateChanged;
            //connection.ProjectPicked += OnProjectPicked;
        }

        public static bool IsPicture(string path)
            => ExtensionsContains(PICTURES_EXTENSIONS, path);

        public static bool IsVideo(string path)
            => ExtensionsContains(VIDEO_EXTENSIONS, path);

        public static bool IsMedia(string path)
            => IsPicture(path) || IsVideo(path);

        public static bool IsIfc(string path)
            => ExtensionsContains(IFC_EXTENSIONS, path);

        public string GetFullPath(string fileName)
            => Combine(IsMedia(fileName) ? MediaDirectory : ProjectDirectory, fileName);

        private static string Combine(params string[] strings)
            => Path.GetFullPath(Path.Combine(strings));

        private static bool ExtensionsContains(string[] source, string path)
            => source.Contains(Path.GetExtension(path).ToLower());

        //private void OnConnectionStateChanged(ConnectionState obj)
        //    => project = DEFAULT_PROJECT_DIRECTORY_NAME;

        //private void OnProjectPicked(Project newProject)
        //{
        //    project = newProject.Name;
        //    PathChanged?.Invoke();
        //}
    }
}