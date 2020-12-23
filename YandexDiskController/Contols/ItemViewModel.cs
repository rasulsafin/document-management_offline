using DocumentManagement.Base;
using DocumentManagement.Connection.YandexDisk;
using DocumentManagement.Models;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManagement.Contols
{
    public class ItemViewModel : BaseViewModel
    {
        private static readonly string DIR_NAME = "data";
        private static readonly string TEMP_DIR = "Temp.Yandex";
        private static readonly string OBJECTIVE_FILE = "objective.json";
        private static readonly string PROJECT_FILE = "projects.json";
        private static readonly string ITEM_FILE = "items.json";
        YandexDisk yandex;
        private ProjectModel selectedProject;
        private ObjectiveModel selectedObjective;

        public ItemViewModel()
        {


            Initilization();
        }

        private async void Initilization()
        {
            await Task.Run(() =>
            {
                var fileName = Path.Combine(DIR_NAME, PROJECT_FILE);
                if (File.Exists(fileName))
                {
                    var json = File.ReadAllText(fileName);
                    List<ProjectDto> collection = JsonConvert.DeserializeObject<List<ProjectDto>>(json);

                    Projects.Clear();
                    foreach (ProjectDto item in collection)
                    {
                        Projects.Add((ProjectModel)item);
                    }
                    SelectedProject = Projects.First();

                }
            });

            await Task.Run(() =>
            {
                UpdateObjecteve();
            });
        }

        private void UpdateObjecteve()
        {
            if (SelectedProject != null)
            {
                string projDir = Path.Combine(DIR_NAME, SelectedProject.Title);
                if (!Directory.Exists(projDir)) Directory.CreateDirectory(projDir);
                string fileName = Path.Combine(projDir, OBJECTIVE_FILE);
                    Objectives.Clear();
                if (File.Exists(fileName))
                {
                    var json = File.ReadAllText(fileName);
                    List<ObjectiveDto> collection = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);

                    foreach (ObjectiveDto item in collection)
                    {
                        Objectives.Add((ObjectiveModel)item);
                    }
                    SelectedObjective = Objectives.First();
                }
            }
        }

        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = new ObservableCollection<ObjectiveModel>();
        public ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();
        public ProjectModel SelectedProject { get => selectedProject; 
            set { selectedProject = value; UpdateObjecteve(); OnPropertyChanged(); } }
        public ObjectiveModel SelectedObjective { get => selectedObjective; set { selectedObjective = value; OnPropertyChanged(); } }

    }
}