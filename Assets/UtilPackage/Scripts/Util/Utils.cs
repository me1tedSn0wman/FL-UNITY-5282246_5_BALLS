using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Utils
{
    public class Util : MonoBehaviour
    {
        static public Vector3 Bezier(float u, List<Vector3> list, int i0 = 0, int i1 = -1) {
            if (i1 == -1) i1 = list.Count - 1;
            if (i0 == i1) {
                return list[i0];
            }

            Vector3 l = Bezier(u, list, i0, i1 - 1);
            Vector3 r = Bezier(u, list, i0 + 1, i1);

            Vector3 res = Vector3.LerpUnclamped(l, r, u);
            return res;
        }

        static public Vector3 Bezier(float u, params Vector3[] vecs) {
            return Bezier(u, new List<Vector3>(vecs));
        }

        static public Quaternion Bezier(float u, List<Quaternion> list, int i0 = 0, int i1 = -1)
        {
            if (i1 == -1) i1 = list.Count - 1;

            if (i0 == i1)
            {
                return (list[i0]);
            }

            Quaternion l = Bezier(u, list, i0, i1 - 1);
            Quaternion r = Bezier(u, list, i0 + 1, i1);
            Quaternion res = Quaternion.SlerpUnclamped(l, r, u);

            return (res);
        }

        static public Quaternion Bezier(float u, params Quaternion[] arr)
        {
            return (Bezier(u, new List<Quaternion>(arr)));
        }

        static public float Bezier(float u, List<float> list, int i0 = 0, int i1 = -1)
        {
            if (i1 == -1) i1 = list.Count - 1;
            if (i0 == i1)
            {
                return list[i0];
            }

            float l = Bezier(u, list, i0, i1 - 1);
            float r = Bezier(u, list, i0 + 1, i1);

            float res = Mathf.Lerp(l, r, u);
            return res;
        }

        static public float Bezier(float u, params float[] vecs)
        {
            return Bezier(u, new List<float>(vecs));
        }
    }

    [System.Serializable]
    public class EasingCachedCurve {
        public List<string> curves = new List<string>();
        public List<float> mods = new List<float>();
    }

    public class Easing
    {
        static public string Linear = ",Linear|";
        static public string In = ",In|";
        static public string Out = ",Out|";
        static public string InOut = ",InOut|";
        static public string Sin = ",Sin|";
        static public string SinIn = ",SinIn|";
        static public string SinOut = ",SinOut|";

        static public Dictionary<string, EasingCachedCurve> cache;

        static public float Ease(float u, params string[] curveParams) {
            if (cache == null) {
                cache = new Dictionary<string, EasingCachedCurve>();
            }

            float u2 = u;
            foreach (string curve in curveParams) {
                if (!cache.ContainsKey(curve)) {
                    EaseParse(curve);
                }
                u2 = EaseP(u2, cache[curve]);
            }
            return u2;
        }
        
        static private void EaseParse(string curveIn)
        {
            EasingCachedCurve ecc = new EasingCachedCurve();
            string[] curves = curveIn.Split(',');
            foreach (string curve in curves)
            {
                if (curve == "") continue;

                string[] curveA = curve.Split('|');
                ecc.curves.Add(curveA[0]);
                if (curveA.Length == 1 || curveA[1] == "")
                {
                    ecc.mods.Add(float.NaN);
                }
                else
                {
                    float parseRes;
                    if (float.TryParse(curveA[1], out parseRes))
                    {
                        ecc.mods.Add(parseRes);
                    }
                    else
                    {
                        ecc.mods.Add(float.NaN);
                    }
                }
            }
            cache.Add(curveIn, ecc);
        }

        static public float Ease(float u, string curve, float mod)
        {
            return (EaseP(u, curve, mod));
        }

        static private float EaseP(float u, EasingCachedCurve ec)
        {
            float u2 = u;
            for (int i = 0; i < ec.curves.Count; i++)
            {
                u2 = EaseP(u2, ec.curves[i], ec.mods[i]);
            }
            return (u2);
        }

        static private float EaseP(float u, string curve, float mod)
        {
            float u2 = u;

            switch (curve)
            {
                case "In":
                    if (float.IsNaN(mod)) mod = 2;
                    u2 = Mathf.Pow(u, mod);
                    break;

                case "Out":
                    if (float.IsNaN(mod)) mod = 2;
                    u2 = 1 - Mathf.Pow(1 - u, mod);
                    break;

                case "InOut":
                    if (float.IsNaN(mod)) mod = 2;
                    if (u <= 0.5f)
                    {
                        u2 = 0.5f * Mathf.Pow(u * 2, mod);
                    }
                    else
                    {
                        u2 = 0.5f + 0.5f * (1 - Mathf.Pow(1 - (2 * (u - 0.5f)), mod));
                    }
                    break;

                case "Sin":
                    if (float.IsNaN(mod)) mod = 0.15f;
                    u2 = u + mod * Mathf.Sin(2 * Mathf.PI * u);
                    break;

                case "SinIn":
                    u2 = 1 - Mathf.Cos(u * Mathf.PI * 0.5f);
                    break;

                case "SinOut":
                    u2 = Mathf.Sin(u * Mathf.PI * 0.5f);
                    break;

                case "Linear":
                default:
                    break;
            }

            return (u2);
        }
    }
}
