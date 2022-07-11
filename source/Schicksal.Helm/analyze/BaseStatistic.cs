using Notung;
using Notung.Services;
using Schicksal.Basic;
using Schicksal.Helm.Dialogs;
using Schicksal.Helm.Properties;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Schicksal.Helm.analyze
{
    public class BaseStatistic : IAnalyze
    {

        public Dictionary<string, string[]> BindDialog(DataTable table, StatisticsParametersDialog dialog)
        {
            dialog.Text = Resources.BASIC_STATISTICS;
            return AppManager.Configurator.GetSection<Program.Preferences>().BaseStatSettings;
        }

        public RunBase GetProcessor(DataTable table, StatisticsParametersDialog dialog)
        {
            return new DescriptionStatisticsCalculator(table, dialog.DataSource.Predictors.ToArray(),
                    dialog.DataSource.Result, dialog.DataSource.Filter, dialog.DataSource.Probability);
        }
        public LaunchParameters GetLaunchParameters()
        {
            return new LaunchParameters
            {
                Caption = Resources.BASIC_STATISTICS,
                Bitmap = Resources.column_chart
            };
        }

        public void SaveData(StatisticsParametersDialog dialog)
        {
            dialog.DataSource.Save(AppManager.Configurator.GetSection<Program.Preferences>().BaseStatSettings);
        }

        public void BindTheResultForm(RunBase processor, object table_form, StatisticsParametersDialog dialog)
        {
            var results_form = new BasicStatisticsForm();
            var currentProcessor = (DescriptionStatisticsCalculator)processor;
            results_form.Text = string.Format("{0}: {1}, p={2}; {3}",
                Resources.BASIC_STATISTICS, table_form.GetType().GetProperty("Text").GetValue(table_form, null),
                dialog.DataSource.Probability, dialog.DataSource.Filter);
            results_form.DataSorce = currentProcessor.Result;
            results_form.Factors = currentProcessor.Factors;
            results_form.ResultColumn = dialog.DataSource.Result;
            results_form.Show();
        }
    }
}
