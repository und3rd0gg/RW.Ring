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
        Dictionary<string, string[]> BindDialog(DataTable table, StatisticsParametersDialog dialog);
        RunBase GetProcessor(DataTable table, StatisticsParametersDialog dialog);
        LaunchParameters GetLaunchParameters();
        void SaveData(StatisticsParametersDialog dialog);
        void BindTheResultForm(RunBase processor, object table_form, StatisticsParametersDialog dialog);
    }
}
