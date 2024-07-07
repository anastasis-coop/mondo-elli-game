using System.Globalization;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//Brought In from the private Acciaio library.
namespace Acciaio.Editor
{
    public sealed class Csv : IEnumerable<Csv.Column>
    {
#region Builder
        public abstract class CsvBuilder
        {
            private CultureInfo _parsingCulture = CultureInfo.InvariantCulture; 
            private string _separator = DefaultSeparator;
            private string _endOfLine = DefaultEndOfLine;
            private char _escapeCharacter = DefaultEscapeCharacter;
            private bool _firstLineIsHeaders = true;

            protected CsvBuilder() { }

            public CsvBuilder UsingParsingCulture(CultureInfo parsingCulture)
            {
                _parsingCulture = parsingCulture;
                return this;
            }

            public CsvBuilder UsingSeparator(string separator)
            {
                _separator = separator;
                return this;
            }

            public CsvBuilder UsingEndOfLine(string endOfLine)
            {
                _endOfLine = endOfLine;
                return this;
            }

            public CsvBuilder UsingEscapeCharacter(char escapeCharacter)
            {
                _escapeCharacter = escapeCharacter;
                return this;
            }

            public CsvBuilder WithFirstLineAsHeaders(bool firstLineIsHeaders)
            {
                _firstLineIsHeaders = firstLineIsHeaders;
                return this;
            }

            public Csv Empty() =>
                new("", _parsingCulture, _separator, _endOfLine, _escapeCharacter, false);

            public Csv Parse(string content) => 
                new(content, _parsingCulture, _separator, _endOfLine, _escapeCharacter, _firstLineIsHeaders);

            public Csv FromFile(string path) 
            {
                if (path == null) throw new ArgumentNullException("path");
                if (path.Length == 0) throw new ArgumentException("Invalid empty path string");
                return Parse(File.ReadAllText(path));
            }

            public Csv FromStream(Stream stream)
            {
                if (stream == null) throw new ArgumentNullException("stream");
                using StreamReader reader = new(stream);
                return Parse(reader.ReadToEnd());
            }
        }

        private sealed class ConcreteBuilder : CsvBuilder { }
#endregion

#region Cell
        public abstract class Cell
        {
            private readonly IFormatProvider _formatProvider;
            protected string _value;

            public Column Column { get; }
            public int ColumnIndex => Column.Index;
            public abstract int RowIndex { get; }

            public bool IsEmpty => _value == "";

            public string StringValue 
            {
                get => _value;
                set => _value = value ?? "";
            }
            public int IntValue 
            {
                get => int.Parse(_value, _formatProvider);
                set => value.ToString(_formatProvider);
            }
            public long LongValue 
            {
                get => long.Parse(_value, _formatProvider);
                set => value.ToString(_formatProvider);
            }
            public float FloatValue 
            {
                get => float.Parse(_value, _formatProvider);
                set => value.ToString(_formatProvider);
            }
            public double DoubleValue 
            {
                get => double.Parse(_value, _formatProvider);
                set => value.ToString(_formatProvider);
            }

            protected Cell(Column column, IFormatProvider formatProvider, string value) 
            {
                Column = column;
                _formatProvider = formatProvider;
                _value = value;
            }

            public bool TryGetIntValue(out int value) => 
                int.TryParse(_value, NumberStyles.Integer, _formatProvider, out value);
            public bool TryGetLongValue(out long value) => 
                long.TryParse(_value, NumberStyles.Integer, _formatProvider, out value);
            public bool TryGetFloatValue(out float value) => 
                float.TryParse(_value, NumberStyles.Float, _formatProvider, out value);
            public bool TryGetDoubleValue(out double value) => 
                double.TryParse(_value, NumberStyles.Float, _formatProvider, out value);

            public void Clear() => _value = "";

            public override string ToString() => $"Cell('{StringValue}', {RowIndex}, {ColumnIndex})";
        }

        private sealed class ConcreteCell : Cell
        { 
            private int _row;

            public override int RowIndex => _row;

            public ConcreteCell(Column column, IFormatProvider formatProvider, string value, int row) : 
                base(column, formatProvider, value) => _row = row;

            public void SetRow(int row) => _row = row;
        }
#endregion

#region Column
        public abstract class IndexedCellsCollection : IEnumerable<Cell> 
        {
            public abstract int Index { get; }
            public abstract int Count { get; }

