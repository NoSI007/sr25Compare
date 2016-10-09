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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
//using Sr25Compare;

namespace Sr25Compare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
    

        Sr25DataSet foo = null;
  
        List<Sr25DataSet.FOOD_DESRow> food_res = null;

        List<Sr25DataSet.FOOD_DESRow> _selected = new List<Sr25DataSet.FOOD_DESRow>();

        BackgroundWorker bg = new BackgroundWorker();


     

        public MainWindow()
        {
            InitializeComponent();
            foo = (Sr25DataSet)FindResource("SR25DS");
            bg.DoWork += bg_DoWork;
            bg.RunWorkerCompleted += bg_RunWorkerCompleted;
            this.Loaded += MainWindow_Loaded;
            FooList.SelectionMode = SelectionMode.Multiple;
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CompareButtons(true);
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            Loadndata();
        }

        private void Loadndata()
        {
            Sr25DataSetTableAdapters.ndataTableAdapter nddd = new Sr25DataSetTableAdapters.ndataTableAdapter();
            nddd.Fill(foo.ndata);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Sr25DataSetTableAdapters.Fd_GrpTableAdapter fgda = new Sr25DataSetTableAdapters.Fd_GrpTableAdapter();
            fgda.Fill(foo.Fd_Grp);
            fgda.Dispose();

            Sr25DataSetTableAdapters.FOOD_DESTableAdapter fdda = new Sr25DataSetTableAdapters.FOOD_DESTableAdapter();
            fdda.Fill(foo.FOOD_DES);
            fdda.Dispose();

            Sr25DataSetTableAdapters.NUT_DEFTableAdapter ndda = new Sr25DataSetTableAdapters.NUT_DEFTableAdapter();
            ndda.Fill(foo.NUT_DEF);
            ndda.Dispose();
            CompareButtons(false);
            bg.RunWorkerAsync();
            Browse();

        }


        private void CompareButtons(bool shw)
        {
            if (shw == false)
            {
                z_comp.Visibility = System.Windows.Visibility.Hidden;
                z_comp1.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                z_comp.Visibility = System.Windows.Visibility.Visible;
                z_comp1.Visibility = System.Windows.Visibility.Visible;
            }
        }




        private void z_PanelComp(object sender, RoutedEventArgs e)
        {
            PanComp();
        }


        bool _browse = true;

        private void grpList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (grpList.SelectedItem != null)
            {
                int fgc = (int)grpList.SelectedValue;
                //Title = fgc.ToString();

                if (_browse )
                {
                    var fl = foo.FOOD_DES.Where(f => f.FdGrp_Cd == fgc);
                    FooList.DataContext = fl;
                    foolisting.Header = string.Format("Foods ( {0} )", fl.Count());
                }
                else
                {
                    var fl = food_res.Where(f=> f.FdGrp_Cd == fgc);
                    FooList.DataContext = fl;
                    foolisting.Header = string.Format("Foods ( {0} )", fl.Count());
                }
            }
        }








        /// <summary>
        /// Browsing all groups/foods.
        /// </summary>
        private void Browse()
        {
            _browse = true;
            grpList.DataContext = foo.Fd_Grp;
        }

        private void Search()
        {
            _browse = false;

            if (string.IsNullOrWhiteSpace(Wild.Text) == true)
            {
                Browse();
                return;
            }

            var _fs_res  = foo.FOOD_DES.Where(f => f.Desc.Contains(Wild.Text));//HACK if nothing found return to browse.

            var gcd = (from f in _fs_res
                       select f.FdGrp_Cd).Distinct();

            var _gRes = foo.Fd_Grp.Where(g=>gcd.Contains(g.FdGrp_CD));

            grpList.DataContext = _gRes;
            food_res = _fs_res.ToList<Sr25DataSet.FOOD_DESRow>();
        }



        /// <summary>
        /// store the selected food in the cmp listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void z_add_Click(object sender, RoutedEventArgs e)
        {
            AddItem2cmp();
        }
        /// <summary>
        /// remove an item from the cmp ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void z_del_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedItem();
        }

        private void RemoveSelectedItem()
        {
            if (cmp.SelectedItem == null)
                return;
    
            ListBoxItem lbi = cmp.SelectedItem as ListBoxItem;
            Sr25DataSet.FOOD_DESRow sr = (Sr25DataSet.FOOD_DESRow)lbi.Tag;
            cmp.Items.Remove(lbi);

            UpdateCount();
        }

        private void UpdateCount()
        {
            z_cmp_count.Content = string.Format("{0} Items To Compare.", cmp.Items.Count);
        }
        /// <summary>
        /// Clear cmp and all TheResTab 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void z_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearWorkspace();
        }

        private void ClearWorkspace()
        {
            cmp.Items.Clear();
            UpdateCount();
        }
        /// <summary>
        /// Load all data from ACCES for the selected foods
        /// and display in datagrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void z_comp_Click(object sender, RoutedEventArgs e)
        {
            Compare();
        }

        //int round = 0;

        private void Compare()
        {
            if (TempList2pass())
            {

                SRTable res = new SRTable();

                res.foo = this.foo;

                res._selected = _selected;

                res.ShowDialog();
            }
            //round++;
        }

        private bool TempList2pass()
        {
            _selected.Clear();

            foreach (ListBoxItem lbi in cmp.Items)
            {
                _selected.Add((Sr25DataSet.FOOD_DESRow)lbi.Tag);
            }
            return _selected.Count > 0;
        }





        private void PanComp()
        {
            if (TempList2pass())
            {
                PanelRes res = new PanelRes();

                res.foo = this.foo;

                res._selected = _selected;

                res.ShowDialog();
            }
        }

        private void z_go_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }



        private void AddItem2cmp()
        {
            foreach(var x in FooList.SelectedItems)
            {
                Sr25DataSet.FOOD_DESRow fr = (Sr25DataSet.FOOD_DESRow)x;
                if (fr == null)
                    return;
                Add2cmpList(fr);
            }

        }

        private void Add2cmpList(Sr25DataSet.FOOD_DESRow fr)
        {
            foreach (ListBoxItem li in cmp.Items)
            {
                if (li.Tag.Equals(fr) == true)
                {
                    MessageBox.Show("Items already in comparison table");
                    return;
                }
            }
            ListBoxItem lbi = new ListBoxItem();
            lbi.Tag = fr;
            lbi.Content = fr.Desc;
            cmp.Items.Add(lbi);
            //z_cmp_count.Content = string.Format("{0} Items To Compare.", cmp.Items.Count);
            UpdateCount();
        }
    }
}
