using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolving
{
    public class Cell
    {
        private int PrValue { get; set; }
        private List<int> PrPossibleValues { get; set; }
        private int GetUniqueValue => PrPossibleValues.Count == 1 ? PrPossibleValues.FirstOrDefault() : 0;

        public List<int> PossibleValues
        {
            get => PrPossibleValues.OrderBy(x => x).ToList();
            set
            {
                if (value.Count == 0)
                {
                    throw new Exception($"Unexpected error. no possible values in cell {IndexCell}");
                }
                else
                {
                    PrPossibleValues = value;
                    PrValue = GetUniqueValue;
                }
            }
        }

        public string PossibleValuesToCompare => string.Join("", PossibleValues.Select(x => x.ToString()));

        public int Value
        {
            get => PrValue != 0 ? PrValue : GetUniqueValue;
            set { PrValue = value; PossibleValues = new List<int>() { value }; }
        }

        public bool HasValue => Value != 0;

        public int IndexCell { get; set; }
        public int IndexRow { get => (int)Math.Truncate(IndexCell / 9.0M); }
        public int IndexColumn { get => IndexCell - (IndexRow * 9); }
        public int IndexBox
        {
            get
            {
                return IndexCell switch
                {
                    0 or 1 or 2 or 9 or 10 or 11 or 18 or 19 or 20 => 0,
                    3 or 4 or 5 or 12 or 13 or 14 or 21 or 22 or 23 => 1,
                    6 or 7 or 8 or 15 or 16 or 17 or 24 or 25 or 26 => 2,
                    27 or 28 or 29 or 36 or 37 or 38 or 45 or 46 or 47 => 3,
                    30 or 31 or 32 or 39 or 40 or 41 or 48 or 49 or 50 => 4,
                    33 or 34 or 35 or 42 or 43 or 44 or 51 or 52 or 53 => 5,
                    54 or 55 or 56 or 63 or 64 or 65 or 72 or 73 or 74 => 6,
                    57 or 58 or 59 or 66 or 67 or 68 or 75 or 76 or 77 => 7,
                    60 or 61 or 62 or 69 or 70 or 71 or 78 or 79 or 80 => 8,
                    _ => 0,
                };
            }
        }

        public int GetIndex(GroupedBy groupedBy) => groupedBy switch
        {
            GroupedBy.Column => IndexColumn,
            GroupedBy.Row => IndexRow,
            GroupedBy.Box => IndexBox,
            _ => -1,
        };


        public Cell(char value, int indexCell) : this(value.ToString(), indexCell) { }
        public Cell(string value, int indexCell) : this(int.Parse(value), indexCell) { }
        public Cell(int value, int indexCell)
        {
            PrValue = value;
            PrPossibleValues = PrValue != 0 ? new List<int>() { value } : new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            IndexCell = indexCell;
        }

        public Cell(Cell cell) : this(cell.Value, cell.IndexCell) { PossibleValues = cell.PossibleValues; }


        public void AddPossibleValue(int possibleValue) => PrPossibleValues.Add(possibleValue);
        public void AddPossibleValue(IEnumerable<int> possibleValues) => PrPossibleValues.AddRange(possibleValues);
        public void RemovePossibleValue(int possibleValue) => PrPossibleValues.Remove(possibleValue);
        public void RemovePossibleValue(IEnumerable<int> possibleValues) => PrPossibleValues.RemoveAll(x => x.In(possibleValues));
        public void RemovePossibleValue(Predicate<int> match) => PrPossibleValues.RemoveAll(match);

        public bool Equals(Cell otherCell) => IndexCell.Equals(otherCell.IndexCell);

        public override bool Equals(object obj) => Equals((Cell)obj);

        public override int GetHashCode() => base.GetHashCode();

        public bool EqualsPossibleValues(Cell otherCell) => PossibleValues.Count == otherCell.PossibleValues.Count && PossibleValues.SequenceEqual(otherCell.PossibleValues);

        public override string ToString()
        {
            return HasValue ? Value.ToString() : $"[{PossibleValuesToCompare}]";
        }
    }
}
