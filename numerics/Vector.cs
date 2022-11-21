namespace PROJECT.numerics;

// % ***** Класс вектора ***** % //
public class Vector<T> : IEnumerable, ICloneable
                where T : System.Numerics.INumber<T>
{
    //: Поля и свойства
    private T[] vector;                 /// Вектор
    public int Length { get; init; }    /// Размерность вектора

    //: Реализация IEnumerable
    public IEnumerator GetEnumerator() => vector.GetEnumerator();

    //: Перегрузка неявного преобразования
    public static explicit operator T[](Vector<T> vec) {
        return vec.vector;
    }

    //: Деконструктор
    public void Deconstruct(out T[] vec) {
        vec = this.vector;
    }

    //: Конструктор (с размерностью)
    public Vector(int lenght) {
        vector = new T[lenght];
        this.Length = lenght;
    }

    //: Конструктор (с массивом)
    public Vector(T[] array) {
        vector = new T[array.Length];
        this.Length = array.Length;
        Array.Copy(array, vector, array.Length);
    }

    //: Индексатор
    public T this[int index] {
        get => vector[index];
        set => vector[index] = value;
    }

    //: Перегрузка умножения двух векторов
    public static T operator *(Vector<T> vec1, Vector<T> vec2) {
        T result = T.Zero;
        for (int i = 0; i < vec1.Length; i++)
            result += vec1[i] * vec2[i];
        return result;
    }

    //: Перегрузка умножения на констунту (double)
    public static Vector<T> operator *(T Const, Vector<T> vector) {
        var result = new Vector<T>(vector.Length);
        for (int i = 0; i < vector.Length; i++)
            result[i] = Const * vector[i];
        return result;
    }
    public static Vector<T> operator *(Vector<T> vector, T Const) => Const * vector;

    //: Перегрузка умножения (на числовой вектор)
    public static Vector<T> operator *(Matrix<T> mat, Vector<T> vec)  { 
        var result = new Vector<T>(vec.Length);
        for (int i = 0; i < vec.Length; i++)
            for (int j = 0; j < vec.Length; j++)
                result[i] += mat[i, j] * vec[j];
        return result;
    }

    //: Перегрузка деления на константу (double)
    public static Vector<T> operator /(Vector<T> vector, T Const) {
        var result = new Vector<T>(vector.Length);
        for (int i = 0; i < vector.Length; i++)
            result[i] = vector[i] / Const;
        return result;
    }

    //: Перегрузка сложения двух векторов
    public static Vector<T> operator +(Vector<T> vec1, Vector<T> vec2) {
        var result = new Vector<T>(vec1.Length);
        for (int i = 0; i < vec1.Length; i++)
            result[i] = vec1[i] + vec2[i];
        return result;
    }

    //: Перегрузка вычитания двух векторов
    public static Vector<T> operator -(Vector<T> vec1, Vector<T> vec2) {
        var result = new Vector<T>(vec1.Length);
        for (int i = 0; i < vec1.Length; i++)
            result[i] = vec1[i] - vec2[i];
        return result;
    }

    //: Перегрузка тернарного минуса
    public static Vector<T> operator -(Vector<T> vector) {
        var result = new Vector<T>(vector.Length);
        for (int i = 0; i < vector.Length; i++)
            result[i] = -vector[i];
        return result;
    }

    //: Заполнение вектора числом (double)
    public void Fill(T val) {
        for (int i = 0; i < Length; i++)
            vector[i] = val;
    }

    //: Копирование вектора
    public static void Copy(Vector<T> source, Vector<T> dest) {
        for (int i = 0; i < source.Length; i++)
            dest[i] = source[i];
    }

    //: Очистка вектора
    public static void Clear(Vector<T> vector) {
        for (int i = 0; i < vector.Length; i++)
            vector[i] = T.Zero;
    }

    //: Выделение памяти под вектор
    public static void Resize(ref Vector<T> vector, int lenght) {
        vector = new(lenght);
    }

    //: Строковое представление вектора
    public override string ToString() {
        var vec = new StringBuilder();
        if (vector == null) return vec.ToString();

        for (int i = 0; i < Length; i++)
            vec.Append(vector[i].ToString() + "\n");    
        
        return vec.ToString();
    }

    //: Реализация метода ICloneable
    public object Clone() => new Vector<T>(vector);
}