            public abstract bool Contains(Cell cell);

            public override string ToString() => $"CellsCollection({Index}, {Count})";

            public abstract IEnumerator<Cell> GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public abstract class Row : IndexedCellsCollection
        {
            private readonly Csv _csv;
            private readonly int _index;

            public override int Index => _index;

            public override int Count => _csv.ColumnsCount;

            public Cell this[int index]
            {
                get
                {
                    if (index < 0 || index >= _csv.ColumnsCount)
                    {
                        string msg = $"index of value {index} is outside the bounds of this row {Index}";
                        throw new IndexOutOfRangeException(msg);
                    }
                    return _csv[Index, index];
                }
            }

            public Cell this[string header] => _csv[Index, header];

            protected Row(Csv csv, int index)
            {
                _csv = csv;
                _index = index;
            }

            public bool HasHeader(string header) => _csv.HasHeader(header);

            public override IEnumerator<Cell> GetEnumerator()
            {
                foreach (var column in _csv)
                    yield return column[Index];
            }

            public override bool Contains(Cell cell) => cell.RowIndex == Index;

            public override string ToString() => $"Row({Index}, {Count})";
        }

        private sealed class ConcreteRow : Row
        {
            public ConcreteRow(Csv csv, int index) : base(csv, index) { }
        }

        public abstract class Column : IndexedCellsCollection
        {
            private string _header;

            protected readonly Csv _csv;
            protected int _index;

            public override int Index => _index;

            public string Header 
            { 
                get => _header; 
                set 
                {
                    if (!string.IsNullOrEmpty(value) && _csv._columns.Any(c => c != this && c.Header == value))
                        throw new ArgumentException($"Another column called {value} already exists");
                    _header = value ?? "";
                }
            }

            public abstract Cell this[int index] { get; }

            protected Column(Csv csv, int index, string header)
            {
                _index = index;
                _csv = csv;
                Header = header;
            }

            public override bool Contains(Cell cell) => cell.ColumnIndex == _index;

            public override string ToString() => $"Column({Index}, '{Header}', {Count})";
        }

        private sealed class ConcreteColumn : Column
        {
            private readonly List<ConcreteCell> _cells = new();

            public override Cell this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                    {
                        string msg = $"index of value {index} is outside the bounds of this column {Index} ({Header})";
                        throw new IndexOutOfRangeException(msg);
                    }
                    return _cells[index];
                }
            }

            public override int Count => _cells.Count;

            public ConcreteColumn(Csv csv, int index, string header) : 
                base(csv, index, header) { }

            public void SetIndex(int index) => _index = index;

            public void Add(string value) => 
                _cells.Add(new ConcreteCell(this, _csv.ParsingCulture, value, Count));

            public void Insert(int index, string value)
            {
                _cells.Insert(index, new ConcreteCell(this, _csv.ParsingCulture, value, index));
                for (int i = index + 1; i < Count; i++)
                    _cells[i].SetRow(_cells[i].RowIndex + 1);
            }

            public void RemoveAtRow(int rowIndex)
            {
                _cells.RemoveAt(rowIndex);
                for (int i = rowIndex; i < Count; i++)
                    _cells[i].SetRow(_cells[i].RowIndex - 1);
            }

            public void Clear() => _cells.Clear();

            public override IEnumerator<Cell> GetEnumerator() => _cells.GetEnumerator();
        }
#endregion

        public const string DefaultSeparator = ",";
        public const string DefaultEndOfLine = "\n";
        public const char DefaultEscapeCharacter = '\"';

        private static void ThrowHeaderException() => throw new InvalidOperationException("Can't access this CSV's columns by headers");

        public static CsvBuilder UsingParsingCulture(CultureInfo parsingCulture) => 
            new ConcreteBuilder().UsingParsingCulture(parsingCulture);

        public static CsvBuilder UsingSeparator(string separator) => 
            new ConcreteBuilder().UsingSeparator(separator);

        public static CsvBuilder UsingEndOfLine(string endOfLine) => 
            new ConcreteBuilder().UsingEndOfLine(endOfLine);
        
        public static CsvBuilder UsingEscapeCharacter(char escapeCharacter) => 
            new ConcreteBuilder().UsingEscapeCharacter(escapeCharacter);

