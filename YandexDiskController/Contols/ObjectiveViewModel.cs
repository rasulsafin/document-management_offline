using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class ObjectiveViewModel : BaseViewModel
    {
        #region Data and bending
        
        DiskManager yandex;
        ProjectModel selectedProject;
        private ObjectiveModel selectedObjective;
        private ObjectiveModel editObjective = new ObjectiveModel();
        bool isLocalDB = true;
        private string statusOperation;
        private bool progressVisible;
        private double progressMax;
        private double progressValue;

        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = ObjectModel.Objectives;
        public ObservableCollection<ProjectModel> Projects { get; set; } = ObjectModel.Projects;
        public ProjectModel SelectedProject
        {
            get => selectedProject;
            set
            {
                selectedProject = value;
                UpdateObjectives();
                OnPropertyChanged();
            }
        }
        public ObjectiveModel SelectedObjective
        {
            get => selectedObjective;
            set
            {
                selectedObjective = value;
                EditObjective = selectedObjective;
                OnPropertyChanged();
            }
        }
        public ObjectiveModel EditObjective { get => editObjective; set { editObjective = value; OnPropertyChanged(); } }
        public bool IsLocalDB { get => isLocalDB; set { isLocalDB = value; OnPropertyChanged(); } }
        public bool ProgressVisible { get => progressVisible; set { progressVisible = value; OnPropertyChanged(); } }
        public double ProgressValue { get => progressValue; set { progressValue = value; OnPropertyChanged(); } }
        public double ProgressMax { get => progressMax; set { progressMax = value; OnPropertyChanged(); } }
        public string StatusOperation { get => statusOperation; set { statusOperation = value; OnPropertyChanged(); } }

        public int NextId
        {
            get => Properties.Settings.Default.ObjectiveNextId;
            set
            {
                Properties.Settings.Default.ObjectiveNextId = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public HCommand UpdateProjectsCommand { get; }
        public HCommand<bool> LocalDBCommand { get; }
        public HCommand LoadProjectOfflineCommand { get; }
        public HCommand ZeroIdCommand { get; }
        public HCommand AddObjectiveOfflineCommand { get; }
        public HCommand LoadObjectiveOfflineCommand { get; }
        public HCommand SaveObjectiveOfflineCommand { get; }
        public HCommand ChengeStatusOfflineCommand { get; }
        public HCommand DeleteObjectiveOfflineCommand { get; }
        public HCommand OpenFileOfflineCommand { get; }
        public HCommand UploadObjectiveCommand { get; }
        public HCommand DownloadObjectiveCommand { get; }
        public HCommand GetIDCommand { get; }
        public HCommand UpdateObjectiveOfflineCommand { get; }
        public HCommand GetObjectiveForIdCommand { get; }
        public HCommand<int> AddObjectivesCommand { get; }
        #endregion

        public ObjectiveViewModel()
        {
            ZeroIdCommand = new HCommand(ZeroId);
            AddObjectiveOfflineCommand = new HCommand(CreateObjective);

            UpdateProjectsCommand = new HCommand(UpdateProjects);
            DeleteObjectiveOfflineCommand = new HCommand(DeleteObjective);
            LoadObjectiveOfflineCommand = new HCommand(UpdateObjectives);
            //SaveObjectiveOfflineCommand = new HCommand(SaveObjectiveOffline);
            ChengeStatusOfflineCommand = new HCommand(ChengeStatusOffline);
            UpdateObjectiveOfflineCommand = new HCommand(UpdateObjectiveOffline);

            GetObjectiveForIdCommand = new HCommand(GetObjectiveForID);

            UploadObjectiveCommand = new HCommand(UploadToServerAsync);
            DownloadObjectiveCommand = new HCommand(DownloadFromServerAsync);
            GetIDCommand = new HCommand(GetIDAsync);
            AddObjectivesCommand = new HCommand<int>(AddObjectives);


            UpdateProjects();
            //UpdateObjectives();
        }

        private void ZeroId()
        {
            NextId = 1;
        }

        private void CreateObjective()
        {
            ObjectiveModel model = new ObjectiveModel();
            model.ID = NextId++;
            model.ProjectID = SelectedProject.ID;
            model.CreationDate = DateTime.Now;
            model.DueDate = DateTime.Now;
            model.Title = EditObjective.Title;
            model.Description = EditObjective.Description;
            model.Status = ObjectiveStatus.Undefined;
            //obj.TaskType = new ObjectiveTypeDto();            

            Objectives.Add(model);
            //SaveObjective(SelectedProject.dto, model.dto);

            ObjectModel.SaveObjectives(SelectedProject.dto);
            ObjectModel.Synchronizer.Update(model.dto.ID, SelectedProject.dto.ID);
        }
        private void UpdateObjectiveOffline()
        {
            SelectedObjective.Title = EditObjective.Title;
            SelectedObjective.Description = EditObjective.Description;
            SelectedObjective.Status = EditObjective.Status;
            SelectedObjective.DueDate = DateTime.Now;

            ObjectModel.SaveObjectives(SelectedProject.dto);
            ObjectModel.Synchronizer.Update(SelectedObjective.dto.ID, SelectedProject.dto.ID);
        }
        private void ChengeStatusOffline()
        {
            if (EditObjective == null)
                return;

            string[] collect = Enum.GetNames<ObjectiveStatus>();
            var newVal = WinBox.SelectorBox(collect);

            var status = Enum.Parse<ObjectiveStatus>(newVal);

            EditObjective.Status = status;
            SelectedObjective.Status = status;
            ObjectModel.SaveObjectives(SelectedProject.dto);
            ObjectModel.Synchronizer.Update(SelectedObjective.dto.ID, SelectedProject.dto.ID);
        }
        private void UpdateObjectives()
        {
            if (SelectedProject != null)
            {
                ObjectModel.UpdateObjectives(SelectedProject.dto);
            }
        }
        private void DeleteObjective()
        {
            if (SelectedObjective != null)
            {
                if (WinBox.ShowQuestion($"Удалить задание {SelectedObjective.Title}?", "Удаление"))
                {
                    ObjectModel.Synchronizer.Update(SelectedObjective.dto.ID, SelectedProject.dto.ID);
                    Objectives.Remove(SelectedObjective);
                    ObjectModel.SaveObjectives(SelectedProject.dto);
                }
            }
        }
        private void UpdateProjects()
        {
            ObjectModel.UpdateProjects();
        }
        private void AddObjectives(int obj)
        {
            string[] names;
            string[] discriptions;

            string namesFile = "Names.txt";
            if (!File.Exists(namesFile))
            {
                if (WinBox.ShowQuestion("Файл с именами не существует создать его?"))
                {
                    OpenHelper.Geany(namesFile);
                }
                return;
            }
            else
            {
                names = File.ReadAllLines(namesFile);
            }

            string discriptionsFile = "Discriptions.txt";
            if (!File.Exists(discriptionsFile))
            {
                if (WinBox.ShowQuestion("Файл с описаниями не существует создать его?"))
                {
                    OpenHelper.Geany(discriptionsFile);
                }
                return;
            }
            else
            {
                discriptions = File.ReadAllLines(discriptionsFile);
            }
            if (discriptions == null || names == null) return;

            Random random = new Random();

            for (int i = 0; i < obj; i++)
            {
                ObjectiveModel model = new ObjectiveModel();

                model.ID = NextId++;

                model.ProjectID = SelectedProject.ID;
                model.CreationDate = DateTime.Now;
                model.DueDate = DateTime.Now;

                int index = random.Next(0, names.Length);

                model.Title = names[index];

                index = random.Next(0, discriptions.Length);
                model.Description = discriptions[index];

                model.Status = (ObjectiveStatus)random.Next(0, 4);
                Objectives.Add(model);
                ObjectModel.Synchronizer.Update(model.dto.ID, SelectedProject.dto.ID);
            }
            ObjectModel.SaveObjectives(SelectedProject.dto);
            //SaveObjectiveOffline();
        }
        private void GetObjectiveForID()
        {
            WinBox.ShowMessage("Эта кнопка больше ничего не делает");
            //if (WinBox.ShowInput(
            //    title: "Открыть задачу",
            //    question: "Введи id",
            //    input: out string input,
            //    okText: "Открыть", cancelText: "Отмена",
            //    defautValue: SelectedObjective == null ? "" : SelectedObjective.ID.ToString()
            //    ))
            //{
            //    if (int.TryParse(input, out int id))
            //    {
            //        (ObjectiveDto objective, ProjectDto project) = ObjectModel.GetObjective((ID<ObjectiveDto>)id);
            //        string message = "ObjectiveDto:\n";
            //        if (objective != null)
            //        {
            //            message += $"ID={objective.ID}\n";
            //            message += $"ProjectID={objective.ProjectID}\n";
            //            message += $"Title={objective.Title}\n";
            //            message += $"Status={objective.Status}\n";
            //            message += $"Description={objective.Description}\n";
            //            message += $"CreationDate={objective.CreationDate}\n";
            //            message += $"DueDate={objective.DueDate}\n";
            //        }
            //        message += $"project:\n";
            //        if (project != null)
            //        {
            //            message += $"ID={project.ID}\n";
            //            message += $"Title={project.Title}\n";
            //        }
            //        WinBox.ShowMessage(message);
            //    }
            //}
        }
        private async void GetIDAsync()
        {
            //ChechYandex();
            //try
            //{
            //    List<ID<ObjectiveDto>> list = await yandex.GetObjectivesIdAsync(SelectedProject.dto);
            //    StringBuilder message = new StringBuilder();
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        if (i != 0) message.Append(',');
            //        if (i % 10 == 0) message.Append('\n');
            //        message.Append(list[i].ToString());
            //    }
            //    WinBox.ShowMessage(message.ToString());
            //}
            //catch (FileNotFoundException fileNot)
            //{
            //    WinBox.ShowMessage("Папка проекта отсутвует на диске!");
            //}
            //catch (Exception ex)
            //{
            //    WinBox.ShowMessage(ex.Message);
            //}
            WinBox.ShowMessage("Эта кнопка больше ничего не делает!");
        }

        private async void UploadToServerAsync()
        {
            WinBox.ShowMessage("Эта кнопка больше ничего не делает!");
            //ChechYandex();
            //if (WinBox.ShowQuestion($"Загрузить Objective '{SelectedObjective.Title}' на диск?"))
            //{
            //    //var objs = Objectives.Select(x => x.dto).ToArray();
            //    //    ProgressMax = objs.Length;
            //    //ProgressVisible = true;
            //    //ProgressValue =0;

            //    //foreach (var item in objs)
            //    //{
            //    await yandex.UploadObjectiveAsync(SelectedObjective.dto, SelectedProject.dto);
            //    //ProgressValue++;
            //    //}
            //    //ProgressVisible = false;
            //}
        }
        private async void DownloadFromServerAsync()
        {
            WinBox.ShowMessage("Эта кнопка больше ничего не делает!");
            //ChechYandex();
            //if (WinBox.ShowInput(
            //    title: "Скачивание...",
            //    question: "Введите id оbjective которую надо скачать с диска?",
            //    input: out string text,
            //    okText: "Скачать",
            //    cancelText: "Отмена"))
            //{
            //    if (int.TryParse(text, out int num))
            //    {
            //        ID<ObjectiveDto> id = (ID<ObjectiveDto>)num;
            //        ObjectiveDto objective = await yandex.GetObjectiveAsync(SelectedProject.dto, id);
            //        Objectives.Add((ObjectiveModel)objective);
            //    }

            //    //ProgressVisible = true;
            //    //ObjectiveDto[] collect = await yandex.GetObjectiveAsync(SelectedProject.dto);
            //    //ProgressVisible = false;
            //    //if (collect == null)
            //    //    WinBox.ShowMessage("Скачивание завершилось провалом!");
            //    //else
            //    //{
            //    //    Objectives.Clear();
            //    //    foreach (ObjectiveDto item in collect)
            //    //    {
            //    //        Objectives.Add(new ObjectiveModel(item));
            //    //    }
            //    //}
            //}
        }

        public static void DeleteObjective(ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }



        //public static List<ObjectiveDto> GetObjectives(ProjectDto project)
        //{
        //    var result = new List<ObjectiveDto>();

        //    string dirProj = PathManager.GetProjectDir(project);
        //    if (!Directory.Exists(dirProj)) return result;

        //    string dirObj = PathManager.GetObjectivesDir(project);
        //    if (!Directory.Exists(dirObj)) return result;

        //    DirectoryInfo dirInfoObj = new DirectoryInfo(dirObj);

        //    foreach (var item in dirInfoObj.GetFiles())
        //    {
        //        if (item.Name.StartsWith("objective"))
        //        {
        //            var json = File.ReadAllText(item.FullName);
        //            ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
        //            result.Add(objective);
        //        }
        //    }
        //    return result;
        //}

    }
}