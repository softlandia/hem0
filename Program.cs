using System;
using System.Collections;

namespace hem0
{
     class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("hem encode start");

            Console.Write("введите сигнал:\t\t");
            string inputMsg = Console.ReadLine();
            //string inputMsg = "101";
            int n = inputMsg.Length;
            if ((n < 2) || (n > 10)) {
                Console.WriteLine("количесво бит в сообщении должно быть от 3 до 10.");
                Environment.Exit(0);
            }
            // объект класса Hem обеспечивает кодирование/декодирование, проверку сообщений
            // в нём хранятся матрицы соответствующие данной длине сообщения
            Hem hem = new Hem(n);  
            if (hem.k < 0) {
                Console.WriteLine("количество бит во входном сигнале больше допустимого, количество бит должно быть от 3 до 10");
            }
            //отладочный вывод матриц
            {
            /*Console.WriteLine("G");
            hem.PrintG();
            Console.WriteLine("H");
            hem.PrintH();
            Console.WriteLine("Ht");
            hem.PrintHt();*/
            }
            int[] a = Hem.StrToBits(inputMsg);
            int[] b = hem.encode(a);
            Console.WriteLine("закодированный сигнал:\t" + Hem.BitsToStr(b));
            Console.Write("исправьте сигнал:\t");
            b = Hem.StrToBits(Console.ReadLine());
            //b = Hem.StrToBits("001101");

