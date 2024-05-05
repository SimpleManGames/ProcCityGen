namespace ProcCityGen
{
    using Unity.Mathematics;

    public struct Tensor
    {
        // A tensor is a 2x2 symmetric and traceless matrix of the form
        // R * | cos(2theta)  sin(2theta) |  = | a b |
        //     | sin(2theta) -cos(2theta) |    | _ _ |
        // where R >= 0 and theta is [0, 2pi)

        public readonly double a;
        public readonly double b;

        public Tensor(double a, double b)
        {
            this.a = a;
            this.b = b;
        }

        public static Tensor FromRTheta(double r, double theta)
        {
            return new Tensor(r * math.cos(2 * theta), r * math.sin(2 * theta));
        }
        
        public static Tensor FromXY(float2 xy)
        {
            float xy2 = -2 * xy.x * xy.y;
            float diffSquares = xy.y * xy.y - xy.x * xy.x;
            return Normalize(new Tensor(diffSquares, xy2));
        }
        
        public static Tensor Normalize(Tensor tensor)
        {
            double l = math.sqrt(tensor.a * tensor.a + tensor.b * tensor.b);

            return math.abs(l) < float.Epsilon ? new Tensor(0, 0) : new Tensor(tensor.a / l, tensor.b / l);
        }
        
        public static Tensor operator +(Tensor left, Tensor right)
        {
            return new Tensor(left.a + right.a, left.b + right.b);
        }

        public static Tensor operator *(double left, Tensor right)
        {
            return new Tensor(left * right.a, left * right.b);
        }
        
        public void EigenValues(out double e1, out double e2)
        {
            double eval = math.sqrt(a * a + b * b);

            e1 = eval;
            e2 = -eval;
        }
        
        public void EigenVectors(out float2 major, out float2 minor)
        {
            if (math.abs(b) < 0.0000001f)
            {
                if (math.abs(a) < 0.0000001f)
                {
                    major = float2.zero;
                    minor = float2.zero;
                }
                else
                {
                    major = new float2(1, 0);
                    minor = new float2(0, 1);
                }
            }
            else
            {
                EigenValues(out double e1, out double e2);

                major = new float2((float)b, (float)(e1 - a));
                minor = new float2((float)b, (float)(e2 - a));
            }
        }
    }
}