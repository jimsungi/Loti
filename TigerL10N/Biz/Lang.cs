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
    public class Lang :BindableBase
    {

        private bool? _isSelected;
        public bool IsSelected
        {
            get => _isSelected ??= false;
            set => SetProperty(ref _isSelected, value);
        }


        private CultureInfo? _culture;
        public CultureInfo? Culture
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
                        r = new RegionInfo(Culture.LCID);
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

        public Lang(CultureInfo cultureInfo)
        {
            Culture = cultureInfo;
        }
    }
}
