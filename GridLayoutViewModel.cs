using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Windows.Markup;
using System.IO;
using System.Xml;

namespace GridLayoutApp
{

  public class RelayCommand : ICommand
  {
    private Action<object> execute;
    private Func<object, bool> canExecute;

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
      return this.canExecute == null || this.canExecute(parameter);
    }

    public void Execute(object parameter)
    {
      this.execute(parameter);
    }
  }

  public class GridCell : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static Random _rndColor = new Random();
    private TextBlock _textBlock;
    private Border _border;
    private Image _image;
    private int _row = 0;
    private int _column = 0;
    private int _rowSpan = 1;
    private int _columnSpan = 1;
    private Thickness _margin = new Thickness(5, 5, 5, 5);
    private bool _isSelected = false;
    private bool _isEditing = false;
    private bool _isHighlightOverlay = false;
    private bool _isHighlighted = false;
    private bool _isOn = true; 
    private bool _useImage = false;
    private bool _isLocked = false;
    private bool _displayContent = true;
    private GridLayoutViewModel _parent = null;
    private System.Drawing.RectangleF _extent = new System.Drawing.RectangleF(0, 0, 1, 1);

    private static T DeepCopy<T>(T element)
    {
      var xaml = XamlWriter.Save(element);
      var xamlString = new StringReader(xaml);
      var xmlTextReader = new XmlTextReader(xamlString);
      var deepCopyObject = (T)XamlReader.Load(xmlTextReader);
      return deepCopyObject;
    }

    public GridCell(int row, int col, int rowSpan, int colSpan, GridLayoutViewModel parent = null)
    {
      _row = row;
      _column = col;
      _rowSpan = rowSpan;
      _columnSpan = colSpan;
      _parent = parent;
      _textBlock = new TextBlock();
      _border = new Border();
      _image = new Image();
      _textBlock.TextWrapping = TextWrapping.Wrap;
      Border.Background = new SolidColorBrush(Color.FromRgb((byte)_rndColor.Next(256), (byte)_rndColor.Next(256), (byte)_rndColor.Next(256)));
      ApplyButtonStyle();
    }

    public GridCell(GridCell copyFrom)
    {
      _row = copyFrom.Row;
      _column = copyFrom.Column;
      _rowSpan = copyFrom.RowSpan;
      _columnSpan = copyFrom.ColumnSpan;
      _textBlock = DeepCopy<TextBlock>(copyFrom.TextBlock);
      _border = DeepCopy<Border>(copyFrom.Border);
      _image = DeepCopy<Image>(copyFrom.Image);
      _margin = copyFrom.Margin;
      _isSelected = copyFrom.IsSelected;
      _isEditing = copyFrom.IsEditing;
      _isHighlightOverlay = copyFrom.IsHighlightOverlay;
      _isOn = copyFrom.IsOn;
      _useImage = copyFrom.UseImage;
      _isLocked = copyFrom.IsLocked;
      _displayContent = copyFrom.DisplayContent;
      _extent = copyFrom.Extent;
      _parent = copyFrom.Parent;
      Border.Background = new SolidColorBrush(Color.FromRgb((byte)_rndColor.Next(256), (byte)_rndColor.Next(256), (byte)_rndColor.Next(256)));
      ApplyButtonStyle();
    }

    public void ApplyButtonStyle()
    {
      TextBlock.FontSize = 16;
      TextBlock.FontWeight = FontWeights.Bold;
      //cell.TextBlock.Foreground = new SolidColorBrush(Color.FromRgb((byte)_rndColor.Next(256), (byte)_rndColor.Next(256), (byte)_rndColor.Next(256)));
      TextBlock.Text = Row.ToString().PadRight(2) + "," + Column.ToString().PadRight(2);
      TextBlock.Foreground = Brushes.Black;
      Border.BorderThickness = new Thickness(1);
    }

    public bool IsGroup { get { return _rowSpan > 1 || _columnSpan > 1; } }

    public int Row
    {
      get { return _row; }
      set
      {
        if (value < 0)
          return;
        _row = value;
        NotifyPropertyChanged(nameof(Row));
      }
    }

    public int RowSpan
    {
      get { return _rowSpan; }
      set
      {
        if (value < 1)
          return;
        _rowSpan = value;
        NotifyPropertyChanged(nameof(RowSpan));
      }
    }

    public int Column
    {
      get { return _column; }
      set
      {
        if (value < 0)
          return;
        _column = value;
        NotifyPropertyChanged(nameof(Column));
      }
    }

    public int ColumnSpan
    {
      get { return _columnSpan; }
      set
      {
        if (value < 1)
          return;
        _columnSpan = value;
        NotifyPropertyChanged(nameof(ColumnSpan));
      }
    }

    public Thickness Margin
    {
      get { return _margin; }
      set
      {
        _margin = value;
        NotifyPropertyChanged(nameof(Margin));
      }
    }

    public bool IsEditing
    {
      get { return _isEditing; }
      set
      {
        if (_isLocked)
          return;
        _isEditing = value;
        NotifyPropertyChanged(nameof(IsEditing));
      }
    }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
         _isSelected = value;
        NotifyPropertyChanged(nameof(IsSelected));
      }
    }

    public bool IsHighlightOverlay
    {
      get { return _isHighlightOverlay; }
      set
      {
        _isHighlightOverlay = value;
        NotifyPropertyChanged(nameof(IsHighlightOverlay));
      }
    }

    public bool IsHighlighted
    {
      get { return _isHighlighted; }
      set
      {
        _isHighlighted = value;
        NotifyPropertyChanged(nameof(IsHighlighted));
      }
    }

    public bool IsLocked
    {
      get { return _isLocked; }
      set
      {
        _isLocked = value;
        NotifyPropertyChanged(nameof(IsLocked));
        if(_isLocked)
        {
          _isEditing = false;
          NotifyPropertyChanged(nameof(IsEditing));
        }
      }
    }

    public bool IsOn
    {
      get { return _isOn; }
      set
      {
        _isOn = value;
        NotifyPropertyChanged(nameof(IsOn));
      }
    }

    public bool UseImage
    {
      get { return _useImage; }
      set
      {
        _useImage = value;
        NotifyPropertyChanged(nameof(UseImage));
        NotifyPropertyChanged(nameof(HasImage));
      }
    }

    public bool HasImage
    {
      get { return _useImage && _image?.Source != null; }
    }

    public object Tag { get; set; } = null;

    public System.Drawing.Rectangle CellExtent
    {
      get { return new System.Drawing.Rectangle(Column, Row, ColumnSpan, RowSpan); }
      set
      {
        Row = value.Y;
        Column = value.X;
        RowSpan = value.Height;
        ColumnSpan = value.Width;
        NotifyPropertyChanged(nameof(CellExtent));
      }
    }

    public TextBlock TextBlock { get { return _textBlock; } }

    public Border Border { get { return _border; } }

    public Image Image { get { return _image; } }

    public bool DisplayContent
    {
      get { return _displayContent; }
      set
      {
        _displayContent = value;
        NotifyPropertyChanged(nameof(DisplayContent));
      }
    }

    public float PercentTop
    {
      get { return _extent.Y; }
      set
      {
        _extent.Y = value;
        NotifyPropertyChanged(nameof(PercentTop));
        NotifyPropertyChanged(nameof(Extent));
      }
    }

    public float PercentLeft
    {
      get { return _extent.X; }
      set
      {
        _extent.X = value;
        NotifyPropertyChanged(nameof(PercentLeft));
        NotifyPropertyChanged(nameof(Extent));
      }
    }

    public float PercentWidth
    {
      get { return _extent.Width; }
      set
      {
        _extent.Width = value;
        NotifyPropertyChanged(nameof(PercentWidth));
        NotifyPropertyChanged(nameof(Extent));
      }
    }

    public float PercentHeight
    {
      get { return _extent.Height; }
      set
      {
        _extent.Height = value;
        NotifyPropertyChanged(nameof(PercentHeight));
        NotifyPropertyChanged(nameof(Extent));
      }
    }

    public System.Drawing.RectangleF Extent
    {
      get { return _extent; }
      set
      {
        _extent = value;
        NotifyPropertyChanged(nameof(Extent));
      }
    }

    public GridLayoutViewModel Parent { get => _parent; set => _parent = value; }

    public void Clear()
    {
      UseImage = false;
      Image.Source = null;
      TextBlock.Text = "";
      TextBlock.Foreground = Brushes.Black;
      Border.Background = Brushes.Transparent;
      Border.BorderBrush = Brushes.Black;
      Border.BorderThickness = new Thickness(1);
    }
  }

  public class GridLayoutViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    private GridCell[,] _gridCells = null;
    private List<GridCell> _selectedCells = new List<GridCell>();
    private List<GridCell> _uniqueGridElements = new List<GridCell>();
    private GridCell _editedCell = null;
    private double _actualWidth = double.NaN;
    private double _actualHeight = double.NaN;
    private readonly float EdgeSnapEpsilon = 0.00001f;

    public GridLayoutViewModel()
    {

    }

    public int Rows
    {
      get { return _gridCells.GetLength(0); }
    }

    public int Columns
    {
      get { return _gridCells.GetLength(1); }
    }

    private GridCell[,] GridCells
    {
      get { return _gridCells; }
      set { _gridCells = value; }
    }

    public IReadOnlyCollection<GridCell> GridElements
    {
      get { return _uniqueGridElements; }
    }

    public GridCell EditedCell
    {
      get { return _editedCell; }
      set { _editedCell = value; }
    }

    public List<GridCell> SelectedCells { get { return _selectedCells; } }

    public System.Drawing.Rectangle SelectedExtent { get; set; } = new System.Drawing.Rectangle(0, 0, 0, 0);

    public double ActualWidth { get => _actualWidth; set => _actualWidth = value; }

    public double ActualHeight { get => _actualHeight; set => _actualHeight = value; }

    private void FindGridExtent(ObservableCollection<GridCell> cells, out int rows, out int columns)
    {
      System.Drawing.Rectangle extent = new System.Drawing.Rectangle(0, 0, 0, 0);
      foreach (GridCell cell in cells)
      {
        extent = System.Drawing.Rectangle.Union(extent, cell.CellExtent);
      }
      rows = extent.Height;
      columns = extent.Width;
    }

    public void SelectAll()
    {
      _selectedCells.Clear();
      for (int row = 0; row < Rows; row++)
      {
        for (int col = 0; col < Columns; col++)
        {
          GridCell cell = _gridCells[row, col];
          if (cell.IsGroup && (cell.Row != row || cell.Column != col))
            continue;
          cell.IsSelected = true;
          cell.IsOn = true;
          cell.IsEditing = false;
          cell.IsHighlighted = false;
          cell.IsHighlightOverlay = false;
          _selectedCells.Add(cell);
        }
      }
      SelectedExtent = new System.Drawing.Rectangle(0, 0, Columns, Rows);
      NotifyPropertyChanged(nameof(SelectedCells));
    }

    public void ClearSelection()
    {
      _selectedCells.Clear();
      for (int row = 0; row < Rows; row++)
      {
        for (int col = 0; col < Columns; col++)
        {
          GridCell cell = _gridCells[row, col];
          if (cell.IsGroup && (cell.Row != row || cell.Column != col))
            continue;
          cell.IsSelected = true;
          cell.IsOn = true;
          cell.IsEditing = false;
          cell.IsHighlighted = false;
          cell.IsHighlightOverlay = false;
        }
      }
      SelectedExtent = new System.Drawing.Rectangle(0, 0, 0, 0);
      NotifyPropertyChanged(nameof(SelectedCells));
    }

    public void CreateGrid(int rows, int cols)
    {
      _gridCells = new GridCell[rows, cols];
      float rowSize = 1.0f / rows;
      float colSize = 1.0f / cols;
      for (int row = 0; row < rows; row++)
      {
        for (int col = 0; col < cols; col++)
        {
          GridCell cell = new GridCell(row, col, 1, 1, this);
          cell.Extent = new System.Drawing.RectangleF(col * colSize, row * rowSize, colSize, rowSize);
          _gridCells[row,col] = cell;
        }
      }
      _uniqueGridElements = GetUniqueCells();

      NotifyPropertyChanged(nameof(Rows));
      NotifyPropertyChanged(nameof(Columns));
      NotifyPropertyChanged(nameof(GridElements));
      NotifyPropertyChanged(nameof(GridCells));
    }

    public void MergeCells(int startRow, int startCol, int rowSpan, int colSpan)
    {
      GridCell group = _gridCells[startRow, startCol];
      float percentWidth = 0;
      float percentHeight = 0;
      float[] columnEdges = GetColumnEdgePositions();
      float[] rowEdges = GetRowEdgePositions();
      for (int row = startRow; row < startRow + rowSpan; row++)
      {
        percentHeight += (rowEdges[row + 1] - rowEdges[row]);
      }
      for (int col = startCol; col < startCol + colSpan; col++)
      {
        percentWidth += (columnEdges[col + 1] - columnEdges[col]);
      }

      group.RowSpan = rowSpan;
      group.ColumnSpan = colSpan;
      group.PercentWidth = percentWidth;
      group.PercentHeight = percentHeight;
      for (int row = startRow; row < startRow+rowSpan; row++)
      {
        for (int col = startCol; col < startCol+colSpan; col++)
        {
          _gridCells[row, col] = group;
        }
      }

      HashSet<GridCell> rowSet = new HashSet<GridCell>();
      HashSet<GridCell> colSet = new HashSet<GridCell>();
      if (group.ColumnSpan == Columns && group.Row == 0 && group.RowSpan > 1)
      {
        rowSet.Clear();
        int newRows = Rows - group.RowSpan + 1;
        GridCell[,] newGridCells = new GridCell[newRows, Columns];
        for (int col = 0; col < Columns; col++)
        {
          newGridCells[0, col] = group;
        }
        for (int row = group.RowSpan; row < Rows; row++)
        {
          for (int col = 0; col < Columns; col++)
          {
            GridCell cell = _gridCells[row,col];
            if (!rowSet.Contains(cell))
            {
              cell.Row = cell.Row - (group.RowSpan - 1);
              rowSet.Add(cell);
            }
            newGridCells[row - group.RowSpan + 1, col] = cell;
          }
        }
        group.RowSpan = 1;
        _gridCells = newGridCells;
      }
      else if (group.ColumnSpan == Columns && group.Row == Rows - group.RowSpan && group.RowSpan > 1)
      {
        int newRows = Rows - group.RowSpan + 1;
        GridCell[,] newGridCells = new GridCell[newRows, Columns];
        for (int col = 0; col < Columns; col++)
        {
          newGridCells[newRows - 1, col] = group;
        }
        for (int row = 0; row < Rows - group.RowSpan; row++)
        {
          for (int col = 0; col < Columns; col++)
          {
            GridCell cell = _gridCells[row, col];
            newGridCells[row, col] = cell;
          }
        }
        group.RowSpan = 1;
        _gridCells = newGridCells;
      }
      if (group.RowSpan == Rows && group.Column == 0 && group.ColumnSpan > 1)
      {
        colSet.Clear();
        int newCols = Columns - group.ColumnSpan + 1;
        GridCell[,] newGridCells = new GridCell[Rows, newCols];
        for (int row = 0; row < Rows; row++)
        {
          newGridCells[row, 0] = group;
        }

        for (int row = 0; row < Rows; row++)
        {
          for (int col = group.ColumnSpan; col < Columns; col++)
          {
            GridCell cell = _gridCells[row, col];
            if (!colSet.Contains(cell))
            {
              cell.Column = cell.Column - (group.ColumnSpan - 1);
              colSet.Add(cell);
            }
            newGridCells[row, col - group.ColumnSpan + 1] = cell;
          }
        }
        group.ColumnSpan = 1;
        _gridCells = newGridCells;
      }
      else if (group.RowSpan == Rows && group.Column == Columns - group.ColumnSpan && group.ColumnSpan > 1)
      {
        int newCols = Columns - group.ColumnSpan + 1;
        GridCell[,] newGridCells = new GridCell[Rows, newCols];
        for (int row = 0; row < Rows; row++)
        {
          newGridCells[row, newCols - 1] = group;
        }

        for (int row = 0; row < Rows; row++)
        {
          for (int col = 0; col < Columns - group.ColumnSpan; col++)
          {
            GridCell cell = _gridCells[row, col];
            newGridCells[row, col] = cell;
          }
        }
        group.ColumnSpan = 1;
        _gridCells = newGridCells;
      }

      _uniqueGridElements = GetUniqueCells();
      NotifyPropertyChanged(nameof(Rows));
      NotifyPropertyChanged(nameof(Columns));
      NotifyPropertyChanged(nameof(GridElements));
      NotifyPropertyChanged(nameof(GridCells));
    }

    /// <summary>
    /// Edge positions on a scale of 0 to 1 from top to bottom.
    /// </summary>
    /// <returns></returns>
    public float[] GetRowEdgePositions()
    {
      List<GridCell>[] cellsOrderedByRow = new List<GridCell>[Rows];
      for (int i = 0; i < Rows; i++)
      {
        System.Drawing.Rectangle rowExtent = new System.Drawing.Rectangle(0, i, Columns, 1);
        for (int row = 0; row < Rows; row++)
        {
          for (int col = 0; col < Columns; col++)
          {
            GridCell cell = _gridCells[row, col];
            if (cell.IsGroup && (cell.Row != row || cell.Column != col))
              continue;

            if (cell.CellExtent.IntersectsWith(rowExtent))
            {
              if (cellsOrderedByRow[i] == null)
                cellsOrderedByRow[i] = new List<GridCell>();
              cellsOrderedByRow[i].Add(cell);
            }
          }
        }
      }
      float[] rowSizes = new float[Rows];
      float[] rowEdges = new float[Rows + 1];
      rowEdges[0] = 0;
      for (int row = 0; row < Rows; row++)
      {
        List<GridCell> intersectingCells = cellsOrderedByRow[row];
        int topEdge = row;
        int bottomEdge = row + 1;
        if (intersectingCells == null)
        {
          rowSizes[row] = 0;
          rowEdges[bottomEdge] = rowEdges[topEdge];
          continue;
        }
        float rowSize = float.MaxValue;
        foreach (GridCell cell in intersectingCells)
        {
          if (cell.RowSpan == 1)
            rowSize = cell.PercentHeight;
          else
            rowSize = Math.Min(rowSize, cell.PercentHeight / cell.RowSpan);
        }
        rowSizes[row] = rowSize;
        rowEdges[bottomEdge] = rowEdges[topEdge] + rowSize;
      }
      return rowEdges;
    }

    /// <summary>
    /// Edge positions on a scale of 0 to 1 from left to right.
    /// </summary>
    /// <returns></returns>
    public float[] GetColumnEdgePositions()
    {
      List<GridCell>[] cellsOrderedByColumn = new List<GridCell>[Columns];
      for (int i = 0; i < Columns; i++)
      {
        System.Drawing.Rectangle columnExtent = new System.Drawing.Rectangle(i, 0, 1, Rows);
        for (int row = 0; row < Rows; row++)
        {
          for (int col = 0; col < Columns; col++)
          {
            GridCell cell = _gridCells[row, col];
            if (cell.IsGroup && (cell.Row != row || cell.Column != col))
              continue;

            if (cell.CellExtent.IntersectsWith(columnExtent))
            {
              if (cellsOrderedByColumn[i] == null)
                cellsOrderedByColumn[i] = new List<GridCell>();
              cellsOrderedByColumn[i].Add(cell);
            }
          }
        }
      }
      float[] columnSizes = new float[Columns];
      float[] columnEdges = new float[Columns + 1];
      columnEdges[0] = 0;
      for (int col = 0; col < Columns; col++)
      {
        columnEdges[col+1] = float.NaN;
        columnSizes[col] = float.NaN;
      }
      for (int col = 0; col < Columns; col++)
      {
        if (!float.IsNaN(columnSizes[col]))
          continue;

        List<GridCell> intersectingCells = cellsOrderedByColumn[col];
        int leftEdge = col;
        int rightEdge = col + 1;
        if (intersectingCells == null)
        {
          columnSizes[col] = 0;
          columnEdges[rightEdge] = columnEdges[leftEdge];
          continue;
        }

        bool isUniformSpan = true;
        int minSpanColId = 0;
        float colSize = float.NaN;
        int id = -1;
        foreach (GridCell cell in intersectingCells)
        {
          id++;
          if (isUniformSpan && cell.ColumnSpan != intersectingCells[0].ColumnSpan)
            isUniformSpan = false;
          if(cell.ColumnSpan == 1)
          {
            colSize = cell.PercentWidth;
            break;
          }
          if(cell.ColumnSpan < intersectingCells[minSpanColId].ColumnSpan)
          {
            minSpanColId = id;
          }
        }

        columnSizes[col] = colSize;
        if(float.IsNaN(colSize))
        {
          if (isUniformSpan)
          {
            columnSizes[col] = intersectingCells[0].PercentWidth / intersectingCells[0].ColumnSpan;
          }
          else
          {
            bool overlapOtherCells = false;
            GridCell minSpanCell = intersectingCells[minSpanColId];
            System.Drawing.Rectangle minSpanCellExtent = new System.Drawing.Rectangle(minSpanCell.Column, 0, minSpanCell.ColumnSpan, 1);
            foreach (GridCell cell in intersectingCells)
            {
              if(!(new System.Drawing.Rectangle(cell.Column,0,cell.ColumnSpan,1)).Contains(minSpanCellExtent))
              {
                overlapOtherCells = true;
                break;
              }
            }

            if (!overlapOtherCells)
            {
              colSize = minSpanCell.PercentWidth / minSpanCell.ColumnSpan;
              columnSizes[col] = colSize;
              for (int colToSet = minSpanCell.Column; colToSet < Columns; colToSet++)
              {
                columnSizes[colToSet] = colSize;
              }
            }
          }
        }
      }

      while (true)
      {
        bool allColsKnown = true;
        for (int col = 0; col < Columns; col++)
        {
          if (!float.IsNaN(columnSizes[col]))
          {
            continue;
          }
          allColsKnown = false;
          List<GridCell> intersectingCells = cellsOrderedByColumn[col];
          foreach (GridCell cell in intersectingCells)
          {
            if(cell.ColumnSpan == 1 && !float.IsNaN(columnEdges[cell.Column]))
            {
              columnEdges[col] = cell.PercentWidth;
              break;
            }
            float colSize = cell.PercentWidth;
            for (int colToGet = cell.Column; colToGet < cell.Column+cell.ColumnSpan; colToGet++)
            {
              if (colToGet == col)
                continue;
              if(float.IsNaN(columnSizes[colToGet]))
                break;
              colSize -= columnSizes[colToGet];
            }
            columnSizes[col] = colSize;
          }
        }
        if (allColsKnown)
          break;
      }

      for (int col = 0; col < Columns; col++)
      {
        List<GridCell> intersectingCells = cellsOrderedByColumn[col];
        int leftEdge = col;
        int rightEdge = col + 1;
        columnEdges[rightEdge] = columnEdges[leftEdge] + columnSizes[col];
      }
      return columnEdges;
    }

    private void SetCell(GridCell[,] gridCells, GridCell cell)
    {
      for (int row = cell.Row; row < cell.Row + cell.RowSpan; row++)
      {
        for (int col = cell.Column; col < cell.Column + cell.ColumnSpan; col++)
        {
          gridCells[row, col] = cell;
        }
      }
    }

    private List<GridCell> GetUniqueCells()
    {
      List<GridCell> cells = new List<GridCell>();
      HashSet<GridCell> uniqueSet = new HashSet<GridCell>();
      foreach(GridCell cell in _gridCells)
      {
        if (!uniqueSet.Contains(cell))
        {
          cell.ApplyButtonStyle();
          cells.Add(cell);
          uniqueSet.Add(cell);
        }
      }
      return cells;
    }

    public void SliceHorizontally(GridCell targetCell, int numofparts)
    {
      if (double.IsNaN(ActualWidth) || double.IsNaN(ActualHeight)
        || float.IsNaN(targetCell.PercentWidth) || float.IsNaN(targetCell.PercentHeight)
        || targetCell.PercentWidth == 0 || targetCell.PercentHeight == 0)
        return;

      float fraction = 1.0f / numofparts;
      List<GridCell> splitCells = new List<GridCell>();
      float[] columnEdges = GetColumnEdgePositions();
      int startEdge = targetCell.Column;
      int endEdge = targetCell.Column + targetCell.ColumnSpan;
      float columnWidth = columnEdges[endEdge] - columnEdges[startEdge];
      int columnOffset = 0;
      int newColumns = numofparts - 1;
      bool[] snappedEdges = new bool[columnEdges.Length];
      bool[] snappedSlices = new bool[numofparts - 1];
      for (int i = 0; i < numofparts-1; i++)
      {
        snappedSlices[i] = false;
      }

      for (int i = 0; i < columnEdges.Length; i++)
      {
        snappedEdges[i] = false;
        if (i < startEdge || i > endEdge)
          continue;
        if(i == startEdge || i == endEdge)
        {
          snappedEdges[i] = true;
          continue;
        }
        for (int j = 0; j < numofparts - 1; j++)
        {
          float slicePosition = columnEdges[startEdge] + columnWidth * (fraction * (j + 1));
          if(Math.Abs(slicePosition-columnEdges[i]) <= EdgeSnapEpsilon)
          {
            snappedEdges[i] = true;
            snappedSlices[j] = true;
            newColumns--;
            break;
          }
        }
      }

      for (int i = 0; i < numofparts; i++)
      {
        float leftEdgePosition = columnEdges[startEdge] + columnWidth * (fraction * i);
        float rightEdgePosition = columnEdges[startEdge] + columnWidth * (fraction * (i + 1));
        GridCell splitCell = new GridCell(targetCell);
        splitCell.ColumnSpan = 1;
        splitCell.Column = targetCell.Column + columnOffset;
        splitCell.PercentLeft = targetCell.PercentLeft + (float)(targetCell.PercentWidth * (fraction * i));
        for (int j = startEdge + 1; j < endEdge; j++)
        {
          if (snappedEdges[j])
            continue;
          if (columnEdges[j] > leftEdgePosition && columnEdges[j] <= rightEdgePosition)
          {
            splitCell.ColumnSpan++;
          }
        }

        columnOffset += splitCell.ColumnSpan;
        splitCell.PercentWidth *= (float)fraction;
        splitCells.Add(splitCell);
      }

      List<GridCell> uniqueCells = GetUniqueCells();
      Dictionary<GridCell, int[]> incrementCounters = new Dictionary<GridCell, int[]>();

      for (int i = 0; i < numofparts - 1; i++)
      {
        if (snappedSlices[i])
          continue;
        float slicePosition = columnEdges[startEdge] + columnWidth * (fraction * (i + 1));
        foreach (GridCell cell in uniqueCells)
        {
          if (cell == targetCell)
            continue;
          int[] incrementfCounter = null;
          if (!incrementCounters.ContainsKey(cell))
          {
            incrementfCounter = new int[2] { 0, 0 };
            incrementCounters[cell] = incrementfCounter;
          }
          else
          {
            incrementfCounter = incrementCounters[cell];
          }

          if (slicePosition < columnEdges[cell.Column])
          {
            //increment columns;
            incrementfCounter[0]++;
          }
          else if (slicePosition < columnEdges[cell.Column + cell.ColumnSpan])
          {
            //increment column spans;
            incrementfCounter[1]++;
          }
        }
      }

      foreach(KeyValuePair<GridCell, int[]> pair in incrementCounters)
      {
        pair.Key.Column += pair.Value[0];
        pair.Key.ColumnSpan += pair.Value[1];
      }

      int rows = Rows;
      int newCols = Columns + newColumns;
      GridCell[,] newGridCells = new GridCell[rows, newCols];
      for (int row = 0; row < Rows; row++)
      {
        for (int col = 0; col < Columns; col++)
        {
          GridCell cell = _gridCells[row, col];
          if (cell == targetCell)
            continue;
          SetCell(newGridCells, cell);
        }
      }

      foreach (GridCell cell in splitCells)
      {
        SetCell(newGridCells, cell);
      }

      _gridCells = newGridCells;
      _uniqueGridElements = GetUniqueCells();
      NotifyPropertyChanged(nameof(Columns));
      NotifyPropertyChanged(nameof(GridElements));
      NotifyPropertyChanged(nameof(GridCells));
    }

    public void SliceVertically(GridCell targetCell, int numofparts)
    {
      if (double.IsNaN(ActualHeight) || double.IsNaN(ActualHeight)
        || float.IsNaN(targetCell.PercentHeight) || float.IsNaN(targetCell.PercentHeight)
        || targetCell.PercentHeight == 0 || targetCell.PercentHeight == 0)
        return;

      float fraction = 1.0f / numofparts;
      List<GridCell> splitCells = new List<GridCell>();

      float[] rowEdges = GetRowEdgePositions();
      int startEdge = targetCell.Row;
      int endEdge = targetCell.Row + targetCell.RowSpan;
      float rowHeight = rowEdges[endEdge] - rowEdges[startEdge];
      int rowOffset = 0;
      int newRows = numofparts - 1;
      bool[] snappedEdges = new bool[rowEdges.Length];
      bool[] snappedSlices = new bool[numofparts - 1];
      for (int i = 0; i < numofparts - 1; i++)
      {
        snappedSlices[i] = false;
      }

      for (int i = 0; i < rowEdges.Length; i++)
      {
        snappedEdges[i] = false;
        if (i < startEdge || i > endEdge)
          continue;
        if (i == startEdge || i == endEdge)
        {
          snappedEdges[i] = true;
          continue;
        }
        for (int j = 0; j < numofparts - 1; j++)
        {
          float slicePosition = rowEdges[startEdge] + rowHeight * (fraction * (j + 1));
          if (Math.Abs(slicePosition - rowEdges[i]) <= EdgeSnapEpsilon)
          {
            snappedEdges[i] = true;
            snappedSlices[j] = true;
            newRows--;
            break;
          }
        }
      }

      for (int i = 0; i < numofparts; i++)
      {
        float topEdgePosition = rowEdges[startEdge] + rowHeight * (fraction * i);
        float bottomEdgePosition = rowEdges[startEdge] + rowHeight * (fraction * (i + 1));
        GridCell splitCell = new GridCell(targetCell);
        splitCell.RowSpan = 1;
        splitCell.Row = targetCell.Row + rowOffset;
        splitCell.PercentTop = targetCell.PercentTop + (float)(targetCell.PercentHeight * (fraction * i));
        for (int j = startEdge + 1; j < endEdge; j++)
        {
          if (snappedEdges[j])
            continue;
          if (rowEdges[j] > topEdgePosition && rowEdges[j] <= bottomEdgePosition)
          {
            splitCell.RowSpan++;
          }
        }

        rowOffset += splitCell.RowSpan;
        splitCell.PercentHeight *= (float)fraction;
        splitCells.Add(splitCell);
      }

      List<GridCell> uniqueCells = GetUniqueCells();
      Dictionary<GridCell, int[]> incrementCounters = new Dictionary<GridCell, int[]>();

      for (int i = 0; i < numofparts - 1; i++)
      {
        if (snappedSlices[i])
          continue;
        float slicePosition = rowEdges[startEdge] + rowHeight * (fraction * (i + 1));
        foreach (GridCell cell in uniqueCells)
        {
          if (cell == targetCell)
            continue;
          int[] incrementfCounter = null;
          if (!incrementCounters.ContainsKey(cell))
          {
            incrementfCounter = new int[2] { 0, 0 };
            incrementCounters[cell] = incrementfCounter;
          }
          else
          {
            incrementfCounter = incrementCounters[cell];
          }

          if (slicePosition < rowEdges[cell.Row])
          {
            //increment rows;
            incrementfCounter[0]++;
          }
          else if (slicePosition < rowEdges[cell.Row + cell.RowSpan])
          {
            //increment row spans;
            incrementfCounter[1]++;
          }
        }
      }

      foreach (KeyValuePair<GridCell, int[]> pair in incrementCounters)
      {
        pair.Key.Row += pair.Value[0];
        pair.Key.RowSpan += pair.Value[1];
      }

      int cols = Columns;
      int rows = Rows + newRows;
      GridCell[,] newGridCells = new GridCell[rows, cols];

      for (int row = 0; row < Rows; row++)
      {
        for (int col = 0; col < Columns; col++)
        {
          GridCell cell = _gridCells[row, col];
          if (cell == targetCell)
            continue;
          SetCell(newGridCells, cell);
        }
      }

      foreach (GridCell cell in splitCells)
      {
        SetCell(newGridCells, cell);
      }

      _gridCells = newGridCells;
      _uniqueGridElements = GetUniqueCells();
      NotifyPropertyChanged(nameof(Rows));
      NotifyPropertyChanged(nameof(GridElements));
      NotifyPropertyChanged(nameof(GridCells));
    }

    #region Commands

    private List<GridCell> GetActiveCells(GridCell currentCell)
    {
      List<GridCell> cells = SelectedCells;
      if (!(SelectedCells?.Count > 0))
      {
        cells = new List<GridCell>();
        if (currentCell != null)
          cells.Add(currentCell);
      }
      return cells;
    }
    #region Merge
    private ICommand _mergeCommand = null;
    public ICommand MergeCommand
    {
      get
      {
        if (_mergeCommand == null)
          _mergeCommand = new RelayCommand(new Action<object>((x) => Merge(x)), new Func<object, bool>((x) => CanMerge(x)));
        return _mergeCommand;
      }
    }

    public bool CanMerge(object param)
    {
      return (SelectedCells?.Count > 0 && (SelectedExtent.Width > 1 || SelectedExtent.Height > 1));
    }

    public void Merge(object param)
    {
      MergeCells(SelectedExtent.Y, SelectedExtent.X, SelectedExtent.Height, SelectedExtent.Width);
      NotifyPropertyChanged(nameof(GridCells));
    }
    #endregion

    #region Clear
    private ICommand _clearCommand = null;
    public ICommand ClearCommand
    {
      get
      {
        if (_clearCommand == null)
          _clearCommand = new RelayCommand(new Action<object>((x) => Clear(x)), new Func<object, bool>((x) => CanClear(x)));
        return _clearCommand;
      }
    }

    public bool CanClear(object param)
    {
      GridCell cell = param as GridCell;
      if (cell?.IsLocked == true || cell?.DisplayContent == false)
        return false;
      return true;
    }

    public void Clear(object param)
    {
      List<GridCell> cells = GetActiveCells(param as GridCell);
      foreach (GridCell cell in cells)
      {
        cell.Clear();
      }
    }
    #endregion

    #region Set image
    private ICommand _setImageCommand = null;
    public ICommand SetImageCommand
    {
      get
      {
        if (_setImageCommand == null)
          _setImageCommand = new RelayCommand(new Action<object>((x) => SetImage(x)), new Func<object, bool>((x) => CanSetImage(x)));
        return _setImageCommand;
      }
    }

    public bool CanSetImage(object param)
    {
      GridCell cell = param as GridCell;
      if (cell?.IsLocked == true || cell?.DisplayContent == false)
        return false;
      return true;
    }

    public void SetImage(object param)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
      openFileDialog.FilterIndex = 2;
      openFileDialog.RestoreDirectory = true;
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        //Get the path of specified file
        string filePath = openFileDialog.FileName;
        BitmapImage bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource = new Uri(filePath);
        bmp.EndInit();
        List<GridCell> cells = GetActiveCells(param as GridCell);
        foreach (GridCell cell in cells)
        {
          cell.Image.Source = bmp;
          cell.Image.Stretch = Stretch.Fill;
          cell.UseImage = true;
        }
      }
    }
    #endregion

    #region Set background
    private ICommand _setBackgroundCommand = null;
    public ICommand SetBackgroundCommand
    {
      get
      {
        if (_setBackgroundCommand == null)
          _setBackgroundCommand = new RelayCommand(new Action<object>((x) => SetBackground(x)), new Func<object, bool>((x) => CanSetBackground(x)));
        return _setBackgroundCommand;
      }
    }

    public bool CanSetBackground(object param)
    {
      GridCell cell = param as GridCell;
      if (cell?.IsLocked == true || cell?.DisplayContent == false)
        return false;
      return true;
    }

    public void SetBackground(object param)
    {
      ColorDialog colorDialog = new ColorDialog();
      if (colorDialog.ShowDialog() == DialogResult.OK)
      {
        List<GridCell> cells = GetActiveCells(param as GridCell);
        Color color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
        foreach (GridCell cell in cells)
        {
          cell.Border.Background = new SolidColorBrush(color);
        }
      }
    }
    #endregion

    #region Split horizontally
    private ICommand _splitHorizontallyCommand = null;
    public ICommand SplitHorizontallyCommand
    {
      get
      {
        if (_splitHorizontallyCommand == null)
          _splitHorizontallyCommand = new RelayCommand(new Action<object>((x) => SplitHorizontally(x)), new Func<object, bool>((x) => CanSplitHorizontally(x)));
        return _splitHorizontallyCommand;
      }
    }

    public bool CanSplitHorizontally(object param)
    {
      return true;
    }

    public void SplitHorizontally(object param)
    {
      try
      {
        int parts = int.Parse(param as string);
        foreach (GridCell cell in SelectedCells)
        {
          SliceHorizontally(cell, parts);
        }
      }
      catch (Exception) { };
    }
    #endregion

    #region Split vertically
    private ICommand _splitVerticallyCommand = null;
    public ICommand SplitVerticallyCommand
    {
      get
      {
        if (_splitVerticallyCommand == null)
          _splitVerticallyCommand = new RelayCommand(new Action<object>((x) => SplitVertically(x)), new Func<object, bool>((x) => CanSplitVertically(x)));
        return _splitVerticallyCommand;
      }
    }

    public bool CanSplitVertically(object param)
    {
      return true;
    }

    public void SplitVertically(object param)
    {
      try
      {
        int parts = int.Parse(param as string);
        foreach(GridCell cell in SelectedCells)
        {
          SliceVertically(cell, parts);
        }
      }
      catch (Exception) { };
    }
    #endregion
    #endregion
  }
}
