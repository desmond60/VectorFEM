namespace PROJECT;

// % ***** Класс МКЭ ***** % //
public class FEM 
{
    //: Поля и свойства
    private Node[]     Nodes;               /// Узлы
    private Edge[]     Edges;               /// Ребра
    private Elem[]     Elems;               /// КЭ
    private Kraev[]    Kraevs;              /// Краевые
    private SLAU       slau;                /// Структура СЛАУ

    public string Path { get; set; }        /// Путь к задаче

    public bool IsShowLos  { get; set; }   /// Записать таблицку LOS?
    public bool IsShowSlau { get; set; }   /// Записать СЛАУ?

    //: Конструктор
    public FEM(Grid grid, string _path) {
        (Nodes, Edges, Elems, Kraevs) = grid;
        this.Path = _path;
    }

    //: Основной метод решения
    public void solve() {
        portrait();                                          //? Составление портрета матрицы
        global();                                            //? Составление глобальной матрицы
        LOS los = new LOS(slau, 1e-30, 1000, Path);          //? LOS
        slau.q = los.solve(IsShowLos);                       //? Решение СЛАУ
        AbsolutSolve();                                      //? Точное решение  
        if (IsShowSlau) WriteToSlau();                       //? Записать СЛАУ
        WriteToTable();                                      //? Записать решение
    }

    //: Составление портрета матрицы (ig, jg, выделение памяти)
    private void portrait() {
        Portrait port = new Portrait(Edges.Length);

        // Генерируем массивы ig и jg и размерность
        slau.N_el = port.GenPortrait(ref slau.ig, ref slau.jg, Elems);
        slau.N    = Edges.Length;

        // Выделяем память
        slau.ggl   = new ComplexVector(slau.N_el);
        slau.ggu   = new ComplexVector(slau.N_el);
        slau.di    = new ComplexVector(slau.N);
        slau.f     = new ComplexVector(slau.N);
        slau.q     = new ComplexVector(slau.N);
        slau.q_abs = new ComplexVector(slau.N);
    }

    //: Построение глобальной матрицы
    private void global() {

        // Обходим КЭ
        for (int index_fin_el = 0; index_fin_el < Elems.Length; index_fin_el++) {

            // Составляем локальную матрицу и локальный вектор
            (ComplexMatrix loc_mat, ComplexVector local_f) = local(index_fin_el);

            // Заносим в глобальную матрицу
            EntryMatInGlobalMatrix(loc_mat, Elems[index_fin_el].Edge);
            EntryVecInGlobalMatrix(local_f, Elems[index_fin_el].Edge);
        }

        // Обходим краевые
        for (int index_kraev = 0; index_kraev < Kraevs.Length; index_kraev++) {
            Kraev kraev = Kraevs[index_kraev];
            if (kraev.NumKraev == 1)
                MainKraev(kraev); // главное краевое
            else if (kraev.NumKraev == 2)
                NaturalKraev(kraev); // естественное краевое
        }
    }

    //: Построение локальной матрицы и вектора
    private (ComplexMatrix, ComplexVector) local(int index_fin_el) {
        
        // Подсчет компонент
        double hx   = Nodes[Elems[index_fin_el].Node[1]].x - Nodes[Elems[index_fin_el].Node[0]].x;
        double hy   = Nodes[Elems[index_fin_el].Node[2]].y - Nodes[Elems[index_fin_el].Node[0]].y;
        
        Matrix<double> G           = build_G(index_fin_el, hx, hy);    // Построение матрицы жесткости (G)
        ComplexMatrix M            = build_M(index_fin_el, hx, hy);    // Построение матрицы массы (M)
        ComplexVector local_f      = build_F(index_fin_el, hx, hy);    // Построение локальной правой части
        ComplexMatrix local_matrix = G + new Complex(0, 1) * M;

        return (local_matrix, local_f);
    }

    //: Построение матрицы жесткости (G)
    private Matrix<double> build_G(int index_fin_el, double hx, double hy) {
        
        // Подсчет коэффициентов
        double coef_y_on_x = hy / hx; 
        double coef_x_on_y = hx / hy;
        double coef_mu     = 1.0 / Function.mu;

        // Матрица жесткости
        var G_matrix = new Matrix<double>(new double[4, 4]{
            {1, -1, -1, 1},
            {-1, 1, 1, -1},
            {-1, 1, 1, -1},
            {1, -1, -1, 1}
        });

        // Умножение на coef_y_on_x
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                G_matrix[i,j] *= coef_y_on_x;

        // Умножение на coef_x_on_y
        for (int i = 2; i < 4; i++)
            for (int j = 2; j < 4; j++)
                G_matrix[i,j] *= coef_x_on_y;

        return coef_mu * G_matrix;
    }

    //: Построение матрицы масс (M)
    private ComplexMatrix build_M(int index_fin_el, double hx, double hy) {
        
        // Подсчет коэффициента
        double coef = (Function.omega * Elems[index_fin_el].Sigma * hx * hy) / 6.0;

        // Матрица масс
        var M_matrix = new ComplexMatrix(new Complex[4, 4]{
            {2, 1, 0, 0},
            {1, 2, 0, 0},
            {0, 0, 2, 1},
            {0, 0, 1, 2}
        });                                

        return coef * M_matrix;
    }

    //: Построение вектора правой части (F)
    private ComplexVector build_F(int index_fin_el, double hx, double hy) {
        
        // Подсчет коэффициента
        double coef = (hx * hy) / 6.0;

        // Матрица масс
        var M_matrix = new ComplexMatrix(new Complex[4, 4]{
            {2, 1, 0, 0},
            {1, 2, 0, 0},
            {0, 0, 2, 1},
            {0, 0, 1, 2}
        });
        M_matrix = coef * M_matrix;
        
        // Вычисление f - на серединах ребер КЭ
        var f = new ComplexVector(4);                               
        for (int i = 0; i < f.Length; i++)
            f[i] = Func(Edges[Elems[index_fin_el].Edge[i]], Elems[index_fin_el].Sigma);
        
        return M_matrix * f;
    }

