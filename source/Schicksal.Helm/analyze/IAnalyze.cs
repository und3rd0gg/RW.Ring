using Notung;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Schicksal.Helm;
using Schicksal.Helm.Dialogs;

namespace Schicksal.Helm.analyze
{
    public interface IAnalyze
    {
        StatisticsParametersDialog BindDialog(DataTable table);
        RunBase GetProcessor(DataTable table);
        bool IsRun(RunBase processor);
        void SaveData();
        Form BindTheResultForm(RunBase processor, TableForm table_form);

    }
}
