namespace PROJECT;

// % ***** LOS class ***** % //
public class LOS
{
    //: Поля и свойства
    protected SLAU   slau;      /// Матрица
    protected double EPS;       /// Точность 
    protected int    maxIter;   /// Максимальное количество итераций

    protected string Path { get; set; }   /// Путь к задаче

    private Table table_iter;   /// Табличка каждой итерации и невязки

    //: Конструктор
    public LOS(SLAU _slau, double _EPS, int _maxIter, string _path) {
        
        this.slau    = _slau;
        this.EPS     = _EPS;
        this.maxIter = _maxIter;

        this.Path = _path +  "/los";

        this.table_iter = new Table("Таблица LOS");
        table_iter.AddColumn(("Iter", 5), ("Nev", 15));
    }

    //: Метод решения СЛАУ
    public ComplexVector solve(bool IsShowLos) {

        // Вспомогательные переменные
        var r      = new ComplexVector(slau.N);
        var z      = new ComplexVector(slau.N);
        var multLr = new ComplexVector(slau.N);
        var Lr     = new ComplexVector(slau.N);
        var p      = new ComplexVector(slau.N);
        Complex alpha, betta;
        double Eps;
        int iter = 0;

        // Диагональное преобладание
        ComplexVector L = new ComplexVector(Enumerable.Range(0, slau.N).Select(i => new Complex(1, 0) / slau.di[i]).ToArray());
        
        // Решение СЛАУ
        ComplexVector multX = slau.Mult(slau.q);
        for (int i = 0; i < r.Length; i++) {
            r[i] = L[i] * (slau.f[i] - multX[i]);
            z[i] = L[i] * r[i];
        }
        ComplexVector multZ = slau.Mult(z);
        for (int i = 0; i < p.Length; i++)
            p[i] = L[i] * multZ[i];

         do {
            betta = Scalar(p, p);
            alpha = Scalar(p, r) / betta;
            for (int i = 0; i < slau.q.Length; i++) {
                slau.q[i]  += alpha * z[i];
                r[i]       -= alpha * p[i];
                Lr[i]       = L[i] * r[i];
            }
            multLr = slau.Mult(Lr);
            for (int i = 0; i < Lr.Length; i++)
                multLr[i] = L[i] * multLr[i];
            betta = -Scalar(p, multLr) / betta;
            for (int i = 0; i < z.Length; i++) {
                z[i] = L[i] * r[i] + betta * z[i];
                p[i] = multLr[i] + betta * p[i];
            }

            // Невязка
            Eps = Norm(Scalar(r, r));

            // Занесение данных в таблицу
            if (IsShowLos)
                table_iter.AddRow($"{++iter}", $"{Eps.ToString("E6")}");
        } while (iter < maxIter && Eps  > EPS);

        // Запись таблички
        if (IsShowLos) {
            Directory.CreateDirectory(this.Path);
            File.WriteAllText(Path + "/table_iter.txt", table_iter.ToString());
        }

        return slau.q;
    }
}