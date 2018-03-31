using UnityEngine;

public class PhaseImportanceSample : MonoBehaviour
{
    [Range(4, 10)]
    public int NumSamplesPowOf2 = 6;

    public bool DisplaySplitted = true;

    public bool MultiplyByPDF = true;

    [Range(0.0f, 0.1f)]
    public float MaxCosineDifference = 0.01f;

    [Range(-1.0f, 1.0f)]
    public float CornetteShanksPDF_Assymetry = 0.76f;

    [Range(3, 8)]
    public int DescreteCDFNumSamplesPowOf2 = 5;

    public float DescretePDFMinBound = 0.0f;
    public float DescretePDFMaxBound = 0.0f;

    public float[] DescretePDF = null;
    public float[] DescreteCDF = null;

    public delegate float DelegatePDF(float CosTheta);

    static Vector3 SphericalToCartesian(float PhiAngle, float CosTheta)
    {
        float ClampedCosine = Mathf.Clamp(CosTheta, -1.0f, 1.0f);
        if (ClampedCosine != CosTheta)
        {
            //Debug.Log("Broken CosTheta = " + CosTheta);
        }

        float SinTheta = Mathf.Sqrt(1.0f - CosTheta * CosTheta);

        return new Vector3(SinTheta * Mathf.Cos(PhiAngle), SinTheta * Mathf.Sin(PhiAngle), CosTheta);
    }

    float RayleighPDF(float CosTheta)
    {
        if (MultiplyByPDF)
        {
            return (1.0f + CosTheta * CosTheta) * 3.0f / (16.0f * Mathf.PI);
        }
        return 0.1f;
    }

    float CornetteShanksPDF(float CosTheta)
    {
        if (MultiplyByPDF)
        {
            float g = CornetteShanksPDF_Assymetry;
            float d = 1.0f + g * (g - 2.0f * CosTheta);
            float g2 = g * g;
            float d3 = d * d * d;
            float Den = 8.0f * Mathf.PI * (2.0f + g2) * Mathf.Sqrt(d3);
            float Nom = (1.0f - g2) * 3.0f * (1.0f + CosTheta * CosTheta);
            return Nom / Den;
        }
        return 0.1f;
    }

    public static float RayleighMDF(float CosTheta)
    {
        return (3.0f / 8.0f) * (1.0f + CosTheta * CosTheta) * Mathf.Sqrt(1.0f - CosTheta * CosTheta);
    }

    public float CornetteShanksMDF(float CosTheta)
    {
        float g = CornetteShanksPDF_Assymetry;
        float d = 1.0f + g * (g - 2.0f * CosTheta);
        float g2 = g * g;
        float d3 = d * d * d;
        float Den = 4.0f * (2.0f + g2) * Mathf.Sqrt(d3);
        float Nom = (1.0f - g2) * 3.0f * (1.0f + CosTheta * CosTheta) * Mathf.Sqrt(1.0f - CosTheta * CosTheta);
        return Nom / Den;
    }

    public static float SampleRayleigh(float UnitRandomValue)
    {
        float b = 4.0f * UnitRandomValue - 2.0f;
        float a = Mathf.Pow(Mathf.Sqrt(b * b + 1.0f) - b, 1.0f / 3.0f);
        float CosTheta = 1.0f / a - a;
        return CosTheta;
    }

    Vector3 ImportanceSampleRayleigh(Vector2 xi)
    {
        float Phi = xi.x;
        float CosTheta = xi.y;
        // Remap from [-1.0, 1.0] to [0.0 1.0]
        float Param = CosTheta * 0.5f + 0.5f;
        return SphericalToCartesian(Phi, SampleRayleigh(Param)) * RayleighPDF(CosTheta);
    }

    Vector3 ImportanceSampleUniform(Vector2 xi)
    {
        float Phi = xi.x;
        float CosTheta = xi.y;
        return SphericalToCartesian(Phi, CosTheta) * RayleighPDF(CosTheta);
    }

    static Vector2 ClampedPhiAngleCosTheta(float PhiAngle, float CosTheta)
    {
        return new Vector2(Mathf.Clamp(PhiAngle, 0.0f, 2.0f * Mathf.PI), Mathf.Clamp(CosTheta, -1.0f, 1.0f));
    }

