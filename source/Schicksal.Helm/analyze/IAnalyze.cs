using Notung;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Schicksal.Helm;
using Schicksal.Helm.Dialogs;
using Notung.Services;

namespace Schicksal.Helm.analyze
{
    public interface IAnalyze
    {
        string GetText();
        Dictionary<string, string[]> GetSettings();
        RunBase GetProcessor(DataTable table, AnovaDialogData data);
        LaunchParameters GetLaunchParameters();
        void BindTheResultForm(RunBase processor, object table_form, AnovaDialogData data);
    }
}