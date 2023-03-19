using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerL10N.Biz
{
    [Serializable]
    public class LCulture : BindableBase
    {
        private CultureInfo? c = null;
        public LCulture()
        {

        }
        public LCulture(string name)
        {
            try
            {
                c = new CultureInfo(name);
                _nativeName = c.NativeName;
                _isNeutralCulture = c.IsNeutralCulture;
                _lCID = c.LCID;
                _twoLetterISOLanguageName = c.TwoLetterISOLanguageName;
                _englishName = c.EnglishName;
            }
            catch { }
        }


        private string? _englishName;
        public string EnglishName
        {
            get => _englishName ??= "";
            set => SetProperty(ref _englishName, value);
        }



        private string? _twoLetterISOLanguageName;
        public string TwoLetterISOLanguageName
        {
            get => _twoLetterISOLanguageName ??= "";
            set => SetProperty(ref _twoLetterISOLanguageName, value);
        }


        private bool? _isNeutralCulture;
        public bool IsNeutralCulture
        {
            get => _isNeutralCulture ??= false;
            set => SetProperty(ref _isNeutralCulture, value);
        }



        private int? _lCID;
        public int? LCID
        {
            get => _lCID;
            set => SetProperty(ref _lCID, value);
        }



        private string? _name;
        public string Name
        {
            get => _name ??= "";
            set => SetProperty(ref _name, value);
        }


        private string? _nativeName;
        public string NativeName
        {
            get => _nativeName ??= "";
            set => SetProperty(ref _nativeName, value);
        }


    }

    [Serializable]
    public class Lang :BindableBase
    {
        public Lang()
        {

        }

        public Lang(string langCode)
        {
            Culture = new LCulture(langCode);
        }


        private bool? _isSelected;
        public bool IsSelected
        {
            get => _isSelected ??= false;
            set => SetProperty(ref _isSelected, value);
        }



        private LCulture? _culture;
        public LCulture? Culture
        {
            get => _culture;
            set => SetProperty(ref _culture, value);
        }


        //private string? _displayName;
        public string DisplayName
        {
            get {
                if (Culture == null)
                    return CultureInfo.CurrentCulture.DisplayName;
                return Culture.NativeName;
            }
            //set => SetProperty(ref _displayName, value);
        }

        public string Code
        {
            get
            {
                if (Culture == null)
                    return "";
                RegionInfo? r = null;
                string countryCode = string.Empty;

                if (Culture.IsNeutralCulture)
                {

                }
                else if(Culture.LCID == 127) // Invariant[
                    { }
                else
                {
                    try
                    {
                        int? lcid = Culture.LCID;
                        if (lcid.HasValue)
                        {
                            r = new RegionInfo(lcid.Value);
                        }
                    }
                    catch //(Exception ex)
                    {
                        //int m = 0;
                    }
                }
                
                if (r!=null)
                {
                    countryCode = r.Name;
                }
                string languageCode = Culture.TwoLetterISOLanguageName;

                return string.Format("{0}-{1}", languageCode, countryCode);
            }
        }
        public string EngName
        {
            get
            {
                if (Culture == null)
                    return CultureInfo.CurrentCulture.DisplayName;
                return Culture.EnglishName;
            }
        }

        public string NativeName
        {
            get
            {
                if (Culture == null)
                    return CultureInfo.CurrentCulture.DisplayName;
                return Culture.NativeName;
            }
        }

        public Lang(LCulture cultureInfo)
        {
            Culture = cultureInfo;
        }
    }
}
