namespace PROJECT.other;

// % ***** Структура сетки ***** % //
public struct Grid 
{
    //: Поля и свойства
    public int Count_Node  { get; set; }    /// Общее количество узлов
    public int Count_Elem  { get; set; }    /// Общее количество КЭ
    public int Count_Kraev { get; set; }    /// Количество краевых
    public int Count_Edge  { get; set; }    /// Количество ребер

    public Node[]  Nodes;      /// Узлы
    public Elem[]  Elems;      /// КЭ
    public Kraev[] Kraevs;     /// Краевые
    public Edge[]  Edges;      /// Ребра      

    //: Конструктор
    public Grid(Node[] nodes, Edge[] edges, Elem[] elem, Kraev[] kraevs) {
        this.Count_Node  = nodes.Length;
        this.Count_Edge  = edges.Length;
        this.Count_Elem  = elem.Length;
        this.Count_Kraev = kraevs.Length;
        this.Nodes       = nodes;
        this.Edges       = edges;
        this.Elems       = elem;
        this.Kraevs      = kraevs;
    }

    //: Деконструктор
    public void Deconstruct(out Node[]  nodes,
                            out Edge[]  edges,
                            out Elem[]  elems,
                            out Kraev[] kraevs) {
        edges  = this.Edges;
        nodes  = this.Nodes;
        elems  = this.Elems;
        kraevs = this.Kraevs;
    }
}

// % ***** Структура узла ***** % //
public struct Node 
{
    //: Поля и свойства
    public double x { get; set; }  /// Координата X 
    public double y { get; set; }  /// Координата Y 

    //: Конструктор
    public Node(double _x, double _y) {
        x = _x; y = _y;
    }

    //: Деконструктор
    public void Deconstruct(out double x, 
                            out double y) 
    {
        x = this.x;
        y = this.y;
    }

    //: Cтроковое представление
    public override string ToString() => $"{x,0} {y,20}";
}

// % ***** Структура ребра ***** % //
public struct Edge {

    //: Поля и свойства
    public Node node1 { get; set; }  /// Левый узел ребра, либо нижний
    public Node node2 { get; set; }  /// Правый узел ребра, либо верхний

    //: Конструктор
    public Edge(Node _node1, Node _node2) {
        node1 = _node1; node2 = _node2;
    }

    //: Деконструктор
    public void Deconstruct(out Node node1, 
                            out Node node2) 
    {
        node1 = this.node1;
        node2 = this.node2;
    }

    //: Cтроковое представление
    public override string ToString() => $"{node1,0} {node2,40}";
}

// % ***** Структура конечного элемента (КЭ) ***** % //
public struct Elem
{
    //: Поля и свойства 
    public int[]  Node;   /// Номера узлов КЭ
    public int[]  Edge;   /// Номера ребер КЭ
    public double Sigma;  /// Значение Sigma

    //: Конструктор
    public Elem(double sigma, params int[] node) { 
        this.Sigma = sigma;
        this.Node  = node; 
    }

    //: Деконструктор
    public void Deconstruct(out int[] nodes) { nodes = this.Node; }

    //: Строковое представление КЭ
    public override string ToString() {
        StringBuilder str_elem = new StringBuilder();
        str_elem.Append($"{Node[0],0}");
        for (int i = 1; i < Node.Count(); i++)
            str_elem.Append($"{Node[i],8}");
        str_elem.Append($"\t");
        for (int i = 0; i < Edge.Count(); i++)
            str_elem.Append($"{Edge[i],8}");
        return str_elem.ToString();
    }
}

// % ***** Структура краевого ***** % //
public struct Kraev
{
    //: Поля и свойства
    public int      Edge     { get; set; }       /// Номер ребра краевого
    public int      NumKraev { get; set; }       /// Номер краевого
    public int      NumSide  { get; set; }       /// Номер стороны на котором задано краевое
    public Complex  Value    { get; set; }       /// Значение для главного краевого условия
                                                 /// или theta для естественного краевого условия
    
    //: Конструктор
    public Kraev(int num, int side, int edge, Complex value) { 
        this.NumKraev = num;
        this.NumSide  = side;
        this.Edge     = edge;
        this.Value    = value;
    }

    //: Деконструктор
    public void Deconstruct(out int num, out int side, out int edge, out Complex value) { 
        num   = this.NumKraev;
        side  = this.NumSide;
        edge  = this.Edge;
        value = this.Value;
    }

    //: Строковое представление краевого
    public override string ToString() {
        StringBuilder str_elem = new StringBuilder();
        str_elem.Append($"{NumKraev,0} {NumSide,3} {Edge,5} {Value,10}");
        return str_elem.ToString();
    }
}

// % ***** Структура СЛАУ ***** % //
public struct SLAU
{
    //: Поля и свойства
    public ComplexVector di, ggl, ggu;   /// Матрица
    public Vector<int> ig, jg;           /// Массивы с индексами
    public ComplexVector f, q;           /// Правая часть и решение
    public ComplexVector q_abs;          /// Абсолютные значения U-функции
    public int N;                        /// Размерность матрицы
    public int N_el;                     /// Размерность gl и gu

    //: Умножение матрицы на вектор
    public ComplexVector Mult(ComplexVector x) {
        ComplexVector result = new ComplexVector(N);
        for (int i = 0; i < N; i++) {
            result[i] = di[i]*x[i];
            for (int j = ig[i]; j < ig[i + 1]; j++) {
                result[i]      += ggl[j]*x[jg[j]];
                result[jg[j]]  += ggu[j]*x[i];
            }
        }
        return result;
    }
}

public static class Helper {

    //* Скалярное произведение векторов
    public static Complex Scalar(ComplexVector frst, ComplexVector scnd) {
        Complex res = 0;
        for (int i = 0; i < frst.Length; i++)
            res += frst[i]*scnd[i];
        return res;
    }

    //* Модуль комплексного вектора
    public static double Norm(ComplexVector vec) {
        double norm = 0;
        for (int i = 0; i < vec.Length; i++)
            norm += vec[i].Real*vec[i].Real + vec[i].Imaginary*vec[i].Imaginary;
        return Sqrt(norm);
    }

    //* Модуль комплексного числа
    public static double Norm(Complex ch) {
        return Sqrt(ch.Real*ch.Real + ch.Imaginary*ch.Imaginary);
    }

    //* Окно помощи при запуске (если нет аргументов или по команде)
    public static void ShowHelp() {
        WriteLine("----Команды----                        \n" + 
        "-help             - показать справку             \n" + 
        "-i                - входной файл                 \n");
    }
}