using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TigerL10N.Biz;

namespace TigerL10N.Control
{
    
    public class GoItemNode : BindableBase//, INotifyPropertyChanged
    {
        public GoItemNode(string filePath)
        {
            IconKind = "Folder";
            Path = filePath;
            Name = System.IO.Path.GetFileName(Path);
            Children = new List<GoItemNode>();
        }

        #region Properties



        private object? _projectItem;
        // Object which indicate
        public object? ProjectItem
        {
            get => _projectItem;
            set => SetProperty(ref _projectItem, value);
        }

        public void SetProject(LProject project)
        {
            ProjectItem = project;
        }


        public string Name { get; private set; }
        public List<GoItemNode> Children { get; private set; }
        public bool IsInitiallySelected { get; private set; }

        bool? _isChecked = null;
        GoItemNode? _parent;

        #region IsChecked

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }


        private bool? _expand=false;
        public bool Expand
        {
            get => _expand ??= false;
            set => SetProperty(ref _expand, value);
        }



        private string? _path;
        public string Path
        {
            get => _path ??= "";
            set => SetProperty(ref _path, value);
        }


        private bool? _isFile;
        public bool IsFile
        {
            get => _isFile ??= false;
            set => SetProperty(ref _isFile, value);
        }


        private string? _iconKind;
        public string IconKind
        {
            get => _iconKind ??= "";
            set => SetProperty(ref _iconKind, value);
        }


        private string? _itemType;
        public string ItemType
        {
            get => _itemType ??= "";
            set => SetProperty(ref _itemType, value);
        }





        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked) return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue) 
                Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null) _parent.VerifyCheckedState();

            SetProperty(ref _isChecked, value);
            //NotifyPropertyChanged("IsChecked");
        }

        void VerifyCheckedState()
        {
            bool? state = null;

            for (int i = 0; i < Children.Count; ++i)
            {
                bool? current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }

            SetIsChecked(state, false, true);
        }

        #endregion

        #endregion

        void Initialize()
        {
            foreach (GoItemNode child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        public static List<GoItemNode> SetTree(string topLevelName)
        {
            List<GoItemNode> treeView = new List<GoItemNode>();
            GoItemNode tv = new GoItemNode(topLevelName);

            treeView.Add(tv);

            //Perform recursive method to build treeview 

            #region Test Data
            //Doing this below for this example, you should do it dynamically 
            //***************************************************
            GoItemNode tvChild4 = new GoItemNode("Child4");

            tv.Children.Add(new GoItemNode("Child1"));
            tv.Children.Add(new GoItemNode("Child2"));
            tv.Children.Add(new GoItemNode("Child3"));
            tv.Children.Add(tvChild4);
            tv.Children.Add(new GoItemNode("Child5"));

            GoItemNode grtGrdChild2 = (new GoItemNode("GrandChild4-2"));

            tvChild4.Children.Add(new GoItemNode("GrandChild4-1"));
            tvChild4.Children.Add(grtGrdChild2);
            tvChild4.Children.Add(new GoItemNode("GrandChild4-3"));

            grtGrdChild2.Children.Add(new GoItemNode("GreatGrandChild4-2-1"));
            //***************************************************
            #endregion

            tv.Initialize();

            return treeView;
        }

        public static List<string> GetTree()
        {
            List<string> selected = new List<string>();

            //select = recursive method to check each tree view item for selection (if required)

            return selected;

            //***********************************************************
            //From your window capture selected your treeview control like:   CheckFileTreeModel root = (CheckFileTreeModel)TreeViewControl.Items[0];
            //                                                                List<string> selected = new List<string>(CheckFileTreeModel.GetTree());
            //***********************************************************
        }

        #region INotifyPropertyChanged Members

        //void NotifyPropertyChanged(string info)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(info));
        //    }
        //}

        //public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}