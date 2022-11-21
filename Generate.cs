namespace PROJECT;

// % ****** Generate class ***** % //
public class Generate 
{
    //: Поля и свойства
    protected Vector<double> begin  { get; set; }      /// Начальная точка сетки
    protected Vector<double> end    { get; set; }      /// Конечная точка сетки
    protected double         hx     { get; set; }      /// Шаг по Оси X
    protected double         hy     { get; set; }      /// Шаг по Оси Y
    protected double         kx     { get; set; }      /// Коэффициент разрядки по Оси X  
    protected double         ky     { get; set; }      /// Коэффициент разрядки по Оси Y
    protected Vector<double> sigma  { get; set; }      /// Коэффициенты сигмы на всех слоя Y

    protected string Path     { get; set; }   /// Путь к папке с задачей
    protected bool IsShowGrid { get; set; }   /// Записать сетку?

    private int N_X;                                            /// Количество узлов по Оси X
    private int N_Y;                                            /// Количество узлов по Оси Y
    private int Count_Node  => N_X * N_Y;                       /// Общее количество узлов
    private int Count_Elem  => (N_X - 1)*(N_Y - 1);             /// Общее количество КЭ
    private int Count_Kraev => 2*(N_X - 1) + 2*(N_Y - 1);       /// Количество краевых
    private int Count_Edge  => N_X*(N_Y - 1) + N_Y*(N_X - 1);   /// Количество ребер
    private int[]? SideKraev;                                   /// Номера краевых на сторонах

    //: Конструктор
    public Generate(Data _data, string _path) {
        
        (begin, end, hx, hy, kx, ky, sigma) = _data;
        
        this.Path = _path +  "/grid";
        this.IsShowGrid = _data.IsShowGrid;
        if (IsShowGrid)
            Directory.CreateDirectory(Path);

        // Подсчет количества узлов на Осях (X & Y)
        N_X = kx != 1
            ? (int)(Log(1 - (end[0] - begin[0])*(kx - 1) / (hx*(-1))) / Log(kx) + 2)
            : (int)Math.Ceiling(((end[0] - begin[0]) / hx + 1));
        N_Y = ky != 1
            ? (int)(Log(1 - (end[1] - begin[1])*(ky - 1) / (hy*(-1))) / Log(ky) + 2)
            : (int)Math.Ceiling(((end[1] - begin[1]) / hy + 1));
    }

    //: Инициализации сторон номерами краевых
    public void SetKraev(int side0, int side1, int side2, int side3) { 
        SideKraev = new int[] {side0, side1, side2, side3}; 
    }

    //: Основная функция генерации сетки
    public Grid generate() {
        if (SideKraev == null) throw new ArgumentException($"Boundary conditions are not set!\nUse the function \"SetKraev\"");

        Node[]  nodes  = generate_coords();             //? Генерация узлов
        Elem[]  elems  = generate_elems();              //? Генерация КЭ
        Edge[]  edges  = generate_edges(elems, nodes);  //? Генерация ребер
        Kraev[] kraevs = generate_kraevs(edges);        //? Генерация краевых

        return new Grid(nodes, edges, elems, kraevs);
    }

    //: Генерация узлов
    private Node[] generate_coords() {
        
        // Генерация узлов по Осям (X & Y)
        Vector<double> X_vec = generate_array(begin[0], end[0], hx, kx, N_X);
        Vector<double> Y_vec = generate_array(begin[1], end[1], hy, ky, N_Y);
    
        Node[] nodes = new Node[Count_Node];

        for (int i = 0; i < N_X; i++)
            for (int j = 0; j < N_Y; j++)
                    nodes[j*N_X + i] = new Node(X_vec[i], Y_vec[j]);
        
        // Запись узлов
        if (IsShowGrid)
            File.WriteAllText(Path + "/nodes.txt", String.Join("\n", nodes));

        return nodes;
    }

    //: Генерация КЭ
    private Elem[] generate_elems() {

        Elem[] elems = new Elem[Count_Elem];

        for (int i = 0, id = 0; i < N_Y - 1; i++)
            for (int j = 0; j < N_X - 1; j++, id++) {
                    elems[id] = new Elem(
                          sigma[i],
                          i   *N_X + j,
                          i   *N_X + j + 1,
                         (i+1)*N_X + j,
                         (i+1)*N_X + j + 1
                    );
            }

        return elems;
    }

