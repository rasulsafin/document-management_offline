using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using Newtonsoft.Json;

namespace MRS.DocumentManagement
{
    public static class ObjectModel
    {
        private static ProjectModel selectedProject;
        private static ObjectiveModel selectedObjective;
        private static ItemModel selectedItem;
        private static List<ObjectiveDto> objectiveDtoList;

        static ObjectModel()
        {
            Objectives.CollectionChanged += Objectives_CollectionChanged;
        }

        public static ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();

        public static ObservableCollection<ObjectiveModel> Objectives { get; set; } = new ObservableCollection<ObjectiveModel>();

        public static ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();

        public static ObservableCollection<UserModel> Users { get; set; } = new ObservableCollection<UserModel>();

        public static Synchronizer Synchronizer { get; set; } = new Synchronizer();

        public static ProjectModel SelectedProject
        {
            get => selectedProject;
            set
            {
                selectedProject = value;
                if (selectedProject != null)
                {
                    UpdateObjectives(selectedProject.dto);
                    UpdateItems(selectedProject.dto);
                }
            }
        }

        public static ObjectiveModel SelectedObjective
        {
            get => selectedObjective;
            set
            {
                selectedObjective = value;
                if (selectedObjective != null)
                    UpdateItems(selectedProject.dto, selectedObjective.dto);
            }
        }

        public static ItemModel SelectedItem { get => selectedItem; set => selectedItem = value; }

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
                List<UserDto> usersList = JsonConvert.DeserializeObject<List<UserDto>>(json);
                users.AddRange(usersList);
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
                List<ProjectDto> projectsList = JsonConvert.DeserializeObject<List<ProjectDto>>(json);
                projects.AddRange(projectsList);
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
            if (project == null)
            {
                Objectives.Clear();
                return;
            }
            Objectives.CollectionChanged -= Objectives_CollectionChanged;
            // string objDir = PathManager.GetObjectivesDir(project);
            objectiveDtoList = GetObjectives(project);
            var buf = new List<ObjectiveModel>();
            Objectives.Clear();
            for (int i = 0; i < objectiveDtoList.Count; i++)
            {
                ObjectiveDto item = objectiveDtoList[i];
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

            Objectives.CollectionChanged += Objectives_CollectionChanged;

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

            string dirProj = PathManager.GetLocalProjectDir(project);
            if (!Directory.Exists(dirProj)) return result;
            try
            {
                string path = PathManager.GetObjectivesFile(project);
                var json = File.ReadAllText(path);
                List<ObjectiveDto> objectives = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);
                result.AddRange(objectives);
            }
            catch (FileNotFoundException)
            {
            }

            return result;
        }

        public static void SaveObjectives(ProjectDto project, List<ObjectiveDto> objectives = null)
        {
            string dirProj = PathManager.GetLocalProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);

            if (objectives == null)
                objectives = objectiveDtoList;

            objectiveDtoList.RemoveAll(x => !x.ID.IsValid);
            // objectives = Objectives.Select(x => x.dto).ToList();
            string filename = PathManager.GetObjectivesFile(project);
            var json = JsonConvert.SerializeObject(objectives);
            File.WriteAllText(filename, json);
        }

        #endregion
        #region Item
        public static void UpdateItems(ProjectDto project, ObjectiveDto objective = null)
        {
            List<ItemDto> collection = new List<ItemDto>();
            if (project != null)
            {
                if (objective == null)
                {
                    collection = GetItems(project);
                }
                else
                {
                    if (objective.Items == null)
                        objective.Items = collection;
                    else
                        collection = objective.Items.ToList();
                }
            }

            Items.Clear();
            if (collection != null)
            {
                foreach (ItemDto item in collection)
                {
                    Items.Add((ItemModel)item);
                }
            }
        }

        public static List<ItemDto> GetItems(ProjectDto project)
        {
            if (project.Items == null) project.Items = new List<ItemDto>();
            return project.Items?.ToList();
        }

        public static List<ItemDto> GetItems(ID<ProjectDto> id)
        {
            var list = GetProjects();
            var proj = list.Find(x => x.ID == id);
            if (proj != null)
                return GetItems(proj);
            return null;
        }

        public static List<ItemDto> GetItems(ProjectDto project, ID<ObjectiveDto> objectiveID)
        {
            if (SelectedProject.ID == (int)project.ID || SelectedObjective.ID == (int)objectiveID)
                return SelectedObjective.dto.Items.ToList();
            ObjectiveDto objective = GetObjectives(project).Find(x => x.ID == objectiveID);
            if (objective == null || objective.Items == null) return new List<ItemDto>();
            return objective.Items.ToList();
        }

        public static void SaveItems(ProjectDto project)
        {
            List<ItemDto> items = Items.Select(x => x.dto).ToList();
            SaveItems(project, items);
        }

        public static void SaveItems(ProjectDto project, ObjectiveDto objective)
        {
            List<ItemDto> items = Items.Select(x => x.dto).ToList();
            SaveItems(project, objective, items);
        }

        public static void SaveItems(ProjectDto project, List<ItemDto> items)
        {
            // var dirProj = PathManager.GetLocalProjectDir(project);
            // if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);
            project.Items = items;
            var projects = GetProjects();
            var index = projects.FindIndex(x => x.ID == project.ID);
            if (index < 0)
                projects.Add(project);
            else
                projects[index] = project;
            SaveProjects(projects);
        }

        public static void SaveItems(ProjectDto project, ObjectiveDto objective, List<ItemDto> items)
        {
            var dirProj = PathManager.GetLocalProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);
            objective.Items = items;
            if (SelectedProject.ID == (int)project.ID && SelectedObjective.ID == (int)objective.ID)
            {
                SelectedObjective.dto = objective;
                SaveObjectives(project);
            }
            else
            {
                var objectiveList = GetObjectives(project);
                foreach (var objectiv in objectiveList)
                {
                    if (objectiv.ID == objective.ID)
                    {
                        objectiv.Items = items;
                        break;
                    }
                }

                SaveObjectives(project, objectiveList);
            }
        }

        private static void Objectives_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    if (item is ObjectiveModel model)
                    {
                        objectiveDtoList.Add(model.dto);
                    }

                    // else if (item is ObjectiveDto dto)
                    // {
                    //    objectiveDtoList.Add(dto);
                    // }
                }
            }
        }
        #endregion

    }
}
