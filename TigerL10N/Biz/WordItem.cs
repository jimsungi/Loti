using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Prism.Commands;
using Prism.Mvvm;
using TigerL10N.Service;

namespace TigerL10N.Biz
{
    [Serializable]
    public class WordItem : BindableBase
    {
        public WordItem ()
        {

        }
        public string TmpFile = "";

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
            set
            {
                SetProperty(ref _useAuto, value);
            }
        }

        private DelegateCommand? _asIdCmd = null;
        public DelegateCommand AsIdCmd =>
            _asIdCmd ??= new DelegateCommand(AsIdFunc);
        void AsIdFunc()
        {
            AsId = !AsId;
            // throw new NotImplementException();
        }

        private bool? _asId;
        public bool AsId
        {
            get => _asId ??= false;
            set
            {
                SetProperty(ref _asId, value);
                if (_asId == true)
                {
                    FinalId = FinalId.Replace("L.", "G.");
                   
                }
                else
                {
                    FinalId = FinalId.Replace("G.", "L.");
                }
            }
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

            if (Ignore == true)
            {
                Ignore = false;
                FinalId = LocService.GetRecommandID(TargetString, true, false);
            }
            else
            {
                Ignore = true;
                FinalId = "";
            }
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
        private bool? _dirty = false;
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
        private string? _statusColor = "white";
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

        private Dictionary<string, string> TargetStrings = new Dictionary<string, string>();


        private string? _langCode;
        public string LangCode
        {
            get => _langCode ??= "";
            set
            {
                SetProperty(ref _langCode, value);
                if (_langCode == null)                
                    _langCode = "";
                
                else if (_langCode != null && TargetStrings.Keys.Contains(_langCode))
                {
                    TargetString = TargetStrings[_langCode];
                }
                else
                {
                    TargetStrings.Add(_langCode, TargetStrings[""]);
                }
            }
        }

        private string? _targetString;
        public string TargetString
        {
            get
            {
                if(_langCode != null)
                    return TargetStrings[_langCode];
                return "";
            }
            set
            {
                string set_value = value;
                if (_langCode == null)
                    _langCode = "";
                if (!TargetStrings.Keys.Contains(_langCode))
                {
                    TargetStrings.Add(_langCode, set_value);
                }
                else
                {
                    TargetStrings[_langCode] = set_value;
                }
                SetProperty(ref _targetString, set_value);
                if (_targetString == set_value)
                {
                    // accept current value
                    Dirty = true;
                }
                else if (!string.IsNullOrWhiteSpace(_targetString))
                {
                    if (UseAuto)
                        UseAuto = false;
                    if (AsId)
                        AsId = false;
                    if (Ignore)
                        Ignore = false;
                    Dirty = true;
                }
                //int dupCount = 0;
                if (!string.IsNullOrEmpty(FinalId))
                {
                    RefreshAllDupIds(FinalId, true);
                }
                //DupIdCount=dupCount;
            }
        }



        private int? _dupIdCount = 0;
        public int? DupIdCount
        {
            get => _dupIdCount;
            set
            {
                SetProperty(ref _dupIdCount, value);
                string _dupColor = "white";
                if (_dupIdCount > 1)
                    _dupColor = "red";
                DupState = _dupColor;
            }
        }


        private string? _dupState = "white";
        public string DupState
        {
            get => _dupState ??= "";
            set => SetProperty(ref _dupState, value);
        }



        private string? _findalId;
        public string FinalId
        {
            get => _findalId ??= "";
            set
            {
                SetProperty(ref _findalId, value);
                //int dupCount = 0;
                if (!string.IsNullOrEmpty(_findalId))
                {
                    RefreshAllDupIds(_findalId, true);
                }
                //DupIdCount = dupCount;
            }
        }
        [XmlIgnore]
        public List<WordItem>? RefAll = null;

        public void RefreshAllDupIds(string id, bool isString)
        {
            //int ret = 0;
            if (RefAll != null)
            {
                List<string> matchingTargetString = new List<string>();
                foreach (WordItem eachWord in RefAll)
                {
                    if (eachWord.FinalId == id && eachWord.Dirty)
                    {
                        if (!matchingTargetString.Contains(eachWord.SourceString))
                        {
                            matchingTargetString.Add(eachWord.SourceString);
                        }
                    }
                }
                int dupCount = matchingTargetString.Count();
                foreach (WordItem eachWord in RefAll)
                {
                    if (eachWord.FinalId == id && eachWord.Dirty)
                    {
                        eachWord.DupIdCount = dupCount;
                    }
                }
            }
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
            get { return !MultiLine; }
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
