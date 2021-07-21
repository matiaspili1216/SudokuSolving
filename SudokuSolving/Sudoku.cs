using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolving
{
    public class Sudoku
    {
        private List<Cell> CellsInSudoku;
        private int CellsSolved => CellsInSudoku.Count(x => x.HasValue);

        private int PosibleValuesPending => CellsInSudoku.Where(x => !x.HasValue).Select(z => z.PossibleValues.Count).Count();

        public Sudoku(string sudoku)
        {
            char[] charSudo = sudoku.ToCharArray();
            int index = 0;

            CellsInSudoku = charSudo.Select(x =>
            {
                var value = new Cell(x, index);
                index++;
                return value;
            }).ToList();
        }

        public Sudoku(Sudoku sudoku) => CellsInSudoku = sudoku.CellsInSudoku.Select(x => new Cell(x)).ToList();

        public void Solve()
        {
            int cellSolvedBefore;
            int cellSolvedAfter;

            do
            {
                cellSolvedBefore = CellsSolved;

                if (cellSolvedBefore < 17) { throw new Exception("Solve a Sudoku with a number of clues minor of 17 is not possible.\nMore info: (arXiv:1201.0749v2 [cs.DS] 1 Sep 2013) https://arxiv.org/abs/1201.0749"); }

                SetPossibleValues();

                if (CellsSolved == 81) break;

                DeletePossibleValues();

                cellSolvedAfter = CellsSolved;
                if (cellSolvedAfter == 81) break;

            } while (cellSolvedBefore != cellSolvedAfter);
        }

        public void SolveByBruteForce()
        {
            Sudoku currentSudoku = new(this);
            Sudoku tempSudoku;

            foreach (var cellWithoutValue in CellsInSudoku.Where(x => !x.HasValue))
            {
                tempSudoku = new(currentSudoku);

                foreach (var value in cellWithoutValue.PossibleValues)
                {
                    CellsInSudoku.ElementAt(cellWithoutValue.IndexCell).Value = value;

                    try { Solve(); }
                    catch (Exception) { CellsInSudoku = tempSudoku.CellsInSudoku; }

                    if (IsSolved()) { return; }
                    else { CellsInSudoku = tempSudoku.CellsInSudoku; }
                }
            }
        }

        public bool IsComplete() => CellsInSudoku.All(x => x.HasValue);

        public bool IsSolved()
        {
            var listOK = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            return IsComplete()
                && CellsInSudoku.GroupBy(cell => cell.IndexBox).All(group => group.Select(cellGrouped => cellGrouped.Value).OrderBy(value => value).SequenceEqual(listOK))
                && CellsInSudoku.GroupBy(cell => cell.IndexColumn).All(group => group.Select(cellGrouped => cellGrouped.Value).OrderBy(value => value).SequenceEqual(listOK))
                && CellsInSudoku.GroupBy(cell => cell.IndexRow).All(group => group.Select(cellGrouped => cellGrouped.Value).OrderBy(value => value).SequenceEqual(listOK))
                ;
        }

        /// <summary>
        /// Set in Sudo
        /// </summary>
        private void SetPossibleValues()
        {
            int cellSolvedBefore;
            int cellSolvedAfter;

            int posibleValuesPendingBefore;
            int posibleValuesPendingAfter;

            do
            {
                cellSolvedBefore = CellsSolved;
                posibleValuesPendingBefore = PosibleValuesPending;

                SetPossibleValuesByCells();
                SetPossibleValuesByValues();

                cellSolvedAfter = CellsSolved;
                if (cellSolvedAfter == 81) break;

                posibleValuesPendingAfter = PosibleValuesPending;
            } while (cellSolvedBefore != cellSolvedAfter || posibleValuesPendingBefore != posibleValuesPendingAfter);

        }

        private void DeletePossibleValues()
        {
            DeletePossibleValuesByLockedCandidateInBox();
            DeletePossibleValuesByLockedSet();
            DeletePossibleValuesByLockedValuesCandidates();
        }

        private void SetPossibleValuesByCells()
        {
            foreach (var cell in CellsInSudoku.Where(x => !x.HasValue))
            {
                if (cell.HasValue) { cell.PossibleValues = new List<int>() { cell.Value }; }
                else
                {
                    var possibleValues = cell.PossibleValues;
                    possibleValues.RemoveAll(x => GetValuesInLinkedCells(cell).Contains(x));
                    cell.PossibleValues = possibleValues;
                }
            }
        }

        private void SetPossibleValuesByValues()
        {
            for (int value = 1; value < 10; value++)
            {
                for (int boxIndex = 0; boxIndex < 9; boxIndex++)
                {
                    if (!ExistValueInGroup(value, GroupedBy.Box, boxIndex)) SetValueIfHasUniquePossibleCells(value, GetPossibleCells(value, GroupedBy.Box, boxIndex));
                }

                for (int rowIndex = 0; rowIndex < 9; rowIndex++)
                {
                    if (!ExistValueInGroup(value, GroupedBy.Row, rowIndex)) SetValueIfHasUniquePossibleCells(value, GetPossibleCells(value, GroupedBy.Row, rowIndex));
                }

                for (int columnIndex = 0; columnIndex < 9; columnIndex++)
                {
                    if (!ExistValueInGroup(value, GroupedBy.Column, columnIndex)) SetValueIfHasUniquePossibleCells(value, GetPossibleCells(value, GroupedBy.Column, columnIndex));
                }
            }
        }

        /// <summary>
        /// If a possible Value in a Box has all possible Cells with the same Row or Column, this value is not possible in the other cell in same Row or Column 
        /// </summary>
        private void DeletePossibleValuesByLockedCandidateInBox()
        {
            int cellSolvedBefore;
            int cellSolvedAfter;

            do
            {
                cellSolvedBefore = CellsSolved;

                for (int value = 1; value < 10; value++)
                {
                    for (int boxIndex = 0; boxIndex < 9; boxIndex++)
                    {
                        List<Cell> possibleCells = GetPossibleCells(value, GroupedBy.Box, boxIndex);

                        if (possibleCells == null || !possibleCells.Any() || possibleCells.Count == 1) continue;
                        else
                        {
                            if (possibleCells.Select(x => x.IndexRow).AllElementAreEquals(out int rowEqual))
                            {
                                RemovePossibleValues(value, GroupedBy.Row, rowEqual, possibleCells.Select(x => x.IndexCell).ToArray());
                            }

                            if (possibleCells.Select(x => x.IndexColumn).AllElementAreEquals(out int columnEqual))
                            {
                                RemovePossibleValues(value, GroupedBy.Column, columnEqual, possibleCells.Select(x => x.IndexCell).ToArray());
                            }
                        }
                    }
                }

                cellSolvedAfter = CellsSolved;
                if (cellSolvedAfter == 81) break;

            } while (cellSolvedBefore != cellSolvedAfter);
        }

        /// <summary>
        /// If [n] Cells in a Box/Column/Row have the same [n] possible Values, this values are not possible in the other cells in same Row or Column or Box
        /// </summary>
        private void DeletePossibleValuesByLockedSet()
        {
            int cellSolvedBefore;
            int cellSolvedAfter;

            do
            {
                cellSolvedBefore = CellsSolved;

                DeletePossibleValuesByLockedSet(GroupedBy.Box);
                DeletePossibleValuesByLockedSet(GroupedBy.Row);
                DeletePossibleValuesByLockedSet(GroupedBy.Column);

                cellSolvedAfter = CellsSolved;
                if (cellSolvedAfter == 81) break;

            } while (cellSolvedBefore != cellSolvedAfter);
        }

        /// <summary>
        /// If [n] Cells in a "GroupedBy" have the same [n] possible Values, this values are not possible in the other cell in same "GroupedBy"
        /// </summary>
        private void DeletePossibleValuesByLockedSet(GroupedBy groupedBy)
        {
            for (int index = 0; index < 9; index++)
            {
                List<Cell> cellsWithoutValue = GetCells(groupedBy, index).Where(y => !y.HasValue).ToList();

                if (cellsWithoutValue == null || !cellsWithoutValue.Any() || cellsWithoutValue.Count == 1) continue;
                else
                {
                    var cellsWithSamePossibleValues = cellsWithoutValue.GroupBy(c => c.PossibleValuesToCompare).Select(group => group.Select(g => g)).ToList();

                    foreach (var cells in cellsWithSamePossibleValues)
                    {
                        var possibleValues = cells.FirstOrDefault().PossibleValues;
                        if (cells.Count().Equals(possibleValues.Count))
                        {
                            RemovePossibleValues(possibleValues, groupedBy, cells.FirstOrDefault().GetIndex(groupedBy), cells.Select(c => c.IndexCell).ToArray());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If [n] values have same possible Cells, this value is not possible in the other cell  
        /// </summary>
        private void DeletePossibleValuesByLockedValuesCandidates()
        {
            int cellSolvedBefore;
            int cellSolvedAfter;

            do
            {
                cellSolvedBefore = CellsSolved;

                for (int boxIndex = 0; boxIndex < 9; boxIndex++)
                {
                    var valuesWithSamePossibleCellsInBox = GetValuesWithSamePossibleCellsInGroup(GroupedBy.Box, boxIndex);

                    foreach (var item in valuesWithSamePossibleCellsInBox)
                    {
                        var values = item.Item1.ToList();
                        var possibleCells = item.Item2;
                        if (values.Count.Equals(possibleCells.Count))
                        {
                            foreach (var cell in possibleCells)
                            {
                                cell.PossibleValues = values;
                                RemovePossibleValues(values, GroupedBy.Row, cell.IndexRow, possibleCells.Select(c => c.IndexCell).ToArray());
                                RemovePossibleValues(values, GroupedBy.Column, cell.IndexColumn, possibleCells.Select(c => c.IndexCell).ToArray());
                                RemovePossibleValues(values, GroupedBy.Box, cell.IndexBox, possibleCells.Select(c => c.IndexCell).ToArray());
                            }
                        }
                    }
                }

                for (int rowIndex = 0; rowIndex < 9; rowIndex++)
                {
                    var valuesWithSamePossibleCellsInRow = GetValuesWithSamePossibleCellsInGroup(GroupedBy.Row, rowIndex);

                    foreach (var item in valuesWithSamePossibleCellsInRow)
                    {
                        var values = item.Item1.ToList();
                        var possibleCells = item.Item2;
                        if (values.Count.Equals(possibleCells.Count))
                        {
                            foreach (var cell in possibleCells)
                            {
                                cell.PossibleValues = values;
                                RemovePossibleValues(values, GroupedBy.Row, cell.IndexRow, possibleCells.Select(c => c.IndexCell).ToArray());
                                RemovePossibleValues(values, GroupedBy.Column, cell.IndexColumn, possibleCells.Select(c => c.IndexCell).ToArray());
                                RemovePossibleValues(values, GroupedBy.Box, cell.IndexBox, possibleCells.Select(c => c.IndexCell).ToArray());
                            }
                        }
                    }
                }

                for (int columnIndex = 0; columnIndex < 9; columnIndex++)
                {
                    var valuesWithSamePossibleCellsInColumn = GetValuesWithSamePossibleCellsInGroup(GroupedBy.Column, columnIndex);

                    foreach (var item in valuesWithSamePossibleCellsInColumn)
                    {
                        var values = item.Item1.ToList();
                        var possibleCells = item.Item2;
                        if (values.Count.Equals(possibleCells.Count))
                        {
                            foreach (var cell in possibleCells)
                            {
                                cell.PossibleValues = values;
                                RemovePossibleValues(values, GroupedBy.Row, cell.IndexRow, possibleCells.Select(c => c.IndexCell).ToArray());
                                RemovePossibleValues(values, GroupedBy.Column, cell.IndexColumn, possibleCells.Select(c => c.IndexCell).ToArray());
                                RemovePossibleValues(values, GroupedBy.Box, cell.IndexBox, possibleCells.Select(c => c.IndexCell).ToArray());
                            }
                        }
                    }
                }

                cellSolvedAfter = CellsSolved;
                if (cellSolvedAfter == 81) break;

            } while (cellSolvedBefore != cellSolvedAfter);
        }
        private IEnumerable<Tuple<IEnumerable<int>, List<Cell>>> GetValuesWithSamePossibleCellsInGroup(GroupedBy groupedBy, int index)
        {
            var valuesWithPossibleCells = GetPossibleValuesInGroup(groupedBy, index)
                .Select(x => new { value = x, possibleCells = GetPossibleCells(x, groupedBy, index) });

            return valuesWithPossibleCells.GroupBy(a => a.possibleCells)
                .Select(group => new Tuple<IEnumerable<int>, List<Cell>>(group.Select(rr => rr.value), group.Key));

        }

        private void RemovePossibleValues(int possibleValueToRemove, GroupedBy inGroupedBy, int index, params int[] indexCellExcept) => RemovePossibleValues(new List<int>() { possibleValueToRemove }, inGroupedBy, index, indexCellExcept);
        private void RemovePossibleValues(List<int> possibleValuesToRemove, GroupedBy inGroupedBy, int index, params int[] indexCellExcept)
        {
            foreach (var cell in GetCells(inGroupedBy, index).Where(x => !indexCellExcept.Contains(x.IndexCell)))
            {
                cell.RemovePossibleValue(possibleValuesToRemove);
            }
        }

        private List<Cell> GetPossibleCells(int value, GroupedBy groupedBy, int index)
        {
            List<Cell> cells = GetCells(groupedBy, index);

            var cellWithThisValue = cells.Where(x => x.Value == value);
            return cellWithThisValue.Any()
                ? new List<Cell>() { cellWithThisValue.FirstOrDefault() }
                : cells.Where(x => !x.HasValue && value.In(x.PossibleValues)).ToList();
        }

        private void SetValueIfHasUniquePossibleCells(int value, List<Cell> possibleCells)
        {
            if (possibleCells.Count == 1)
            {
                Cell possibleCell = possibleCells.FirstOrDefault();

                if (possibleCell.HasValue)
                {
                    if (possibleCell.Value != value) throw new Exception($"The cell '{possibleCell.IndexCell}' has value '{possibleCell.Value}', is not possible set a new value '{value}'");
                }
                else
                {
                    CellsInSudoku.ElementAt(possibleCell.IndexCell).Value = possibleCell.PossibleValues.Contains(value)
                        ? value
                        : throw new Exception($"The value '{value}' not in the possible values in cell '{possibleCell.IndexCell}': [{possibleCell}] ");
                }
            }
        }

        private List<Cell> GetCells(GroupedBy groupedBy, int index)
        {
            return index is >= 9 or < 0
                ? throw new ArgumentException("Invalid index.")
                : CellsInSudoku.Where(c => c.GetIndex(groupedBy) == index).ToList();
        }

        private List<Cell> GetLinkedCells(Cell cell)
        {
            var result = GetCells(GroupedBy.Box, cell.IndexBox);
            result.AddRange(GetCells(GroupedBy.Column, cell.IndexColumn));
            result.AddRange(GetCells(GroupedBy.Row, cell.IndexRow));

            return result.Where(x => x.IndexCell != cell.IndexCell).ToList();
        }

        private List<int> GetValuesInLinkedCells(Cell cell) => GetLinkedCells(cell).Where(x => x.HasValue).Select(y => y.Value).ToList();

        private List<int> GetPossibleValuesInGroup(GroupedBy groupedBy, int index) => new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Where(x => !ExistValueInGroup(x, groupedBy, index)).ToList();

        private bool ExistValueInGroup(int value, GroupedBy groupedBy, int index) => CellsInSudoku.Exists(c => c.GetIndex(groupedBy) == index && c.Value == value);

        public override string ToString() => string.Join("|", CellsInSudoku.Select(x => x.ToString()));
    }
}