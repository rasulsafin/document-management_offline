using CommandLine;

namespace MRS.DocumentManagement.Launcher
{
    public class AppOptions
    {
        public AppOptions()
            : this(false, null, null)
        {
        }

        public AppOptions(bool devMode, string dMExecutable, string languageTag)
        {
            DevMode = devMode;
            DMExecutable = dMExecutable;
            LanguageTag = languageTag;
        }

        [Option('d', "develop", Default = false, HelpText = "Set development mode")]
        public bool DevMode { get; }

        [Option('e', "executable", HelpText = "Path to DM service executable")]
        public string DMExecutable { get; }

        [Option('l', "language", HelpText = "UI language")]
        public string LanguageTag { get; }
    }
}
