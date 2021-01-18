using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MRS.DocumentManagement
{
    /// <summary>Код от сюда должен перекачивать в yandexManager</summary>
    public static class ObjectModel
    {
        public static ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public static ObservableCollection<ObjectiveModel> Objectives { get; set; } = new ObservableCollection<ObjectiveModel>();
        public static ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();
        public static ObservableCollection<UserModel> Users { get; set; } = new ObservableCollection<UserModel>();
        public static Synchronizer Synchronizer { get; set; } = new Synchronizer();

        #region User
        public static void UpdateUsers()
        {
            var list = GetUsers();
            Users.Clear();
            foreach (var item in list)
            {
                Users.Add((UserModel)item);
            }
        }
        public static List<UserDto> GetUsers()
        {
            List<UserDto> users = new List<UserDto>();
            FileInfo usrFile = new FileInfo(PathManager.GetUsersFile());
            if (usrFile.Exists)
            {
                string json = File.ReadAllText(usrFile.FullName);
                List<UserDto> _users = JsonConvert.DeserializeObject<List<UserDto>>(json);
                users.AddRange(_users);
            }
            return users;
        }

        public static void SaveUsers(List<UserDto> users = null)
        {
            if (users == null)
                users = Users.Select(x => x.dto).ToList();
            string json = JsonConvert.SerializeObject(users);
            string path = PathManager.GetUsersFile();
            File.WriteAllText(path, json);
        }
        #endregion
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
            FileInfo projFile = new FileInfo(PathManager.GetProjectsFile());
            if (projFile.Exists)
            {
                string json = File.ReadAllText(projFile.FullName);
                List<ProjectDto> _projects = JsonConvert.DeserializeObject<List<ProjectDto>>(json);
                projects.AddRange(_projects);
            }
            return projects;
        }
        public static void SaveProjects(List<ProjectDto> projects = null)
        {
            if (projects == null)
                projects = Projects.Select(x => x.dto).ToList();
            string json = JsonConvert.SerializeObject(projects);
            string path = PathManager.GetProjectsFile();
            File.WriteAllText(path, json);
        }

        #endregion
        #region Objective
        public static void UpdateObjectives(ProjectDto project)
        {
            //string objDir = PathManager.GetObjectivesDir(project);
            var list = GetObjectives(project);
            var buf = new List<ObjectiveModel>();
            Objectives.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                ObjectiveDto item = list[i];
                buf.Add(new ObjectiveModel(item));
            }

            for (int i = 0; buf.Count != 0;)
            {
                ObjectiveModel model = buf[i];                
                    ObjectiveModel parent = FindParent(model.ParentObjectiveID);
                    if (parent == null)
                        Objectives.Add(model);
                    else
                        parent.SubObjectives.Add(model);                
                buf.RemoveAt(i);
            }

            ObjectiveModel FindParent(int? parentID)
            {
                if (parentID != null)
                {                    
                    foreach (var item in Objectives)
                    {
                        if (item.ID == parentID)
                            return item;
                    }
                    foreach (var item in buf)
                    {
                        if (item.ID == parentID)
                            return item;
                    }
                }
                return null;
            }

        }


        public static List<ObjectiveDto> GetObjectives(ProjectDto project)
        {
            var result = new List<ObjectiveDto>();

            string dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) return result;
            try
            {
                string path = PathManager.GetObjectivesFile(project);
                var json = File.ReadAllText(path);
                List<ObjectiveDto> objectives = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);
                result.AddRange(objectives);
            }
            catch (FileNotFoundException) { }
            return result;
        }
        public static void SaveObjectives(ProjectDto project, List<ObjectiveDto> objectives = null)
        {
            string dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);

            if (objectives == null)
                objectives = Objectives.Select(x => x.dto).ToList();

            string filename = PathManager.GetObjectivesFile(project);
            var json = JsonConvert.SerializeObject(objectives);
            File.WriteAllText(filename, json);
        }
        //public static (ObjectiveDto objective, ProjectDto project) GetObjective(ID<ObjectiveDto> id)
        //{
        //    //var projects = Projects.Select(x => x.dto).ToList();
        //    //foreach (var project in projects)
        //    //{
        //    //    var dir = PathManager.GetObjectivesDir(project);
        //    //    DirectoryInfo dirInfoObj = new DirectoryInfo(dir);
        //    //    if (dirInfoObj.Exists)
        //    //    {
        //    //        foreach (var item in dirInfoObj.GetFiles())
        //    //        {
        //    //            if (PathManager.TryParseObjectiveId(item.Name, out ID<ObjectiveDto> _id) && id == _id)
        //    //            {
        //    //                var json = File.ReadAllText(item.FullName);
        //    //                ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
        //    //                return (objective, project);
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    return (null, null);
        //}

        #endregion
        #region Item
        public static void UpdateItems(ProjectDto project, ObjectiveDto objective = null)
        {
            List<ItemDto> collection = GetItems(project, objective);
            Items.Clear();
            foreach (ItemDto item in collection)
            {
                Items.Add((ItemModel)item);
            }
        }
        public static List<ItemDto> GetItems(ProjectDto project, ObjectiveDto objective = null)
        {
            try
            {
                string path = "";
                if (objective == null)
                    path = PathManager.GetItemsFile(project);
                else
                    path = PathManager.GetItemsFile(objective, project);
                if (!File.Exists(path)) return new List<ItemDto>();
                var json = File.ReadAllText(path);
                List<ItemDto> items = JsonConvert.DeserializeObject<List<ItemDto>>(json);
                return items;
            }
            catch { }
            return new List<ItemDto>();
        }
        public static void SaveItems(ProjectDto project, ObjectiveDto objective = null)
        {
            var dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);

            List<ItemDto> items = Items.Select(x => x.dto).ToList();

            string path = "";
            if (objective == null)
                path = PathManager.GetItemsFile(project);
            else
                path = PathManager.GetItemsFile(objective, project);


            string json = JsonConvert.SerializeObject(items);
            File.WriteAllText(path, json);
        }

        public static void SaveItems(ProjectDto project, List<ItemDto> items)
        {
            var dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);
            string path = PathManager.GetItemsFile(project);
            string json = JsonConvert.SerializeObject(items);
            File.WriteAllText(path, json);
        }
        public static void SaveItems(ProjectDto project, ObjectiveDto objective, List<ItemDto> items)
        {
            var dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);
            string path = PathManager.GetItemsFile(objective, project);
            string json = JsonConvert.SerializeObject(items);
            File.WriteAllText(path, json);
        }
        //public static void DeleteItem(ItemDto item, ProjectDto project, ObjectiveDto objective = null)
        //{
        //    var dirProj = PathManager.GetProjectDir(project);
        //    if (!Directory.Exists(dirProj)) return;

        //    var dirItems = PathManager.GetItemsDir(project);
        //    if (!Directory.Exists(dirItems)) return;

        //    string path = PathManager.GetItemFile(item, project, objective);

        //    File.Delete(path);
        //    if (objective == null)
        //        Synchronizer.Update(item.ID, project.ID);
        //    else
        //        Synchronizer.Update(item.ID, objective.ID, project.ID);
        //}
        //public static (ItemDto items, ObjectiveDto objective, ProjectDto project) GetItem(ID<ItemDto> id)
        //{
        //    var projects = Projects.Select(x => x.dto).ToList();
        //    foreach (var project in projects)
        //    {
        //        var fileName = PathManager.GetItemFile(id, project);
        //        if (File.Exists(fileName))
        //        {
        //            var json = File.ReadAllText(fileName);
        //            ItemDto item = JsonConvert.DeserializeObject<ItemDto>(json);
        //            return (item, null, project);
        //        }

        //        var objectives = GetObjectives(project);
        //        foreach (var objective in objectives)
        //        {
        //            fileName = PathManager.GetItemFile(id, project, objective);
        //            if (File.Exists(fileName))
        //            {
        //                var json = File.ReadAllText(fileName);
        //                ItemDto item = JsonConvert.DeserializeObject<ItemDto>(json);
        //                return (item, objective, project);
        //            }
        //        }

        //    }
        //    return (null, null, null);
        //}
        #endregion


    }
}