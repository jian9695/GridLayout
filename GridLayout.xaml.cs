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

  public class MultiBooleanToVisibilityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      foreach(object obj in values)
      {
        if ((bool)obj == false)
          return Visibility.Collapsed;
      }
      return Visibility.Visible;
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return null;
    }
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

  internal class PercentageToPositionConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      try
      {
        double parentSize = (double)values[0];
        double percentPosition = (double)values[1];
        double position = percentPosition / parentSize;
        return position;
      }
      catch (Exception ex)
      {
        return double.NaN;
      }
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return null;
    }
  }

  internal class MarginConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      Thickness currentMargin = (Thickness)values[0];
      Thickness newMargin = (Thickness)values[1];
      if (double.IsNaN(newMargin.Left) || double.IsNaN(newMargin.Right) || double.IsNaN(newMargin.Top) || double.IsNaN(newMargin.Bottom))
        return currentMargin;
      return newMargin;
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return null;
    }
  }

  public class GridButton : Button
  {
    public GridButton()
    {
      this.SizeChanged += GridButton_SizeChanged;
    }

    private void GridButton_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (!this.IsLoaded)
        return;
      GridCell cell = DataContext as GridCell;
      if (cell == null)
        return;
      //cell.PercentHeight = (float)(this.ActualHeight / cell.Parent.ActualHeight);
      //cell.PercentWidth = (float)(this.ActualWidth / cell.Parent.ActualWidth);
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
      this.SizeChanged += GridLayout_SizeChanged;
      this.Loaded += GridLayout_Loaded;
    }

    private void GridLayout_Loaded(object sender, RoutedEventArgs e)
    {
      GridLayoutViewModel vm = GridLayoutViewModel;
      if (vm == null)
        return;
      _highLightCell.IsOn = _highLightCell.IsHighlightOverlay = (vm.SelectedCells?.Count > 0);
      if (_highLightCell.IsOn)
      {
        _highLightCell.CellExtent = vm.SelectedExtent;
        _highLightCell.IsHighlightOverlay = true;
      }
      Button highlightButton = GenerateButton(_highLightCell);
      _highLightCell.TextBlock.Text = "";
      highlightButton.IsHitTestVisible = false;
      highlightButton.DataContext = _highLightCell;
      //MainCanvas.Children.Add(highlightButton);
    }

    private void GridLayout_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      GridLayoutViewModel vm = GridLayoutViewModel;
      if (vm == null)
        return;
      vm.ActualHeight = this.ActualHeight;
      vm.ActualWidth = this.ActualWidth;
    }

    public object DataContext 
    {
      get
      {
        return base.DataContext;
      }
      set
      {
        base.DataContext = value;
        //_highLightCell.Parent = GridLayoutViewModel;
      }
    }

    private GridButton GenerateButton(GridCell cell)
    {
      GridButton button = new GridButton();
      button.Style = this.Resources["GridCellStyle"] as Style;
      button.DataContext = cell;
      button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
      button.MouseDoubleClick += Button_MouseDoubleClick;
      button.KeyUp += Button_KeyUp;
      return button;
    }

    private void UpdateGridCells()
    {
      //Dictionary<GridCell, Button> lookup = new Dictionary<GridCell, Button>();
      //foreach (UIElement element in MainCanvas.Children)
      //{
      //  Button button = element as Button;
      //  if (button == null || !(button.DataContext is GridCell) || button.DataContext == _highLightCell)
      //    continue;
      //  lookup[button.DataContext as GridCell] = button;
      //}

      //MainCanvas.Children.Clear();
      //GridLayoutViewModel vm = GridLayoutViewModel;
      //if (vm?.GridElements == null)
      //  return;

      //// Generate items;
      //foreach (GridCell cell in vm.GridElements)
      //{
      //  cell.TextBlock.Text = cell.Row.ToString().PadRight(2) + "," + cell.Column.ToString().PadRight(2);
      //  Button button = null;
      //  if (lookup.ContainsKey(cell))
      //    button = lookup[cell];
      //  else
      //    button = GenerateButton(cell);
      //  MainCanvas.Children.Add(button);
      //}

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

    //_highLightCell.IsOn = _highLightCell.IsHighlightOverlay = (vm.SelectedCells?.Count > 0);
    //  if (_highLightCell.IsOn)
    //  {
    //    _highLightCell.CellExtent = vm.SelectedExtent;
    //    _highLightCell.IsHighlightOverlay = true;
    //  }
    //  Button highlightButton = GenerateButton(_highLightCell);
    //  _highLightCell.TextBlock.Text = "";
    //  highlightButton.IsHitTestVisible = false;
    //  highlightButton.DataContext = _highLightCell;
    //  MainCanvas.Children.Add(highlightButton);
    }

    private void UpdateSelectedCells()
    {
      //GridLayoutViewModel vm = GridLayoutViewModel;
      //if (vm == null)
      //  return;
      //_highLightCell.CellExtent = vm.SelectedExtent;
      //if(vm.SelectedExtent.Width * vm.SelectedExtent.Height >= 2)
      //{
      //  _highLightCell.IsOn = true;
      //  _highLightCell.IsHighlightOverlay = true;
      //}
      //else
      //{
      //  _highLightCell.IsOn = false;
      //  _highLightCell.IsHighlightOverlay = false;
      //}
    }

    private void ClearSelection()
    {
      GridLayoutViewModel vm = GridLayoutViewModel;
      if (!(vm?.GridElements?.Count > 0) || !(vm?.SelectedCells?.Count > 0))
        return;
      foreach (GridCell cell in vm.SelectedCells)
      {
        cell.IsSelected = false;
        cell.IsHighlighted = false;
      }
      vm.SelectedCells.Clear();
      //_highLightCell.IsOn = false;
    }

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
        {
          //_highLightCell.IsOn = false;
          cell.IsEditing = false;
        }
      }
      else if (e.Key == Key.H)
      {
        ClearSelection();
        GridLayoutViewModel.SliceHorizontally(cell, 2);
      }
      else if (e.Key == Key.V)
      {
        ClearSelection();
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
      ClearSelection();
      GridLayoutViewModel vm = GridLayoutViewModel;
      if (vm == null)
        return;
      if (vm.EditedCell != null)
        vm.EditedCell.IsEditing = false;
      //_highLightCell.IsOn = false;
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
        //_highLightCell.IsOn = true;
        //_highLightCell.IsHighlightOverlay = true;
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
              cell.IsSelected = true;
              cell.IsHighlighted = true;
              vm.SelectedCells.Add(cell);
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

        //_highLightCell.CellExtent = extent;
        vm.SelectedExtent = extent;
      }
      else
      {
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
        {
          ClearSelection();
        }
        //_highLightCell.IsOn = false;
        if (vm.SelectedCells.Contains(gridCellVM))
        {
          gridCellVM.IsSelected = false;
          gridCellVM.IsHighlighted = false;
          vm.SelectedCells.Remove(gridCellVM);
        }
        else
        {
          gridCellVM.IsSelected = true;
          gridCellVM.IsHighlighted = true;
          vm.SelectedCells.Add(gridCellVM);
        }

        if (vm.SelectedCells.Count == 0)
          return;
        GridCell lastSelected = vm.SelectedCells[vm.SelectedCells.Count - 1];
        System.Drawing.Rectangle extent = System.Drawing.Rectangle.Union(lastSelected.CellExtent, gridCellVM.CellExtent);
        foreach (GridCell cell in vm.SelectedCells)
        {
          cell.IsHighlighted = true;
          extent = System.Drawing.Rectangle.Union(cell.CellExtent, extent);
        }
        vm.SelectedExtent = extent;
        //_highLightCell.CellExtent = extent;
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
      ClearSelection();
      cell.IsSelected = true;
      cell.IsHighlighted = true;
      vm.SelectedCells.Add(cell);
    }
  }
}
