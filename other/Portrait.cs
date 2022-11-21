namespace PROJECT.other;

// Класс подходит и для построение портрета по ребрам и по узлам
// Просто замена Edge на Node

// % ****** Класс создания портрета ***** % //
public class Portrait
{
    private int countEdge;
    private int[] lportrait;

    //* Конструктор
    public Portrait(int n_edges) { 
        this.countEdge    = n_edges; 
        lportrait = new int[this.countEdge];
        for (int i = 0; i < this.countEdge; i++)
            lportrait[i] = i;
    }

    //* Генерация ig, jg (размерность - n)
    public int GenPortrait(ref Vector<int> ig, ref Vector<int> jg, Elem[] elems) {

        var list = new int[countEdge][];

        var listI = new HashSet<int>();
        for (int i = 0; i < lportrait.Length; i++) {
            int value = lportrait[i];
            for (int k = 0; k < elems.Count(); k++) {
                if (elems[k].Edge.Contains(value))
                    for (int p = 0; p < elems[k].Edge.Count(); p++)
                        if (elems[k].Edge[p] < value)
                            listI.Add(elems[k].Edge[p]);
            }
            list[i] = listI.OrderBy(n => n).ToArray();
            listI.Clear();
        }

        // Заполнение ig[]
        ig = new Vector<int>(countEdge + 1);
        ig[0] = ig[1] = 0;
        for (int i = 1; i < countEdge; i++)
            ig[i + 1] = (ig[i] + list[i].Length);

        // Заполнение jg[]
        jg = new Vector<int>(ig[countEdge]);
        int jj = 0;
        for (int i = 0; i < countEdge; i++)
            for (int j = 0; j < list[i].Length; j++, jj++)
                jg[jj] = list[i][j];

        return ig[countEdge];
    }
}