    //: Генерация ребер
    private Edge[] generate_edges(Elem[] elems, Node[] nodes) {

        Edge[] edges = new Edge[Count_Edge];
        
        for (int i = 0; i < N_Y - 1; i++)
            for (int j = 0; j < N_X - 1; j++) {
                int left   = i*((N_X - 1) + N_X) + (N_X - 1) + j;
                int right  = i*((N_X - 1) + N_X) + (N_X - 1) + j + 1;
                int bottom = i*((N_X - 1) + N_X) + j;
                int top    = (i + 1)*((N_X - 1) + N_X) + j;
                int n_elem = i*(N_X - 1) + j;

                edges[left]   = new Edge(nodes[elems[n_elem].Node[0]], nodes[elems[n_elem].Node[2]]); 
                edges[right]  = new Edge(nodes[elems[n_elem].Node[1]], nodes[elems[n_elem].Node[3]]); 
                edges[bottom] = new Edge(nodes[elems[n_elem].Node[0]], nodes[elems[n_elem].Node[1]]); 
                edges[top]    = new Edge(nodes[elems[n_elem].Node[2]], nodes[elems[n_elem].Node[3]]); 

                elems[n_elem] = elems[n_elem] with { Edge = new [] { left, right, bottom, top} };
            }

        // Запись ребер и КЭ
        if (IsShowGrid) {
            File.WriteAllText(Path + "/edges.txt", String.Join("\n", edges));
            File.WriteAllText(Path + "/elems.txt", String.Join("\n", elems));
        }

        return edges;
    }

    //: Генерация краевых
    private Kraev[] generate_kraevs(Edge[] edges) {

        Kraev[] kraevs = new Kraev[Count_Kraev];
        int id = 0;

        // Нижняя сторона
        for (int i = 0; i < N_X - 1; i++, id++)
            kraevs[id] = new Kraev(
                SideKraev![0],
                0,
                i,
                SideKraev![0] == 1 ? Absolut(edges[i]) : Theta(edges[i])
            );

        // Правая сторона
        for (int i = 1; i < N_Y; i++, id++)
            kraevs[id] = new Kraev(
                 SideKraev![1],
                 1,
                 i*N_X + i*(N_X - 1) - 1,
                 SideKraev![1] == 1 ? Absolut(edges[i*N_X + i*(N_X - 1) - 1]) : Theta(edges[i*N_X + i*(N_X - 1) - 1])
            );

        // Верхняя сторона
        for (int i = 0; i < N_X - 1; i++, id++)
            kraevs[id] = new Kraev(
                SideKraev![2],
                2,
                N_X*(N_Y - 1) + (N_X - 1)*(N_Y - 1) + i,
                SideKraev![2] == 1 ? Absolut(edges[N_X*(N_Y - 1) + (N_X - 1)*(N_Y - 1) + i]) : Theta(edges[N_X*(N_Y - 1) + (N_X - 1)*(N_Y - 1) + i])
            );

        // Левая сторона
        for (int i = 0; i < N_Y - 1; i++, id++)
            kraevs[id] = new Kraev(
                 SideKraev![3],
                 3,
                 (i + 1)*(N_X - 1) + i*N_X,
                 SideKraev![3] == 1 ? Absolut(edges[(i + 1)*(N_X - 1) + i*N_X]) : Theta(edges[(i + 1)*(N_X - 1) + i*N_X])
            );

        // Сортируем по номеру краевого
        kraevs = kraevs.OrderByDescending(n => n.NumKraev).ToArray();

        // Запись краевых
        if (IsShowGrid)
            File.WriteAllText(Path + "/kraevs.txt", String.Join("\n", kraevs));
        
        return kraevs;
    }

    //: Генерация массива по Оси (с шагом и коэффицентом разрядки)
    private Vector<double> generate_array(double start, double end, double h, double k, int n) {
        var coords = new Vector<double>(n);
        coords[0]     = start;
        coords[n - 1] = end;
        for (int i = 1; i < n - 1; i++, h *= k) 
            coords[i] = coords[i - 1] + h;
        return coords;
    }
}