using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MRS.DocumentManagement
{
    public static class ObjectModel
    {
        public static ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public static ObservableCollection<ObjectiveModel> Objectives { get; set; } = new ObservableCollection<ObjectiveModel>();
        public static ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();
        public static Synchronizer Synchronizer { get; set; } = new Synchronizer();

        #region Project
        public static void UpdateProject()
        {
            var list = GetProjects();
            Projects.Clear();
            foreach (var item in list)
            {
                Projects.Add((ProjectModel)item);
            }
        }
        public static List<ProjectDto> GetProjects()
        {
            List<ProjectDto> projects = new List<ProjectDto>();
            DirectoryInfo projDir = new DirectoryInfo(PathManager.GetProjectsDir());
            if (projDir.Exists)
            {
                foreach (var item in projDir.GetFiles())
                {
                    if (item.Name.StartsWith("project"))
                    {
                        string json = File.ReadAllText(item.FullName);
                        ProjectDto dto = JsonConvert.DeserializeObject<ProjectDto>(json);
                        projects.Add(dto);
                    }
                }
            }
            return projects;
        }

        public static ProjectDto GetProject(ID<ProjectDto> id)
        {
            if (!Directory.Exists(PathManager.GetProjectsDir())) return null;
            try
            {
                string filename = PathManager.GetProjectFile(id);
                string json = File.ReadAllText(filename);
                ProjectDto dto = JsonConvert.DeserializeObject<ProjectDto>(json);
                return dto;
            }
            catch (FileNotFoundException) { }
            return null;
        }
        public static void SaveProject(ProjectDto project)
        {
            if (!Directory.Exists(PathManager.GetProjectsDir())) 
                Directory.CreateDirectory(PathManager.GetProjectsDir());
            string fileName = PathManager.GetProjectFile(project);

            string json = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(fileName, json);
            Synchronizer.AddTransaction(TransType.Update, project.ID);
        }


        public static void DeleteProject(ID<ProjectDto> id)
        {
            if (!Directory.Exists(PathManager.GetProjectsDir())) return;
            var project = Projects.First(x => x.ID == (int)id);
            string fileName = PathManager.GetProjectFile(project.dto);
            string dirName = PathManager.GetProjectDir(project.dto);
            File.Delete(fileName);
            Directory.Delete(dirName, true);

            Projects.Remove(project);

            Synchronizer.AddTransaction(TransType.Delete, id);
        }

        public static void RenameProject(ID<ProjectDto> id, string newName)
        {
            var project = Projects.First(x => x.dto.ID == id);

            string oldDirName = PathManager.GetProjectDir(project.dto);
            project.Title = newName;
            var newDirName = PathManager.GetProjectDir(project.dto);
            if (Directory.Exists(oldDirName))
                Directory.Move(oldDirName, newName);
            else
                Directory.CreateDirectory(newName);
        }


        #endregion
        public static void SaveObjective(ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }

        public static void DeleteObjective(ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }

        public static void SaveItem(ItemDto item, ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }

        public static void DeleteItem(ItemDto item, ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }
    }
}