    // According to http://lgdv.cs.fau.de/uploads/publications/spherical_fibonacci_mapping_opt.pdf
    static Vector2 SphericalFibonacci(int i, int n)
    {
        float GoldenRatio = Mathf.Sqrt(5.0f) * 0.5f + 0.5f;
        float z = ((float)i) / GoldenRatio;
        float x = 2.0f * Mathf.PI * (z - Mathf.Floor(z));
        float y = 1.0f - (1.0f + 2.0f * (float)i) / (float)n;
        return ClampedPhiAngleCosTheta(x, y);
    }

    static Vector2 SphericalFibonacci2(int i, int n)
    {
        float PhiAngle = 2.0f * Mathf.PI * (float)(i * 2654435769u) / 4294967296.0f;
        float CosTheta = 1.0f - (1.0f + 2.0f * (float)i) / (float)n;
        return ClampedPhiAngleCosTheta(PhiAngle, CosTheta);
    }

    static Vector2 SphereRandom()
    {
        float PhiAngle = 2.0f * Mathf.PI * Random.Range(0.0f, 1.0f);
        float CosTheta = Mathf.Cos(Random.Range(0.0f, Mathf.PI));
        return ClampedPhiAngleCosTheta(PhiAngle, CosTheta);
    }

    void DrawLocation(Vector3 RelativePosition, Color color)
    {
        float Size = 0.02f;
        Vector3 pos = gameObject.transform.position + RelativePosition;
        Vector3 udir = Vector3.up * Size;
        Vector3 fdir = Vector3.forward * Size;
        Vector3 rdir = Vector3.right * Size;
        Debug.DrawLine(pos - udir, pos + udir, color);
        Debug.DrawLine(pos - fdir, pos + fdir, color);
        Debug.DrawLine(pos - rdir, pos + rdir, color);
    }

    // Color palettes from https://c0de517e.blogspot.jp/2017/11/coder-color-palettes-for-data.html
    static Color GetColor1(int i, int n)
    {

        float x = Mathf.Clamp((float)i / (float)n, 0.0f, 1.0f);
        float r = -0.121f + 0.893f * x + 0.276f * Mathf.Sin(1.94f - 5.69f * x);
        float g = 0.07f + 0.947f * x;
        float b = 0.107f + (1.5f - 1.22f * x) * x;
        return new Color(r, g, b, 1.0f);
    }

    static Color GetColor2(int i, int n)
    {
        float x = Mathf.Clamp((float)i / (float)n, 0.0f, 1.0f) * 2.0f - 1.0f;
        float r = 0.569f + (0.396f + 0.834f * x) * Mathf.Sin(2.15f + 0.93f * x);
        float g = 0.911f + (-0.06f - 0.863f * x) * Mathf.Sin(0.181f + 1.3f * x);
        float b = 0.939f + (-0.309f - 0.705f * x) * Mathf.Sin(0.125f + 2.18f * x);
        /*
        float r = 0.484f + (0.432f - 0.104f * x) * Mathf.Sin(1.29f + 2.53f * x);
        float g = 0.334f + (0.585f + 0.00332f * x) * Mathf.Sin(1.82f + 1.95f * x);
        float b = 0.517f + (0.406f - 0.0348f * x) * Mathf.Sin(1.23f + 2.49f * x);
        */

        return new Color(r, g, b, 1.0f);
    }

    static Color GetColor2(float x)
    {
        float r = 0.569f + (0.396f + 0.834f * x) * Mathf.Sin(2.15f + 0.93f * x);
        float g = 0.911f + (-0.06f - 0.863f * x) * Mathf.Sin(0.181f + 1.3f * x);
        float b = 0.939f + (-0.309f - 0.705f * x) * Mathf.Sin(0.125f + 2.18f * x);
        //float r = 0.484f + (0.432f - 0.104f * x) * Mathf.Sin(1.29f + 2.53f * x);
        //float g = 0.334f + (0.585f + 0.00332f * x) * Mathf.Sin(1.82f + 1.95f * x);
        //float b = 0.517f + (0.406f - 0.0348f * x) * Mathf.Sin(1.23f + 2.49f * x);

        return new Color(r, g, b, 1.0f);
    }

    public float SampleDescretePDF(float UnitRandomValue)
    {
        for (int i = 0; i < DescreteCDF.Length; ++i)
        {
            if (UnitRandomValue < DescreteCDF[i])
            {
                float IntervalFraction = (UnitRandomValue - DescreteCDF[i - 1]) / (DescreteCDF[i] - DescreteCDF[i - 1]);
                float InvertedCDFValue = (IntervalFraction + (float)(i - 1)) / (float)(DescreteCDF.Length - 1);
                return Mathf.Cos(DescretePDFMinBound + (DescretePDFMaxBound - DescretePDFMinBound) * InvertedCDFValue);
            }
        }
        return 1.0f;
    }