    //: Занесение матрицы в глоабальную матрицу
    private void EntryMatInGlobalMatrix(ComplexMatrix mat, int[] index) { 
        for (int i = 0, h = 0; i < mat.Dim; i++) {
            int ibeg = index[i];
            for (int j = i + 1; j < mat.Dim; j++) {
                int iend = index[j];
                int temp = ibeg;

                if (temp < iend)
                    (iend, temp) = (temp, iend);

                h = slau.ig[temp];
                while (slau.jg[h++] - iend != 0);
                --h;
                slau.ggl[h] += mat[i, j];
                slau.ggu[h] += mat[i, j];
            }
            slau.di[ibeg] += mat[i, i];
        }
    }

    //: Занесение вектора в глолбальный вектор
    private void EntryVecInGlobalMatrix(ComplexVector vec, int[] index) {
        for (int i = 0; i < vec.Length; i++)
            slau.f[index[i]] += vec[i];
    }

    //: Учет главного краевого условия
    private void MainKraev(Kraev kraev) {
        
        // Номер ребра и значение краевого
        (int row, Complex value) = (kraev.Edge, kraev.Value);

        // Учет краевого
        slau.di[row]   = new Complex(1, 0);
        slau.f[row]    = value;

        // Зануляем в треугольнике (столбцы)
        for (int i = slau.ig[row]; i < slau.ig[row + 1]; i++) {
            slau.f[slau.jg[i]] -= slau.ggu[i]*value;
            slau.ggl[i] = 0;
            slau.ggu[i] = 0;
        }

        // Зануляем в треугольнике (строки)
        for (int i = row + 1; i < slau.N; i++) {
            for (int j = slau.ig[i]; j < slau.ig[i + 1]; j++) {
                if (slau.jg[j] == row) {
                    slau.f[i] -= slau.ggl[j]*value;
                    slau.ggl[j] = 0;
                    slau.ggu[j] = 0;
                }
            }
        }
    }

    //: Учет естественного краевого условия
    private void NaturalKraev(Kraev kraev) {
        
        // Ребро на котором задано
        Edge edge = Edges[kraev.Edge];

        // Определение вектора внешней нормали к ребру
        var n = kraev.NumSide switch {
            0 => new Vector<double>(new []{  0.0, -1.0 }),
            1 => new Vector<double>(new []{  1.0,  0.0 }),
            2 => new Vector<double>(new []{  0.0,  1.0 }),
            3 => new Vector<double>(new []{ -1.0,  0.0 }),
            _ => new Vector<double>(new []{  0.0,  0.0 })
        };

        // Подсчет коэффициента
        Complex coeff = (1 / mu) * kraev.Value * (-n[1]*(edge.node2.x - edge.node1.x) + n[0]*(edge.node2.y - edge.node1.y));
        
        // Учет краевого
        slau.f[kraev.Edge] += coeff;
    }

    //: Вычисление точного решения
    private void AbsolutSolve() {
        for (int i = 0; i < Edges.Length; i++)
            slau.q_abs[i] = Absolut(Edges[i]);
    }

    //: Запись СЛАУ 
    private void WriteToSlau() {
        var path = Path + "/slau";
        Directory.CreateDirectory(path);
        File.WriteAllText(path + "/kuslau.txt", String.Join(" ", new int[] { slau.N, slau.N_el }));
        File.WriteAllText(path + "/di.txt" , slau.di  .ToString());
        File.WriteAllText(path + "/ggl.txt", slau.ggl .ToString());
        File.WriteAllText(path + "/ggu.txt", slau.ggu .ToString());
        File.WriteAllText(path + "/f.txt"  , slau.f   .ToString());
        File.WriteAllText(path + "/q.txt"  , slau.q   .ToString());
        File.WriteAllText(path + "/ig.txt" , slau.ig  .ToString());
        File.WriteAllText(path + "/jg.txt" , slau.jg  .ToString());
    }

    //: Запись решения
    private void WriteToTable() {
        
        // Табличка
        Table sol = new Table("Solution");

        // Точное решение
        (ComplexVector SubA, double Norma) = Norm(slau.q_abs, slau.q);

        sol.AddColumn(
            ("A`", 30),         // Точное
            ("A", 30),          // Решение МКЭ
            ("|A` - A|", 30),   // Погрешность
            ("||A` - A||", 20)  // Норма
        );

        sol.AddRow(
            slau.q_abs[0].ToString("E4"),
            slau.q[0].ToString("E4"),
            SubA[0].ToString("E4"),
            Norma.ToString("E4")
        );
        
        for (int i = 1; i < slau.q.Length; i++) {
            sol.AddRow(
                slau.q_abs[i].ToString("E4"),
                slau.q[i].ToString("E4"),
                SubA[i].ToString("E4"),
                "_"
            );
        }

        sol.WriteToFile(Path + "/solution.txt");
        //sol.WriteToCSV("/solution.csv");
    }

    //: Расчет погрешности и нормы решения
    private (ComplexVector, double) Norm(ComplexVector q_abs, ComplexVector q) {
        
        ComplexVector SubA = new ComplexVector(q.Length);

        for (int i = 0; i < q.Length; i++) {
            SubA[i] = q_abs[i] - q[i];
            SubA[i] = new Complex(Abs(SubA[i].Real), Abs(SubA[i].Imaginary));
        }
        return (SubA, Helper.Norm(SubA) / Helper.Norm(q_abs));
    }
}