using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;

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

        private string? _targetString;
        public string TargetString
        {
            get => _targetString ??= "";
            set => SetProperty(ref _targetString, value);
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
