namespace ProcCityGen.Data
{
    using Unity.Mathematics;

    public struct Tensor
    {
        // A tensor is a 2x2 symmetric and traceless matrix of the form
        // R * | cos(2theta)  sin(2theta) |  = | a b |
        //     | sin(2theta) -cos(2theta) |    | _ _ |
        // where R >= 0 and theta is [0, 2pi)

        public double r;
        public double a;
        public double b;

        public Tensor(double r, double a, double b)
        {
            this.r = r;
            this.a = a;
            this.b = b;
        }

        public static Tensor FromAngle(double angle)
        {
            return new Tensor(1, math.cos(angle * 4), math.sin(angle * 4));
        }

        public static Tensor FromTheta(double r, double theta)
        {
            return new Tensor(r, r * math.cos(2 * theta), r * math.sin(2 * theta));
        }

        public static Tensor Normalize(Tensor tensor)
        {
            double l = math.sqrt(tensor.a * tensor.a + tensor.b * tensor.b);

            return math.abs(l) < math.EPSILON ? new Tensor(0, 0, 0) : new Tensor(1, tensor.a / l, tensor.b / l);
        }

        public static Tensor operator +(Tensor left, Tensor right)
        {
            return new Tensor(1, left.a + right.a, left.b + right.b);
        }

        public static Tensor operator *(double left, Tensor right)
        {
            return new Tensor(1, left * right.a, left * right.b);
        }

        public void EigenVectors(out float2 major, out float2 minor)
        {
            float theta = (float)math.atan2(b / r, a / r) / 2;
            float angle = theta + math.PI / 2;

            major = new float2(math.cos(theta), math.sin(theta));
            minor = new float2(math.cos(angle), math.sin(angle));
        }

        public Tensor Combine(Tensor other, bool smooth)
        {
            a = a * r + other.a * other.r;
            b = b * r + other.b * other.r;

            if (smooth)
            {
                r = math.sqrt(a * a + b * b);
                a /= r;
                b /= r;
            }
            else
            {
                r = 2;
            }

            return this;
        }

        public Tensor Scale(double s)
        {
            r *= s;
            return this;
        }
    }
}
