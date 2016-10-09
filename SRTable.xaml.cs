using System;
using System.Collections.Generic;
using System.Linq;
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
using Sr25Compare;
using System.Data;
using System.Windows.Threading;
using System.ComponentModel;

namespace Sr25Compare
{
    /// <summary>
    /// Interaction logic for SRTable.xaml
    /// </summary>
    public partial class SRTable : Window
    {
        private delegate void u4progbar(double zt);


        public Sr25DataSet foo { get; set; }
        public List<Sr25DataSet.FOOD_DESRow> _selected { get; set; }


        DataTable TheResTab = new DataTable("TheResultsTable");

        BackgroundWorker bgw = new BackgroundWorker();
       

        public SRTable()
        {
            InitializeComponent();
            //this.Closing += SRTable_Closing;
            this.Loaded += SRTable_Loaded;
            //bgw.WorkerReportsProgress = true;
            //bgw.DoWork += bgw_DoWork;
            //bgw.RunWorkerCompleted += bgw_RunWorkerCompleted;
            //bgw.ProgressChanged += bgw_ProgressChanged;
            
        }

        void SRTable_Closing(object sender, CancelEventArgs e)
        {
            SleepTillDone();
        }

        private void SleepTillDone()
        {
            if (bgw != null && bgw.IsBusy == true)
            {
                this.Visibility = System.Windows.Visibility.Hidden;
                for (; ; )
                {
                    if (bgw.IsBusy == true)
                    {
                        System.Threading.Thread.Sleep(2000);
                    }
                    else
                        break;
                }
            }
        }


        async void SRTable_Loaded(object sender, RoutedEventArgs e)
        {
             await ProcComp();
        }

        private async Task ProcComp()
        {
            if (foo == null || _selected == null || _selected.Count <= 0)
            {
                //MessageBox.Show("Required Data Not passed.");
                Close();
            }
            //Title = string.Format("Compare  {0}  Foods.", _selected.Count());
            MakeResTab();
            ShowProg();
            //bgw.RunWorkerAsync();
            await Compare();

            z_res.ItemsSource = TheResTab.DefaultView;

            this.HideProg();
        }

        /// <summary>
        /// Make The DataTable for The results Datagrid.
        /// </summary>
        private void MakeResTab()
        {

            //TheResTab.Columns.Clear();
            if (foo.NUT_DEF.Rows.Count <= 0)
            {
                Sr25DataSetTableAdapters.ndataTableAdapter  ddda = new Sr25DataSetTableAdapters.ndataTableAdapter();
                ddda.Fill(foo.ndata);
                ddda.Dispose();
            }
            DataColumn ndc = new DataColumn("Food List", typeof(string));
            ndc.Caption = "000";
            TheResTab.Columns.Add(ndc);// Food column Header.......
            foreach (Sr25DataSet.NUT_DEFRow nd in foo.NUT_DEF)
            {
                //if (nd.IsSelected != null && )
                //{
                    //colname = string.Format("{0}-({1}).",nd.NutrDesc,nd.Units);
                    ndc = new DataColumn(nd.Nutr_No.ToString(), typeof(float));
                    ndc.Caption = nd.Nutr_No.ToString();// save the nutr_no key!!
                    TheResTab.Columns.Add(ndc);
                //}
            }

           

        }

        private void z_res_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;
            
            if (e.Column.Header.ToString().StartsWith("Food") == true)
            {
                e.Column.HeaderStyle = (Style)FindResource("Column1HeaderStyle");
                DataGridTextColumn dgc = e.Column as DataGridTextColumn;

                //dgc.CellStyle = (Style)FindResource("Col0");
                dgc.ElementStyle = (Style)FindResource("Col0");
            }
            else
            {
                if (IsEmpty(e) == true)
                {
                    e.Column.Visibility = System.Windows.Visibility.Collapsed;
                }
                e.Column.HeaderStyle = (Style)FindResource("NutrientColumns");

                short nutno = short.Parse(e.Column.Header.ToString());
                string colname = null;
                int dec = 0;
                string strfmt = null;

                foreach (Sr25DataSet.NUT_DEFRow df in foo.NUT_DEF)
                {
                    if (df.Nutr_No == nutno )
                    {
                        colname = string.Format("{0}-({1})", df.NutrDesc, df.Units);
                        dec = int.Parse(df.Decimal);
                        break;
                    }
                }

                e.Column.Header = colname;
                DataGridTextColumn dgc = e.Column as DataGridTextColumn;

                //dgc.CellStyle = (Style)FindResource("ValueCells");
                dgc.ElementStyle = (Style)FindResource("ValueCells");
                strfmt = Helper.StrFormat(dec);
                dgc.Binding.StringFormat = strfmt;
            }
        }
        /// <summary>
        /// if the colum is all zeros return true
        /// else
        /// return false.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsEmpty(DataGridAutoGeneratingColumnEventArgs e)
        {
            foreach(DataRow dr in this.TheResTab.Rows)
            {
                float c0 = float.Parse(dr[e.Column.Header.ToString()].ToString());
                if( c0 > (float)0)
                    return false;
            }
            return true;
        }


        private async Task Compare()
        {

            if (_selected.Count <= 0)
                return;
            
            // the progress steps from the iterations number.
            double pv = (double)((double)200.0 / (double)_selected.Count);

            for (int i = 0; i < _selected.Count; i++)
            {
                Sr25DataSet.FOOD_DESRow lbi = _selected[i];
                AddFoodRow(lbi);
                //bgw.ReportProgress((int)pv);
            }
            await Task.Delay(100);

        }

        private void AddFoodRow(Sr25DataSet.FOOD_DESRow fr)
        {
            if (foo.ndata.Rows.Count == 0)
            {
                //Title = string.Format("lazy loader busy .. Loading data before  .. {0}", fr.NDB_No);
                Sr25DataSetTableAdapters.ndataTableAdapter ddda = new Sr25DataSetTableAdapters.ndataTableAdapter();
                ddda.Fill(foo.ndata);
                ddda.Dispose();
            }

            var nuts4foo = foo.ndata.Where(f => f.NDB_No == fr.NDB_No).ToDictionary(nno => nno.Nutr_No);


            DataRow dr = TheResTab.NewRow();

            float v = 0F;
            dr[0] = fr.Desc;

           
            for (int i = 1; i < TheResTab.Columns.Count; i++)
            {
                short _nut_no = short.Parse(TheResTab.Columns[i].Caption);

                //Title = DateTime.Now.ToLongTimeString();

                try
                {
                    var _nut_val = nuts4foo[_nut_no];
                    dr[i] = _nut_val.Nutr_Val;
                }
                catch(KeyNotFoundException eee)
                {
                        dr[i] = v;
                }

            }

            TheResTab.Rows.Add(dr);

        }
       

        private void HideProg()
        {
            z_prog.Visibility = System.Windows.Visibility.Hidden;
            z_prog.IsIndeterminate = true;
        }
        /// <summary>
        /// shows progress bar and hide
        /// results tab and compare button.
        /// </summary>
        private void ShowProg()
        {
            z_prog.IsIndeterminate = false;
            z_prog.Visibility = System.Windows.Visibility.Visible;
            

        }

     


    }
}
