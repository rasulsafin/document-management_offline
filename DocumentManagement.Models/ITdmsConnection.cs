using System.Collections.Generic;

namespace MRS.Bim.DocumentManagement.Tdms 
{ 
    public interface ITdmsConnection 
    { 
        bool Add(DMFile file); 
        bool Add(DMFile[] files);
        Issue Add(Issue issue); 
        bool Add(Issue[] issues); 
        bool Add(Job job); 
        bool Add(Job[] jobs); 
        bool Change(DMFile file); 
        bool Change(DMFile[] files); 
        bool Change(Issue issue); 
        bool Change(Issue[] issues); 
        bool Change(Job job); 
        bool Change(Job[] jobs); 
        (bool, string) Connect((string login, string password, string server, string db) parameters); 
        bool Disconnect(bool unused); 
        bool Upload((string id, DMFile file) parameters); 
        bool Upload((string id, DMFile[] files) parameters); 
        bool Download((string id, DMFile file) parameters); 
        bool Download((string id, DMFile[] files) parameters); 
        DMFile GetDMFile((string id, string fileId) parameters); 
        DMFile[] GetDMFiles((string id, DMFileType type) parameters);
        Issue GetIssue(string id); 
        Issue[] GetIssues(string id); 
        Job GetJob(string id); 
        Job[] GetJobs(string id); 
        Project GetProject(string id); 
        Project[] GetProjects(string id);
        DMAccount GetAccountInfo(bool b);
        Dictionary<string, DMItem[]> GetEnums(bool b);
        bool Remove(DMFile file); 
        bool Remove(DMFile[] files); 
        bool Remove(Issue issue); 
        bool Remove(Issue[] issues); 
        bool Remove(Job job); 
        bool Remove(Job[] jobs); 
        bool Remove(Project project);
        bool Cancel(bool unused);
    } 
}