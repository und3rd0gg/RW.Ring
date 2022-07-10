using Notung;
using Notung.Services;
using Schicksal.Basic;
using Schicksal.Helm.Dialogs;
using Schicksal.Helm.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Schicksal.Helm.analyze
{
    public class BaseStatistic : IAnalyze
    {
        public StatisticsParametersDialog Dialog { get; private set; }

        public BaseStatistic()
        {
            Dialog = new StatisticsParametersDialog();
        }

        public StatisticsParametersDialog BindDialog(DataTable table)
        {
            Dialog.Text = Resources.BASIC_STATISTICS;
            Dialog.DataSource = new AnovaDialogData(table, AppManager.Configurator.GetSection<Program.Preferences>().BaseStatSettings);
            return Dialog;
        }

        public RunBase GetProcessor(DataTable table)
        {
            return new DescriptionStatisticsCalculator(table, Dialog.DataSource.Predictors.ToArray(),
                    Dialog.DataSource.Result, Dialog.DataSource.Filter, Dialog.DataSource.Probability);
        }
        public bool IsRun(RunBase processor)
        {
            return AppManager.OperationLauncher.Run(processor,
                    new LaunchParameters
                    {
                        Caption = Resources.BASIC_STATISTICS,
                        Bitmap = Resources.column_chart
                    }) == TaskStatus.RanToCompletion;
        }

        public void SaveData()
        {
            Dialog.DataSource.Save(AppManager.Configurator.GetSection<Program.Preferences>().BaseStatSettings);
        }

        public Form BindTheResultForm(RunBase processor, TableForm table_form)
        {
            var results_form = new BasicStatisticsForm();
            var currentProcessor = (DescriptionStatisticsCalculator)processor;
            results_form.Text = string.Format("{0}: {1}, p={2}; {3}",
                Resources.BASIC_STATISTICS, table_form.Text, Dialog.DataSource.Probability, Dialog.DataSource.Filter);
            results_form.DataSorce = currentProcessor.Result;
            results_form.Factors = currentProcessor.Factors;
            results_form.ResultColumn = Dialog.DataSource.Result;
            return results_form;
        }

        

       
    }
}
