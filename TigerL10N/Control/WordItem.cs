using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using TigerL10N.Service;

namespace TigerL10N.Control
{
    public class WordItem : BindableBase
    {

        private bool? _ignore;
        public bool Ignore
        {
            get => _ignore ??= false;
            set => SetProperty(ref _ignore, value);
        }


        private bool? _useAuto;
        public bool UseAuto
        {
            get => _useAuto ??= false;
            set => SetProperty(ref _useAuto, value);
        }

        private DelegateCommand? _asIdCmd = null;
        public DelegateCommand AsIdCmd =>
            _asIdCmd ??= new DelegateCommand(AsIdFunc);
        void AsIdFunc()
        {
            AsId = ! AsId;
            // throw new NotImplementException();
        }

        private bool? _asId;
        public bool AsId
        {
            get => _asId ??= false;
            set => SetProperty(ref _asId, value);
        }


        private string? _shortFileName;
        public string ShortFileName
        {
            get => _shortFileName ??= "";
            set => SetProperty(ref _shortFileName, value);
        }

        private string? _prevRef;
        public string PrevRef
        {
            get => _prevRef ??= "";
            set => SetProperty(ref _prevRef, value);
        }


        private string? _currentRef;
        public string CurrentRef
        {
            get => _currentRef ??= "";
            set => SetProperty(ref _currentRef, value);
        }

        private string? _nextRef;
        public string NextRef
        {
            get => _nextRef ??= "";
            set => SetProperty(ref _nextRef, value);
        }


        private DelegateCommand<string>? _specialKeyCmd = null;
        public DelegateCommand<string> SpecialKeyCmd =>
            _specialKeyCmd ??= new DelegateCommand<string>(SpecialKeyFunc);
        void SpecialKeyFunc(string param)
        {
            // throw new NotImplementException();
        }


        private DelegateCommand? _useAutoCmd = null;
        public DelegateCommand UseAutoCmd =>
            _useAutoCmd ??= new DelegateCommand(UseAutoFunc);
        void UseAutoFunc()
        {
            UseAuto = !UseAuto;
        }



        private DelegateCommand? _ignoreCmd = null;
        public DelegateCommand IgnoreCmd =>
            _ignoreCmd ??= new DelegateCommand(IgnoreFunc);
        void IgnoreFunc()
        {
            Ignore = !Ignore;
            // throw new NotImplementException();
        }
        private DelegateCommand? _useTranslationCmd = null;
        public DelegateCommand UseTranslationCmd =>
            _useTranslationCmd ??= new DelegateCommand(UseTranslationFunc);
        void UseTranslationFunc()
        {
            UseAuto = !UseAuto;
        }
    
        /// <summary>
        /// There is any touch alreay, you can refer this value as edited 
        /// </summary>
        private bool? _dirty=false;
        public bool Dirty
        {
            get => _dirty ??= false;
            set
            {
                if (!init)
                    return;
                SetProperty(ref _dirty, value);
                StatusColor = "red";
            }
        }


        /// <summary>
        /// Show Editing Status, it support 
        /// init : white
        /// dirty : gray
        /// </summary>
        private string? _statusColor="white";
        public string StatusColor
        {
            get
            {
                bool dirty = Dirty;
                if (dirty)
                    return "lightgray";
                return "white";
            }
            set
            {
                string acolor = "white";
                if (Dirty)
                    acolor = "lightgray";
                SetProperty(ref _statusColor, acolor);
            }
        }

        public bool init = false;

        private string? _targetString;
        public string TargetString
        {
            get => _targetString ??= "";
            set
            {                
                SetProperty(ref _targetString, value);
                if(UseAuto && !string.IsNullOrWhiteSpace(TargetId))
                {
                    if (_targetString != null)
                    {
                        FindalId = LocService.GetRecommandID(SourceString, true);
                    }
                    Dirty = true;
                }
                if (!string.IsNullOrWhiteSpace(_targetString))
                {
                    if (UseAuto)
                        UseAuto = false;
                    if (AsId)
                        AsId = false;
                    if (Ignore)
                        Ignore = false;
                    Dirty = true;
                }
            }
        }


        private string? _findalId;
        public string FindalId
        {
            get => _findalId ??= "";
            set => SetProperty(ref _findalId, value);
        }

        private Dictionary<string, string> LanguageStringDict = new Dictionary<string, string>();


        private string? _targetLanguage;
        public string TargetLanguage
        {
            get => _targetLanguage ??= "";
            set => SetProperty(ref _targetLanguage, value);
        }



        public bool AcceptReturn
        {
            get  { return !MultiLine;}
        }


        public bool MultiLine
        {
            get { return TargetString.Contains("\r\n"); }
        }


        private string? _fileName;
        public string FileName
        {
            get => _fileName ??= "";
            set
            {
                SetProperty(ref _fileName, value);
                _shortFileName = Path.GetFileName(_fileName);
            }
        }


        private string? _sourceString;
        public string SourceString
        {
            get => _sourceString ??= "";
            set => SetProperty(ref _sourceString, value);
        }


        private string? _targetId;
        public string TargetId
        {
            get => _targetId ??= "";
            set => SetProperty(ref _targetId, value);
        }

    }
}
