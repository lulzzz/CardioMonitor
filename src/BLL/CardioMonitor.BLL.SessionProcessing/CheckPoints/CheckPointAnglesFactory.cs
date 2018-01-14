namespace CardioMonitor.BLL.SessionProcessing.CheckPoints
{
    public class CheckPointAnglesFactory
    {
        public double[] Create(MaxAngle maxAngle)
        {
            switch (maxAngle)
            {
                case MaxAngle._7Dot5:
                    return new[]
                    {
                        0,
                        7.5,
                        0
                    };
                case MaxAngle._9:
                    return new double[]
                    {
                        0,
                        9,
                        0
                    };
                case MaxAngle._10Dot5:
                    return new[]
                    {
                        0,
                        10.5,
                        0
                    };
                case MaxAngle._12:
                    return new double[]
                    {
                        0,
                        12,
                        0
                    };
                case MaxAngle._13Do5:
                    return new[]
                    {
                        0,
                        6,
                        13.5,
                        6,
                        0
                    };
                case MaxAngle._15:
                    return new[]
                    {
                        0,
                        7.5,
                        15,
                        7.5,
                        0
                    };
                case MaxAngle._16Dot5:
                    return new[]
                    {
                        0,
                        7.5,
                        16.5,
                        7.5,
                        0
                    };
                case MaxAngle._18:
                    return new double[]
                    {
                        0,
                        9,
                        18,
                        9,
                        0
                    };
                case MaxAngle._19Dot5:
                    return new[]
                    {
                        0,
                        9,
                        19.5,
                        9,
                        0
                    };
                case MaxAngle._21:
                    return new[]
                    {
                        0,
                        10.5,
                        21,
                        10.5,
                        0
                    };
                case MaxAngle._22Dot5:
                    return new[]
                    {
                        0,
                        7.5,
                        15,
                        22.5,
                        15,
                        7.5,
                        0
                    };
                case MaxAngle._24:
                    return new[]
                    {
                        0,
                        7.5,
                        15,
                        24,
                        15,
                        7.5,
                        0
                    };
                case MaxAngle._25Dot5:
                    return new[]
                    {
                        0,
                        9,
                        16.5,
                        25.5,
                        16.5,
                        9,
                        0
                    };
                case MaxAngle._27:
                    return new double[]
                    {
                        0,
                        9,
                        18,
                        27,
                        18,
                        9,
                        0
                    };
                case MaxAngle._28Dot5:
                    return new[]
                    {
                        0,
                        9,
                        19.5,
                        28.5,
                        19.5,
                        9,
                        0
                    };
                case MaxAngle._30:
                    return new[]
                    {
                        0,
                        10.5,
                        19.5,
                        30,
                        19.5,
                        10.5,
                        0
                    };
                case MaxAngle._31Dot5:
                    return new[]
                    {
                        0,
                        10.5,
                        21,
                        31.5,
                        21,
                        10.5,
                        0
                    };
                default:
                    return new[] {0.0};
            }
        }
    }
}