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
      TextBlock.FontSize = 12;
      TextBlock.FontWeight = FontWeights.Bold;
      //cell.TextBlock.Foreground = new SolidColorBrush(Color.FromRgb((byte)_rndColor.Next(256), (byte)_rndColor.Next(256), (byte)_rndColor.Next(256)));
      //TextBlock.Text = Row.ToString().PadRight(2) + "," + Column.ToString().PadRight(2);
      TextBlock.Text = Extent.ToString() + "\n" 
        + Row.ToString().PadRight(2) + "," + Column.ToString().PadRight(2) + "," + RowSpan.ToString().PadRight(2) + "," + ColumnSpan.ToString().PadRight(2);
      //System.Diagnostics.Debug.WriteLine("{" + Column.ToString().PadRight(2) + "," + Row.ToString().PadRight(2) + "," + ColumnSpan.ToString().PadRight(2) + "," + RowSpan.ToString().PadRight(2) + "}" +
       //         "{" + Extent.X.ToString("F3") + "," + Extent.Y.ToString("F3") + "," + Extent.Width.ToString("F3") + "," + Extent.Height.ToString("F3") + "}");

      TextBlock.Foreground = Brushes.Black;
      Border.BorderThickness = new Thickness(1);
    }

    public bool IsMerged { get { return _rowSpan > 1 || _columnSpan > 1; } }

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
    private readonly float EdgeSnapEpsilon = 0.00001f;
    private System.Drawing.Rectangle _selectedExtent = new System.Drawing.Rectangle(0, 0, 0, 0);

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

    public IReadOnlyCollection<GridCell> Graticule
    {
      get
      {
        List<GridCell> graticuleCells = new List<GridCell>();
        foreach (GridCell cell in _uniqueGridElements)
        {
          GridCell colCell = new GridCell(0, 0, 1, 1, this);
          colCell.Extent = new System.Drawing.RectangleF(cell.Extent.X, 0, cell.Extent.Width, 1);
          GridCell rowCell = new GridCell(0, 0, 1, 1, this);
          rowCell.Extent = new System.Drawing.RectangleF(0, cell.Extent.Y, 1, cell.Extent.Height);
          graticuleCells.Add(rowCell);
          graticuleCells.Add(colCell);
        }
        return graticuleCells;
      }
    }

    public GridCell EditedCell
    {
      get { return _editedCell; }
      set { _editedCell = value; }
    }

    public IReadOnlyList<GridCell> SelectedCells { get { return  _selectedCells.AsReadOnly(); } }

    public System.Drawing.Rectangle SelectedExtent { get { return _selectedExtent; } }

    #region Public methods
    public void SelectAll()
    {
      _selectedCells.Clear();
      for (int row = 0; row < Rows; row++)
      {
        for (int col = 0; col < Columns; col++)
        {
          GridCell cell = _gridCells[row, col];
          if (cell.IsMerged && (cell.Row != row || cell.Column != col))
            continue;
          cell.IsSelected = true;
          cell.IsOn = true;
          cell.IsEditing = false;
          cell.IsHighlighted = false;
          cell.IsHighlightOverlay = false;
          _selectedCells.Add(cell);
        }
      }
      UpdateSelectionExtent();
      NotifyPropertyChanged(nameof(SelectedCells));
    }

    public void AddToSelection(GridCell cell)
    {
      if (cell == null)
        return;
      cell.IsSelected = true;
      cell.IsHighlighted = true;
      if(!_selectedCells.Contains(cell))
      {
        _selectedCells.Add(cell);
        UpdateSelectionExtent();
        NotifyPropertyChanged(nameof(SelectedCells));
      }
    }

    public void RemoveFromSelection(GridCell cell)
    {
      if (cell == null)
        return;
      cell.IsSelected = false;
      cell.IsHighlighted = false;
      if (_selectedCells.Remove(cell))
      {
        UpdateSelectionExtent();
        NotifyPropertyChanged(nameof(SelectedCells));
      }
    }

    public void ClearSelection()
    {
      _selectedCells.Clear();
      for (int row = 0; row < Rows; row++)
      {
        for (int col = 0; col < Columns; col++)
        {
          GridCell cell = _gridCells[row, col];
          if (cell.IsMerged && (cell.Row != row || cell.Column != col))
            continue;
          cell.IsSelected = true;
          cell.IsOn = true;
          cell.IsEditing = false;
          cell.IsHighlighted = false;
          cell.IsHighlightOverlay = false;
        }
      }
      NotifyPropertyChanged(nameof(SelectedCells));
      UpdateSelectionExtent();
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
          _gridCells[row, col] = cell;
        }
      }
      _uniqueGridElements = GetUniqueElements();

      NotifyPropertyChanged(nameof(Rows));
      NotifyPropertyChanged(nameof(Columns));
      NotifyPropertyChanged(nameof(GridElements));
      NotifyPropertyChanged(nameof(GridCells));
      NotifyPropertyChanged(nameof(Graticule));
    }

    public void Merge(int startRow, int startCol, int rowSpan, int colSpan)
    {
      GridCell group = _gridCells[startRow, startCol];
      float percentWidth = 0;
      float percentHeight = 0;
      bool success = false;
      float[] columnEdges = GetColumnEdgePositions(ref success);
      //failed to determine position at all column edges
      if (!success)
        return;
      float[] rowEdges = GetRowEdgePositions(ref success);
      //failed to determine position at all row edges
      if (!success)
        return;
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
      for (int row = startRow; row < startRow + rowSpan; row++)
      {
        for (int col = startCol; col < startCol + colSpan; col++)
        {
          _gridCells[row, col] = group;
        }
      }

      ClearSelection();
      UpdateCells();
      CondenseRowsAndColumns();
    }

    public void SliceHorizontally(GridCell targetCell, int numofparts)
    {
      if (float.IsNaN(targetCell.PercentWidth) || float.IsNaN(targetCell.PercentHeight)
        || targetCell.PercentWidth == 0 || targetCell.PercentHeight == 0)
        return;

      float fraction = 1.0f / numofparts;
      List<GridCell> splitCells = new List<GridCell>();
      bool success = false;
      float[] columnEdges = GetColumnEdgePositions(ref success);
      //failed to determine position at all row edges
      if (!success)
      {
        return;
      }
      int startEdge = targetCell.Column;
      int endEdge = targetCell.Column + targetCell.ColumnSpan;
      float columnWidth = columnEdges[endEdge] - columnEdges[startEdge];
      int columnOffset = 0;
      int newColumns = numofparts - 1;
      bool[] snappedEdges = new bool[columnEdges.Length];
      bool[] snappedSlices = new bool[numofparts - 1];
      for (int i = 0; i < numofparts - 1; i++)
      {
        snappedSlices[i] = false;
      }

      for (int i = 0; i < columnEdges.Length; i++)
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
          float slicePosition = columnEdges[startEdge] + columnWidth * (fraction * (j + 1));
          if (Math.Abs(slicePosition - columnEdges[i]) <= EdgeSnapEpsilon)
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

      List<GridCell> uniqueCells = GetUniqueElements();
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
          int[] incrementCounter = null;
          if (!incrementCounters.ContainsKey(cell))
          {
            incrementCounter = new int[2] { 0, 0 };
            incrementCounters[cell] = incrementCounter;
          }
          else
          {
            incrementCounter = incrementCounters[cell];
          }

          if (slicePosition < columnEdges[cell.Column])
          {
            //increase the column count by 1;
            incrementCounter[0]++;
          }
          else if (slicePosition < columnEdges[cell.Column + cell.ColumnSpan])
          {
            //increase the column span by 1;
            incrementCounter[1]++;
          }
        }
      }

      foreach (KeyValuePair<GridCell, int[]> pair in incrementCounters)
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
      UpdateCells();
      CondenseRowsAndColumns();
    }

    public void SliceVertically(GridCell targetCell, int numofparts)
    {
      if (float.IsNaN(targetCell.PercentHeight) || float.IsNaN(targetCell.PercentHeight)
        || targetCell.PercentHeight == 0 || targetCell.PercentHeight == 0)
        return;

      float fraction = 1.0f / numofparts;
      List<GridCell> splitCells = new List<GridCell>();

      bool success = false;
      float[] rowEdges = GetRowEdgePositions(ref success);
      //failed to determine position at all row edges
      if (!success)
        return;
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

      List<GridCell> uniqueCells = GetUniqueElements();
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
          int[] incrementCounter = null;
          if (!incrementCounters.ContainsKey(cell))
          {
            incrementCounter = new int[2] { 0, 0 };
            incrementCounters[cell] = incrementCounter;
          }
          else
          {
            incrementCounter = incrementCounters[cell];
          }

          if (slicePosition < rowEdges[cell.Row])
          {
            //increase the row count by 1;
            incrementCounter[0]++;
          }
          else if (slicePosition < rowEdges[cell.Row + cell.RowSpan])
          {
            //increse the row span by 1;
            incrementCounter[1]++;
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
      UpdateCells();
      CondenseRowsAndColumns();
    }

    /// <summary>
    /// Edge positions on a scale of 0 to 1 from top to bottom.
    /// </summary>
    /// <returns></returns>
    public float[] GetRowEdgePositions(ref bool success)
    {
      List<GridCell>[] cellsOrderedByRow = GetCellsOrderedByRow();

      float[] rowHeights = new float[Rows];
      float[] rowEdges = new float[Rows + 1];
      rowEdges[0] = 0;
      for (int row = 0; row < Rows; row++)
      {
        rowEdges[row + 1] = float.NaN;
        rowHeights[row] = float.NaN;
      }

      //Pass 1:
      //(1) For a single-row element, the row height is determined by the element height;
      //(2) For a contiguous group of rows in which all elements are of equal row span, the row of each row is determined by evenly distributing the element height over all the rows;
      //(3) For a contiguous group of rows in which the rows of one element are shared by all the other elements, the heights of these overlapping rows are determined by dividing the height by the row span of that element;
      //(4) The top/bottom edge of a element determines the position of the corresponding row edge, e.g., the edge position of row 0 is the same as the top edge of an element whose starts at row 0.
      for (int row = 0; row < Rows; row++)
      {
        if (!float.IsNaN(rowHeights[row]))
          continue;

        List<GridCell> intersectingCells = cellsOrderedByRow[row];
        int topEdge = row;
        int bottomEdge = row + 1;
        if (intersectingCells == null || intersectingCells.Count == 0)
        {
          rowHeights[row] = 0;
          rowEdges[bottomEdge] = rowEdges[topEdge];
          continue;
        }

        bool isUniformSpan = true;
        int minSpanRowId = 0;
        float rowSize = float.NaN;
        int id = -1;
        //System.Drawing.Rectangle overlappedExtent = intersectingCells[0].CellExtent;
        //overlappedExtent = new System.Drawing.Rectangle(overlappedExtent.X, 0, overlappedExtent.Height, 1);
        foreach (GridCell cell in intersectingCells)
        {
          id++;
          rowEdges[cell.Row] = cell.Extent.Top;
          rowEdges[cell.Row + cell.RowSpan] = cell.Extent.Bottom;
          if (isUniformSpan && (cell.Row != intersectingCells[0].Row || cell.RowSpan != intersectingCells[0].RowSpan))
            isUniformSpan = false;
          //overlappedExtent.Intersect(new System.Drawing.Rectangle(cell.CellExtent.X, 0, cell.CellExtent.Height, 1));
          if (cell.RowSpan == 1)
          {
            rowSize = cell.PercentHeight;
            break;
          }
          if (cell.RowSpan < intersectingCells[minSpanRowId].RowSpan)
          {
            minSpanRowId = id;
          }
        }

        rowHeights[row] = rowSize;
        if (float.IsNaN(rowSize))
        {
          if (isUniformSpan)
          {
            rowHeights[row] = intersectingCells[0].PercentHeight / intersectingCells[0].RowSpan;
          }
          else
          {
            bool hasOverlappingExtent = true;
            GridCell minSpanCell = intersectingCells[minSpanRowId];
            System.Drawing.Rectangle minSpanCellExtent = new System.Drawing.Rectangle(0, minSpanCell.Row, 1, minSpanCell.RowSpan);
            foreach (GridCell cell in intersectingCells)
            {
              if (!(new System.Drawing.Rectangle(0, cell.Row, 1, cell.RowSpan)).Contains(minSpanCellExtent))
              {
                hasOverlappingExtent = false;
                break;
              }
            }

            if (hasOverlappingExtent)
            {
              rowSize = minSpanCell.PercentHeight / minSpanCell.RowSpan;
              rowHeights[row] = rowSize;
              for (int rowToSet = minSpanCell.Row; rowToSet < minSpanCell.Row + minSpanCell.RowSpan; rowToSet++)
              {
                rowHeights[rowToSet] = rowSize;
              }
            }
          }
        }
      }

      //Pass 2:
      //(1) For a multi-row element, if the row heights of all but one row are knowned, the unknown row height is determined by subtracting the summed height of the knowned rows from the element height;
      for (int row = 0; row < Rows; row++)
      {
        if (!float.IsNaN(rowHeights[row]))
        {
          continue;
        }
        List<GridCell> intersectingCells = cellsOrderedByRow[row];
        foreach (GridCell cell in intersectingCells)
        {
          if (cell.RowSpan == 1 && !float.IsNaN(rowHeights[cell.Row]))
          {
            rowHeights[row] = cell.PercentHeight;
            break;
          }
          float rowSize = cell.PercentHeight;
          bool isSet = true;
          for (int rowToGet = cell.Row; rowToGet < cell.Row + cell.RowSpan; rowToGet++)
          {
            if (rowToGet == row)
              continue;
            if (float.IsNaN(rowHeights[rowToGet]))
            {
              isSet = false;
              break;
            }
            rowSize -= rowHeights[rowToGet];
          }
          if (isSet)
            rowHeights[row] = rowSize;
        }
      }

      //Pass 3:
      //(1) First pass to determine positions at edges where both the positions of the top neighboring edge and the row height are known.
      for (int row = 0; row < Rows; row++)
      {
        int topEdge = row;
        int bottomEdge = row + 1;
        if (float.IsNaN(rowEdges[topEdge]) || float.IsNaN(rowHeights[row]))
          continue;
        rowEdges[bottomEdge] = rowEdges[topEdge] + rowHeights[row];
      }

      //Pass 4:
      //(1) For a contiguous group of rows shared by all overlapping elements across the rows, the total height of these rows are is equal to the difference of the bottom edge and top edge positions (if both are known), and the total height is evenly distributed over the rows to determine the row height;
      for (int row = 0; row < Rows; row++)
      {
        if (!float.IsNaN(rowHeights[row]))
          continue;
        if (float.IsNaN(rowEdges[row]))
          continue;
        int startRow = row;
        int endRow = row;
        while (endRow < Rows - 1)
        {
          if (!float.IsNaN(rowHeights[endRow]))
          {
            endRow--;
            break;
          }
          endRow++;
        }
        int topEdge = row;
        int bottomEdge = endRow + 1;
        while (bottomEdge > topEdge)
        {
          if (float.IsNaN(rowEdges[bottomEdge]))
            bottomEdge--;
          else
            break;
        }
        if (float.IsNaN(rowEdges[topEdge]) || float.IsNaN(rowEdges[bottomEdge]))
          continue;
        float rowSize = (rowEdges[bottomEdge] - rowEdges[topEdge]) / (endRow - startRow + 1);
        while (startRow <= endRow)
        {
          rowHeights[startRow] = rowSize;
          startRow++;
        }
        for (int rowToSet = 0; rowToSet < Rows; rowToSet++)
        {
          if (!float.IsNaN(rowEdges[rowToSet + 1]))
            continue;
          if (float.IsNaN(rowEdges[rowToSet]) || float.IsNaN(rowHeights[rowToSet]))
            continue;
          rowEdges[rowToSet + 1] = rowEdges[rowToSet] + rowHeights[rowToSet];
        }
      }

      //Pass 5:
      //(1) Second pass to determine positions at unkown edges where both the positions of the top neighboring edge and the row height are known.
      for (int row = 0; row < Rows; row++)
      {
        if (!float.IsNaN(rowEdges[row + 1]))
          continue;
        int topEdge = row;
        int bottomEdge = row + 1;
        if (float.IsNaN(rowEdges[topEdge]) || float.IsNaN(rowHeights[row]))
          continue;
        rowEdges[bottomEdge] = rowEdges[topEdge] + rowHeights[row];
      }

      //Return true only if all edges are known.
      success = true;
      for (int row = 0; row < Rows; row++)
      {
        if (float.IsNaN(rowEdges[row + 1]) || float.IsNaN(rowHeights[row]))
          success = false;
      }

      return rowEdges;
    }

    /// <summary>
    /// Determin edge positions on a scale of 0 to 1 from left to right.
    /// </summary>
    /// <returns></returns>
    public float[] GetColumnEdgePositions(ref bool success)
    {
      List<GridCell>[] cellsOrderedByColumn = GetCellsOrderedByColumn();

      float[] columnWidths = new float[Columns];
      float[] columnEdges = new float[Columns + 1];
      columnEdges[0] = 0;
      for (int col = 0; col < Columns; col++)
      {
        columnEdges[col + 1] = float.NaN;
        columnWidths[col] = float.NaN;
      }

      //Pass 1:
      //(1) For a single-column element, the column width is determined by the element width;
      //(2) For a contiguous group of columns in which all elements are of equal column span, the width of each column is determined by evenly distributing the element width over all the columns;
      //(3) For a contiguous group of columns in which the columns of one element are shared by all the other elements, the widths of these overlapping columns are determined by dividing the width by the column span of that element;
      //(4) The left/right edge of a element determines the position of the corresponding column edge, e.g., the edge position of column 0 is the same as the left edge of an element whose starts at column 0.
      for (int col = 0; col < Columns; col++)
      {
        if (!float.IsNaN(columnWidths[col]))
          continue;

        List<GridCell> intersectingCells = cellsOrderedByColumn[col];
        int leftEdge = col;
        int rightEdge = col + 1;
        if (intersectingCells == null || intersectingCells.Count == 0)
        {
          columnWidths[col] = 0;
          columnEdges[rightEdge] = columnEdges[leftEdge];
          continue;
        }

        bool isUniformSpan = true;
        int minSpanColId = 0;
        float colSize = float.NaN;
        int id = -1;
        //System.Drawing.Rectangle overlappedExtent = intersectingCells[0].CellExtent;
        //overlappedExtent = new System.Drawing.Rectangle(overlappedExtent.X, 0, overlappedExtent.Width, 1);
        foreach (GridCell cell in intersectingCells)
        {
          id++;
          columnEdges[cell.Column] = cell.Extent.Left;
          columnEdges[cell.Column + cell.ColumnSpan] = cell.Extent.Right;
          if (isUniformSpan && (cell.Column != intersectingCells[0].Column || cell.ColumnSpan != intersectingCells[0].ColumnSpan))
            isUniformSpan = false;
          //overlappedExtent.Intersect(new System.Drawing.Rectangle(cell.CellExtent.X, 0, cell.CellExtent.Width, 1));
          if (cell.ColumnSpan == 1)
          {
            colSize = cell.PercentWidth;
            break;
          }
          if (cell.ColumnSpan < intersectingCells[minSpanColId].ColumnSpan)
          {
            minSpanColId = id;
          }
        }

        columnWidths[col] = colSize;
        if (float.IsNaN(colSize))
        {
          if (isUniformSpan)
          {
            columnWidths[col] = intersectingCells[0].PercentWidth / intersectingCells[0].ColumnSpan;
          }
          else
          {
            bool hasOverlappingExtent = true;
            GridCell minSpanCell = intersectingCells[minSpanColId];
            System.Drawing.Rectangle minSpanCellExtent = new System.Drawing.Rectangle(minSpanCell.Column, 0, minSpanCell.ColumnSpan, 1);
            foreach (GridCell cell in intersectingCells)
            {
              if (!(new System.Drawing.Rectangle(cell.Column, 0, cell.ColumnSpan, 1)).Contains(minSpanCellExtent))
              {
                hasOverlappingExtent = false;
                break;
              }
            }

            if (hasOverlappingExtent)
            {
              colSize = minSpanCell.PercentWidth / minSpanCell.ColumnSpan;
              columnWidths[col] = colSize;
              for (int colToSet = minSpanCell.Column; colToSet < minSpanCell.Column + minSpanCell.ColumnSpan; colToSet++)
              {
                columnWidths[colToSet] = colSize;
              }
            }
          }
        }
      }

      //Pass 2:
      //(1) For a multi-column element, if the column widths of all but one column are knowned, the unknown column width is determined by subtracting the summed width of the knowned columns from the element width;
      for (int col = 0; col < Columns; col++)
      {
        if (!float.IsNaN(columnWidths[col]))
        {
          continue;
        }
        List<GridCell> intersectingCells = cellsOrderedByColumn[col];
        foreach (GridCell cell in intersectingCells)
        {
          if (cell.ColumnSpan == 1 && !float.IsNaN(columnWidths[cell.Column]))
          {
            columnWidths[col] = cell.PercentWidth;
            break;
          }
          float colSize = cell.PercentWidth;
          bool isSet = true;
          for (int colToGet = cell.Column; colToGet < cell.Column + cell.ColumnSpan; colToGet++)
          {
            if (colToGet == col)
              continue;
            if (float.IsNaN(columnWidths[colToGet]))
            {
              isSet = false;
              break;
            }
            colSize -= columnWidths[colToGet];
          }
          if (isSet)
            columnWidths[col] = colSize;
        }
      }

      //Pass 3:
      //(1) First pass to determine positions at edges where both the positions of the left neighboring edge and the column width are known.
      for (int col = 0; col < Columns; col++)
      {
        int leftEdge = col;
        int rightEdge = col + 1;
        if (float.IsNaN(columnEdges[leftEdge]) || float.IsNaN(columnWidths[col]))
          continue;
        columnEdges[rightEdge] = columnEdges[leftEdge] + columnWidths[col];
      }

      //Pass 4:
      //(1) For a contiguous group of columns shared by all overlapping elements across the rows, the total width of these columns are is equal to the difference of the right edge and left edge positions (if both are known), and the total width is evenly distributed over the columns to determine the column width;
      for (int col = 0; col < Columns; col++)
      {
        if (!float.IsNaN(columnWidths[col]))
          continue;
        if (float.IsNaN(columnEdges[col]))
          continue;
        int startCol = col;
        int endCol = col;
        while (endCol < Columns - 1)
        {
          if (!float.IsNaN(columnWidths[endCol]))
          {
            endCol--;
            break;
          }
          endCol++;
        }
        int leftEdge = col;
        int rightEdge = endCol + 1;
        while (rightEdge > leftEdge)
        {
          if (float.IsNaN(columnEdges[rightEdge]))
            rightEdge--;
          else
            break;
        }
        if (float.IsNaN(columnEdges[leftEdge]) || float.IsNaN(columnEdges[rightEdge]))
          continue;
        float colSize = (columnEdges[rightEdge] - columnEdges[leftEdge]) / (endCol - startCol + 1);
        while (startCol <= endCol)
        {
          columnWidths[startCol] = colSize;
          startCol++;
        }
        for (int colToSet = 0; colToSet < Columns; colToSet++)
        {
          if (!float.IsNaN(columnEdges[colToSet + 1]))
            continue;
          if (float.IsNaN(columnEdges[colToSet]) || float.IsNaN(columnWidths[colToSet]))
            continue;
          columnEdges[colToSet + 1] = columnEdges[colToSet] + columnWidths[colToSet];
        }
      }

      //Pass 5:
      //(1) Second pass to determine positions at unkown edges where both the positions of the left neighboring edge and the column width are known.
      for (int col = 0; col < Columns; col++)
      {
        if (!float.IsNaN(columnEdges[col + 1]))
          continue;
        int leftEdge = col;
        int rightEdge = col + 1;
        if (float.IsNaN(columnEdges[leftEdge]) || float.IsNaN(columnWidths[col]))
          continue;
        columnEdges[rightEdge] = columnEdges[leftEdge] + columnWidths[col];
      }

      //Return true only if all edges are known.
      success = true;
      for (int col = 0; col < Columns; col++)
      {
        if (float.IsNaN(columnEdges[col + 1]) || float.IsNaN(columnWidths[col]))
          success = false;
      }

      return columnEdges;
    }

    public void Print()
    {
      string divs = "";
      int index = 0;
      foreach(GridCell cell in _uniqueGridElements)
      {
        //System.Diagnostics.Debug.WriteLine("{" + Column.ToString().PadRight(2) + "," + Row.ToString().PadRight(2) + "," + ColumnSpan.ToString().PadRight(2) + "," + RowSpan.ToString().PadRight(2) + "}" +
        //         "{" + Extent.X.ToString("F3") + "," + Extent.Y.ToString("F3") + "," + Extent.Width.ToString("F3") + "," + Extent.Height.ToString("F3") + "}");

        //< div id = "element_1" cell = "0 ,0 ,2 ,1" extent = "0.000,0.000,0.667,0.333" style = "overflow-x:auto;border-style:solid;" >

        //   < img id = "News" src = "" >

        //      </ img >

        //    </ div >
        Color bkColor = (cell.Border.Background as SolidColorBrush).Color;
        string div = "<div ";
        //div += ("element_" + cell.Column.ToString() + "_" + cell.Row.ToString() + "_" + cell.ColumnSpan.ToString() + "_" + cell.RowSpan.ToString() + " ");
        div += ("id=" + "\"" + "element_" + index.ToString() + "\"" + " ");
        div += ("cell=" + "\"" + cell.Column.ToString() + "," + cell.Row.ToString() + "," + cell.ColumnSpan.ToString() + "," + cell.RowSpan.ToString() + "\"" + " ");
        div += ("extent=" + "\"" + cell.Extent.X.ToString() + "," + cell.Extent.Y.ToString() + "," + cell.Extent.Width.ToString() + "," + cell.Extent.Height.ToString() + "\"" + " ");
        //div += ("style=" + "\"" + "overflow-x:auto;border-style:solid;" + "width=" + cell.PercentWidth.ToString() + "%;" + "background-color:rgb(" + bkColor.R.ToString() + "," + bkColor.G.ToString() + "," + bkColor.B.ToString() + ");" + "\"" + ">" + "\n");
        div += ("style=" + "\"" + "overflow-x:auto;overflow-y:hidden;border-style:solid;" + "background-color:rgb("+ bkColor.R.ToString() + ","+ bkColor.G.ToString() + "," + bkColor.B.ToString() + ");" + "\"" + ">" + "\n");
        div += ("<p>" + cell.TextBlock.Text + "</p>" + "\n");
        div += ("</div>" + "\n");

        divs += div;
        index++;
      }
      System.Diagnostics.Debug.WriteLine("");
      System.Diagnostics.Debug.Write(divs);
      bool success = false;
      float[] rowEdges = GetRowEdgePositions(ref success);
      string rowEdgePositions = "\"";
      for (int i = 0; i < rowEdges.Length; i++)
      {
        rowEdgePositions += rowEdges[i].ToString();
        if (i != rowEdges.Length - 1)
          rowEdgePositions += ",";
      }
      rowEdgePositions += "n\"";
      System.Diagnostics.Debug.Write(rowEdgePositions);
    }
    #endregion

    #region Private methods
    private void UpdateCells()
    {
      _uniqueGridElements = GetUniqueElements();
      NotifyPropertyChanged(nameof(Rows));
      NotifyPropertyChanged(nameof(Columns));
      NotifyPropertyChanged(nameof(GridElements));
      NotifyPropertyChanged(nameof(GridCells));
      NotifyPropertyChanged(nameof(Graticule));
      Print();
    }

    private void UpdateSelectionExtent()
    {
      if (_selectedCells?.Count > 0)
      {
        System.Drawing.Rectangle extent = _selectedCells[0].CellExtent;
        for (int i = 1; i < _selectedCells.Count; i++)
        {
          extent = System.Drawing.Rectangle.Union(extent, _selectedCells[i].CellExtent);
        }
        _selectedExtent = extent;
      }
      else
      {
        _selectedExtent = new System.Drawing.Rectangle(0, 0, 0, 0);
      }
      NotifyPropertyChanged(nameof(SelectedExtent));
    }

    /// <summary>
    /// Combine shared rows/columns into a single row/column
    /// </summary>
    private bool ReclaimRowsAndColumns()
    {
      Tuple<int, int> colsToShrink = null;
      Tuple<int, int> rowsToShrink = null;
      List<GridCell>[] cellsOrderedByColumn = GetCellsOrderedByColumn();
      List<GridCell>[] cellsOrderedByRow = GetCellsOrderedByRow();
      List<GridCell> intersectingCells = null;
      for (int i = 0; i < cellsOrderedByColumn.Length; i++)
      {
        intersectingCells = cellsOrderedByColumn[i];
        if (intersectingCells == null || intersectingCells.Count == 0)
          continue;
        System.Drawing.Rectangle overlappedExtent = intersectingCells[0].CellExtent;
        overlappedExtent = new System.Drawing.Rectangle(overlappedExtent.X, 0, overlappedExtent.Width, 1);
        for (int j = 0; j < intersectingCells.Count; j++)
        {
          overlappedExtent.Intersect(new System.Drawing.Rectangle(intersectingCells[j].CellExtent.X, 0, intersectingCells[j].CellExtent.Width, 1));
        }
        if (overlappedExtent.Width > 1)
        {
          colsToShrink = new Tuple<int, int>(overlappedExtent.X, overlappedExtent.Width);
          break;
        }
      }

      if (colsToShrink != null && colsToShrink.Item2 > 1)
      {
        intersectingCells = cellsOrderedByColumn[colsToShrink.Item1];
        int spanToReduce = colsToShrink.Item2 - 1;
        int newCols = Columns - spanToReduce;
        GridCell[,] newGridCells = new GridCell[Rows, newCols];
        foreach (GridCell cell in intersectingCells)
        {
          cell.ColumnSpan -= spanToReduce;
        }
        foreach (GridCell cell in _uniqueGridElements)
        {
          if (cell.Column != colsToShrink.Item1 && cell.Column >= (colsToShrink.Item1 + colsToShrink.Item2))
            cell.Column = cell.Column - spanToReduce;
          SetCell(newGridCells, cell);
        }
        _gridCells = newGridCells;
        UpdateCells();
        return true;
      }

      for (int i = 0; i < cellsOrderedByRow.Length; i++)
      {
        intersectingCells = cellsOrderedByRow[i];
        if (intersectingCells == null || intersectingCells.Count == 0)
          continue;
        System.Drawing.Rectangle overlappedExtent = intersectingCells[0].CellExtent;
        overlappedExtent = new System.Drawing.Rectangle(0, overlappedExtent.Y, 1, overlappedExtent.Height);
        for (int j = 0; j < intersectingCells.Count; j++)
        {
          overlappedExtent.Intersect(new System.Drawing.Rectangle(0, intersectingCells[j].CellExtent.Y, 1, intersectingCells[j].CellExtent.Height));
        }
        if (overlappedExtent.Height > 1)
        {
          rowsToShrink = new Tuple<int, int>(overlappedExtent.Y, overlappedExtent.Height);
          break;
        }
      }

      if (rowsToShrink != null && rowsToShrink.Item2 > 1)
      {
        intersectingCells = cellsOrderedByRow[rowsToShrink.Item1];
        int spanToReduce = rowsToShrink.Item2 - 1;
        int newRows = Rows - spanToReduce;
        GridCell[,] newGridCells = new GridCell[newRows, Columns];
        foreach (GridCell cell in intersectingCells)
        {
          cell.RowSpan -= spanToReduce;
        }
        foreach (GridCell cell in _uniqueGridElements)
        {
          if (cell.Row != rowsToShrink.Item1 && cell.Row >= (rowsToShrink.Item1 + rowsToShrink.Item2))
            cell.Row = cell.Row - spanToReduce;
          SetCell(newGridCells, cell);
        }
        _gridCells = newGridCells;
        UpdateCells();
      }

      return false;
    }

    /// <summary>
    /// Combine shared rows/columns into a single row/column
    /// </summary>
    private void CondenseRowsAndColumns()
    {
      while (true)
      {
        if (!ReclaimRowsAndColumns())
          break;
      }
    }

    private List<GridCell>[] GetCellsOrderedByRow()
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
            if (cell.IsMerged && (cell.Row != row || cell.Column != col))
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
      return cellsOrderedByRow;
    }

    private List<GridCell>[] GetCellsOrderedByColumn()
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
            if (cell.IsMerged && (cell.Row != row || cell.Column != col))
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
      return cellsOrderedByColumn;
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

    private List<GridCell> GetUniqueElements()
    {
      List<GridCell> cells = new List<GridCell>();
      HashSet<GridCell> uniqueSet = new HashSet<GridCell>();
      foreach (GridCell cell in _gridCells)
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
    #endregion

    #region Commands

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
      Merge(SelectedExtent.Y, SelectedExtent.X, SelectedExtent.Height, SelectedExtent.Width);
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
      foreach (GridCell cell in SelectedCells)
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
        foreach (GridCell cell in SelectedCells)
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
        Color color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
        foreach (GridCell cell in SelectedCells)
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
        ClearSelection();
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
        ClearSelection();
      }
      catch (Exception) { };
    }
    #endregion
    #endregion
  }
}
