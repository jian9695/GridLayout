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
      int rows = 4;
      int cols = 4;
      vm.CreateGrid(rows, cols);

      vm.MergeCells(0, 0, 2, 2);
      //vm.MergeCells(2, 4, 3, 3);
      //vm.MergeCells(5, 6, 4, 4);
      //vm.MergeCells(6, 2, 3, 3);
    }
  }
}
