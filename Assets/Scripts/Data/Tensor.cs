namespace ProcCityGen.Data
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
            return new Tensor(math.cos(theta * 4), math.sin(theta * 4));
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

            return math.abs(l) < math.EPSILON ? new Tensor(0, 0) : new Tensor(tensor.a / l, tensor.b / l);
        }

        public static Tensor operator +(Tensor left, Tensor right)
        {
            return new Tensor(left.a + right.a, left.b + right.b);
        }

        public static Tensor operator *(double left, Tensor right)
        {
            return new Tensor(left * right.a, left * right.b);
        }

        public void EigenVectors(out float2 major, out float2 minor)
        {
            float theta = (float)math.atan2(b / 0.1, a / 0.1) / 2;
            float angle = theta + math.PI / 2;

            major = new float2(math.cos(theta), math.sin(theta));
            minor = new float2(math.cos(angle), math.sin(angle));
        }
    }
}
