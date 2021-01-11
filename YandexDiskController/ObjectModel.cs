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
        public static void UpdateProjects()
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
            if (Directory.Exists(dirName))
                Directory.Delete(dirName, true);
            Projects.Remove(project);
            Synchronizer.AddTransaction(TransType.Delete, id);
        }
        public static void RenameProject(ID<ProjectDto> id, string newName)
        {
            var project = Projects.First(x => x.dto.ID == id);

            string oldDirName = PathManager.GetProjectDir(project.dto);
            project.Title = newName;
            SaveProject(project.dto);
            var newDirName = PathManager.GetProjectDir(project.dto);
            if (Directory.Exists(oldDirName))
                Directory.Move(oldDirName, newName);
            else
                Directory.CreateDirectory(newName);            
        }
        #endregion
        #region Objective
        public static void UpdateObjectives(ProjectDto project)
        {
            string objDir = PathManager.GetObjectivesDir(project);
            var list = GetObjectives(project);
            //list.Sort((x, y) => ((int)x.ID).CompareTo((int)y.ID));

            Objectives.Clear();
            foreach (ObjectiveDto item in list)
            {
                Objectives.Add(new ObjectiveModel(item));
            }
        }
        public static List<ObjectiveDto> GetObjectives(ProjectDto project)
        {
            var result = new List<ObjectiveDto>();

            string dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) return result;

            string dirObj = PathManager.GetObjectivesDir(project);
            if (!Directory.Exists(dirObj)) return result;

            DirectoryInfo dirInfoObj = new DirectoryInfo(dirObj);

            foreach (var item in dirInfoObj.GetFiles())
            {
                if (item.Name.StartsWith("objective"))
                {
                    var json = File.ReadAllText(item.FullName);
                    ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
                    result.Add(objective);
                }
            }
            return result;
        }
        public static void SaveObjective(ObjectiveDto objective, ProjectDto project)
        {
            string dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);

            string dirObj = PathManager.GetObjectivesDir(project);
            if (!Directory.Exists(dirObj)) Directory.CreateDirectory(dirObj);

            string filename = PathManager.GetObjectiveFile(objective, project);
            var json = JsonConvert.SerializeObject(objective, Formatting.Indented);
            File.WriteAllText(filename, json);
            Synchronizer.AddTransaction(TransType.Update, objective.ID);
        }
        public static void DeleteObjective(ObjectiveDto objective, ProjectDto project)
        {
            string filename = PathManager.GetObjectiveFile(objective, project);
            File.Delete(filename);
            Synchronizer.AddTransaction(TransType.Delete, objective.ID);
        }
        public static (ObjectiveDto objective, ProjectDto project) GetObjective(ID<ObjectiveDto> id)
        {
            var projects = Projects.Select(x => x.dto).ToList();
            foreach (var project in projects)
            {
                var dir = PathManager.GetObjectivesDir(project);
                DirectoryInfo dirInfoObj = new DirectoryInfo(dir);
                if (dirInfoObj.Exists)
                {
                    foreach (var item in dirInfoObj.GetFiles())
                    {
                        if (PathManager.TryParseObjectiveId(item.Name, out ID<ObjectiveDto> _id) && id == _id)
                        {
                            var json = File.ReadAllText(item.FullName);
                            ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
                            return (objective, project);
                        }
                    }
                }
            }
            return (null, null);
        }


        #endregion

        #region Item
        public static void UpdateItems(ProjectDto project, ObjectiveDto objective =  null)
        {            
            List<ItemDto> collection = GetItems(project, objective);
            Items.Clear();
            foreach (ItemDto item in collection)
            {
                Items.Add((ItemModel)item);
            }
        }
        public static void SaveItem(ItemDto item, ProjectDto project, ObjectiveDto objective = null)
        {
            var dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);

            var dirItems = PathManager.GetItemsDir(project);
            if (!Directory.Exists(dirItems)) Directory.CreateDirectory(dirItems);

            string path = PathManager.GetItemFile(item, project, objective);


            string json = JsonConvert.SerializeObject(item, Formatting.Indented);
            File.WriteAllText(path, json);
            Synchronizer.AddTransaction(TransType.Update, item.ID);
        }        
        public static void DeleteItem(ItemDto item, ProjectDto project, ObjectiveDto objective = null)
        {
            var dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) return;

            var dirItems = PathManager.GetItemsDir(project);
            if (!Directory.Exists(dirItems)) return;

            string path = PathManager.GetItemFile(item, project, objective);
            //string path = objective == null
            //        ? PathManager.GetItemFile(item, project, objective)
            //        : PathManager.GetItemFile(item, project);            
            File.Delete(path);
            Synchronizer.AddTransaction(TransType.Delete, item.ID);
        }
        public static List<ItemDto> GetItems(ProjectDto project, ObjectiveDto objective = null)
        {
            static void readFiles(List<ItemDto> result, DirectoryInfo dir, ID<ObjectiveDto> tagret)
            {
                foreach (var file in dir.GetFiles())
                {
                    if (PathManager.TryParseItemId(file.Name, out ID<ItemDto> idItem, out ID<ObjectiveDto> idObjective))
                    {
                        if (idObjective == tagret)
                        {
                            var json = File.ReadAllText(file.FullName);
                            ItemDto item = JsonConvert.DeserializeObject<ItemDto>(json);
                            result.Add(item);
                        }
                    }
                }
            }

            var dirItem = PathManager.GetItemsDir(project);
            if (!Directory.Exists(dirItem)) return new List<ItemDto>();
            var result = new List<ItemDto>();
            DirectoryInfo dir = new DirectoryInfo(dirItem);
            if (objective == null)
            {
                readFiles(result, dir, ID<ObjectiveDto>.InvalidID);
            }
            else 
            {
                readFiles(result, dir, objective.ID);                
            }
            return result;
        }
        public static (ItemDto items, ObjectiveDto objective, ProjectDto project) GetItem(ID<ItemDto> id)
        {
            var projects = Projects.Select(x => x.dto).ToList();
            foreach (var project in projects)
            {
                var fileName = PathManager.GetItemFile(id, project);
                if (File.Exists(fileName))
                {
                    var json = File.ReadAllText(fileName);
                    ItemDto item = JsonConvert.DeserializeObject<ItemDto>(json);
                    return (item, null, project);
                }

                var objectives = GetObjectives(project);
                foreach (var objective in objectives)
                {
                    fileName = PathManager.GetItemFile(id, project, objective);
                    if (File.Exists(fileName))
                    {
                        var json = File.ReadAllText(fileName);
                        ItemDto item = JsonConvert.DeserializeObject<ItemDto>(json);
                        return (item, objective, project);
                    }
                }
                
            }
            return (null, null, null);
        }
        #endregion


    }
}