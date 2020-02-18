using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace GridLayoutApp
{
  public class Utils
  {
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
      if (depObj != null)
      {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
          DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
          if (child != null && child is T)
          {
            yield return (T)child;
          }

          foreach (T childOfChild in FindVisualChildren<T>(child))
          {
            yield return childOfChild;
          }
        }
      }
    }
  }

  public class BooleanConverter<T> : IValueConverter
  {
    public BooleanConverter(T trueValue, T falseValue)
    {
      True = trueValue;
      False = falseValue;
    }

    public T True { get; set; }
    public T False { get; set; }

    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is bool && ((bool)value) ? True : False;
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
    }
  }

  public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
  {
    public BooleanToVisibilityConverter() :
        base(Visibility.Visible, Visibility.Collapsed)
    { }
  }

  [ValueConversion(typeof(bool), typeof(bool))]
  public class InverseBooleanConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
     // if (targetType != typeof(bool))
     //   throw new InvalidOperationException("The target must be a boolean");

      return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }

    #endregion
  }

  internal class PercentageToSizeConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      try
      {
        double parentSize = System.Convert.ToDouble(values[0]);
        double percentSize = System.Convert.ToDouble(values[1]);
        double size = parentSize * percentSize;
        return size;
      }
      catch(Exception ex)
      {
        return double.NaN;
      }
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return null;
    }
  }

  /// <summary>
  /// Interaction logic for GridLayout.xaml
  /// </summary>
  public partial class GridLayout : UserControl
  {
    private Random _rndColor = new Random();
    private GridCell _highLightCell = new GridCell(0, 0, 1, 1);
    public GridLayoutViewModel GridLayoutViewModel
    {
      get { return DataContext as GridLayoutViewModel; }
    }

    public GridLayout()
    {
      InitializeComponent();
    }

    private void GridLayout_Loaded(object sender, RoutedEventArgs e)
    {
      GridLayoutViewModel vm = GridLayoutViewModel;
      if (vm == null)
        return;
    }
    ////Generate horizontal GridSplitters
    //for (int row = 0; row < vm.Rows - 1; row++)
    //{
    //  int startIndex = 0;
    //  int span = 0;
    //  for (int col = 0; col < vm.Columns; col++)
    //  {
    //    GridCell cell = vm.GridCells[row, col];
    //    bool isBreak = false;
    //    bool isGroup = cell.IsGroup;
    //    bool isGroupEdge = (isGroup && (cell.Row + cell.RowSpan - 1 == row));
    //    bool isStart = false;
    //    if (col == 0)
    //    {
    //      span++;
    //      continue;
    //    }

    //    if (isGroup && !isGroupEdge)
    //    {
    //      if (cell.Column + cell.ColumnSpan == col)
    //      {
    //        isBreak = true;
    //      }
    //      else if (cell.Column == col)
    //      {
    //        isBreak = true;
    //        span--;
    //      }
    //      isStart = true;
    //    }
    //    else if (col == vm.Columns - 1)
    //    {
    //      isBreak = true;
    //    }

    //    span++;

    //    if (isBreak)
    //    {
    //      if (span > 0)
    //      {
    //        GridSplitter splitter = new GridSplitter();
    //        Grid.SetRow(splitter, row);
    //        Grid.SetColumn(splitter, startIndex);
    //        Grid.SetColumnSpan(splitter, span);
    //        splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
    //        splitter.VerticalAlignment = VerticalAlignment.Bottom;
    //        splitter.ResizeDirection = GridResizeDirection.Rows;
    //        splitter.Height = 5;
    //        splitter.Style = Resources["GridSplitterStyle"] as Style;
    //        MainGrid.Children.Add(splitter);
    //      }
    //      startIndex = col + 1;
    //      span = 0;
    //    }
    //    else if (isStart)
    //    {
    //      startIndex = col + 1;
    //      span = 0;
    //    }
    //  }
    //}

    ////Generate vertical GridSplitters
    //for (int col = 0; col < vm.Columns - 1; col++)
    //{
    //  int startIndex = 0;
    //  int span = 0;
    //  for (int row = 0; row < vm.Rows; row++)
    //  {
    //    GridCell cell = vm.GridCells[row, col];
    //    bool isBreak = false;
    //    bool isGroup = cell.IsGroup;
    //    bool isGroupEdge = (isGroup && (cell.Column + cell.ColumnSpan - 1 == col));
    //    bool isStart = false;
    //    if (row == 0)
    //    {
    //      span++;
    //      continue;
    //    }

    //    if (isGroup && !isGroupEdge)
    //    {
    //      if (cell.Row + cell.RowSpan == row)
    //      {
    //        isBreak = true;
    //      }
    //      else if (cell.Row == row)
    //      {
    //        isBreak = true;
    //        span--;
    //      }
    //      isStart = true;
    //    }
    //    else if (row == vm.Rows - 1)
    //    {
    //      isBreak = true;
    //    }

    //    span++;

    //    if (isBreak)
    //    {
    //      if (span > 0)
    //      {
    //        GridSplitter splitter = new GridSplitter();
    //        Grid.SetColumn(splitter, col);
    //        Grid.SetRow(splitter, startIndex);
    //        Grid.SetRowSpan(splitter, span);
    //        splitter.HorizontalAlignment = HorizontalAlignment.Right;
    //        splitter.VerticalAlignment = VerticalAlignment.Stretch;
    //        splitter.ResizeDirection = GridResizeDirection.Columns;
    //        splitter.Width = 5;
    //        splitter.Style = this.Resources["GridSplitterStyle"] as Style;
    //        MainGrid.Children.Add(splitter);
    //      }
    //    }
    //    else if (isStart)
    //    {
    //      startIndex = row + 1;
    //      span = 0;
    //    }
    //  }
    //}
    private void Button_KeyUp(object sender, KeyEventArgs e)
    {
      Button button = sender as Button;
      if (button?.DataContext == null)
        return;
      GridCell cell = button.DataContext as GridCell;
      if (cell == null)
        return;
      if(e.Key == Key.Escape)
      {
        if (cell.IsEditing)
          cell.IsEditing = false;
      }
      else if (e.Key == Key.H)
      {
        GridLayoutViewModel.ClearSelection();
        GridLayoutViewModel.SliceHorizontally(cell, 2);
      }
      else if (e.Key == Key.V)
      {
        GridLayoutViewModel.ClearSelection();
        GridLayoutViewModel.SliceVertically(cell, 2);
      }
      else if (e.Key == Key.M)
      {
        GridLayoutViewModel.Merge(cell);
      }
    }

    private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      Button button = sender as Button;
      if (button?.DataContext == null)
        return;
      GridCell cell = button.DataContext as GridCell;
      if (cell == null)
        return;
      GridLayoutViewModel vm = GridLayoutViewModel;
      if (vm == null)
        return;
      vm.ClearSelection();
      if (vm.EditedCell != null)
        vm.EditedCell.IsEditing = false;
      cell.IsEditing = true;
      vm.EditedCell = cell;

      IEnumerable<TextBox> textBoxes = Utils.FindVisualChildren<TextBox>(sender as DependencyObject);
      TextBox textBox = textBoxes.Cast<TextBox>().First();
      if (textBox != null)
      {
        textBox.Focusable = true;
        textBox.Focus();
      }
    }

    private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      Button button = sender as Button;
      if (button?.DataContext == null)
        return;
      GridCell gridCellVM = button.DataContext as GridCell;
      if (gridCellVM == null)
        return;
      GridLayoutViewModel vm = GridLayoutViewModel;
      if (vm == null)
        return;

      if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && vm.SelectedCells.Count > 0)
      {
        GridCell lastSelected = vm.SelectedCells[vm.SelectedCells.Count - 1];
        System.Drawing.Rectangle extent = System.Drawing.Rectangle.Union(lastSelected.CellExtent, gridCellVM.CellExtent);
        Dictionary<GridCell, GridCell> selectSet = new Dictionary<GridCell, GridCell>();
        foreach (GridCell cell in vm.SelectedCells)
        {
          selectSet[cell] = cell;
          extent = System.Drawing.Rectangle.Union(cell.CellExtent, extent);
        }

        while (true)
        {
          int selCount = 0;
          foreach (GridCell cell in vm.GridElements)
          {
            if (cell.CellExtent.IntersectsWith(extent))
            {
              if (selectSet.ContainsKey(cell))
                continue;
              extent = System.Drawing.Rectangle.Union(cell.CellExtent, extent);
              vm.AddToSelection(cell);
              selectSet[cell] = cell;
              selCount++;
            }
            else
            {
              cell.IsHighlighted = false;
            }
          }

          if (selCount == 0)
            break;
        }
      }
      else
      {
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
        {
          vm.ClearSelection();
        }
        if (vm.SelectedCells.Contains(gridCellVM))
        {
          gridCellVM.IsSelected = false;
          gridCellVM.IsHighlighted = false;
          vm.RemoveFromSelection(gridCellVM);
        }
        else
        {
          gridCellVM.IsSelected = true;
          gridCellVM.IsHighlighted = true;
          vm.AddToSelection(gridCellVM);
        }

        if (vm.SelectedCells.Count == 0)
          return;
      }
    }

    private void Button_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      Button button = sender as Button;
      if (button?.DataContext == null)
        return;
      GridCell cell = button.DataContext as GridCell;
      if (cell == null)
        return;
      GridLayoutViewModel vm = GridLayoutViewModel;
      if (vm == null)
        return;
      if (vm.SelectedCells.Contains(cell))
        return;
      vm.ClearSelection();
      vm.AddToSelection(cell);
    }

    private void Graticule_PreviewDragEnter(object sender, DragEventArgs e)
    {

    }

    private void Graticule_PreviewDragLeave(object sender, DragEventArgs e)
    {

    }

    private void Graticule_PreviewDragOver(object sender, DragEventArgs e)
    {

    }

    private void Graticule_PreviewDrop(object sender, DragEventArgs e)
    {

    }

    GridCell _elementResizing = null;
    Point _mouseDownPoint = new Point(0, 0);
    private void Graticule_MouseEnter(object sender, MouseEventArgs e)
    {
      Button button = sender as Button;
      if (button?.DataContext == null)
        return;
      GridCell cell = button.DataContext as GridCell;
      if (cell == null)
        return;
      cell.IsResizing = true;
    }

    private void Graticule_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      Button button = sender as Button;
      if (button?.DataContext == null)
        return;
      GridCell cell = button.DataContext as GridCell;
      if (cell == null)
        return;
      cell.IsResizing = true;
      _elementResizing = cell;
      _elementResizing.IsResizing = true;
      _mouseDownPoint = Mouse.GetPosition(this);
    }

    private void Graticule_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
      Button button = sender as Button;
      if (button?.DataContext == null)
        return;
      GridCell cell = button.DataContext as GridCell;
      if (cell == null)
        return;
      cell.IsResizing = false;
      if (_elementResizing != null)
      {
        _elementResizing.IsResizing = false;
        _elementResizing = null;
      }
    }

    private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (_elementResizing != null)
      {
        Point point = Mouse.GetPosition(this);
        double deltaX = point.X - _mouseDownPoint.X;
        double deltaPercentX = deltaX / this.ActualWidth;
        _mouseDownPoint = point;
        _elementResizing.PercentLeft += (float)deltaPercentX;
        _elementResizing.PercentWidth -= (float)deltaPercentX;
        System.Diagnostics.Debug.WriteLine("deltaX: " + deltaX.ToString());
      }
    }
  }
}
