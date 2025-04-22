using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplexMethod
{
    class SimplexMethod
    {
        public void simplexMethodSolution(double[,] constraints, double[] targetFunction, string type)
        {
            int vars = targetFunction.Length; // колво переменных в целевой функции
            int rows = constraints.GetLength(0); // колво строк в таблице 
            int columns = vars + rows + 1; //колво столбцов в таблице 

            double[,] simplexTable = new double[rows + 1, columns]; //симплекс таблица со строками + строка целевой функции и столбик для столбца свободных коэфоф
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < vars; j++)
                {
                    simplexTable[i, j] = constraints[i, j]; // перенос коэффициентов ограничений
                }
                simplexTable[i, vars + i] = 1; // единицы в столбцах добавочных переменных
                simplexTable[i, columns - 1] = constraints[i, vars]; //столбец свободных коэфоф
            }

            for (int j = 0; j < vars; j++)
            {
                if (type == "max")
                {
                    simplexTable[rows, j] = -targetFunction[j]; //заполнение строки целевой функции при решении задачи на максимум
                }
                else
                {
                    simplexTable[rows, j] = targetFunction[j]; //заполнение строки целевой функции при решении задачи на минимум
                }
            }

            while (true) //основной алгоритм решения задачи 
            {

                int mainColumn = findMainColumn(simplexTable, type);
                if (mainColumn == -1) break; // ведущий столбец не найден, план оптимальный

                int mainString = findMainString(simplexTable, mainColumn);
                if (mainString == -1) break; // решение не ограничено 

                normalizationAndZeroing(simplexTable, mainColumn, mainString);
            }
            printResult(simplexTable, vars, rows, type);
        }
        public int findMainColumn(double[,] simplexTable, string type) // метод для нахождения ведущего столбца
        {
            int lastRow = simplexTable.GetLength(0) - 1; // строка целевой функции
            int cols = simplexTable.GetLength(1); // столбцы
            int mainСolumn = -1;

            for (int j = 0; j < cols; j++)
            {
                if (type == "max") // поиск ведущего столбца для решения задачи на максимум
                {
                    if (simplexTable[lastRow, j] < 0)
                    {
                        if (mainСolumn == -1 || simplexTable[lastRow, j] < simplexTable[lastRow, mainСolumn])
                        {
                            mainСolumn = j;
                        }
                    }
                }
                else // поиск ведущего столбца для решения задачи на минимум
                {
                    if (simplexTable[lastRow, j] > 0)
                    {
                        if (mainСolumn == -1 || simplexTable[lastRow, j] > simplexTable[lastRow, mainСolumn])
                        {
                            mainСolumn = j;
                        }
                    }
                }
            }
            return mainСolumn;
        }

        public int findMainString(double[,] simplexTable, int mainColumn)
        {
            int rows = simplexTable.GetLength(0) - 1;
            int lastCol = simplexTable.GetLength(1) - 1;
            int mainString = -1;
            double minZnach = double.MaxValue;

            for (int i = 0; i < rows; i++)
            {
                if (simplexTable[i, mainColumn] != 0) // исключение деления на ноль
                {
                    double n = simplexTable[i, lastCol] / simplexTable[i, mainColumn]; //подсчет деления: строки свободных коэфоф на элементы ведущего столбца для нахождения ведущей строки
                    if (n > 0 && n < minZnach)
                    {
                        minZnach = n;
                        mainString = i;
                    }
                }
            }
            return mainString;
        }

        public void normalizationAndZeroing(double[,] simplexTable, int mainColumn, int mainString) // метод в котором нормализуется ведущая строка и зануляются элементы в ведущем столбце
        {
            int cols = simplexTable.GetLength(1);
            int rows = simplexTable.GetLength(0);
            double mainElement = simplexTable[mainString, mainColumn]; // главный элемент

            for (int j = 0; j < cols; j++)
                simplexTable[mainString, j] /= mainElement; // нормализация ведущей строки

            for (int i = 0; i < rows; i++)
            {
                if (i != mainString)
                {
                    double koef = simplexTable[i, mainColumn];
                    for (int j = 0; j < cols; j++)
                        simplexTable[i, j] -= simplexTable[mainString, j] * koef; // зануление элементов в ведущем столбце
                }

            }
        }
        public void printResult(double[,] simplexTable, int vars, int rows, string type)
        {
            int lastRow = simplexTable.GetLength(0) - 1;
            int lastCol = simplexTable.GetLength(1) - 1;

            Console.WriteLine($"Оптимальное значение: {(type == "max" ? 1 : -1) * simplexTable[lastRow, lastCol]}");
            for (int j = 0; j < vars; j++)
            {
                double value = 0;

                for (int i = 0; i < rows; i++)
                {
                    if (simplexTable[i, j] == 1)
                    {
                        bool isBasic = true;

                        for (int k = 0; k < rows; k++)
                        {
                            if (k != i && Math.Abs(simplexTable[k, j]) > 1e-8)
                            {
                                isBasic = false;
                                break;
                            }
                        }

                        if (isBasic)
                        {
                            value = simplexTable[i, vars + rows];
                            break;
                        }
                    }
                }

                Console.WriteLine($"x{j + 1} = {value}");
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            SimplexMethod simplexMethod = new SimplexMethod();
            double[] targetFunctionMax = { 6, 5 };
            double[,] constraintsMax = {
            { 4, 7, 49 },
            { 8, 3, 51 },
            { 9, 5, 45 }
        };

            Console.WriteLine("=== Максимизация ===");
           simplexMethod.simplexMethodSolution(constraintsMax, targetFunctionMax, "max");

            double[] targetFunctionMin = { 2, 5 };
            double[,] constraintsMin = {
            { -1, -1, -6 },
            { -1, -3, -9 },
            { 2, 1, 8 }
        };

            Console.WriteLine("\n=== Минимизация ===");
            simplexMethod.simplexMethodSolution(constraintsMin, targetFunctionMin, "min");
            Console.ReadKey();
        }
    }
}

