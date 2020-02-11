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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GridLayoutApp
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      GridLayoutViewModel vm = new GridLayoutViewModel();
      this.GridLayout.DataContext = vm;
      this.DataContext = vm;
      //int rows = 10;
      //int cols = 10;
      //vm.CreateGrid(rows, cols);
      //vm.MergeCells(0, 0, 2, 2);
      //vm.MergeCells(2, 2, 3, 3);
      //vm.MergeCells(5, 5, 2, 2);
      //vm.MergeCells(7, 7, 3, 3);
      int rows = 3;
      int cols = 3;
      vm.CreateGrid(rows, cols);
      vm.MergeCells(0, 0, 1, 2);
      //vm.MergeCells(1, 1, 1, 2);
      //vm.MergeCells(2, 0, 1, 3);
    }
  }
}