        public static CsvBuilder WithFirstLineAsHeaders(bool firstLineIsHeaders) => 
            new ConcreteBuilder().WithFirstLineAsHeaders(firstLineIsHeaders);

        public static Csv Empty() =>
            new ConcreteBuilder().Empty();

        public static Csv Parse(string content) => 
            new ConcreteBuilder().Parse(content);

        public static Csv FromFile(string path) =>
            new ConcreteBuilder().FromFile(path);

        public static Csv FromStream(Stream stream) =>
            new ConcreteBuilder().FromStream(stream);

        private readonly List<ConcreteColumn> _columns;

        public CultureInfo ParsingCulture { get; } 
        public string Separator { get; }
        public string EndOfLine { get; }
        public char EscapeCharacter { get; }
        public string[] ColumnHeaders => _columns.Select(c => c.Header).ToArray();
        public bool HasHeaders => _columns.Any(c => !string.IsNullOrEmpty(c.Header));

        public int ColumnsCount => _columns.Count;
        public int RowsCount => ColumnsCount == 0 ? 0 : _columns[0].Count;
        public int CellsCount => _columns.Sum(c => c.Count);

        public Column this[int index] 
        {
            get
            {
                if (index < 0 || index >= ColumnsCount)
                    throw new IndexOutOfRangeException("Invalid column index");
                return _columns[index];
            }
        }
        public Column this[string header]
        {
            get
            {
                if (!HasHeaders) ThrowHeaderException();
                if (header == "") throw new ArgumentException("Invalid empty header");

                var column = _columns.Find(c => c.Header.Equals(header, StringComparison.Ordinal));
                if (column == null) throw new ArgumentException($"Unknown column with header {header}");
                return column;
            }
        }

        public Cell this[int row, int column] => this[column][row];
        public Cell this[int row, string header] => this[header][row];

        private Csv(string csvContent, CultureInfo parsingCulture, string separator, 
            string endOfLine, char escapeCharacter, bool firstLineIsHeaders)
        {
            if (separator == null) throw new ArgumentNullException(nameof(separator));
            if (endOfLine == null) throw new ArgumentNullException(nameof(endOfLine));
            if (separator.Length == 0) throw new ArgumentException("Invalid empty separator string");
            if (endOfLine.Length == 0) throw new ArgumentException("Invalid empty endOfLine string");
            if (separator == endOfLine) throw new ArgumentException("Cannot set separator and endOfLine to the same value");
            if (separator.Contains('\"')) throw new ArgumentException("separator cannot contain escaping character \"");
            if (endOfLine.Contains('\"')) throw new ArgumentException("endOfLine cannot contain escaping character \"");
            
            parsingCulture ??= CultureInfo.InvariantCulture;

            ParsingCulture = parsingCulture;
            Separator = separator;
            EndOfLine = endOfLine;
            EscapeCharacter = escapeCharacter;

            _columns = new List<ConcreteColumn>();

            Parse(csvContent, firstLineIsHeaders);
        }

        private void Parse(string content, bool firstLineIsHeaders)
        {
            StringBuilder builder = new();
            var rowIndex = 0;
            var columnIndex = 0;
            var isEscaping = false;
            
            //For each character in content
            for (var i = 0; i < content.Length; i++)
            {
                var element = content[i];
                
                if (element == EscapeCharacter)
                {
                    if (content[i + 1] == EscapeCharacter) builder.Append(content[++i]);
                    else isEscaping = !isEscaping;
                    continue;
                }

                //Append the character
                builder.Append(element);

                var separatorStart = builder.Length - Separator.Length;
                var endOfLineStart = builder.Length - EndOfLine.Length;
                var isSeparator = !isEscaping && builder.Length >= Separator.Length && 
                                  builder.ToString(separatorStart, Separator.Length).Equals(Separator, StringComparison.Ordinal);
                var isEndOfLine = !isEscaping && builder.Length >= EndOfLine.Length &&
                                  builder.ToString(endOfLineStart, EndOfLine.Length).Equals(EndOfLine, StringComparison.Ordinal);

                //If the last characters added build up to separator or endOfLine remove
                //them from the builder and create a new Cell element for its content
                if (!isSeparator && !isEndOfLine && i != content.Length - 1) continue;
                
                if (isSeparator) builder.Remove(separatorStart, Separator.Length);
                if (isEndOfLine) builder.Remove(endOfLineStart, EndOfLine.Length);

                if (rowIndex == 0)
                {
                    var header = firstLineIsHeaders ? builder.ToString() : "";
                    var column = new ConcreteColumn(this, ColumnsCount, header);
                    if (!firstLineIsHeaders) column.Add(builder.ToString());
                    _columns.Add(column);
                } 
                else _columns[columnIndex].Add(builder.ToString());

                if (isSeparator) columnIndex++;
                if (isEndOfLine) 
                {
                    columnIndex = 0;
                    rowIndex++;
                }

                builder.Clear();
            }

            var rowsCountMax = ColumnsCount == 0 ? 0 : _columns.Max(c => c.Count);
            foreach (var column in _columns)
            {
                for (var i = column.Count; i < rowsCountMax; i++)
                    column.Add("");
            }
        }

