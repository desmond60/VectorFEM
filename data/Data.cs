namespace PROJECT;

// % ***** Data class for JSON ***** % //
public class Data
{
    //: Данные для сетки
    public double[] Begin { get; set; }   /// Начальная точка сетки
    public double[] End   { get; set; }   /// Конечная точка сетки
    public double   Hx    { get; set; }   /// Шаг по Оси X
    public double   Hy    { get; set; }   /// Шаг по Оси Y
    public double   Kx    { get; set; }   /// Коэффициент разрядки по Оси X
    public double   Ky    { get; set; }   /// Коэффициент разрядки по Оси Y
    public double[] Sigma { get; set; }   /// Значения Sigma

    //: Данные для задачи
    public uint     N     { get; set; }    /// Номер задачи
    public int[]    Kraev { get; set; }    /// Номера краевых на сторонах (н, п, в, л)

    //: Данные для вывода
    public bool IsShowLos  { get; set; }   /// Записать таблицку LOS?
    public bool IsShowGrid { get; set; }   /// Записать сетку?
    public bool IsShowSlau { get; set; }   /// Записать СЛАУ?

    //: Деконструктор (для сетки)
    public void Deconstruct(out Vector<double> begin,
                            out Vector<double> end,
                            out double hx, 
                            out double hy, 
                            out double kx, 
                            out double ky,
                            out Vector<double> sigma) 
    {
        begin = new Vector<double>(this.Begin);
        end   = new Vector<double>(this.End);
        sigma = new Vector<double>(this.Sigma);
        hx    = this.Hx;
        hy    = this.Hy;
        kx    = this.Kx;
        ky    = this.Ky;
    }

    //: Проверка входных данных
    public bool Incorrect(out string mes) {
        StringBuilder errorStr = new StringBuilder("");
        
        if (Begin[0] > End[0])
            errorStr.Append($"Incorrect data (start[0] > end[0]): {Begin[0]} > {End[0]}\n");

        if (Begin[1] > End[1])
            errorStr.Append($"Incorrect data (start[1] > end[1]): {Begin[1]} > {End[1]}\n");

        if (Hx <= 0)
            errorStr.Append($"Incorrect data (hx <= 0): {Hx} <= {0}\n");

        if (Hy <= 0)
            errorStr.Append($"Incorrect data (hy <= 0): {Hy} <= {0}\n");

        // if (Hx == Hy)
        //     errorStr.Append($"Incorrect data (hx == hy): {Hx} == {Hy}\n");

        if (Kx < 1)
            errorStr.Append($"Incorrect data (kx < 1): {Kx} < {1}\n");

        if (Ky < 1)
            errorStr.Append($"Incorrect data (ky < 1): {Ky} < {1}\n");

        // Подсчитаем количество узлов по Оси Y, чтобы сравнить с количеством sigma
        int N_Y = Ky != 1
            ? (int)(Log(1 - (End[1] - Begin[1])*(Ky - 1) / (Hy*(-1))) / Log(Ky) + 2)
            : (int)Math.Ceiling(((End[1] - Begin[1]) / Hy + 1));
        if ( (N_Y - 1) != Sigma.Length)
            errorStr.Append($"Incorrect data: count segment by axis Y doesn't match count sigma!\n");

        if (!errorStr.ToString().Equals("")) {
            mes = errorStr.ToString();
            return false;
        }

        mes = errorStr.ToString();
        return true;
    }
}