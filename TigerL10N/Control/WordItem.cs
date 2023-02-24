using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace TigerL10N.Control
{
    public class WordItem : BindableBase
    {

        private string? _fileName;
        public string FileName
        {
            get => _fileName ??= "";
            set => SetProperty(ref _fileName, value);
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
