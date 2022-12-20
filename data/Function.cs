namespace PROJECT;

// % ***** Function class ***** % //
public static class Function
{
    //: Поля
    public static uint   numberFunc;     /// Номер задачи
    public static double mu;             /// Значение mu
    public static double omega;          /// Значение omega

    //: Инициализация задачи
    public static void Init(uint numF) {
        numberFunc = numF;

        switch(numberFunc) {
            
            // Разное местоположение второго краевого
            case 1:
                mu = 2;
                omega = 1;
            break;

            // Полином второй степени
            case 2:
                mu = 2;
                omega = 1;
            break;

            // Полином третьей стпени
            case 3:
                mu = 2;
                omega = 1;
            break;

            // Полином четвертой степени
            case 4:
                mu = 2;
                omega = 1;
            break;

        }
    }

    //: Заданная функция вектор (нахождение через ребро)
    public static Complex Absolut(Edge edge) {
        
        // Определение компоненты вектора и узла
        char axe;
        Node node = new Node();
        if (edge.node1.y == edge.node2.y) {
            axe = 'x';
            node.x = edge.node1.x + (edge.node2.x - edge.node1.x) / 2.0;
            node.y = edge.node1.y;
        } else {
            axe = 'y';
            node.x = edge.node1.x;
            node.y = edge.node1.y + (edge.node2.y - edge.node1.y) / 2.0;
        }

        return Absolut(node, axe);
    }

    //: Заданная функция вектор (нахождение через узел)
    public static Complex Absolut(Node node, char axe) {
        switch (numberFunc) 
        {
            // Разное местоположение второго краевого
            case 1:
                return axe switch {
                    'x' => new Complex(2*node.x + 3*node.y, 6*node.x + 7*node.y),
                    'y' => new Complex(3*node.x - 2*node.y, node.x + node.y),
                    _   => new Complex(0, 0)
                };

            // Полином второй степени
            case 2:
                return axe switch {
                    'x' => new Complex(2*Pow(node.x, 2) + 3*Pow(node.y, 2), 6*Pow(node.x, 2) + 7*Pow(node.y, 2)),
                    'y' => new Complex(3*Pow(node.x, 2) - 2*Pow(node.y, 2), Pow(node.x, 2) + Pow(node.y, 2)),
                    _   => new Complex(0, 0)
                };

            // Полином третьей степени
            case 3: 
                return axe switch {
                    'x' => new Complex(2*Pow(node.x, 3) + 3*Pow(node.y, 3), 6*Pow(node.x, 3) + 7*Pow(node.y, 3)),
                    'y' => new Complex(3*Pow(node.x, 3) - 2*Pow(node.y, 3), Pow(node.x, 3) + Pow(node.y, 3)),
                    _   => new Complex(0, 0)
                };

            // Полином четвертой степени
            case 4:
                return axe switch {
                    'x' => new Complex(2*Pow(node.x, 4) + 3*Pow(node.y, 4), 6*Pow(node.x, 4) + 7*Pow(node.y, 4)),
                    'y' => new Complex(3*Pow(node.x, 4) - 2*Pow(node.y, 4), Pow(node.x, 4) + Pow(node.y, 4)),
                    _   => new Complex(0, 0)
                };
            
            default: 
            return 0;
        }
    }


    //: Вектор-функция правой части
    public static Complex Func(Edge edge, double sigma) {
        
        // Определение компоненты вектора и узла
        char axe;
        Node node = new Node();
        if (edge.node1.y == edge.node2.y) {
            axe = 'x';
            node.x = edge.node1.x + (edge.node2.x - edge.node1.x) / 2.0;
            node.y = edge.node1.y;
        } else {
            axe = 'y';
            node.x = edge.node1.x;
            node.y = edge.node1.y + (edge.node2.y - edge.node1.y) / 2.0;
        }

        // Коеффициент второго слагаемого уравнения
        Complex sigma_omega_A = new Complex(0, 1) * sigma * omega * Absolut(node, axe);
        
        switch (numberFunc)
        {
            // Разное местоположение второго краевого
            case 1:
                return axe switch {
                    'x' => 1/mu*new Complex(0, 0) + sigma_omega_A,
                    'y' => 1/mu*new Complex(0, 0) + sigma_omega_A,
                    _   => new Complex(0, 0)  
                };

            // Полином второй степени
            case 2:
                return axe switch {
                    'x' => 1/mu*new Complex(-6, -14) + sigma_omega_A,
                    'y' => 1/mu*new Complex(-6, -2) + sigma_omega_A,
                    _   => new Complex(0, 0)
                };

            // Полином третьей степени
            case 3:
                return axe switch {
                    'x' => 1/mu*new Complex(-18*node.y, -42*node.y) + sigma_omega_A,
                    'y' => 1/mu*new Complex(-18*node.x, -6*node.x) + sigma_omega_A,
                    _   => new Complex(0, 0)
                };

            // Полином четвертой степени
            case 4:
                return axe switch {
                    'x' => 1/mu*new Complex(-36*Pow(node.y, 2), -84*Pow(node.y, 2)) + sigma_omega_A,
                    'y' => 1/mu*new Complex(-36*Pow(node.x, 2), -12*Pow(node.x, 2)) + sigma_omega_A,
                    _   => new Complex(0, 0)
                };

            default:
            return 0;
        }
    }

    //: Вычисление компоненты Theta
    public static Complex Theta(Edge edge) {
        
        // Определение узла
        Node node = new Node();
        if (edge.node1.y == edge.node2.y) {
            node.x = edge.node1.x + (edge.node2.x - edge.node1.x) / 2.0;
            node.y = edge.node1.y;
        } else {
            node.x = edge.node1.x;
            node.y = edge.node1.y + (edge.node2.y - edge.node1.y) / 2.0;
        }

        // Левая производная
        Node node_diff_left = node with { x = node.x + 1e-10 };
        Complex diff_left = (Absolut(node_diff_left, 'y') - Absolut(node, 'y')) / 1e-10;

        // Правая производная
        Node node_diff_right = node with { y = node.y + 1e-10 };
        Complex diff_right = (Absolut(node_diff_right, 'x') - Absolut(node, 'x')) / 1e-10;
        
        return diff_left - diff_right;
    }
}