            //sindrom() возвращает битовый массив ошибки
            //результат нужно проверять на null - это признак внутренней ошибки
            int[] sind = hem.sindrom(b);
            Console.WriteLine("синдром:\t\t" + Hem.BitsToStr(sind));
            int e = Hem.BitsToInt(sind);
            if (e == 0) {
                Console.WriteLine("ошибка отсутствует:\t" + e);
            } else {
                //если ошибка есть, то надо ковырять
                //ищем в генерирующей таблице G строку в которой контрольные биты совпадают с синдромом
                e = hem.errorPos(sind);
                if (e >= 0) {
                    Console.WriteLine("позиция ошибки:\t\t" + e);
                    b = hem.fix(b, e);
                } else {
                    Console.WriteLine("сигнал получен верный, ошибочно передан один из битов коррекции");
                }
            }
            int[] B = hem.signal(b);
            Console.WriteLine("правильный сигнал:\t" + Hem.BitsToStr(B));
        }
    }

    class Hem
    {
        public static int changeBit(int i) {
            if (i == 0) {
                return 1;
            } else {
                return 0;
            }
        }

        //последний бит у чётного числа 0, а у нечётного 1
        private static bool isEven(int a) {
            return (a & 1) == 0;
        }
        
        //умножение вектора на матрицу
        //строка на столбец
        public static int[] multVectToMatrix(int[,] m, int[] v) {
            int rows = m.GetUpperBound(0) + 1;
            int cols = m.GetUpperBound(1) + 1;
            if (rows != v.Length) {
                return null;
            }
            int[] c = new int[cols];
            for (int i = 0; i < cols; i++) {
                c[i] = 0;
                for (int j = 0; j < rows; j++) {
                    c[i] = c[i] + m[j,i] * v[j];
                }
                //чётные преобразуем в 0, а нечётные в 1 
                if (c[i] > 1) {
                    if (isEven(c[i])) {
                        c[i] = 0;
                    } else {
                        c[i] = 1;
                    }
                }
            }
            return c;
        }
        
        //преобразование массива бит в целое число
        //из двоичного представления синдром преобразуем в целое число -- позицию 
        public static int BitsToInt(int[] s) {
            int t = 0;
            for (int i = 0; i < s.Length; i++){
                t += s[i] * (int) Math.Pow(2, s.Length - 1 - i);
            }
            return t;
        }

        //битовый массив в строку
        public static string BitsToStr(int[] bits) {
            string s = "";
            int i = 0;
            foreach (int b in bits) {
                if (b == 0) {
                    s += '0';
                } else {
                    s += '1';
                }
                i++;
            }
            return s;
        }

        //строку в битовый массив
        public static int[] StrToBits(string s) {
            int n = s.Length;
            int[] t = new int[n];
            char[] CH = s.ToCharArray();
            int i = 0;
            foreach (char ch in CH) {
                if (ch == '0') {
                    t[i] = 0;
                } else {
                    t[i] = 1;
                }
                i++;
            }
            return t;
        }

        //errorPos - возвращает позицию ошибочного бита
        //если позиция не найдена (return -1), значит синдром отражает некорректно полученный бит коррекции, но это не страшно, сигнал получен правильный
        public int errorPos(int[] s) {
            //проходим только по битам коррекции, они начинаются в матрице G с позиции k
            //сравниваем биты коррекции из G с s
            int sum;
            for (int j = 0; j < k; j++) { //проход по строкам матрицы G
                sum = 0;
                for (int i = k; i < n; i++) { //проход по столбцам матрицы G
                    if (G[j,i] != s[i]) {
                        sum++;
                    }
                }
                if (sum == 0) {
                    //строка в матрице G совпала с синдромом
                    return j;
                }
            }
            return -1;
        }

        //если ошибка позиции в информационных битах, то исправляет информационный бит
        //входной параметр e - позиция ошибки должна быть от 0 до k-1, 
        //начиная с позиции k идут биты коррекции, их не правим, возвращает входной параметр
        public int[] fix(int[] b, int e) {
            if (e >= k) {
                return b;
            }
            int[] t = new int[b.Length];
            for (int i = 0; i < b.Length; i++) {
                if (i == e) {
                    t[i] = changeBit(b[i]);
                } else {
                    t[i] = b[i];
                }
            }
            return t;
        }

        //возвращает битовый массив из информационных битов
        public int[] signal(int[] b) {
            int[] r = new int[k];
            for (int i = 0; i < k; i++) {
                r[i] = b[i];
            }
            return r;
        }

        //кодируем кодом хэмминга битовый массив в новый битовый массив
        public int[] encode(int[] a) {
            return multVectToMatrix(G, a);
        }

        //сложение по модулю 2 двух битовых последовательностей
        //размер конечно должен быть одинаковый
        public int[] xorVectVect(int[] a, int[] b) {
            if (a == null) {
                Console.WriteLine("PANIC! <xorVectVect(a,b)> input parameter a is null");
                return null;
            }
            if (b == null) {
                Console.WriteLine("PANIC! <xorVectVect(a,b)> input parameter b is null");
                return null;
            }
            if (a.Length != b.Length) {
                return null;
            }
            int[] sum = new int[a.Length];
            for (int i = 0; i < a.Length; i++) {
                if (Hem.isEven(a[i]+b[i])) {
                    sum[i] = 0;
                } else {
                    sum[i] = 1;
                }
            }
            return sum;
        }

        //sindrom() - расчёт битового массива ошибки
        //если сигнал пришёл без ошибки, то код хэмминга от информационных битов получится как пришедший, синдром будет 0
        //пусть
        //B - пришедший сигнал с ошибкой (в примере из методички B=1111001)
        //берём от пришедшего сигнала значищие биты (первые k=4, это 1111)
        //вычисляем по ним код хэмминга, получаем b=1111111
        //сравниваем с пришедшим сигналом (B xor b) = 0000110 - отличен от 0, 
        //первые k бит соответствующие информационным ВСЕГДА будут 0, потому что код хэмминга в этих позицыях сохраняет исходный сигнал,
        //в генерирующей матрице сначала идён диагональная единичная матрица - умножая на неё первые k бит не изменяются
        //у нас получились биты коррекции 110, в генерирующей матрице это вторая строка - значит второй с лева бит сигнала
        public int[] sindrom(int[] a) {
            int[] b = new int[k];
            //выделяем в b только информационные биты (первые k)
            for (int i = 0; i < k; i++) {
                b[i] = a[i];
            }
            int[] s = multVectToMatrix(G, b);
            return xorVectVect(a, s);
        }

        public int k;
        public int r;
        public int n;
        public int[,] G;
/*        int[,] H;
        int[,] Ht;*/

        //если numBit неверное, устанавливаем признак ошибки k = -1
        public Hem(int numBits)
        {
            switch (numBits) {
            case 3:
                k = 3;
                r = 3;
                n = 6;
                G = G3;
                break;
            case 4:
                k = 4;
                r = 3;
                n = 7;
                G = G4;
                break;
            case 5:
                k = 5;
                r = 4;
                n = 9;
                G = G5;
                break;
            case 6: 
                k = 6;
                r = 4;
                n = 10;
                G = G6;
                break;
            case 7:
                k = 7;
                r = 4;
                n = 11;
                G = G7;
                break;
            case 8:
                k = 8;
                r = 4;
                n = 12;
                G = G8;
                break;
            case 9:
                k = 9;
                r = 4;
                n = 13;
                G = G9;
                break;
            case 10:
                k = 10;
                r = 4;
                n = 14;
                G = G0;
                break;
            default:
                k = -1;
                break;
            }
        }

        public void PrintG() {
            int rows = G.GetUpperBound(0) + 1;
            int cols = G.GetUpperBound(1) + 1;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write("{0:D} ", G[i,j]);
                }
                Console.WriteLine();
            }
        }

        //матрицы для 3 битовой последовательности
        //генерирующая таблица для 3 битовой последовательности
        public static int[,] G3 = { {1, 0, 0, 1, 1, 0},
                                    {0, 1, 0, 1, 0, 1},
                                    {0, 0, 1, 0, 1, 1}};

        //матрицы для 4 битовой последовательности
        //генерирующая таблица 
        public static int[,] G4 = { {1, 0, 0, 0, 1, 1, 1}, 
                                    {0, 1, 0, 0, 1, 1, 0}, 
                                    {0, 0, 1, 0, 1, 0, 1}, 
                                    {0, 0, 0, 1, 0, 1, 1}};


        //матрицы для 5 битной последовательности
        //генерирующая таблица 
        public static int[,] G5 = { {1, 0, 0, 0, 0, 1, 0, 0, 1}, 
                                    {0, 1, 0, 0, 0, 0, 1, 1, 1}, 
                                    {0, 0, 1, 0, 0, 0, 1, 1, 0}, 
                                    {0, 0, 0, 1, 0, 0, 1, 0, 1},
                                    {0, 0, 0, 0, 1, 0, 0, 1, 1}};


        //матрицы для 6 битной последовательности
        //генерирующая таблица 
        public static int[,] G6 = { {1, 0, 0, 0, 0, 0, 1, 0, 1, 0}, 
                                    {0, 1, 0, 0, 0, 0, 1, 0, 0, 1}, 
                                    {0, 0, 1, 0, 0, 0, 0, 1, 1, 1}, 
                                    {0, 0, 0, 1, 0, 0, 0, 1, 1, 0},
                                    {0, 0, 0, 0, 1, 0, 0, 1, 0, 1},
                                    {0, 0, 0, 0, 0, 1, 0, 0, 1, 1}};

        //матрицы для 7 битной последовательности
        //генерирующая таблица 
        public static int[,] G7 = { {1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1}, 
                                    {0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0}, 
                                    {0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1}, 
                                    {0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1},
                                    {0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0},
                                    {0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1},
                                    {0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1}};

        //матрицы для 8 битной последовательности
        //генерирующая таблица 
        public static int[,] G8 = { {1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0}, 
                                    {0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1}, 
                                    {0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0}, 
                                    {0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1},
                                    {0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1},
                                    {0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0},
                                    {0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1},
                                    {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1}};
        //матрицы для 9 битной последовательности
        //генерирующая таблица 
        public static int[,] G9 = { {1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1}, 
                                    {0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0}, 
                                    {0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1}, 
                                    {0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0},
                                    {0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1},
                                    {0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1},
                                    {0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0},
                                    {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1},
                                    {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1}};

        //матрицы для 10 битной последовательности
        //генерирующая таблица 
        public static int[,] G0 = { {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0}, 
                                    {0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1}, 
                                    {0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0}, 
                                    {0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1},
                                    {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0},
                                    {0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1},
                                    {0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1},
                                    {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0},
                                    {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1},
                                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1}};
    }
}