        public bool HasHeader(string header) => HasHeaders && _columns.Any(c => c.Header.Equals(header, StringComparison.Ordinal));

        public Column CreateColumn() => CreateColumn(ColumnsCount, "");
        
        public Column CreateColumn(int index) => CreateColumn(index, "");
        
        public Column CreateColumn(string header) => CreateColumn(ColumnsCount, header);
        
        public Column CreateColumn(int index, string header)
        {
            if (index < 0 || index > ColumnsCount) 
                throw new IndexOutOfRangeException("index must be positive and lower than or equal to ColumnsCount");

            var column = new ConcreteColumn(this, index, header ?? "");
            for (var i = 0; i < RowsCount; i++)
                column.Add("");
            
            _columns.Insert(index, column);

            if (index >= ColumnsCount - 1) return column;
            
            for(var i = index + 1; i < ColumnsCount; i++)
                _columns[i].SetIndex(_columns[i].Index + 1);
                
            return column;
        }

        public void RemoveColumn(int index)
        {
            if (index < 0 || index >= _columns.Count)
                throw new IndexOutOfRangeException("Invalid column index");
            
            _columns.RemoveAt(index);
            
            for (var i = index; i < ColumnsCount; i++)
                _columns[i].SetIndex(_columns[i].Index - 1);
        }

        public void RemoveColumn(string header) => RemoveColumn(this[header].Index);

        public Row CreateRow() => CreateRow(RowsCount);

        public Row CreateRow(int index)
        {
            if (index < 0 || index > RowsCount) 
                throw new IndexOutOfRangeException("index must be positive and lower than or equal to RowsCount");

            foreach(var column in _columns) column.Insert(index, "");
            return new ConcreteRow(this, index);
        }

        public Row GetRow(int index)
        {
            if (index < 0 || index >= RowsCount) 
                throw new IndexOutOfRangeException("index must be positive and lower than RowsCount");
            return new ConcreteRow(this, index);
        }

        public List<Row> GetRows()
        {
            List<Row> list = new();
            for (var i = 0; i < RowsCount; i++)
                list.Add(new ConcreteRow(this, i));
            return list;
        }

        public void RemoveRow(int index) => _columns.ForEach(c => c.RemoveAtRow(index));

        public void Clear(bool keepHeaders)
        {
            if (!keepHeaders) _columns.Clear();
            else _columns.ForEach(c => c.Clear());
        }

        public string Dump()
        {
            void DumpRow(StringBuilder builder, Func<int, string> getter)
            {
                for (var i = 0; i < ColumnsCount; i++)
                {
                    var value = getter(i);
                    var mustBeEscaped = value.Contains(Separator) || value.Contains(EndOfLine);
                    if (mustBeEscaped) builder.Append('\"');
                    builder.Append(value);
                    if (mustBeEscaped) builder.Append('\"');
                    if (i < ColumnsCount - 1) builder.Append(Separator);
                }
                builder.Append(EndOfLine);
            }

            StringBuilder builder = new();
            if (HasHeaders) DumpRow(builder, i => _columns[i].Header);
            for (var i = 0; i < RowsCount; i++) DumpRow(builder, j => this[j, i].StringValue);

            return builder.ToString();
        }

        public void DumpToFile(string path) => File.WriteAllText(path, Dump());


        public override string ToString() => $"CSV({RowsCount}x{ColumnsCount})";

        public IEnumerator<Column> GetEnumerator() => _columns.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}