    public Vector3 ImportanceSampleDescretePDF(Vector2 xi, DelegatePDF PDF)
    {
        float Phi = xi.x;
        float CosTheta = xi.y;
        // Remap from [-1.0, 1.0] to [0.0 1.0]
        float Param = CosTheta * 0.5f + 0.5f;
        return SphericalToCartesian(Phi, SampleDescretePDF(Param)) * PDF(CosTheta);
    }

    public void PrecomputeDescretePDF(float MinBound, float MaxBound, DelegatePDF PDF)
    {
        int DescreteCDF_NumSamples = 1 << DescreteCDFNumSamplesPowOf2;

        if (DescreteCDF != null && DescreteCDF.Length != DescreteCDF_NumSamples)
        {
            DescretePDF = null;
            DescreteCDF = null;
        }

        if (DescreteCDF == null)
        {
            DescretePDF = new float[DescreteCDF_NumSamples];
            DescreteCDF = new float[DescreteCDF_NumSamples];

            int NumIntervals = DescreteCDF_NumSamples - 1;

            float Dx = (MaxBound - MinBound) / (float)NumIntervals;

            DescretePDFMinBound = MinBound;
            DescretePDFMaxBound = MaxBound;

            DescretePDF[0] = PDF(Mathf.Cos(MinBound));
            DescreteCDF[0] = 0.0f;

            for (int i = 1; i < NumIntervals; ++i)
            {
                float X = MinBound + Dx * (float)i;
                float PdfX = PDF(Mathf.Cos(X));

                DescretePDF[i] = PdfX;
                DescreteCDF[i] = PdfX * Mathf.Abs(Dx) + DescreteCDF[i - 1];
            }

            DescretePDF[NumIntervals] = PDF(Mathf.Cos(MaxBound));
            DescreteCDF[NumIntervals] = 1.0f;
        }
    }

    void Update ()
    {
        PrecomputeDescretePDF(Mathf.PI, 0.0f, CornetteShanksMDF);

        int NumSamples = 1 << NumSamplesPowOf2;

        for (int i = 0; i < NumSamples; ++i)
        {
            Vector2 xi = SphericalFibonacci(i, NumSamples);
            Vector2 xi2 = SphereRandom();

            Vector3 Dir1 = 5.0f * ImportanceSampleUniform(xi);
            Vector3 Dir2 = 5.0f * ImportanceSampleRayleigh(xi);

            Vector3 Dir3 = 5.0f * ImportanceSampleUniform(xi2);
            Vector3 Dir4 = 5.0f * ImportanceSampleRayleigh(xi2);

            Vector3 Dir5 = 5.0f * ImportanceSampleDescretePDF(xi, CornetteShanksPDF);

            float Dot = Vector3.Dot(Vector3.Normalize(Dir1), Vector3.Normalize(Dir2));
            float Dot2 = Vector3.Dot(Vector3.Normalize(Dir3), Vector3.Normalize(Dir4));

            float ColorFactor = Mathf.Clamp(Dot - (1.0f - MaxCosineDifference), 0.0f, MaxCosineDifference) / MaxCosineDifference * 2.0f - 1.0f;
            float ColorFactor2 = Mathf.Clamp(Dot2 - (1.0f - MaxCosineDifference), 0.0f, MaxCosineDifference) / MaxCosineDifference * 2.0f - 1.0f;

            if (DisplaySplitted)
            {
                DrawLocation(-Vector3.forward + Dir1, Color.green);
                DrawLocation( Vector3.forward + Dir2, GetColor2(ColorFactor));

                DrawLocation(Vector3.up - Vector3.forward + Dir3, Color.green);
                DrawLocation(Vector3.up + Vector3.forward + Dir4, GetColor2(ColorFactor2));
            }
            else
            {
                DrawLocation(Dir1, Color.green);
                DrawLocation(Dir2, GetColor2(ColorFactor));

                DrawLocation(Vector3.up + Dir3, Color.green);
                DrawLocation(Vector3.up + Dir4, GetColor2(ColorFactor2));

                Debug.DrawLine(gameObject.transform.position + Dir1, gameObject.transform.position + Dir2, GetColor2(ColorFactor));
                Debug.DrawLine(gameObject.transform.position + Vector3.up + Dir3, gameObject.transform.position + Vector3.up + Dir4, GetColor2(ColorFactor2));
            }

            DrawLocation(Dir5 - Vector3.up, Color.blue);
        }
    }
}
