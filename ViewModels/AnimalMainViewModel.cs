﻿using DierenTuinWPF.Models;
using DierenTuinWPF.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Threading;

namespace DierenTuinWPF.ViewModels
{
    public class AnimalMainViewModel : BaseViewModel
    {
        #region private properties
        private ObservableCollection<Animal> _AllAnimals;
        private AnimalTypes[] _AnimalTypesArr;
        private AnimalTypes _SelctFeedType;
        private AnimalTypes _SelctAddType;
        private DispatcherTimer dispatcherTimer;
        private int maxAnimals = 20;
        private ObservableCollection<string> _Messages;

        private bool IsAscendSortT;
        private bool IsAscendSortG;
        #endregion

        #region public properties
        public ObservableCollection<Animal> AllAnimals
        {
            get { return _AllAnimals; }
            set
            {
                _AllAnimals = value;
                OnPropertyChanged();
            }
        }
        public AnimalTypes[] AnimalTypesArr
        {
            get { return _AnimalTypesArr; }
            set
            {
                _AnimalTypesArr = value;
                OnPropertyChanged();
            }
        }
        public AnimalTypes SelctFeedType
        {
            get { return _SelctFeedType; }
            set
            {
                _SelctFeedType = value;
                OnPropertyChanged();
            }
        }
        public AnimalTypes SelctAddType
        {
            get { return _SelctAddType; }
            set
            {
                _SelctAddType = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> Messages
        {
            get { return _Messages; }
            set
            {
                _Messages = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public AnimalMainViewModel()
        {
            InitializeVariables();
            InitalizeTimer();
        }

        #region Methods
        #region Initalization & Helper methods
        private void InitializeVariables()
        {
            AllAnimals = new ObservableCollection<Animal>()
            {
                new Lion(),
                new Monkey(),
                new Elephant()
            };
            //Add eventHandlers
            foreach (Animal ani in AllAnimals)
                ani.IsStarving += Animal_IsStarving; 
            AllAnimals.CollectionChanged += AllAnimals_CollectionChanged;

            AnimalTypesArr = Enum.GetValues(typeof(AnimalTypes)).Cast<AnimalTypes>().ToArray();
            Messages = new ObservableCollection<string>();

            IsAscendSortG = true;
            IsAscendSortT = true;
        }
        private void AllAnimals_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Using CommandManager eventhandling
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            //Using RaiseCanExecuteChanged eventhandling
            //FeedAllCmd.RaiseCanExecuteChanged();
            //FeedGroupCmd.RaiseCanExecuteChanged();
        }

        private void InitalizeTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Timer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
            dispatcherTimer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach(Animal animal in AllAnimals.ToList())
            {
                animal.UseEnergy();
                if (HasAnimalStarved(animal))
                {
                    AllAnimals.Remove(animal);
                }
            }
        }
        private bool HasAnimalStarved(Animal animal)
        {
            if(animal != null)
            {
                if (animal.Energy <= 0)
                    return true;
            }
            return false;
        }
        #endregion
        #region MessageMethods
        public void Animal_IsStarving(object sender, EventArgs e)
        {
            try
            {
                Animal animal = sender as Animal;
                Messages.Add(new string(string.Format("{0} is starving.", animal.Name)));
            }
            catch (InvalidCastException exc)
            {
                Console.WriteLine("Invalid Cast Exception: {0}", exc.Message);
            }

        }
        #endregion

        #region ActionMethods
        public Animal GetAnimal(AnimalTypes animalType)
        {
            var ns = typeof(AnimalTypes).Namespace; //or your classes namespace if different
            var typeName = ns + "." + animalType.ToString();

            return (Animal)Activator.CreateInstance(Type.GetType(typeName));
        }

        public void AddAnimal()
        {
            //Instantiate animal, connect event handler, add animal to collection.
            var tempAnimal = GetAnimal(SelctAddType);
            tempAnimal.IsStarving += Animal_IsStarving;
            AllAnimals.Add(tempAnimal);
        }

        /// <summary>
        /// Sorts by group, ascending or descending.
        /// </summary>
        public void SortAnimalType()
        {
            AllAnimals.Sort(ani => ani.Type, IsAscendSortT);
            //Switch boolean to sort in opposite order next click.
            IsAscendSortT = !IsAscendSortT;
        }
        public void SortAnimalEnergy()
        {
            AllAnimals.Sort(ani => ani.RelativeEnergy, IsAscendSortG);
            IsAscendSortG = !IsAscendSortG;
        }
        public void FeedAll()
        {
            foreach(Animal animal in AllAnimals)
            {
                animal.Eat();
            }
        }
        public void FeedGroup()
        {
            var animalgroup = AllAnimals.Where(animal => animal.Type == SelctFeedType);
            foreach (Animal animal in animalgroup)
            {
                animal.Eat();
            }
        }
        #endregion
        #region CanExecute Methods
        public bool HasAnimals()
        {
            if (AllAnimals.Count != 0)
                return true;
            else
                return false;
        }
        public bool HasMultipleTypes()
        {
            if (HasAnimals())
            {
                if (AllAnimals.GroupBy(ani => ani.Type).Count() > 1)
                    return true;
            }
            return false;
        }
        public bool IsAbleToFeedGroup()
        {
            if (HasAnimals())
            {
                if (AllAnimals.Where(ani => ani.Type == SelctFeedType).Any())
                    return true;
            }
            return false;
        }
        public bool HasReachedMaxAnimals()
        {
            if (AllAnimals.Count == maxAnimals)
                return true;
            else
                return false;
        }
        #endregion
        #endregion

        #region RelayCommands
        private RelayCommand _FeedAllCmd; public RelayCommand FeedAllCmd
        {
            get
            {
                if (_FeedAllCmd == null) {
                    _FeedAllCmd = new RelayCommand(
                        p => FeedAll(),
                        p => HasAnimals());
                }
                return _FeedAllCmd;
            }
        }
        private RelayCommand _FeedGroupCmd; public RelayCommand FeedGroupCmd
        {
            get
            {
                if (_FeedGroupCmd == null) {
                    _FeedGroupCmd = new RelayCommand(
                        p => FeedGroup(),
                        p => IsAbleToFeedGroup());
                }
                return _FeedGroupCmd;
            }
        }
        private RelayCommand _AddAnimalCmd; public RelayCommand AddAnimalCmd
        {
            get
            {
                if (_AddAnimalCmd == null) {
                    _AddAnimalCmd = new RelayCommand(
                        p => AddAnimal(),
                        p => !HasReachedMaxAnimals());
                }
                return _AddAnimalCmd;
            }
        }
        private RelayCommand _SortAnimalTypeCmd; public RelayCommand SortAnimalTypeCmd
        {
            get
            {
                if (_SortAnimalTypeCmd == null)
                {
                    _SortAnimalTypeCmd = new RelayCommand(
                        p => SortAnimalType(),
                        p => HasMultipleTypes());
                }
                return _SortAnimalTypeCmd;
            }
        }
        private RelayCommand _SortAnimalEnergyCmd; public RelayCommand SortAnimalEnergyCmd
        {
            get
            {
                if (_SortAnimalEnergyCmd == null)
                {
                    _SortAnimalEnergyCmd = new RelayCommand(
                        p => SortAnimalEnergy(),
                        p => HasAnimals());
                }
                return _SortAnimalEnergyCmd;
            }
        }
        #endregion



    }
}