/*
        //проверочная таблица для 3 битовой последовательности
        public static int[,] H3 = { {1, 1, 0, 1, 0, 0},
                                    {1, 0, 1, 0, 1, 0},
                                    {0, 1, 1, 0, 0, 1}};
        //транспонированая проверочная 
        public static int[,] Ht3 = {{1, 1, 0},
                                    {1, 0, 1}, 
                                    {0, 1, 1},
                                    {1, 0, 0},
                                    {0, 1, 0}, 
                                    {0, 0, 1}};

        //проверочная таблица
        public static int[,] H4 = { {1, 1, 1, 0, 1, 0, 0},
                                    {1, 1, 0, 1, 0, 1, 0}, 
                                    {1, 0, 1, 1, 0, 0, 1}};
        //транспонированая проверочная 
        public static int[,] Ht4 = {{1, 1, 1},
                                    {1, 1, 0},
                                    {1, 0, 1},
                                    {0, 1, 1},
                                    {1, 0, 0},
                                    {0, 1, 0},
                                    {0, 0, 1}};
        //проверочная таблица
        public static int[,] H5 = { {1, 0, 0, 0, 0, 1, 0, 0, 0},
                                    {0, 1, 1, 1, 0, 0, 1, 0, 0}, 
                                    {0, 1, 1, 0, 1, 0, 0, 1, 0},
                                    {1, 1, 0, 1, 1, 0, 0, 0, 1}};

        //транспонированая проверочная 
        public static int[,] Ht5 = {{1, 0, 0, 1},
                                    {0, 1, 1, 1},
                                    {0, 1, 1, 0},
                                    {0, 1, 0, 1},
                                    {0, 0, 1, 1},
                                    {1, 0, 0, 0},
                                    {0, 1, 0, 0},
                                    {0, 0, 1, 0},
                                    {0, 0, 0, 1}};

        public void PrintH() {
            int rows = H.GetUpperBound(0) + 1;
            int cols = H.GetUpperBound(1) + 1;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write("{0:D} ", H[i, j]);
                }
                Console.WriteLine();
            }
        }

        public void PrintHt() {
            int rows = Ht.GetUpperBound(0) + 1;
            int cols = Ht.GetUpperBound(1) + 1;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write("{0:D} ", Ht[i, j]);
                }
                Console.WriteLine();
            }
        }
       
*/