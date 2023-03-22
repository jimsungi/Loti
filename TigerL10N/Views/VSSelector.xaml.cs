using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Setup.Configuration;
namespace TigerL10N.Views
{
    /// <summary>
    /// VSSelector.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VSSelector : Window
    {
        public VSSelector()
        {
            InitializeComponent();
            LoadVisualStudioInstances();
        }

        private void LoadVisualStudioInstances()
        {
            List<VisualStudioInstance> instance = new List<VisualStudioInstance>();
            instance = VisualStudioInstanceHelper.GetVisualStudioInstances();
            VisualStudioInstancesComboBox.ItemsSource = instance;
        }

        public string SelectedPath
        {
            get;set;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VisualStudioInstance ins = VisualStudioInstancesComboBox.SelectedItem as VisualStudioInstance;
            if (ins != null)
            {
                SelectedPath = ins.InstallationPath;
            }
            this.Close();
        }
    }


    public class VisualStudioInstance
    {

        private string? _instanceId;
        public string InstanceId
        {
            get => _instanceId ??= "";
            set => _instanceId = value;
        }


        private string? _displayName;
        public string DisplayName
        {
            get => _displayName ??= "";
            set => _displayName= value;
        }


        private string? _installationVersion;
        public string InstallationVersion
        {
            get => _installationVersion ??= "";
            set => _installationVersion = value;
        }


        private string? _installationPath;
        public string InstallationPath
        {
            get => _installationPath ??= "";
            set => _installationPath = value;
        }
    }

    public static class VisualStudioInstanceHelper
    {
        public static List<VisualStudioInstance> GetVisualStudioInstances()
        {
            var visualStudioInstances = new List<VisualStudioInstance>();

            try
            {
                var setupConfiguration = new SetupConfiguration();
                var setupInstances = setupConfiguration.EnumInstances();

                int fetched;
                var instances = new ISetupInstance[1];

                while (true)
                {
                    setupInstances.Next(1, instances, out fetched);
                    if (fetched <= 0) break;

                    var instance = instances[0];

                    visualStudioInstances.Add(new VisualStudioInstance
                    {
                        InstanceId = instance.GetInstanceId(),
                        DisplayName = instance.GetDisplayName(),
                        InstallationVersion = instance.GetInstallationVersion(),
                        InstallationPath = instance.GetInstallationPath()
                    });
                }
            }
            catch (COMException) { }
            catch { }

            return visualStudioInstances;
        }
    }
}
