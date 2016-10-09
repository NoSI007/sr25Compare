using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace Sr25Compare
{
    /// <summary>
    /// Interaction logic for PanelRes.xaml
    /// </summary>
    public partial class PanelRes : Window
    {

        public Sr25DataSet foo { get; set; }
        public List<Sr25DataSet.FOOD_DESRow> _selected { get; set; }

        BackgroundWorker bw = new BackgroundWorker();

        //DataTable TheResTab = new DataTable("TheResultsTable");
        DataTable PanRes = new DataTable("PanelResultsTable");

        List<Sr25DataSet.ndataRow> Tempres;


        public PanelRes()
        {
            InitializeComponent();
            MakeDataTable();
            this.Loaded += PanelRes_Loaded;

        }

        /// <summary>
        /// Make and load the datatable for required foods.
        /// </summary>
        private void MakeDataTable()
        {

            DataColumn c1 = new DataColumn("Values", typeof(string));
            DataColumn c2 = new DataColumn("Foods", typeof(string));
            PanRes.Columns.Add(c1);
            PanRes.Columns.Add(c2);
            

        }

        void PanelRes_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                if (_selected == null || _selected.Count <= 0)
                {
                    this.Close();
                }

                if (foo == null || _selected == null)
                {
                    MessageBox.Show("Required Program data error");
                    this.Close();
                }

                CheckDataSet();
                InitStart();
                ResGrid.ItemsSource = PanRes.DefaultView;
            }
            catch (System.Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void CheckDataSet()
        {
            if (foo == null
                || foo.NUT_DEF.Rows.Count == 0
                || foo.Fd_Grp.Rows.Count == 0
                || foo.FOOD_DES.Rows.Count == 0
                || foo.ndata.Rows.Count == 0)
            {
                throw new System.Exception("-- Data Error -- !!! ");
            }

        }

        private void InitStart()
        {
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;

            LeftPanel();
            bw.RunWorkerAsync();
        }

        private void LeftPanel()
        {
            ResList.DataContext = foo.NUT_DEF.ToList<Sr25DataSet.NUT_DEFRow>();

            ResListHead.Content = string.Format("( {0} ) Nutrients", foo.NUT_DEF.Rows.Count);
            ResList.Visibility = System.Windows.Visibility.Hidden;
            PageHead.Visibility = System.Windows.Visibility.Hidden;
        }

        #region background
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {

            var data = (from r in _selected
                        join d in foo.ndata on r.NDB_No equals d.NDB_No
                        select d).ToList<Sr25DataSet.ndataRow>();

            Tempres = data;
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            zprog.Visibility = System.Windows.Visibility.Hidden;
            ResList.Visibility = System.Windows.Visibility.Visible;
            PageHead.Visibility = System.Windows.Visibility.Visible;
        }

        #endregion background




        #region UI
        private void ResList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (foo.NUT_DEF.Rows.Count > 0)
            {
                try
                {
                    LoadRdg();
                }
                catch (System.Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
            }
        }

        private void LoadRdg()
        {
            
            Sr25DataSet.NUT_DEFRow nd = (Sr25DataSet.NUT_DEFRow)ResList.SelectedItem;
            if (nd == null)
                return;

            int decp = int.Parse(nd.Decimal);

            var tisqry = Tempres.Where(n => n.Nutr_No == nd.Nutr_No).ToList<Sr25DataSet.ndataRow>();


            string selection = string.Format("{0} Records found for {1}.",tisqry.Count(), nd.NutrDesc);
            ResGridHead.Content = selection;

            PanRes.Rows.Clear();
          
           
            //ResGrid.ItemsSource = null;

            if (tisqry.Count() <= 0)
            {
                return;
            }
            //StringBuilder sss = new StringBuilder();

            foreach (Sr25DataSet.ndataRow  drow in tisqry)
            {
                DataRow d0 = PanRes.NewRow();
                float x = (float)drow.Nutr_Val;
                string rx = string.Format(Helper.StrFormat(decp), x);
                d0[0] = rx;
                string des = _selected.Where(f => f.NDB_No == drow.NDB_No).First().Desc;
                d0[1] = des;
                PanRes.Rows.Add(d0);
                //sss.AppendLine(string.Format("NDB = {0}   {1}   {2} ",drow.NDB_No,rx,des));
            }
            //MessageBox.Show(sss.ToString());

        }

        #endregion UI

    }
}
